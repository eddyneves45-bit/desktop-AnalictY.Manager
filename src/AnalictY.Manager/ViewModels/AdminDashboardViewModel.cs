using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Models;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class AdminDashboardViewModel : ObservableObject
{
    private readonly HealthService _healthService;
    private readonly VersionService _versionService;
    private bool _isLoading;
    private bool _isServerOnline;
    private string _serverStatus = "Não verificado";
    private string _version = "-";
    private string _channel = "-";
    private string _dataDirectory = "-";
    private string _databaseStatus = "-";
    private string _lastCheckedAt = "-";
    private string _uptimeText = "Aguardando leitura";
    private string _environmentName = "Local";
    private string _machineName = Environment.MachineName;
    private string _cpuUsage = "18%";
    private string _memoryUsage = "42%";
    private string _diskUsage = "35%";
    private string _totalTags = "0";
    private string _activeTags = "0";
    private string _staleTags = "0";
    private string _queuedEvents = "0";
    private string _lastProcessingAt = "-";
    private string _mqttStatus = "Aguardando";
    private string _opcStatus = "Aguardando";
    private string _apiStatus = "Não verificado";
    private string _webSocketStatus = "0 conexões";
    private string _mqttReceived = "0 msg/s";
    private string _mqttSent = "0 msg/s";
    private string _mqttRetained = "0";
    private string _mqttTopics = "0";
    private string _mqttClients = "0";
    private string _databaseFile = "analicty.db";
    private string _databaseSize = "-";
    private string _databaseTables = "-";
    private string _databaseRecords = "-";
    private string _currentClock = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

    public AdminDashboardViewModel(HealthService healthService, VersionService versionService)
    {
        _healthService = healthService;
        _versionService = versionService;
        RefreshCommand = new RelayCommand(async () => await RefreshAsync());
        OpenWebCommand = new RelayCommand(_ => { OpenWeb(); return Task.CompletedTask; });
        OpenLogsCommand = new RelayCommand(_ => { OpenLogsFolder(); return Task.CompletedTask; });
        RestartServicesCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Reinício de serviços será ligado ao AnalictY Server em uma etapa controlada.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
        SyncTagsCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Sincronização de TAGs será ligada ao Runtime quando o endpoint administrativo estiver disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });

        TagRows = new ObservableCollection<AdminTagRow>
        {
            new("M01_Producao_Contador", "Máquina 01", "OPC UA", "ns=2;s=Producao.Contador", "1.250", "Good", CurrentClock),
            new("M01_Status_Maquina", "Máquina 01", "OPC UA", "ns=2;s=Status.Maquina", "1 (Ligado)", "Good", CurrentClock),
            new("M02_Producao_Contador", "Máquina 02", "MQTT", "fabrica/m02/producao", "980", "Good", CurrentClock),
            new("PJ08_Ciclo", "Prensa Joelho 08", "HTTP", "/api/weintek/pj08", "56", "Good", CurrentClock)
        };

        ServiceRows = new ObservableCollection<AdminServiceRow>
        {
            new("AnalictY API", "Em execução", "Aguardando leitura"),
            new("Runtime Service", "Em execução", "Aguardando leitura"),
            new("MQTT Service", "Aguardando", "-"),
            new("OPC UA Service", "Aguardando", "-"),
            new("Database Service", "Em execução", "Aguardando leitura")
        };

        LogRows = new ObservableCollection<AdminLogRow>
        {
            new(CurrentClock, "INFO", "AnalictY Manager iniciado"),
            new(CurrentClock, "INFO", "Aguardando leitura do AnalictY Server"),
            new(CurrentClock, "WARN", "Alguns módulos administrativos ainda usam fallback seguro")
        };
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsServerOnline
    {
        get => _isServerOnline;
        set => SetProperty(ref _isServerOnline, value);
    }

    public string ServerStatus
    {
        get => _serverStatus;
        set => SetProperty(ref _serverStatus, value);
    }

    public string Version
    {
        get => _version;
        set => SetProperty(ref _version, value);
    }

    public string Channel
    {
        get => _channel;
        set => SetProperty(ref _channel, value);
    }

    public string DataDirectory
    {
        get => _dataDirectory;
        set => SetProperty(ref _dataDirectory, value);
    }

    public string DatabaseStatus
    {
        get => _databaseStatus;
        set => SetProperty(ref _databaseStatus, value);
    }

    public string LastCheckedAt
    {
        get => _lastCheckedAt;
        set => SetProperty(ref _lastCheckedAt, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand OpenWebCommand { get; }
    public ICommand OpenLogsCommand { get; }
    public ICommand RestartServicesCommand { get; }
    public ICommand SyncTagsCommand { get; }
    public ObservableCollection<AdminTagRow> TagRows { get; }
    public ObservableCollection<AdminServiceRow> ServiceRows { get; }
    public ObservableCollection<AdminLogRow> LogRows { get; }
    public string UptimeText { get => _uptimeText; set => SetProperty(ref _uptimeText, value); }
    public string EnvironmentName { get => _environmentName; set => SetProperty(ref _environmentName, value); }
    public string MachineName { get => _machineName; set => SetProperty(ref _machineName, value); }
    public string CpuUsage { get => _cpuUsage; set => SetProperty(ref _cpuUsage, value); }
    public string MemoryUsage { get => _memoryUsage; set => SetProperty(ref _memoryUsage, value); }
    public string DiskUsage { get => _diskUsage; set => SetProperty(ref _diskUsage, value); }
    public string TotalTags { get => _totalTags; set => SetProperty(ref _totalTags, value); }
    public string ActiveTags { get => _activeTags; set => SetProperty(ref _activeTags, value); }
    public string StaleTags { get => _staleTags; set => SetProperty(ref _staleTags, value); }
    public string QueuedEvents { get => _queuedEvents; set => SetProperty(ref _queuedEvents, value); }
    public string LastProcessingAt { get => _lastProcessingAt; set => SetProperty(ref _lastProcessingAt, value); }
    public string MqttStatus { get => _mqttStatus; set => SetProperty(ref _mqttStatus, value); }
    public string OpcStatus { get => _opcStatus; set => SetProperty(ref _opcStatus, value); }
    public string ApiStatus { get => _apiStatus; set => SetProperty(ref _apiStatus, value); }
    public string WebSocketStatus { get => _webSocketStatus; set => SetProperty(ref _webSocketStatus, value); }
    public string MqttReceived { get => _mqttReceived; set => SetProperty(ref _mqttReceived, value); }
    public string MqttSent { get => _mqttSent; set => SetProperty(ref _mqttSent, value); }
    public string MqttRetained { get => _mqttRetained; set => SetProperty(ref _mqttRetained, value); }
    public string MqttTopics { get => _mqttTopics; set => SetProperty(ref _mqttTopics, value); }
    public string MqttClients { get => _mqttClients; set => SetProperty(ref _mqttClients, value); }
    public string DatabaseFile { get => _databaseFile; set => SetProperty(ref _databaseFile, value); }
    public string DatabaseSize { get => _databaseSize; set => SetProperty(ref _databaseSize, value); }
    public string DatabaseTables { get => _databaseTables; set => SetProperty(ref _databaseTables, value); }
    public string DatabaseRecords { get => _databaseRecords; set => SetProperty(ref _databaseRecords, value); }
    public string CurrentClock { get => _currentClock; set => SetProperty(ref _currentClock, value); }

    public async Task LoadAsync()
    {
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        IsLoading = true;
        try
        {
            var healthTask = _healthService.CheckAsync();
            var versionTask = _versionService.GetVersionAsync();

            await Task.WhenAll(healthTask, versionTask);

            var health = await healthTask;
            var version = await versionTask;

            IsServerOnline = health.IsOnline;
            ServerStatus = health.Message;
            Version = version.Version;
            Channel = version.Channel;
            DataDirectory = string.IsNullOrWhiteSpace(version.DataDirectory) ? version.Source : version.DataDirectory;
            DatabaseStatus = IsServerOnline ? "Conectado" : "Desconectado";
            CurrentClock = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            LastCheckedAt = CurrentClock;
            UptimeText = IsServerOnline ? "Executando agora" : "Servidor indisponível";
            ApiStatus = IsServerOnline ? "Online" : "Offline";
            TotalTags = TagRows.Count.ToString("N0");
            ActiveTags = IsServerOnline ? TagRows.Count.ToString("N0") : "0";
            StaleTags = IsServerOnline ? "0" : TagRows.Count.ToString("N0");
            QueuedEvents = IsServerOnline ? "0" : "-";
            LastProcessingAt = CurrentClock;
            MqttStatus = IsServerOnline ? "Conectado" : "Aguardando";
            OpcStatus = IsServerOnline ? "Conectado" : "Aguardando";
            WebSocketStatus = IsServerOnline ? "1 conexão" : "0 conexões";
            MqttReceived = IsServerOnline ? "0 msg/s" : "0 msg/s";
            MqttSent = IsServerOnline ? "0 msg/s" : "0 msg/s";
            MqttRetained = "0";
            MqttTopics = IsServerOnline ? "Aguardando coleta" : "0";
            MqttClients = IsServerOnline ? "Aguardando coleta" : "0";
            DatabaseSize = IsServerOnline ? "Aguardando leitura" : "-";
            DatabaseTables = IsServerOnline ? "Aguardando leitura" : "-";
            DatabaseRecords = IsServerOnline ? "Aguardando leitura" : "-";

            LogRows.Insert(0, new AdminLogRow(CurrentClock, IsServerOnline ? "INFO" : "WARN", health.Message));
            while (LogRows.Count > 8)
            {
                LogRows.RemoveAt(LogRows.Count - 1);
            }
        }
        catch (Exception ex)
        {
            ServerStatus = $"Erro: {ex.Message}";
            IsServerOnline = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OpenWeb()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://analicty",
                UseShellExecute = true
            });
        }
        catch
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "http://localhost:3000",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Não foi possível abrir o AnalictY Web: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void OpenLogsFolder()
    {
        try
        {
            var logsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AnalictY", "logs");
            if (Directory.Exists(logsPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logsPath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show("Pasta de logs não encontrada.", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Não foi possível abrir a pasta de logs: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

public sealed record AdminTagRow(string Name, string Machine, string Driver, string Address, string Value, string Quality, string UpdatedAt);
public sealed record AdminServiceRow(string Name, string Status, string Uptime);
public sealed record AdminLogRow(string DateTime, string Level, string Message);
