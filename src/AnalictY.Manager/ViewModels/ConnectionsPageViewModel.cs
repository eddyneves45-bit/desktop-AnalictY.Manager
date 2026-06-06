using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.ViewModels;

public sealed class ConnectionsPageViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando conexoes do servidor...";
    private string _errorMessage = string.Empty;
    private string _searchText = string.Empty;
    private string _selectedTypeFilter = "Todos";
    private ConnectionRow? _selectedConnection;
    private bool _isEditDialogOpen;
    private string _editDialogTitle = string.Empty;
    private string _editDialogType = string.Empty;
    private readonly List<ConnectionRow> _loadedConnections = new();

    public ConnectionsPageViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(LoadAsync);
        NewConnectionCommand = new RelayCommand(OpenNewConnectionDialog);
        EditConnectionCommand = new RelayCommand(EditConnection);
        DeleteConnectionCommand = new RelayCommand(DeleteConnectionAsync);
        TestConnectionCommand = new RelayCommand(TestConnectionAsync);
        SaveConnectionCommand = new RelayCommand(SaveConnectionAsync);
        CancelEditCommand = new RelayCommand(CancelEdit);
        SetMysqlPrimaryCommand = new RelayCommand(SetMysqlPrimaryAsync);
        SetMysqlLocalCommand = new RelayCommand(SetMysqlLocalAsync);
        SetMysqlRemoteCommand = new RelayCommand(SetMysqlRemoteAsync);
        InitMysqlCommand = new RelayCommand(InitMysqlAsync);
        
        OpcUaConnections = new ObservableCollection<ConnectionRow>();
        MqttConnections = new ObservableCollection<ConnectionRow>();
        MysqlConnections = new ObservableCollection<ConnectionRow>();
        FtpConnections = new ObservableCollection<ConnectionRow>();
        AllConnections = new ObservableCollection<ConnectionRow>();
        
        TypeFilters = new ObservableCollection<string> { "Todos", "OPC UA", "MQTT", "MySQL", "SQL Server", "FTP/SFTP" };
        ConnectionTypes = new ObservableCollection<string> { "OPC UA", "MQTT", "MySQL", "SQL Server", "FTP/SFTP" };
    }

    public ObservableCollection<ConnectionRow> OpcUaConnections { get; }
    public ObservableCollection<ConnectionRow> MqttConnections { get; }
    public ObservableCollection<ConnectionRow> MysqlConnections { get; }
    public ObservableCollection<ConnectionRow> FtpConnections { get; }
    public ObservableCollection<ConnectionRow> AllConnections { get; }
    public ObservableCollection<string> TypeFilters { get; }
    public ObservableCollection<string> ConnectionTypes { get; }
    
    public ICommand RefreshCommand { get; }
    public ICommand NewConnectionCommand { get; }
    public ICommand EditConnectionCommand { get; }
    public ICommand DeleteConnectionCommand { get; }
    public ICommand TestConnectionCommand { get; }
    public ICommand SaveConnectionCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand SetMysqlPrimaryCommand { get; }
    public ICommand SetMysqlLocalCommand { get; }
    public ICommand SetMysqlRemoteCommand { get; }
    public ICommand InitMysqlCommand { get; }

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

    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            FilterConnections();
        }
    }

    public string SelectedTypeFilter
    {
        get => _selectedTypeFilter;
        set
        {
            SetProperty(ref _selectedTypeFilter, value);
            FilterConnections();
        }
    }

    public ConnectionRow? SelectedConnection
    {
        get => _selectedConnection;
        set => SetProperty(ref _selectedConnection, value);
    }

    public bool IsEditDialogOpen
    {
        get => _isEditDialogOpen;
        set => SetProperty(ref _isEditDialogOpen, value);
    }

    public string EditDialogTitle
    {
        get => _editDialogTitle;
        set => SetProperty(ref _editDialogTitle, value);
    }

    public string EditDialogType
    {
        get => _editDialogType;
        set => SetProperty(ref _editDialogType, value);
    }

    // Edit Dialog Properties
    private string _editName = string.Empty;
    private string _editEndpoint = string.Empty;
    private string _editHost = string.Empty;
    private string _editPort = string.Empty;
    private string _editDatabase = string.Empty;
    private string _editUsername = string.Empty;
    private string _editPassword = string.Empty;
    private string _editType = string.Empty;
    private string _editSecurityPolicy = "None";
    private string _editSecurityMode = "None";
    private string _editCertificatePath = string.Empty;
    private string _editPrivateKeyPath = string.Empty;
    private string _editFtpPrivateKeyPath = string.Empty;
    private string _editUpdateInterval = "1000";
    private string _editClientId = string.Empty;
    private bool _editTlsEnabled;
    private string _editCaCertPath = string.Empty;
    private string _editClientCertPath = string.Empty;
    private string _editClientKeyPath = string.Empty;
    private string _editTopics = string.Empty;
    private string _editQos = "0";
    private string _editPoolSize = "10";
    private bool _editIsActive = true;
    private bool _editIsPrimary;
    private bool _editIsLocal;
    private string _editProtocol = "SFTP";
    private string _editDirectory = "/";
    private string _editFrequency = "manual";
    private string _editDataType = "production";
    private string _editFileFormat = "CSV";

    public string EditName
    {
        get => _editName;
        set => SetProperty(ref _editName, value);
    }

    public string EditEndpoint
    {
        get => _editEndpoint;
        set => SetProperty(ref _editEndpoint, value);
    }

    public string EditHost
    {
        get => _editHost;
        set => SetProperty(ref _editHost, value);
    }

    public string EditPort
    {
        get => _editPort;
        set => SetProperty(ref _editPort, value);
    }

    public string EditDatabase
    {
        get => _editDatabase;
        set => SetProperty(ref _editDatabase, value);
    }

    public string EditUsername
    {
        get => _editUsername;
        set => SetProperty(ref _editUsername, value);
    }

    public string EditPassword
    {
        get => _editPassword;
        set => SetProperty(ref _editPassword, value);
    }

    public string EditType
    {
        get => _editType;
        set => SetProperty(ref _editType, value);
    }

    public string EditSecurityPolicy { get => _editSecurityPolicy; set => SetProperty(ref _editSecurityPolicy, value); }
    public string EditSecurityMode { get => _editSecurityMode; set => SetProperty(ref _editSecurityMode, value); }
    public string EditCertificatePath { get => _editCertificatePath; set => SetProperty(ref _editCertificatePath, value); }
    public string EditPrivateKeyPath { get => _editPrivateKeyPath; set => SetProperty(ref _editPrivateKeyPath, value); }
    public string EditFtpPrivateKeyPath { get => _editFtpPrivateKeyPath; set => SetProperty(ref _editFtpPrivateKeyPath, value); }
    public string EditUpdateInterval { get => _editUpdateInterval; set => SetProperty(ref _editUpdateInterval, value); }
    public string EditClientId { get => _editClientId; set => SetProperty(ref _editClientId, value); }
    public bool EditTlsEnabled { get => _editTlsEnabled; set => SetProperty(ref _editTlsEnabled, value); }
    public string EditCaCertPath { get => _editCaCertPath; set => SetProperty(ref _editCaCertPath, value); }
    public string EditClientCertPath { get => _editClientCertPath; set => SetProperty(ref _editClientCertPath, value); }
    public string EditClientKeyPath { get => _editClientKeyPath; set => SetProperty(ref _editClientKeyPath, value); }
    public string EditTopics { get => _editTopics; set => SetProperty(ref _editTopics, value); }
    public string EditQos { get => _editQos; set => SetProperty(ref _editQos, value); }
    public string EditPoolSize { get => _editPoolSize; set => SetProperty(ref _editPoolSize, value); }
    public bool EditIsActive { get => _editIsActive; set => SetProperty(ref _editIsActive, value); }
    public bool EditIsPrimary { get => _editIsPrimary; set => SetProperty(ref _editIsPrimary, value); }
    public bool EditIsLocal { get => _editIsLocal; set => SetProperty(ref _editIsLocal, value); }
    public string EditProtocol { get => _editProtocol; set => SetProperty(ref _editProtocol, value); }
    public string EditDirectory { get => _editDirectory; set => SetProperty(ref _editDirectory, value); }
    public string EditFrequency { get => _editFrequency; set => SetProperty(ref _editFrequency, value); }
    public string EditDataType { get => _editDataType; set => SetProperty(ref _editDataType, value); }
    public string EditFileFormat { get => _editFileFormat; set => SetProperty(ref _editFileFormat, value); }

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

            BuildAllConnections();
            FilterConnections();

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
                Id = item.Id,
                Name = item.Name,
                Address = item.ServerUrl,
                Details = $"Driver OPC UA - {item.SecurityPolicy}/{item.SecurityMode}",
                Status = item.IsActive ? NormalizeStatus(item.Status) : "Desativado",
                Type = "OPC UA",
                SecurityPolicy = item.SecurityPolicy,
                SecurityMode = item.SecurityMode,
                Username = item.Username,
                CertificatePath = item.CertificatePath,
                PrivateKeyPath = item.PrivateKeyPath,
                UpdateInterval = item.UpdateInterval,
                IsActive = item.IsActive
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
                Id = item.Id,
                Name = item.Name,
                Address = $"{item.BrokerHost}:{item.BrokerPort}",
                Details = "Broker MQTT",
                Status = item.IsActive ? NormalizeStatus(item.Status) : "Desativado",
                Type = "MQTT",
                Host = item.BrokerHost,
                Port = item.BrokerPort,
                ClientId = item.ClientId,
                Username = item.Username,
                TlsEnabled = item.TlsEnabled,
                CaCertPath = item.CaCertPath,
                ClientCertPath = item.ClientCertPath,
                ClientKeyPath = item.ClientKeyPath,
                Topics = item.Topics,
                Qos = item.Qos,
                IsActive = item.IsActive
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
            var connectionType = IsSqlServer(item.Provider) ? "SQL Server" : "MySQL";
            MysqlConnections.Add(new ConnectionRow
            {
                Id = item.Id,
                Name = item.Name,
                Address = $"{item.Host}:{item.Port}/{item.Database}",
                Details = $"{item.Provider} - {role}",
                Status = item.IsActive ? NormalizeStatus(item.Status) : "Desativado",
                Type = connectionType,
                Host = item.Host,
                Port = item.Port,
                Database = item.Database,
                Username = item.Username,
                PoolSize = item.PoolSize,
                IsActive = item.IsActive,
                IsPrimary = item.IsPrimary,
                IsLocal = item.IsLocal
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
            Id = "ftp",
            Name = result.Name,
            Address = string.IsNullOrWhiteSpace(result.Host) ? result.Directory : $"{result.Host}:{result.Port}",
            Details = $"{result.Protocol} - {(string.IsNullOrWhiteSpace(result.Username) ? result.Directory : result.Username)}",
            Status = result.Enabled ? "Ativo" : "Desativado",
            Type = "FTP/SFTP",
            Protocol = result.Protocol,
            Host = result.Host,
            Port = result.Port,
            Username = result.Username,
            PasswordConfigured = result.PasswordConfigured,
            PrivateKeyPath = result.PrivateKeyPath,
            Directory = result.Directory,
            Frequency = result.Frequency,
            DataType = result.DataType,
            FileFormat = result.FileFormat,
            IsActive = result.Enabled
        });
    }

    private void BuildAllConnections()
    {
        _loadedConnections.Clear();
        _loadedConnections.AddRange(OpcUaConnections);
        _loadedConnections.AddRange(MqttConnections);
        _loadedConnections.AddRange(MysqlConnections);
        _loadedConnections.AddRange(FtpConnections);
    }

    private void FilterConnections()
    {
        var filtered = _loadedConnections.Where(c =>
        {
            var matchesType = SelectedTypeFilter == "Todos" || c.Type == SelectedTypeFilter;
            var matchesSearch = string.IsNullOrWhiteSpace(SearchText) ||
                c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                c.Address.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
            return matchesType && matchesSearch;
        }).ToList();

        AllConnections.Clear();
        foreach (var conn in filtered) AllConnections.Add(conn);
    }

    private void ClearCollections()
    {
        OpcUaConnections.Clear();
        MqttConnections.Clear();
        MysqlConnections.Clear();
        FtpConnections.Clear();
        AllConnections.Clear();
        _loadedConnections.Clear();
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

    // Commands
    private Task OpenNewConnectionDialog()
    {
        ClearEditFields();
        EditDialogTitle = "Nova Conexão";
        EditDialogType = "Create";
        IsEditDialogOpen = true;
        return Task.CompletedTask;
    }

    private Task EditConnection()
    {
        if (SelectedConnection == null) return Task.CompletedTask;
        
        ClearEditFields();
        EditDialogTitle = "Editar Conexão";
        EditDialogType = "Edit";
        EditName = SelectedConnection.Name;
        EditType = SelectedConnection.Type;
        
        // Parse address based on type
        if (SelectedConnection.Type == "OPC UA")
        {
            EditEndpoint = SelectedConnection.Address;
        }
        else if (SelectedConnection.Type == "MQTT")
        {
            var parts = SelectedConnection.Address.Split(':');
            if (parts.Length >= 2)
            {
                EditHost = parts[0];
                EditPort = parts[1];
            }
        }
        else if (SelectedConnection.Type == "MySQL" || SelectedConnection.Type == "SQL Server")
        {
            var parts = SelectedConnection.Address.Split('/');
            if (parts.Length >= 2)
            {
                var hostPort = parts[0].Split(':');
                if (hostPort.Length >= 2)
                {
                    EditHost = hostPort[0];
                    EditPort = hostPort[1];
                }
                EditDatabase = parts[1];
            }
        }
        else if (SelectedConnection.Type == "FTP/SFTP")
        {
            var parts = SelectedConnection.Address.Split(':');
            if (parts.Length >= 2)
            {
                EditHost = parts[0];
                EditPort = parts[1];
            }
            EditDirectory = SelectedConnection.Details.Contains(" - ", StringComparison.Ordinal)
                ? SelectedConnection.Details.Split(" - ").Last()
                : "/";
            EditProtocol = SelectedConnection.Details.StartsWith("FTP", StringComparison.OrdinalIgnoreCase) ? "FTP" : "SFTP";
        }
        
        IsEditDialogOpen = true;
        return Task.CompletedTask;
    }

    public async Task DeleteConnectionAsync()
    {
        if (SelectedConnection == null) return;
        
        // FTP não pode ser excluído (configuração única)
        if (SelectedConnection.Type == "FTP/SFTP")
        {
            ErrorMessage = "Configuração FTP não pode ser excluída, apenas editada.";
            return;
        }

        try
        {
            var result = SelectedConnection.Type switch
            {
                "OPC UA" => await _configService.DeleteOpcUaAsync(SelectedConnection.Id),
                "MQTT" => await _configService.DeleteMqttAsync(SelectedConnection.Id),
                "MySQL" or "SQL Server" => await _configService.DeleteMysqlAsync(SelectedConnection.Id),
                _ => OperationResult.CreateFailed("Tipo de conexão não suportado")
            };

            if (result.Success)
            {
                StatusMessage = result.Message;
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao excluir: {ex.Message}";
        }
    }

    public async Task TestConnectionAsync()
    {
        if (SelectedConnection == null) return;

        try
        {
            var result = SelectedConnection.Type switch
            {
                "OPC UA" => await _configService.TestOpcUaEndpointAsync(SelectedConnection.Address),
                "MQTT" => await _configService.TestMqttAsync(SelectedConnection.Id),
                "MySQL" or "SQL Server" => await _configService.TestMysqlByIdAsync(SelectedConnection.Id),
                "FTP/SFTP" => await _configService.TestFtpExportAsync(BuildFtpRequest()),
                _ => OperationResult.CreateFailed("Tipo de conexão não suportado para teste")
            };

            if (result.Success)
            {
                StatusMessage = result.Message;
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao testar: {ex.Message}";
        }
    }

    public async Task SaveConnectionAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName))
        {
            ErrorMessage = "Nome é obrigatório";
            return;
        }

        if (RequiresPassword(EditType) && string.IsNullOrWhiteSpace(EditPassword))
        {
            ErrorMessage = "Informe a senha correta para salvar/testar esta conexao. A senha cadastrada nao e exibida por seguranca.";
            return;
        }

        try
        {
            if (EditDialogType == "Create")
            {
                var result = EditType switch
                {
                    "OPC UA" => await _configService.CreateOpcUaAsync(BuildOpcUaRequest()),
                    "MQTT" => await _configService.CreateMqttAsync(BuildMqttRequest()),
                    "MySQL" => await _configService.CreateMysqlAsync(BuildMysqlRequest("MySQL")),
                    "SQL Server" => await _configService.CreateMysqlAsync(BuildMysqlRequest("SQLServer")),
                    "FTP/SFTP" => await _configService.SaveFtpExportAsync(BuildFtpRequest()),
                    _ => OperationResult.CreateFailed("Tipo de conexão não suportado")
                };

                if (result.Success)
                {
                    StatusMessage = result.Message;
                    IsEditDialogOpen = false;
                    await LoadAsync();
                }
                else
                {
                    ErrorMessage = BuildConnectionError(result.Message);
                }
            }
            else if (EditDialogType == "Edit" && SelectedConnection != null)
            {
                var result = SelectedConnection.Type switch
                {
                    "OPC UA" => await _configService.UpdateOpcUaAsync(SelectedConnection.Id, BuildOpcUaRequest()),
                    "MQTT" => await _configService.UpdateMqttAsync(SelectedConnection.Id, BuildMqttRequest()),
                    "MySQL" => await _configService.UpdateMysqlAsync(SelectedConnection.Id, BuildMysqlRequest("MySQL")),
                    "SQL Server" => await _configService.UpdateMysqlAsync(SelectedConnection.Id, BuildMysqlRequest("SQLServer")),
                    "FTP/SFTP" => await _configService.SaveFtpExportAsync(BuildFtpRequest()),
                    _ => OperationResult.CreateFailed("Tipo de conexão não suportado")
                };

                if (result.Success)
                {
                    StatusMessage = result.Message;
                    IsEditDialogOpen = false;
                    await LoadAsync();
                }
                else
                {
                    ErrorMessage = BuildConnectionError(result.Message);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao salvar: {ex.Message}";
        }
    }

    private Task CancelEdit()
    {
        IsEditDialogOpen = false;
        ClearEditFields();
        return Task.CompletedTask;
    }

    private void ClearEditFields()
    {
        EditName = string.Empty;
        EditEndpoint = string.Empty;
        EditHost = string.Empty;
        EditPort = string.Empty;
        EditDatabase = string.Empty;
        EditUsername = string.Empty;
        EditPassword = string.Empty;
        EditType = string.Empty;
        EditSecurityPolicy = "None";
        EditSecurityMode = "None";
        EditCertificatePath = string.Empty;
        EditPrivateKeyPath = string.Empty;
        EditUpdateInterval = "1000";
        EditClientId = string.Empty;
        EditTlsEnabled = false;
        EditCaCertPath = string.Empty;
        EditClientCertPath = string.Empty;
        EditClientKeyPath = string.Empty;
        EditTopics = string.Empty;
        EditQos = "0";
        EditPoolSize = "10";
        EditIsActive = true;
        EditIsPrimary = false;
        EditIsLocal = false;
        EditProtocol = "SFTP";
        EditDirectory = "/";
        EditFrequency = "manual";
        EditDataType = "production";
        EditFileFormat = "CSV";
    }

    // MySQL-specific commands
    public async Task SetMysqlPrimaryAsync()
    {
        if (SelectedConnection?.Type != "MySQL") return;
        
        var result = await _configService.SetMysqlPrimaryAsync(SelectedConnection.Id);
        if (result.Success)
        {
            StatusMessage = result.Message;
            await LoadAsync();
        }
        else
        {
            ErrorMessage = result.Message;
        }
    }

    public async Task SetMysqlLocalAsync()
    {
        if (SelectedConnection?.Type != "MySQL") return;
        
        var result = await _configService.SetMysqlLocalAsync(SelectedConnection.Id);
        if (result.Success)
        {
            StatusMessage = result.Message;
            await LoadAsync();
        }
        else
        {
            ErrorMessage = result.Message;
        }
    }

    public async Task SetMysqlRemoteAsync()
    {
        if (SelectedConnection?.Type != "MySQL") return;
        
        var result = await _configService.SetMysqlRemoteAsync(SelectedConnection.Id);
        if (result.Success)
        {
            StatusMessage = result.Message;
            await LoadAsync();
        }
        else
        {
            ErrorMessage = result.Message;
        }
    }

    public async Task InitMysqlAsync()
    {
        if (SelectedConnection?.Type != "MySQL") return;

        var result = await _configService.InitMysqlAsync(SelectedConnection.Id);
        if (result.Success)
        {
            StatusMessage = result.Message;
        }
        else
        {
            ErrorMessage = result.Message;
        }
    }

    public async Task TestFtpAsync()
    {
        var request = BuildFtpRequest();
        var result = await _configService.TestFtpExportAsync(request);
        if (result.Success)
        {
            StatusMessage = result.Message;
        }
        else
        {
            ErrorMessage = result.Message;
        }
    }

    public async Task SendFtpNowAsync()
    {
        var result = await _configService.SendFtpNowAsync();
        if (result.Success)
        {
            StatusMessage = result.Message;
        }
        else
        {
            ErrorMessage = result.Message;
        }
    }

    private OpcUaConnectionRequest BuildOpcUaRequest() => new()
    {
        Name = EditName,
        ServerUrl = EditEndpoint,
        SecurityPolicy = EditSecurityPolicy,
        SecurityMode = EditSecurityMode,
        Username = EditUsername,
        Password = EditPassword,
        CertificatePath = EditCertificatePath,
        PrivateKeyPath = EditPrivateKeyPath,
        UpdateInterval = EditUpdateInterval,
        IsActive = EditIsActive
    };

    private MqttConnectionRequest BuildMqttRequest() => new()
    {
        Name = EditName,
        ClientId = EditClientId,
        BrokerHost = EditHost,
        BrokerPort = EditPort,
        Username = EditUsername,
        Password = EditPassword,
        TlsEnabled = EditTlsEnabled,
        CaCertPath = EditCaCertPath,
        ClientCertPath = EditClientCertPath,
        ClientKeyPath = EditClientKeyPath,
        Topics = EditTopics,
        Qos = EditQos,
        IsActive = EditIsActive
    };

    private MysqlConnectionRequest BuildMysqlRequest(string provider) => new()
    {
        Name = EditName,
        Host = EditHost,
        Port = string.IsNullOrWhiteSpace(EditPort) ? (provider == "SQLServer" ? "1433" : "3306") : EditPort,
        Database = EditDatabase,
        Provider = provider,
        Username = EditUsername,
        Password = EditPassword,
        PoolSize = EditPoolSize,
        IsActive = EditIsActive,
        IsPrimary = EditIsPrimary,
        IsLocal = EditIsLocal
    };

    private FtpExportRequest BuildFtpRequest() => new()
    {
        Name = EditName,
        Enabled = EditIsActive,
        Protocol = EditProtocol,
        Host = EditHost,
        Port = string.IsNullOrWhiteSpace(EditPort) ? (EditProtocol == "FTP" ? "21" : "22") : EditPort,
        Username = EditUsername,
        Password = EditPassword,
        PrivateKeyPath = EditFtpPrivateKeyPath,
        Directory = EditDirectory,
        Frequency = EditFrequency,
        DataType = EditDataType,
        FileFormat = EditFileFormat
    };

    private static bool IsSqlServer(string? provider) =>
        string.Equals(provider, "SQLServer", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(provider, "SqlServer", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(provider, "MSSQL", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(provider, "SQL Server", StringComparison.OrdinalIgnoreCase);

    private static bool RequiresPassword(string type) =>
        type is "MQTT" or "MySQL" or "SQL Server" or "FTP/SFTP";

    private string BuildConnectionError(string message)
    {
        if (EditType != "OPC UA" || !message.Contains("400", StringComparison.OrdinalIgnoreCase))
        {
            return message;
        }

        return $"Erro HTTP 400 ao salvar OPC UA. URL enviada: {EditEndpoint}. Security Policy: {EditSecurityPolicy}. Security Mode: {EditSecurityMode}. Verifique se a URL comeca com opc.tcp:// e se certificado/chave estao vazios quando a seguranca estiver None.";
    }
}

public sealed class ConnectionRow
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string Status { get; set; } = "Cadastrado";
    public string Type { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string SecurityPolicy { get; set; } = "None";
    public string SecurityMode { get; set; } = "None";
    public string CertificatePath { get; set; } = string.Empty;
    public string PrivateKeyPath { get; set; } = string.Empty;
    public string UpdateInterval { get; set; } = "1000";
    public string ClientId { get; set; } = string.Empty;
    public bool TlsEnabled { get; set; }
    public string CaCertPath { get; set; } = string.Empty;
    public string ClientCertPath { get; set; } = string.Empty;
    public string ClientKeyPath { get; set; } = string.Empty;
    public string Topics { get; set; } = string.Empty;
    public string Qos { get; set; } = "0";
    public string PoolSize { get; set; } = "10";
    public string Protocol { get; set; } = "SFTP";
    public string Directory { get; set; } = "/";
    public string Frequency { get; set; } = "manual";
    public string DataType { get; set; } = "production";
    public string FileFormat { get; set; } = "CSV";
    public bool IsActive { get; set; } = true;
    public bool PasswordConfigured { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsLocal { get; set; }
}
