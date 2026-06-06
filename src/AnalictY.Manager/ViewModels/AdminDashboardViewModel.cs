using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class AdminDashboardViewModel : ObservableObject
{
    private readonly AdminApiService _adminApiService;
    private bool _isLoading;
    private bool _isServerOnline;
    private string _serverStatus = "Nao verificado";
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
    private string _apiStatus = "Nao verificado";
    private string _webSocketStatus = "0 conexoes";
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

    public AdminDashboardViewModel(AdminApiService adminApiService)
    {
        _adminApiService = adminApiService;
        RefreshCommand = new RelayCommand(async () => await RefreshAsync());
        OpenWebCommand = new RelayCommand(_ => { OpenWeb(); return Task.CompletedTask; });
        OpenLogsCommand = new RelayCommand(_ => { OpenLogsFolder(); return Task.CompletedTask; });
        RestartServicesCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Reinicio de servicos sera ligado ao AnalictY Server em uma etapa controlada.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
        SyncTagsCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Sincronizacao de TAGs sera ligada ao Runtime quando o endpoint administrativo estiver disponivel.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });

        TagRows = new ObservableCollection<AdminTagRow>();
        ServiceRows = new ObservableCollection<AdminServiceRow>();
        LogRows = new ObservableCollection<AdminLogRow>
        {
            new(CurrentClock, "INFO", "AnalictY Manager iniciado"),
            new(CurrentClock, "INFO", "Aguardando leitura do AnalictY Server")
        };
    }

    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }
    public bool IsServerOnline { get => _isServerOnline; set => SetProperty(ref _isServerOnline, value); }
    public string ServerStatus { get => _serverStatus; set => SetProperty(ref _serverStatus, value); }
    public string Version { get => _version; set => SetProperty(ref _version, value); }
    public string Channel { get => _channel; set => SetProperty(ref _channel, value); }
    public string DataDirectory { get => _dataDirectory; set => SetProperty(ref _dataDirectory, value); }
    public string DatabaseStatus { get => _databaseStatus; set => SetProperty(ref _databaseStatus, value); }
    public string LastCheckedAt { get => _lastCheckedAt; set => SetProperty(ref _lastCheckedAt, value); }
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
            var overviewTask = _adminApiService.GetOverviewAsync();
            var databaseTask = _adminApiService.GetDatabaseAsync();
            var mqttTask = _adminApiService.GetMqttAsync();
            var servicesTask = _adminApiService.GetServicesAsync();
            var logsTask = _adminApiService.GetLogsAsync();
            var tagsTask = _adminApiService.GetTagsAsync();

            await Task.WhenAll(overviewTask, databaseTask, mqttTask, servicesTask, logsTask, tagsTask);

            var overview = await overviewTask;
            var database = await databaseTask;
            var mqtt = await mqttTask;
            var services = await servicesTask;
            var logs = await logsTask;
            var tags = await tagsTask;

            IsServerOnline = string.Equals(overview.ServerStatus, "Online", StringComparison.OrdinalIgnoreCase);
            ServerStatus = overview.ServerStatus;
            Version = overview.Version;
            Channel = overview.Channel;
            DataDirectory = overview.DataDirectory;
            DatabaseStatus = overview.DatabaseStatus;
            CurrentClock = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            LastCheckedAt = CurrentClock;
            UptimeText = overview.Uptime;
            EnvironmentName = overview.Environment;
            MachineName = overview.MachineName;
            ApiStatus = overview.ApiStatus;
            TotalTags = tags.Total;
            ActiveTags = tags.Active;
            StaleTags = tags.Stale;
            QueuedEvents = overview.EventsQueued;
            LastProcessingAt = CurrentClock;
            MqttStatus = mqtt.Status;
            OpcStatus = overview.OpcUaStatus;
            WebSocketStatus = IsServerOnline ? "1 conexao" : "0 conexoes";
            MqttReceived = $"{mqtt.MessagesReceivedPerSecond} msg/s";
            MqttSent = $"{mqtt.MessagesSentPerSecond} msg/s";
            MqttRetained = mqtt.Retained;
            MqttTopics = mqtt.Topics;
            MqttClients = mqtt.ClientsConnected;
            DatabaseFile = database.DatabaseFile;
            DatabaseSize = database.Size;
            DatabaseTables = database.Tables;
            DatabaseRecords = database.Records;

            ServiceRows.Clear();
            foreach (var service in services)
            {
                ServiceRows.Add(new AdminServiceRow(service.Name, service.Status, service.Uptime));
            }

            TagRows.Clear();
            TagRows.Add(new AdminTagRow("Resumo de TAGs", MachineName, "Runtime", "Total cadastrado", TotalTags, IsServerOnline ? "Good" : "Offline", CurrentClock));
            TagRows.Add(new AdminTagRow("TAGs ativas", MachineName, "Runtime", "Leitura atual", ActiveTags, IsServerOnline ? "Good" : "Offline", CurrentClock));
            TagRows.Add(new AdminTagRow("Sem atualizacao", MachineName, "Runtime", "Monitoramento", StaleTags, StaleTags == "0" ? "Good" : "Warn", CurrentClock));

            LogRows.Clear();
            foreach (var log in logs.Take(8))
            {
                LogRows.Add(new AdminLogRow(log.DateTime, log.Level, log.Message));
            }

            if (LogRows.Count == 0)
            {
                LogRows.Add(new AdminLogRow(CurrentClock, "INFO", "AnalictY Server conectado."));
            }
        }
        catch (Exception ex)
        {
            ServerStatus = $"Erro: {ex.Message}";
            IsServerOnline = false;
            LogRows.Insert(0, new AdminLogRow(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), "ERROR", ex.Message));
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static void OpenWeb()
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
            Process.Start(new ProcessStartInfo
            {
                FileName = "http://localhost:3000",
                UseShellExecute = true
            });
        }
    }

    private static void OpenLogsFolder()
    {
        var installLogs = @"C:\Program Files\AnalictY\logs";
        var programDataLogs = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AnalictY", "logs");
        var logsPath = Directory.Exists(installLogs) ? installLogs : programDataLogs;

        if (Directory.Exists(logsPath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = logsPath,
                UseShellExecute = true
            });
            return;
        }

        MessageBox.Show("Pasta de logs nao encontrada.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

public sealed record AdminTagRow(string Name, string Machine, string Driver, string Address, string Value, string Quality, string UpdatedAt);
public sealed record AdminServiceRow(string Name, string Status, string Uptime);
public sealed record AdminLogRow(string DateTime, string Level, string Message);
