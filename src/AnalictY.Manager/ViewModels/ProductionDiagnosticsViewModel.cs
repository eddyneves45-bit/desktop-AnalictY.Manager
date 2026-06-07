using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.ViewModels;

public sealed class ProductionDiagnosticsViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando diagnóstico...";
    private string _errorMessage = string.Empty;
    private string _machineId = string.Empty;
    private DateTime? _selectedDate = DateTime.Today;
    private bool _autoRefresh = true;
    private string _timeZoneId = "America/Sao_Paulo";
    private string _timeZoneLabel = "Brasil - Brasília (GMT-3)";

    public ProductionDiagnosticsViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(Refresh);
        LoadCommand = new RelayCommand(Load);
        ToggleAutoRefreshCommand = new RelayCommand(ToggleAutoRefresh);

        Snapshot = new DiagnosticSnapshot();
        Machines = new ObservableCollection<Machine>();
    }

    private DiagnosticSnapshot _snapshot = new();

    public DiagnosticSnapshot Snapshot
    {
        get => _snapshot;
        private set
        {
            if (SetProperty(ref _snapshot, value))
            {
                OnPropertyChanged(nameof(Totals));
                OnPropertyChanged(nameof(SelectedMachine));
            }
        }
    }
    public ObservableCollection<Machine> Machines { get; }

    public ICommand RefreshCommand { get; }
    public ICommand LoadCommand { get; }
    public ICommand ToggleAutoRefreshCommand { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string MachineId
    {
        get => _machineId;
        set
        {
            if (SetProperty(ref _machineId, value))
            {
                OnPropertyChanged(nameof(SelectedMachine));
            }
        }
    }

    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set => SetProperty(ref _selectedDate, value);
    }

    public bool AutoRefresh
    {
        get => _autoRefresh;
        set => SetProperty(ref _autoRefresh, value);
    }

    public string TimeZoneId
    {
        get => _timeZoneId;
        set => SetProperty(ref _timeZoneId, value);
    }

    public string TimeZoneLabel
    {
        get => _timeZoneLabel;
        set => SetProperty(ref _timeZoneLabel, value);
    }

    public Machine? SelectedMachine => Machines.FirstOrDefault(m => m.Id == MachineId);

    public DiagnosticTotals? Totals => Snapshot.Mysql.Totals;

    private async Task OnRefreshAsync()
    {
        await LoadAsync();
    }

    public Task Refresh() => OnRefreshAsync();

    public Task Load() => LoadAsync();

    public Task ToggleAutoRefresh()
    {
        AutoRefresh = !AutoRefresh;
        return Task.CompletedTask;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando diagnóstico...";

        try
        {
            var date = SelectedDate ?? DateTime.Today;
            var from = date.ToString("yyyy-MM-ddT00:00:00");
            var to = date.ToString("yyyy-MM-ddT23:59:59");

            var result = await _configService.GetProductionDiagnosticsAsync(
                string.IsNullOrWhiteSpace(MachineId) ? null : MachineId,
                from,
                to);

            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                ErrorMessage = result.Error;
                StatusMessage = "Erro ao carregar diagnóstico.";
                return;
            }

            if (result.Snapshot != null)
            {
                Snapshot = result.Snapshot;
                Machines.Clear();
                foreach (var machine in result.Snapshot.Machines)
                {
                    Machines.Add(machine);
                }

                if (string.IsNullOrWhiteSpace(MachineId) && !string.IsNullOrWhiteSpace(result.Snapshot.MachineId))
                {
                    MachineId = result.Snapshot.MachineId;
                }

                StatusMessage = $"Atualizado em {result.Snapshot.GeneratedAt}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadTimeZoneAsync()
    {
        try
        {
            var result = await _configService.GetSystemTimezoneAsync();
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                return;
            }

            TimeZoneId = result.TimeZoneId;
            TimeZoneLabel = result.Label;
        }
        catch
        {
            // Ignore errors loading timezone
        }
    }
}
