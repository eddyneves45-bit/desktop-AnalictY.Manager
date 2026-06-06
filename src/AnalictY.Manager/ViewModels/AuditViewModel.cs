using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.ViewModels;

public sealed class AuditViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando auditoria...";
    private string _errorMessage = string.Empty;
    private string _search = string.Empty;

    public AuditViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(Refresh);

        Logs = new ObservableCollection<AuditLog>();
        FilteredLogs = new ObservableCollection<AuditLog>();
    }

    public ObservableCollection<AuditLog> Logs { get; }
    public ObservableCollection<AuditLog> FilteredLogs { get; }

    public ICommand RefreshCommand { get; }

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

    public string Search
    {
        get => _search;
        set
        {
            if (SetProperty(ref _search, value))
            {
                FilterLogs();
            }
        }
    }

    private async Task OnRefreshAsync()
    {
        await LoadAsync();
    }

    public Task Refresh() => OnRefreshAsync();

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando auditoria...";

        try
        {
            var result = await _configService.GetAuditLogsAsync(200);
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                ErrorMessage = result.Error;
                return;
            }

            Logs.Clear();
            foreach (var log in result.Logs)
            {
                Logs.Add(log);
            }

            FilterLogs();
            StatusMessage = $"{Logs.Count} registro(s) de auditoria carregado(s).";
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

    private void FilterLogs()
    {
        var filtered = string.IsNullOrWhiteSpace(_search)
            ? Logs.ToList()
            : Logs.Where(l =>
                (l.Username?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (l.Role?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (l.Action?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (l.Path?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (l.EntityType?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (l.EntityId?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();

        FilteredLogs.Clear();
        foreach (var log in filtered)
        {
            FilteredLogs.Add(log);
        }
    }
}
