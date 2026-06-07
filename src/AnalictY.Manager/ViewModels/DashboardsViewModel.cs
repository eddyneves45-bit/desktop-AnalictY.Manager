using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.ViewModels;

public sealed class DashboardsViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando dashboards...";
    private string _errorMessage = string.Empty;
    private DashboardConfig? _selectedDashboard;

    // Form fields
    private string _dashboardName = "Dashboard operacional";
    private string _machineId = string.Empty;
    private string _periodPreset = "today";
    private string _refreshInterval = "10";
    private bool _isDefault = true;
    private Machine? _selectedMachine;

    public DashboardsViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(Refresh);
        CreateCommand = new RelayCommand(OpenCreateModal);
        EditCommand = new RelayCommand(OpenEditModal);
        DeleteCommand = new RelayCommand(DeleteDashboard);
        SaveCommand = new RelayCommand(SaveDashboard);

        Dashboards = new ObservableCollection<DashboardConfig>();
        Machines = new ObservableCollection<Machine>();
    }

    public ObservableCollection<DashboardConfig> Dashboards { get; }
    public ObservableCollection<Machine> Machines { get; }

    public ICommand RefreshCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }

    private async Task OnRefreshAsync()
    {
        await LoadAsync();
    }

    public Task Refresh() => OnRefreshAsync();

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

    public DashboardConfig? SelectedDashboard
    {
        get => _selectedDashboard;
        set => SetProperty(ref _selectedDashboard, value);
    }

    public Machine? SelectedMachine
    {
        get => _selectedMachine;
        set
        {
            if (SetProperty(ref _selectedMachine, value))
            {
                MachineId = value?.Id ?? string.Empty;
            }
        }
    }

    // Form fields
    public string DashboardName
    {
        get => _dashboardName;
        set => SetProperty(ref _dashboardName, value);
    }

    public string MachineId
    {
        get => _machineId;
        set => SetProperty(ref _machineId, value);
    }

    public string PeriodPreset
    {
        get => _periodPreset;
        set => SetProperty(ref _periodPreset, value);
    }

    public string RefreshInterval
    {
        get => _refreshInterval;
        set => SetProperty(ref _refreshInterval, value);
    }

    public bool IsDefault
    {
        get => _isDefault;
        set => SetProperty(ref _isDefault, value);
    }

    public bool IsEditing => SelectedDashboard != null;
    public string FormTitle => IsEditing ? "Editar Dashboard" : "Novo Dashboard";

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando dashboards...";

        try
        {
            var machinesTask = _configService.GetProductionDiagnosticsAsync(
                null,
                DateTime.Today.ToString("yyyy-MM-ddT00:00:00"),
                DateTime.Today.ToString("yyyy-MM-ddT23:59:59"));
            var dashboardsResult = await _configService.GetDashboardConfigsAsync();
            var machinesResult = await machinesTask;
            if (!string.IsNullOrWhiteSpace(dashboardsResult.Error))
            {
                ErrorMessage = dashboardsResult.Error;
                StatusMessage = "Erro ao carregar dashboards.";
                return;
            }

            Dashboards.Clear();
            foreach (var dashboard in dashboardsResult.Configs)
            {
                Dashboards.Add(dashboard);
            }

            Machines.Clear();
            if (machinesResult.Snapshot != null)
            {
                foreach (var machine in machinesResult.Snapshot.Machines)
                {
                    Machines.Add(machine);
                }
            }

            StatusMessage = $"{Dashboards.Count} dashboard(s) carregado(s).";
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

    public Task OpenCreateModal()
    {
        SelectedDashboard = null;
        DashboardName = "Dashboard operacional";
        MachineId = string.Empty;
        SelectedMachine = null;
        PeriodPreset = "today";
        RefreshInterval = "10";
        IsDefault = true;

        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));
        return Task.CompletedTask;
    }

    public Task OpenEditModal()
    {
        if (SelectedDashboard == null) return Task.CompletedTask;

        DashboardName = SelectedDashboard.Name ?? string.Empty;
        MachineId = SelectedDashboard.MachineId ?? string.Empty;
        SelectedMachine = Machines.FirstOrDefault(machine => machine.Id == MachineId);
        PeriodPreset = SelectedDashboard.PeriodPreset ?? "today";
        RefreshInterval = SelectedDashboard.RefreshInterval ?? "10";
        IsDefault = SelectedDashboard.IsDefault;

        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));
        return Task.CompletedTask;
    }

    public async Task SaveDashboard()
    {
        if (string.IsNullOrWhiteSpace(DashboardName) || string.IsNullOrWhiteSpace(MachineId))
        {
            ErrorMessage = "Preencha nome e máquina.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new DashboardConfigRequest
            {
                Id = SelectedDashboard?.Id,
                Name = DashboardName,
                MachineId = MachineId,
                PeriodPreset = PeriodPreset,
                RefreshInterval = RefreshInterval,
                IsDefault = IsDefault,
                IsActive = true,
                Widgets = SelectedDashboard?.Widgets ?? new List<DashboardWidget>()
            };

            var result = await _configService.SaveDashboardConfigAsync(request);
            if (result.Success)
            {
                StatusMessage = IsEditing ? "Dashboard atualizado com sucesso." : "Dashboard criado com sucesso.";
                await LoadAsync();
                await OpenCreateModal();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao salvar dashboard.";
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

    public async Task DeleteDashboard()
    {
        if (SelectedDashboard == null || SelectedDashboard.Id == null)
        {
            ErrorMessage = "Selecione um dashboard para excluir.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.DeleteDashboardConfigAsync(SelectedDashboard.Id.Value);
            if (result.Success)
            {
                StatusMessage = "Dashboard excluído com sucesso.";
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao excluir dashboard.";
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
}
