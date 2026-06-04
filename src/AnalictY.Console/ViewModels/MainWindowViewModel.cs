using System.Windows.Input;
using AnalictY.Console.Infrastructure;
using AnalictY.Console.Services;

namespace AnalictY.Console.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly HealthService _healthService;
    private string _serverStatus = "Não verificado";
    private string _serverStatusDetail = "Clique em Verificar servidor para consultar o AnalictY Server local.";
    private string _checkButtonText = "Verificar servidor";
    private string _lastCheckedAt = "Ainda não verificado";
    private bool _isServerOnline;
    private bool _isCheckingServer;

    public MainWindowViewModel(HealthService healthService)
    {
        _healthService = healthService;
        CheckServerCommand = new RelayCommand(CheckServerAsync);
    }

    public string ServerStatus
    {
        get => _serverStatus;
        private set => SetProperty(ref _serverStatus, value);
    }

    public string ServerStatusDetail
    {
        get => _serverStatusDetail;
        private set => SetProperty(ref _serverStatusDetail, value);
    }

    public string CheckButtonText
    {
        get => _checkButtonText;
        private set => SetProperty(ref _checkButtonText, value);
    }

    public string LastCheckedAt
    {
        get => _lastCheckedAt;
        private set => SetProperty(ref _lastCheckedAt, value);
    }

    public bool IsServerOnline
    {
        get => _isServerOnline;
        private set => SetProperty(ref _isServerOnline, value);
    }

    public bool IsCheckingServer
    {
        get => _isCheckingServer;
        private set => SetProperty(ref _isCheckingServer, value);
    }

    public ICommand CheckServerCommand { get; }

    private async Task CheckServerAsync()
    {
        ServerStatus = "Verificando...";
        ServerStatusDetail = "Consultando http://127.0.0.1:5000/api/system/health";
        CheckButtonText = "Verificando...";
        IsCheckingServer = true;
        IsServerOnline = false;

        try
        {
            using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(5));
            var result = await _healthService.CheckAsync(timeout.Token);

            IsServerOnline = result.IsOnline;
            ServerStatus = result.Message;
            ServerStatusDetail = result.IsOnline
                ? "AnalictY Server respondeu com status healthy."
                : "Não foi possível conectar ao AnalictY Server local.";
            LastCheckedAt = $"Última verificação: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        }
        finally
        {
            CheckButtonText = "Verificar servidor";
            IsCheckingServer = false;
        }
    }
}
