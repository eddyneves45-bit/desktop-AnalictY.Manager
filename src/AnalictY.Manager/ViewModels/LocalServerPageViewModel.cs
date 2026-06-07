using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class LocalServerPageViewModel : ObservableObject
{
    private readonly LocalServerService _localServerService;
    private bool _isLoading;
    private string _statusMessage = "Carregando informações do servidor...";
    private string _errorMessage = string.Empty;
    private string _hostname = "-";
    private string _ipAddress = "-";
    private int _port;
    private string _osVersion = "-";
    private int _cpuUsagePercent;
    private int _memoryUsageMb;
    private int _diskUsageGb;
    private int _uptimeSeconds;
    private string _dataDirectory = "-";
    private string _environment = "-";

    public LocalServerPageViewModel(LocalServerService localServerService)
    {
        _localServerService = localServerService;
        
        RefreshCommand = new RelayCommand(async _ => await LoadAsync());
        
        _ = LoadAsync();
    }

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

    public string Hostname
    {
        get => _hostname;
        set => SetProperty(ref _hostname, value);
    }

    public string IpAddress
    {
        get => _ipAddress;
        set => SetProperty(ref _ipAddress, value);
    }

    public int Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }

    public string OsVersion
    {
        get => _osVersion;
        set => SetProperty(ref _osVersion, value);
    }

    public int CpuUsagePercent
    {
        get => _cpuUsagePercent;
        set => SetProperty(ref _cpuUsagePercent, value);
    }

    public int MemoryUsageMb
    {
        get => _memoryUsageMb;
        set => SetProperty(ref _memoryUsageMb, value);
    }

    public int DiskUsageGb
    {
        get => _diskUsageGb;
        set => SetProperty(ref _diskUsageGb, value);
    }

    public int UptimeSeconds
    {
        get => _uptimeSeconds;
        set => SetProperty(ref _uptimeSeconds, value);
    }

    public string UptimeFormatted => FormatUptime(_uptimeSeconds);

    public string DataDirectory
    {
        get => _dataDirectory;
        set => SetProperty(ref _dataDirectory, value);
    }

    public string Environment
    {
        get => _environment;
        set => SetProperty(ref _environment, value);
    }

    public ICommand RefreshCommand { get; }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _localServerService.GetInfoAsync();
            
            if (!string.IsNullOrEmpty(result.Error))
            {
                ErrorMessage = result.Error;
                StatusMessage = "Erro ao carregar";
                return;
            }

            Hostname = result.Hostname ?? "-";
            IpAddress = result.IpAddress ?? "-";
            Port = result.Port;
            OsVersion = result.OsVersion ?? "-";
            CpuUsagePercent = result.CpuUsagePercent;
            MemoryUsageMb = result.MemoryUsageMb;
            DiskUsageGb = result.DiskUsageGb;
            UptimeSeconds = result.UptimeSeconds;
            DataDirectory = result.DataDirectory ?? "-";
            Environment = result.Environment ?? "-";

            OnPropertyChanged(nameof(UptimeFormatted));
            StatusMessage = "Informações carregadas";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar informações: {ex.Message}";
            StatusMessage = "Erro ao carregar";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string FormatUptime(int seconds)
    {
        if (seconds == 0) return "-";
        
        var timeSpan = TimeSpan.FromSeconds(seconds);
        if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays}d {(int)timeSpan.Hours}h {(int)timeSpan.Minutes}m";
        if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours}h {(int)timeSpan.Minutes}m";
        if (timeSpan.TotalMinutes >= 1)
            return $"{(int)timeSpan.TotalMinutes}m {(int)timeSpan.Seconds}s";
        return $"{seconds}s";
    }
}
