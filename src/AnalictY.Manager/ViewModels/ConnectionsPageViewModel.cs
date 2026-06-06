using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class ConnectionsPageViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando conexoes do servidor...";
    private string _errorMessage = string.Empty;

    public ConnectionsPageViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(LoadAsync);
        OpcUaConnections = new ObservableCollection<ConnectionRow>();
        MqttConnections = new ObservableCollection<ConnectionRow>();
        MysqlConnections = new ObservableCollection<ConnectionRow>();
        FtpConnections = new ObservableCollection<ConnectionRow>();
    }

    public ObservableCollection<ConnectionRow> OpcUaConnections { get; }
    public ObservableCollection<ConnectionRow> MqttConnections { get; }
    public ObservableCollection<ConnectionRow> MysqlConnections { get; }
    public ObservableCollection<ConnectionRow> FtpConnections { get; }
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

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Consultando OPC UA, MQTT, MySQL e FTP...";
        ClearCollections();

        try
        {
            await LoadOpcUaAsync();
            await LoadMqttAsync();
            await LoadMysqlAsync();
            await LoadFtpAsync();

            var total = OpcUaConnections.Count + MqttConnections.Count + MysqlConnections.Count + FtpConnections.Count;
            StatusMessage = total == 0
                ? "Nenhuma conexao cadastrada no servidor."
                : $"{total} conexoes carregadas do servidor.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadOpcUaAsync()
    {
        var result = await _configService.GetOpcUaConnectionsAsync();
        if (!string.IsNullOrWhiteSpace(result.Error))
        {
            AddError("OPC UA", result.Error);
            return;
        }

        foreach (var item in result.Connections)
        {
            OpcUaConnections.Add(new ConnectionRow
            {
                Name = item.Name,
                Address = item.Endpoint,
                Details = "Driver OPC UA",
                Status = NormalizeStatus(item.Status)
            });
        }
    }

    private async Task LoadMqttAsync()
    {
        var result = await _configService.GetMqttConnectionsAsync();
        if (!string.IsNullOrWhiteSpace(result.Error))
        {
            AddError("MQTT", result.Error);
            return;
        }

        foreach (var item in result.Connections)
        {
            MqttConnections.Add(new ConnectionRow
            {
                Name = item.Name,
                Address = $"{item.Host}:{item.Port}",
                Details = "Broker MQTT",
                Status = NormalizeStatus(item.Status)
            });
        }
    }

    private async Task LoadMysqlAsync()
    {
        var result = await _configService.GetMysqlConnectionsAsync();
        if (!string.IsNullOrWhiteSpace(result.Error))
        {
            AddError("MySQL", result.Error);
            return;
        }

        foreach (var item in result.Connections)
        {
            var role = item.IsPrimary ? "Principal" : item.IsLocal ? "Local" : "Remoto";
            MysqlConnections.Add(new ConnectionRow
            {
                Name = item.Name,
                Address = $"{item.Host}:{item.Port}/{item.Database}",
                Details = $"{item.Type} - {role}",
                Status = NormalizeStatus(item.Status)
            });
        }
    }

    private async Task LoadFtpAsync()
    {
        var result = await _configService.GetFtpExportAsync();
        if (!string.IsNullOrWhiteSpace(result.Error))
        {
            AddError("FTP/SFTP", result.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(result.Host) && string.IsNullOrWhiteSpace(result.Directory))
        {
            return;
        }

        FtpConnections.Add(new ConnectionRow
        {
            Name = "Exportacao FTP/SFTP",
            Address = string.IsNullOrWhiteSpace(result.Host) ? result.Directory : $"{result.Host}:{result.Port}",
            Details = string.IsNullOrWhiteSpace(result.Username) ? result.Directory : result.Username,
            Status = result.Enabled ? "Ativo" : "Desativado"
        });
    }

    private void ClearCollections()
    {
        OpcUaConnections.Clear();
        MqttConnections.Clear();
        MysqlConnections.Clear();
        FtpConnections.Clear();
    }

    private void AddError(string section, string error)
    {
        ErrorMessage = string.IsNullOrWhiteSpace(ErrorMessage)
            ? $"{section}: {error}"
            : $"{ErrorMessage} | {section}: {error}";
    }

    private static string NormalizeStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("Desconhecido", StringComparison.OrdinalIgnoreCase))
        {
            return "Cadastrado";
        }

        return value;
    }
}

public sealed class ConnectionRow
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string Status { get; set; } = "Cadastrado";
}
