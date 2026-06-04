using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using AnalictY.Console.Infrastructure;
using AnalictY.Console.Models;
using AnalictY.Console.Services;

namespace AnalictY.Console.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly MachineOverviewService _machineOverviewService;
    private readonly StatusOverviewService _statusOverviewService;
    private bool _hasLoadedOverview;
    private bool _hasLoadedStatus;
    private string _currentPage = "Overview";
    private string _serverStatus = "Não verificado";
    private string _serverStatusDetail = "Clique em Atualizar Status para consultar o AnalictY Server local.";
    private string _statusButtonText = "Atualizar Status";
    private string _lastCheckedAt = "Ainda não verificado";
    private string _selectedShift = "Turno atual";
    private string _selectedLine = "Todas as linhas";
    private string _overviewDataMessage = "Entre para carregar máquinas reais; mostrando dados demonstrativos.";
    private string _statusDataMessage = "Status ainda não atualizado.";
    private string _systemVersion = "-";
    private string _systemChannel = "-";
    private string _systemSource = "-";
    private string _dataDirectory = "-";
    private string _databaseStatus = "-";
    private string _runtimeStatus = "-";
    private string _apiStatus = "Não verificado";
    private string _currentServerTime = "-";
    private string _loginUsername = string.Empty;
    private string _loginPassword = string.Empty;
    private string _loginError = string.Empty;
    private string _loginButtonText = "Entrar";
    private AuthSession? _session;
    private bool _isServerOnline;
    private bool _isCheckingServer;
    private bool _isLoadingMachines;
    private bool _isLoadingStatus;
    private bool _isLoggingIn;

    public MainWindowViewModel(
        AuthService authService,
        MachineOverviewService machineOverviewService,
        StatusOverviewService statusOverviewService)
    {
        _authService = authService;
        _machineOverviewService = machineOverviewService;
        _statusOverviewService = statusOverviewService;

        LoginCommand = new RelayCommand(LoginAsync);
        LogoutCommand = new RelayCommand(LogoutAsync);
        CheckServerCommand = new RelayCommand(RefreshStatusAsync);
        RefreshStatusCommand = new RelayCommand(RefreshStatusAsync);
        NavigateCommand = new RelayCommand(async parameter =>
        {
            if (parameter is not string pageKey)
            {
                return;
            }

            if (pageKey == "Exit")
            {
                await LogoutAsync();
                return;
            }

            CurrentPage = pageKey;
            if (pageKey == "Overview")
            {
                await LoadMachineOverviewAsync();
            }
            else if (pageKey == "Status")
            {
                await RefreshStatusAsync();
            }
        });

        NavigationItems = new ObservableCollection<NavigationItem>
        {
            new("Visão Geral", "Overview"),
            new("Status", "Status"),
            new("Histórico Produção", "ProductionHistory"),
            new("Histórico Paradas", "DowntimeHistory"),
            new("Relatório", "Report"),
            new("Alertas", "Alerts"),
            new("Configurações", "Settings"),
            new("Ajuda", "Help"),
            new("Sair", "Exit")
        };

        Kpis = new ObservableCollection<KpiCard>();
        Machines = new ObservableCollection<MachineCard>();
        StatusMetrics = new ObservableCollection<StatusMetricCard>();
        StatusMachines = new ObservableCollection<MachineCard>();
        ModuleCards = CreateModuleCards();
        HelpTopics = CreateHelpTopics();
        ShiftFilters = new ObservableCollection<string> { "Turno atual", "Hoje", "Últimas 24 horas" };
        LineFilters = new ObservableCollection<string> { "Todas as linhas", "Linha Principal", "Preparação", "Célula A" };

        ApplyMachines(MachineOverviewService.CreateFallbackMachines());
        ApplyStatusMachines(MachineOverviewService.CreateFallbackMachines());
        UpdateStatusMetrics();
    }

    public ObservableCollection<NavigationItem> NavigationItems { get; }
    public ObservableCollection<KpiCard> Kpis { get; }
    public ObservableCollection<MachineCard> Machines { get; }
    public ObservableCollection<StatusMetricCard> StatusMetrics { get; }
    public ObservableCollection<MachineCard> StatusMachines { get; }
    public ObservableCollection<ModuleCard> ModuleCards { get; }
    public ObservableCollection<HelpTopic> HelpTopics { get; }
    public ObservableCollection<string> ShiftFilters { get; }
    public ObservableCollection<string> LineFilters { get; }
    public ICommand NavigateCommand { get; }
    public ICommand CheckServerCommand { get; }
    public ICommand RefreshStatusCommand { get; }
    public ICommand LoginCommand { get; }
    public ICommand LogoutCommand { get; }
    public bool IsAuthenticated => _session is not null;
    public string AuthenticatedUserDisplay => _session?.User.Username ?? "Não autenticado";
    public string AuthenticatedUserRole => _session?.User.Role ?? "sem sessão";

    public string CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (SetProperty(ref _currentPage, value))
            {
                OnPropertyChanged(nameof(CurrentPageTitle));
                OnPropertyChanged(nameof(CurrentPageSubtitle));
            }
        }
    }

    public string CurrentPageTitle => CurrentPage switch
    {
        "Overview" => "Visão Geral",
        "Status" => "Status",
        "ProductionHistory" => "Histórico Produção",
        "DowntimeHistory" => "Histórico Paradas",
        "Report" => "Relatório",
        "Alerts" => "Alertas",
        "Settings" => "Configurações",
        "Help" => "Ajuda",
        _ => "AnalictY Console"
    };

    public string CurrentPageSubtitle => CurrentPage switch
    {
        "Overview" => "Acompanhamento inicial da operação com dados do AnalictY Server quando disponíveis.",
        "Status" => "Saúde do servidor, runtime e máquinas com fallback seguro.",
        "Settings" => "Módulos previstos para operação e administração.",
        "Help" => "Orientações rápidas para uso do console.",
        _ => "Tela nativa reservada para a próxima etapa."
    };

    public string ServerStatus { get => _serverStatus; private set => SetProperty(ref _serverStatus, value); }
    public string ServerStatusDetail { get => _serverStatusDetail; private set => SetProperty(ref _serverStatusDetail, value); }
    public string CheckButtonText => StatusButtonText;

    public string StatusButtonText
    {
        get => _statusButtonText;
        private set
        {
            if (SetProperty(ref _statusButtonText, value))
            {
                OnPropertyChanged(nameof(CheckButtonText));
            }
        }
    }

    public string LastCheckedAt { get => _lastCheckedAt; private set => SetProperty(ref _lastCheckedAt, value); }
    public string SelectedShift { get => _selectedShift; set => SetProperty(ref _selectedShift, value); }
    public string SelectedLine { get => _selectedLine; set => SetProperty(ref _selectedLine, value); }
    public string OverviewDataMessage { get => _overviewDataMessage; private set => SetProperty(ref _overviewDataMessage, value); }
    public string StatusDataMessage { get => _statusDataMessage; private set => SetProperty(ref _statusDataMessage, value); }
    public string SystemVersion { get => _systemVersion; private set => SetProperty(ref _systemVersion, value); }
    public string SystemChannel { get => _systemChannel; private set => SetProperty(ref _systemChannel, value); }
    public string SystemSource { get => _systemSource; private set => SetProperty(ref _systemSource, value); }
    public string DataDirectory { get => _dataDirectory; private set => SetProperty(ref _dataDirectory, value); }
    public string DatabaseStatus { get => _databaseStatus; private set => SetProperty(ref _databaseStatus, value); }
    public string RuntimeStatus { get => _runtimeStatus; private set => SetProperty(ref _runtimeStatus, value); }
    public string ApiStatus { get => _apiStatus; private set => SetProperty(ref _apiStatus, value); }
    public string CurrentServerTime { get => _currentServerTime; private set => SetProperty(ref _currentServerTime, value); }

    public string LoginUsername
    {
        get => _loginUsername;
        set
        {
            if (SetProperty(ref _loginUsername, value))
            {
                LoginError = string.Empty;
            }
        }
    }

    public string LoginPassword
    {
        get => _loginPassword;
        set
        {
            if (SetProperty(ref _loginPassword, value))
            {
                LoginError = string.Empty;
            }
        }
    }

    public string LoginError { get => _loginError; private set => SetProperty(ref _loginError, value); }
    public string LoginButtonText { get => _loginButtonText; private set => SetProperty(ref _loginButtonText, value); }
    public bool IsServerOnline { get => _isServerOnline; private set => SetProperty(ref _isServerOnline, value); }
    public bool IsCheckingServer { get => _isCheckingServer; private set => SetProperty(ref _isCheckingServer, value); }
    public bool IsLoadingMachines { get => _isLoadingMachines; private set => SetProperty(ref _isLoadingMachines, value); }
    public bool IsLoadingStatus { get => _isLoadingStatus; private set => SetProperty(ref _isLoadingStatus, value); }
    public bool IsLoggingIn { get => _isLoggingIn; private set => SetProperty(ref _isLoggingIn, value); }

    private async Task LoginAsync()
    {
        LoginButtonText = "Entrando...";
        LoginError = string.Empty;
        IsLoggingIn = true;

        try
        {
            using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
            AuthResult result = await _authService.LoginAsync(LoginUsername.Trim(), LoginPassword, timeout.Token);
            if (!result.Success || result.Session is null)
            {
                LoginError = result.Message;
                return;
            }

            _session = result.Session;
            LoginPassword = string.Empty;
            CurrentPage = "Overview";
            OnPropertyChanged(nameof(IsAuthenticated));
            OnPropertyChanged(nameof(AuthenticatedUserDisplay));
            OnPropertyChanged(nameof(AuthenticatedUserRole));
            await LoadMachineOverviewAsync();
        }
        finally
        {
            LoginButtonText = "Entrar";
            IsLoggingIn = false;
        }
    }

    private async Task LogoutAsync()
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(5));
        await _authService.LogoutAsync(timeout.Token);
        _session = null;
        _hasLoadedOverview = false;
        _hasLoadedStatus = false;
        LoginPassword = string.Empty;
        LoginError = string.Empty;
        OverviewDataMessage = "Entre para carregar máquinas reais; mostrando dados demonstrativos.";
        StatusDataMessage = "Status ainda não atualizado.";
        ApplyMachines(MachineOverviewService.CreateFallbackMachines());
        ApplyStatusMachines(MachineOverviewService.CreateFallbackMachines());
        UpdateStatusMetrics();
        OnPropertyChanged(nameof(IsAuthenticated));
        OnPropertyChanged(nameof(AuthenticatedUserDisplay));
        OnPropertyChanged(nameof(AuthenticatedUserRole));
    }

    private async Task LoadMachineOverviewAsync()
    {
        if (IsLoadingMachines)
        {
            return;
        }

        IsLoadingMachines = true;
        OverviewDataMessage = _hasLoadedOverview ? "Atualizando máquinas..." : "Carregando máquinas...";
        try
        {
            using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(6));
            MachineOverviewResult result = await _machineOverviewService.LoadAsync(timeout.Token);
            ApplyMachines(result.Machines);
            OverviewDataMessage = result.Message;
            _hasLoadedOverview = true;
        }
        finally
        {
            IsLoadingMachines = false;
        }
    }

    private async Task RefreshStatusAsync()
    {
        if (IsLoadingStatus)
        {
            return;
        }

        StatusButtonText = "Atualizando...";
        IsLoadingStatus = true;
        IsCheckingServer = true;
        StatusDataMessage = _hasLoadedStatus ? "Atualizando status..." : "Consultando status do AnalictY Server...";

        try
        {
            using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(8));
            StatusOverviewResult result = await _statusOverviewService.LoadAsync(timeout.Token);
            IsServerOnline = result.IsOnline;
            ServerStatus = result.IsOnline ? "Online" : "Offline";
            ServerStatusDetail = result.HealthStatus;
            ApiStatus = result.ApiStatus;
            SystemVersion = result.Version;
            SystemChannel = result.Channel;
            SystemSource = result.Source;
            DataDirectory = result.DataDirectory;
            DatabaseStatus = result.DatabaseStatus;
            RuntimeStatus = result.RuntimeStatus;
            StatusDataMessage = result.Message;
            CurrentServerTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            LastCheckedAt = $"Última verificação: {CurrentServerTime}";
            ApplyStatusMachines(result.Machines);
            UpdateStatusMetrics();
            _hasLoadedStatus = true;
        }
        finally
        {
            StatusButtonText = "Atualizar Status";
            IsLoadingStatus = false;
            IsCheckingServer = false;
        }
    }

    private void ApplyMachines(IReadOnlyList<MachineCard> machines)
    {
        Machines.Clear();
        foreach (MachineCard machine in machines)
        {
            Machines.Add(machine);
        }

        UpdateKpis(machines);
    }

    private void ApplyStatusMachines(IReadOnlyList<MachineCard> machines)
    {
        StatusMachines.Clear();
        foreach (MachineCard machine in machines.Take(8))
        {
            StatusMachines.Add(machine);
        }
    }

    private void UpdateKpis(IReadOnlyList<MachineCard> machines)
    {
        int total = machines.Count;
        int running = machines.Count(machine => machine.Status == "Em operação");
        int maintenance = machines.Count(machine => machine.Status == "Manutenção");
        int idle = machines.Count(machine => machine.Status == "Ociosa");

        Kpis.Clear();
        Kpis.Add(new KpiCard("Total Máquinas", total.ToString(), "Máquinas retornadas para a visão geral", Brushes.DodgerBlue));
        Kpis.Add(new KpiCard("Total Em Operação", running.ToString(), "Produção ativa agora", new SolidColorBrush(Color.FromRgb(32, 180, 134))));
        Kpis.Add(new KpiCard("Total em Manutenção", maintenance.ToString(), "Aguardando intervenção", new SolidColorBrush(Color.FromRgb(239, 68, 68))));
        Kpis.Add(new KpiCard("Total Ociosas", idle.ToString(), "Sem ordem em execução", new SolidColorBrush(Color.FromRgb(249, 115, 22))));
    }

    private void UpdateStatusMetrics()
    {
        int total = StatusMachines.Count;
        int running = StatusMachines.Count(machine => machine.Status == "Em operação");
        int maintenance = StatusMachines.Count(machine => machine.Status == "Manutenção");
        int idle = StatusMachines.Count(machine => machine.Status == "Ociosa");

        StatusMetrics.Clear();
        StatusMetrics.Add(new StatusMetricCard("Máquinas", total.ToString(), "Amostra exibida no status", Brushes.DodgerBlue));
        StatusMetrics.Add(new StatusMetricCard("Em operação", running.ToString(), "Status normalizado do runtime", new SolidColorBrush(Color.FromRgb(32, 180, 134))));
        StatusMetrics.Add(new StatusMetricCard("Ociosas", idle.ToString(), "Sem ordem em execução", new SolidColorBrush(Color.FromRgb(249, 115, 22))));
        StatusMetrics.Add(new StatusMetricCard("Manutenção", maintenance.ToString(), "Aguardando intervenção", new SolidColorBrush(Color.FromRgb(239, 68, 68))));
    }

    private static ObservableCollection<ModuleCard> CreateModuleCards()
    {
        return new ObservableCollection<ModuleCard>
        {
            new("TAGs", "Cadastro e organização dos pontos monitorados.", "Planejado"),
            new("Weintek HTTP", "Conexão com IHMs e leitura por API local.", "Planejado"),
            new("Alertas", "Regras de notificação e acompanhamento operacional.", "Planejado"),
            new("Máquinas", "Cadastro das máquinas e agrupamento por linha.", "API inicial"),
            new("Logs", "Acesso aos registros do AnalictY Server.", "Planejado"),
            new("Turnos", "Janelas de produção e calendário operacional.", "Planejado"),
            new("Dashboards", "Painéis nativos para acompanhamento da fábrica.", "Planejado"),
            new("Telegram", "Canal de avisos para equipes configuradas.", "Planejado"),
            new("Banco de Dados", "Status e rotinas do banco local.", "Planejado"),
            new("Atualizações", "Verificação e aplicação de versões futuras.", "Planejado")
        };
    }

    private static ObservableCollection<HelpTopic> CreateHelpTopics()
    {
        return new ObservableCollection<HelpTopic>
        {
            new("Funcionamento", "O AnalictY Console é a janela desktop para acompanhar o AnalictY Server instalado neste computador. Ele mostra o estado da operação sem abrir navegador."),
            new("Cadastro e acesso", "O acesso usa o login do AnalictY Server. A senha fica somente em memória durante a tentativa de entrada."),
            new("Configurações", "Os módulos de configuração aparecem como cartões para orientar o fluxo. Eles ainda não gravam dados nesta etapa."),
            new("Telegram/Alertas", "Alertas e Telegram serão usados para avisos operacionais. Por enquanto, a tela mostra apenas a organização prevista."),
            new("Produção/Histórico", "A Visão Geral tenta carregar máquinas reais do AnalictY Server e usa dados demonstrativos se a API não responder."),
            new("Atualizações", "Atualizações do sistema continuam fora do escopo desta etapa. O console apenas reserva a área visual para essa função.")
        };
    }
}
