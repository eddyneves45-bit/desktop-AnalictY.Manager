using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class AuditPageViewModel : ObservableObject
{
    private readonly AuditService _service;
    private string _statusMessage = "Aguardando leitura.";
    private string _errorMessage = string.Empty;
    private string _totalEvents = "0";

    public AuditPageViewModel(AuditService service)
    {
        _service = service;
        Events = new ObservableCollection<AuditRow>();
        LoadCommand = new RelayCommand(async () => await LoadAsync());
    }

    public ObservableCollection<AuditRow> Events { get; }
    public ICommand LoadCommand { get; }
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
    public string TotalEvents { get => _totalEvents; set => SetProperty(ref _totalEvents, value); }

    public async Task LoadAsync()
    {
        StatusMessage = "Carregando auditoria...";
        ErrorMessage = string.Empty;
        Events.Clear();

        try
        {
            var rows = await _service.GetAuditAsync();
            foreach (var row in rows)
            {
                Events.Add(row);
            }

            TotalEvents = rows.Count.ToString();
            StatusMessage = $"{rows.Count} eventos carregados.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusMessage = "Nao foi possivel carregar auditoria.";
        }
    }
}
