using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Models;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class ServerViewModel : ObservableObject
{
    private readonly HealthService _healthService;
    private readonly VersionService _versionService;
    private bool _isLoading;
    private bool _isServerOnline;
    private string _healthStatus = "Não verificado";
    private string _version = "-";
    private string _channel = "-";
    private string _runtimeStatus = "Não verificado";
    private string _apiStatus = "Não verificado";
    private string _servicesStatus = "Não verificado";
    private string _lastCheckedAt = "-";

    public ServerViewModel(HealthService healthService, VersionService versionService)
    {
        _healthService = healthService;
        _versionService = versionService;
        RefreshCommand = new RelayCommand(async () => await RefreshAsync());
        OpenWebCommand = new RelayCommand(_ => { OpenWeb(); return Task.CompletedTask; });
        OpenDataDirectoryCommand = new RelayCommand(_ => { OpenDataDirectory(); return Task.CompletedTask; });
        OpenLogsCommand = new RelayCommand(_ => { OpenLogsFolder(); return Task.CompletedTask; });
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

    public string HealthStatus
    {
        get => _healthStatus;
        set => SetProperty(ref _healthStatus, value);
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

    public string RuntimeStatus
    {
        get => _runtimeStatus;
        set => SetProperty(ref _runtimeStatus, value);
    }

    public string ApiStatus
    {
        get => _apiStatus;
        set => SetProperty(ref _apiStatus, value);
    }

    public string ServicesStatus
    {
        get => _servicesStatus;
        set => SetProperty(ref _servicesStatus, value);
    }

    public string LastCheckedAt
    {
        get => _lastCheckedAt;
        set => SetProperty(ref _lastCheckedAt, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand OpenWebCommand { get; }
    public ICommand OpenDataDirectoryCommand { get; }
    public ICommand OpenLogsCommand { get; }

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
            HealthStatus = health.Message;
            Version = version.Version;
            Channel = version.Channel;
            RuntimeStatus = IsServerOnline ? "Em execução" : "Parado";
            ApiStatus = IsServerOnline ? "Online" : "Offline";
            ServicesStatus = IsServerOnline ? "Ativos" : "Indisponíveis";
            LastCheckedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }
        catch (Exception ex)
        {
            HealthStatus = $"Erro: {ex.Message}";
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

    private void OpenDataDirectory()
    {
        try
        {
            var dataPath = AppContext.BaseDirectory;
            if (Directory.Exists(dataPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = dataPath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show("Pasta de dados não encontrada.", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Não foi possível abrir a pasta de dados: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenLogsFolder()
    {
        try
        {
            var logsPath = Path.Combine(AppContext.BaseDirectory, "logs");
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
