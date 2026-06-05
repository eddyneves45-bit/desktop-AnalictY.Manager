using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Models;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly MachineOverviewService _machineOverviewService;
    private readonly StatusOverviewService _statusOverviewService;
    private readonly ProductionHistoryService _productionHistoryService;
    private readonly DowntimeHistoryService _downtimeHistoryService;
    private readonly AlertService _alertService;
    private readonly ReportService _reportService;
    private bool _hasLoadedOverview;
    private bool _hasLoadedStatus;
    private bool _hasLoadedProductionHistory;
    private bool _hasLoadedDowntimeHistory;
    private bool _hasLoadedAlerts;
    private bool _hasLoadedReport;
    private string _currentPage = "Overview";
    private string _serverStatus = "Não verificado";
    private string _serverStatusDetail = "Clique em Atualizar Status para consultar o AnalictY Server local.";
    private string _statusButtonText = "Atualizar Status";
    private string _lastCheckedAt = "Ainda não verificado";
    private string _selectedShift = "Turno atual";
    private string _selectedLine = "Todas as linhas";
    private string _overviewDataMessage = "Entre para carregar máquinas reais; mostrando dados demonstrativos.";
    private string _statusDataMessage = "Status ainda não atualizado.";
    private string _productionHistoryMessage = "Histórico ainda não carregado.";
    private string _selectedProductionMachineId = string.Empty;
    private string _selectedProductionPeriod = "Hoje";
    private string _productionHistoryButtonText = "Atualizar";
    private string _productionMachineName = "Aguardando filtro";
    private string _productionPeriodLabel = "Hoje";
    private double _productionTotalProduced;
    private double _productionTotalLost;
    private double _productionTotalGood;
    private string _downtimeHistoryMessage = "Histórico ainda não carregado.";
    private string _selectedDowntimeMachineId = string.Empty;
    private string _selectedDowntimePeriod = "Hoje";
    private string _downtimeHistoryButtonText = "Atualizar";
    private string _downtimePeriodLabel = "Hoje";
    private int _downtimeTotalStops;
    private string _downtimeTotalDuration = "0s";
    private string _downtimeMainReason = "-";
    private string _alertMessage = "Alertas ainda não carregados.";
    private string _alertButtonText = "Atualizar";
    private int _activeAlertCount;
    private int _criticalAlertCount;
    private string _telegramAlertStatus = "-";
    private string _lastAlertActivity = "-";
    private string _reportMessage = "Relatório ainda não gerado.";
    private string _reportButtonText = "Gerar prévia";
    private string _selectedReportType = "Produção";
    private string _selectedReportMachineId = string.Empty;
    private string _selectedReportPeriod = "Hoje";
    private string _selectedReportTypeLabel = "Produção";
    private string _selectedReportMachineLabel = "Aguardando seleção";
    private string _selectedReportPeriodLabel = "Hoje";
    private string _reportStatusLabel = "-";
    private string _selectedModuleTitle = "Sistema";
    private string _selectedModuleDescription = "Visão geral dos módulos de configuração e manutenção disponíveis no console.";
    private string _selectedModuleState = "Em breve";
    private string _selectedModuleAction = "Ver detalhes";
    private string _selectedHelpTitle = "Funcionamento";
    private string _selectedHelpContent = "O AnalictY Manager é a janela desktop para acompanhar o AnalictY Server instalado neste computador. Ele mostra o estado da operação sem abrir navegador.";
    private string _selectedHelpAction = "Apenas leitura";
    private string _selectedHelpPageKey = string.Empty;
    private bool _selectedHelpCanOpen;
    private string _selectedModulePageKey = string.Empty;
    private bool _selectedModuleCanOpen;
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
    private bool _isLoginModalOpen;
    private AuthSession? _session;
    private bool _isServerOnline;
    private bool _isCheckingServer;
    private bool _isLoadingMachines;
    private bool _isLoadingStatus;
    private bool _isLoadingProductionHistory;
    private bool _isLoadingDowntimeHistory;
    private bool _isLoadingAlerts;
    private bool _isLoadingReport;
    private bool _isLoggingIn;

    public MainWindowViewModel(
        AuthService authService,
        MachineOverviewService machineOverviewService,
        StatusOverviewService statusOverviewService,
        ProductionHistoryService productionHistoryService,
        DowntimeHistoryService downtimeHistoryService,
        AlertService alertService,
        ReportService reportService)
    {
        _authService = authService;
        _machineOverviewService = machineOverviewService;
        _statusOverviewService = statusOverviewService;
        _productionHistoryService = productionHistoryService;
        _downtimeHistoryService = downtimeHistoryService;
        _alertService = alertService;
        _reportService = reportService;

        LoginCommand = new RelayCommand(_ => LoginAsync());
        OpenLoginModalCommand = new RelayCommand(_ =>
        {
            OpenLoginModal();
            return Task.CompletedTask;
        });
        CloseLoginModalCommand = new RelayCommand(_ =>
        {
            CloseLoginModal();
            return Task.CompletedTask;
        });
        LogoutCommand = new RelayCommand(_ => LogoutAsync());
        CheckServerCommand = new RelayCommand(_ => RefreshStatusAsync());
        RefreshStatusCommand = new RelayCommand(_ => RefreshStatusAsync());
        RefreshProductionHistoryCommand = new RelayCommand(_ => RefreshProductionHistoryAsync());
        RefreshDowntimeHistoryCommand = new RelayCommand(_ => RefreshDowntimeHistoryAsync());
        RefreshAlertsCommand = new RelayCommand(_ => RefreshAlertsAsync());
        RefreshReportCommand = new RelayCommand(_ => RefreshReportAsync());
        SelectModuleCommand = new RelayCommand(parameter =>
        {
            if (parameter is ModuleCard module)
            {
                SelectedModuleTitle = module.Title;
                SelectedModuleDescription = module.Description;
                SelectedModuleState = module.State;
                SelectedModulePageKey = module.PageKey;
                SelectedModuleCanOpen = !string.IsNullOrWhiteSpace(module.PageKey);
                SelectedModuleAction = SelectedModuleCanOpen ? "Abrir módulo" : "Planejado";
            }
            return Task.CompletedTask;
        });
        SelectHelpTopicCommand = new RelayCommand(parameter =>
        {
            if (parameter is HelpTopic topic)
            {
                SelectedHelpTitle = topic.Title;
                SelectedHelpContent = topic.Content;
                SelectedHelpPageKey = topic.PageKey;
                SelectedHelpCanOpen = !string.IsNullOrWhiteSpace(SelectedHelpPageKey);
                SelectedHelpAction = SelectedHelpCanOpen ? "Abrir seção" : "Apenas leitura";
            }
            return Task.CompletedTask;
        });
        OpenSelectedHelpCommand = new RelayCommand(async _ =>
        {
            if (!string.IsNullOrWhiteSpace(SelectedHelpPageKey))
            {
                await NavigateToPageAsync(SelectedHelpPageKey);
            }
        });
        OpenSelectedModuleCommand = new RelayCommand(async _ =>
        {
            if (!string.IsNullOrWhiteSpace(SelectedModulePageKey))
            {
                await NavigateToPageAsync(SelectedModulePageKey);
            }
        });
        NavigateCommand = new RelayCommand(async parameter =>
        {
            if (parameter is not string pageKey)
            {
                return;
            }
            await NavigateToPageAsync(pageKey);
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
        ProductionMachines = new ObservableCollection<ProductionMachineOption>();
        ProductionPeriodOptions = new ObservableCollection<string> { "Hoje", "Última hora", "Mês atual" };
        ProductionHistoryRows = new ObservableCollection<ProductionHistoryRow>();
        DowntimeMachines = new ObservableCollection<DowntimeMachineOption>();
        DowntimePeriodOptions = new ObservableCollection<string> { "Hoje", "Última hora", "Mês atual" };
        DowntimeHistoryRows = new ObservableCollection<DowntimeHistoryRow>();
        AlertRows = new ObservableCollection<AlertRuleRow>();
        ReportMachines = new ObservableCollection<ReportMachineOption>();
        ReportTypeOptions = new ObservableCollection<string> { "Produção", "Paradas", "Status" };
        ReportPeriodOptions = new ObservableCollection<string> { "Hoje", "Última hora", "Mês atual" };
        ReportRows = new ObservableCollection<ReportPreviewRow>();
        ModuleCards = CreateModuleCards();
        if (ModuleCards.Count > 0)
        {
            SelectedModuleTitle = ModuleCards[0].Title;
            SelectedModuleDescription = ModuleCards[0].Description;
            SelectedModuleState = ModuleCards[0].State;
            SelectedModulePageKey = ModuleCards[0].PageKey;
            SelectedModuleCanOpen = !string.IsNullOrWhiteSpace(ModuleCards[0].PageKey);
            SelectedModuleAction = SelectedModuleCanOpen ? "Abrir módulo" : "Planejado";
        }
        HelpTopics = CreateHelpTopics();
        if (HelpTopics.Count > 0)
        {
            SelectedHelpTitle = HelpTopics[0].Title;
            SelectedHelpContent = HelpTopics[0].Content;
            SelectedHelpPageKey = HelpTopics[0].PageKey;
            SelectedHelpCanOpen = !string.IsNullOrWhiteSpace(HelpTopics[0].PageKey);
            SelectedHelpAction = SelectedHelpCanOpen ? "Abrir seção" : "Apenas leitura";
        }
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
    public ObservableCollection<ProductionMachineOption> ProductionMachines { get; }
    public ObservableCollection<string> ProductionPeriodOptions { get; }
    public ObservableCollection<ProductionHistoryRow> ProductionHistoryRows { get; }
    public ObservableCollection<DowntimeMachineOption> DowntimeMachines { get; }
    public ObservableCollection<string> DowntimePeriodOptions { get; }
    public ObservableCollection<DowntimeHistoryRow> DowntimeHistoryRows { get; }
    public ObservableCollection<AlertRuleRow> AlertRows { get; }
    public ObservableCollection<ReportMachineOption> ReportMachines { get; }
    public ObservableCollection<string> ReportTypeOptions { get; }
    public ObservableCollection<string> ReportPeriodOptions { get; }
    public ObservableCollection<ReportPreviewRow> ReportRows { get; }
    public ObservableCollection<ModuleCard> ModuleCards { get; }
    public ObservableCollection<HelpTopic> HelpTopics { get; }
    public ObservableCollection<string> ShiftFilters { get; }
    public ObservableCollection<string> LineFilters { get; }
    public ICommand NavigateCommand { get; }
    public ICommand CheckServerCommand { get; }
    public ICommand RefreshStatusCommand { get; }
    public ICommand RefreshProductionHistoryCommand { get; }
    public ICommand RefreshDowntimeHistoryCommand { get; }
    public ICommand RefreshAlertsCommand { get; }
    public ICommand RefreshReportCommand { get; }
    public ICommand SelectModuleCommand { get; }
    public ICommand SelectHelpTopicCommand { get; }
    public ICommand OpenSelectedHelpCommand { get; }
    public ICommand OpenSelectedModuleCommand { get; }
    public ICommand LoginCommand { get; }
    public ICommand OpenLoginModalCommand { get; }
    public ICommand CloseLoginModalCommand { get; }
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
        _ => "AnalictY Manager"
    };

    public string CurrentPageSubtitle => CurrentPage switch
    {
        "Overview" => "Acompanhamento inicial da operação com dados do AnalictY Server quando disponíveis.",
        "Status" => "Saúde do servidor, runtime e máquinas com fallback seguro.",
        "ProductionHistory" => "Produção por hora com dados reais quando disponíveis.",
        "DowntimeHistory" => "Paradas por período com dados reais quando disponíveis.",
        "Alerts" => "Regras, eventos recentes e Telegram com fallback seguro.",
        "Settings" => "Módulos previstos para operação e administração.",
        "Help" => "Orientações rápidas para uso do console.",
        _ => "Tela nativa reservada para a próxima etapa."
    };

    public string ServerStatus { get => _serverStatus; private set => SetProperty(ref _serverStatus, value); }
    public string ServerStatusDetail { get => _serverStatusDetail; private set => SetProperty(ref _serverStatusDetail, value); }
    public string CheckButtonText => StatusButtonText;
    public string StatusButtonText { get => _statusButtonText; private set { if (SetProperty(ref _statusButtonText, value)) OnPropertyChanged(nameof(CheckButtonText)); } }
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
    public string ProductionHistoryMessage { get => _productionHistoryMessage; private set => SetProperty(ref _productionHistoryMessage, value); }
    public string SelectedProductionMachineId { get => _selectedProductionMachineId; set => SetProperty(ref _selectedProductionMachineId, value); }
    public string SelectedProductionPeriod { get => _selectedProductionPeriod; set => SetProperty(ref _selectedProductionPeriod, value); }
    public string ProductionHistoryButtonText { get => _productionHistoryButtonText; private set => SetProperty(ref _productionHistoryButtonText, value); }
    public string ProductionMachineName { get => _productionMachineName; private set => SetProperty(ref _productionMachineName, value); }
    public string ProductionPeriodLabel { get => _productionPeriodLabel; private set => SetProperty(ref _productionPeriodLabel, value); }
    public string ProductionTotalProducedText => FormatNumber(ProductionTotalProduced);
    public string ProductionTotalLostText => FormatNumber(ProductionTotalLost);
    public string ProductionTotalGoodText => FormatNumber(ProductionTotalGood);
    public double ProductionTotalProduced { get => _productionTotalProduced; private set { if (SetProperty(ref _productionTotalProduced, value)) OnPropertyChanged(nameof(ProductionTotalProducedText)); } }
    public double ProductionTotalLost { get => _productionTotalLost; private set { if (SetProperty(ref _productionTotalLost, value)) OnPropertyChanged(nameof(ProductionTotalLostText)); } }
    public double ProductionTotalGood { get => _productionTotalGood; private set { if (SetProperty(ref _productionTotalGood, value)) OnPropertyChanged(nameof(ProductionTotalGoodText)); } }
    public string DowntimeHistoryMessage { get => _downtimeHistoryMessage; private set => SetProperty(ref _downtimeHistoryMessage, value); }
    public string SelectedDowntimeMachineId { get => _selectedDowntimeMachineId; set => SetProperty(ref _selectedDowntimeMachineId, value); }
    public string SelectedDowntimePeriod { get => _selectedDowntimePeriod; set => SetProperty(ref _selectedDowntimePeriod, value); }
    public string DowntimeHistoryButtonText { get => _downtimeHistoryButtonText; private set => SetProperty(ref _downtimeHistoryButtonText, value); }
    public string DowntimePeriodLabel { get => _downtimePeriodLabel; private set => SetProperty(ref _downtimePeriodLabel, value); }
    public int DowntimeTotalStops { get => _downtimeTotalStops; private set { if (SetProperty(ref _downtimeTotalStops, value)) OnPropertyChanged(nameof(DowntimeTotalStopsText)); } }
    public string DowntimeTotalStopsText => DowntimeTotalStops.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
    public string DowntimeTotalDuration { get => _downtimeTotalDuration; private set => SetProperty(ref _downtimeTotalDuration, value); }
    public string DowntimeMainReason { get => _downtimeMainReason; private set => SetProperty(ref _downtimeMainReason, value); }
    public string AlertMessage { get => _alertMessage; private set => SetProperty(ref _alertMessage, value); }
    public string AlertButtonText { get => _alertButtonText; private set => SetProperty(ref _alertButtonText, value); }
    public int ActiveAlertCount { get => _activeAlertCount; private set { if (SetProperty(ref _activeAlertCount, value)) OnPropertyChanged(nameof(ActiveAlertCountText)); } }
    public string ActiveAlertCountText => ActiveAlertCount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
    public int CriticalAlertCount { get => _criticalAlertCount; private set { if (SetProperty(ref _criticalAlertCount, value)) OnPropertyChanged(nameof(CriticalAlertCountText)); } }
    public string CriticalAlertCountText => CriticalAlertCount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
    public string TelegramAlertStatus { get => _telegramAlertStatus; private set => SetProperty(ref _telegramAlertStatus, value); }
    public string LastAlertActivity { get => _lastAlertActivity; private set => SetProperty(ref _lastAlertActivity, value); }
    public string ReportMessage { get => _reportMessage; private set => SetProperty(ref _reportMessage, value); }
    public string ReportButtonText { get => _reportButtonText; private set => SetProperty(ref _reportButtonText, value); }
    public string SelectedReportType { get => _selectedReportType; set { if (SetProperty(ref _selectedReportType, value)) OnReportSelectionChanged(); } }
    public string SelectedReportMachineId { get => _selectedReportMachineId; set { if (SetProperty(ref _selectedReportMachineId, value)) OnReportSelectionChanged(); } }
    public string SelectedReportPeriod { get => _selectedReportPeriod; set { if (SetProperty(ref _selectedReportPeriod, value)) OnReportSelectionChanged(); } }
    public string SelectedReportTypeLabel { get => _selectedReportTypeLabel; private set => SetProperty(ref _selectedReportTypeLabel, value); }
    public string SelectedReportMachineLabel { get => _selectedReportMachineLabel; private set => SetProperty(ref _selectedReportMachineLabel, value); }
    public string SelectedReportPeriodLabel { get => _selectedReportPeriodLabel; private set => SetProperty(ref _selectedReportPeriodLabel, value); }
    public string ReportStatusLabel { get => _reportStatusLabel; private set => SetProperty(ref _reportStatusLabel, value); }
    public string SelectedModuleTitle { get => _selectedModuleTitle; private set => SetProperty(ref _selectedModuleTitle, value); }
    public string SelectedModuleDescription { get => _selectedModuleDescription; private set => SetProperty(ref _selectedModuleDescription, value); }
    public string SelectedModuleState { get => _selectedModuleState; private set => SetProperty(ref _selectedModuleState, value); }
    public string SelectedModuleAction { get => _selectedModuleAction; private set => SetProperty(ref _selectedModuleAction, value); }
    public string SelectedHelpTitle { get => _selectedHelpTitle; private set => SetProperty(ref _selectedHelpTitle, value); }
    public string SelectedHelpContent { get => _selectedHelpContent; private set => SetProperty(ref _selectedHelpContent, value); }
    public string SelectedHelpAction { get => _selectedHelpAction; private set => SetProperty(ref _selectedHelpAction, value); }
    public string SelectedHelpPageKey { get => _selectedHelpPageKey; private set => SetProperty(ref _selectedHelpPageKey, value); }
    public bool SelectedHelpCanOpen { get => _selectedHelpCanOpen; private set => SetProperty(ref _selectedHelpCanOpen, value); }
    public string SelectedModulePageKey { get => _selectedModulePageKey; private set => SetProperty(ref _selectedModulePageKey, value); }
    public bool SelectedModuleCanOpen { get => _selectedModuleCanOpen; private set => SetProperty(ref _selectedModuleCanOpen, value); }

    public string LoginUsername
    {
        get => _loginUsername;
        set { if (SetProperty(ref _loginUsername, value)) LoginError = string.Empty; }
    }

    public string LoginPassword
    {
        get => _loginPassword;
        set { if (SetProperty(ref _loginPassword, value)) LoginError = string.Empty; }
    }

    public string LoginError { get => _loginError; private set => SetProperty(ref _loginError, value); }
    public string LoginButtonText { get => _loginButtonText; private set => SetProperty(ref _loginButtonText, value); }
    public bool IsServerOnline { get => _isServerOnline; private set => SetProperty(ref _isServerOnline, value); }
    public bool IsCheckingServer { get => _isCheckingServer; private set => SetProperty(ref _isCheckingServer, value); }
    public bool IsLoadingMachines { get => _isLoadingMachines; private set => SetProperty(ref _isLoadingMachines, value); }
    public bool IsLoadingStatus { get => _isLoadingStatus; private set => SetProperty(ref _isLoadingStatus, value); }
    public bool IsLoadingProductionHistory { get => _isLoadingProductionHistory; private set => SetProperty(ref _isLoadingProductionHistory, value); }
    public bool IsLoadingDowntimeHistory { get => _isLoadingDowntimeHistory; private set => SetProperty(ref _isLoadingDowntimeHistory, value); }
    public bool IsLoadingAlerts { get => _isLoadingAlerts; private set => SetProperty(ref _isLoadingAlerts, value); }
    public bool IsLoadingReport { get => _isLoadingReport; private set => SetProperty(ref _isLoadingReport, value); }
    public bool IsLoggingIn { get => _isLoggingIn; private set => SetProperty(ref _isLoggingIn, value); }
    public bool IsLoginModalOpen { get => _isLoginModalOpen; private set => SetProperty(ref _isLoginModalOpen, value); }

    public void OpenLoginModal()
    {
        LoginError = string.Empty;
        IsLoginModalOpen = true;
    }

    public void CloseLoginModal()
    {
        if (!IsLoggingIn)
        {
            LoginError = string.Empty;
            IsLoginModalOpen = false;
        }
    }

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
            IsLoginModalOpen = false;
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
        _hasLoadedProductionHistory = false;
        _hasLoadedDowntimeHistory = false;
        _hasLoadedAlerts = false;
        LoginPassword = string.Empty;
        LoginError = string.Empty;
        IsLoginModalOpen = false;
        OverviewDataMessage = "Entre para carregar máquinas reais; mostrando dados demonstrativos.";
        StatusDataMessage = "Status ainda não atualizado.";
        ProductionHistoryMessage = "Histórico ainda não carregado.";
        DowntimeHistoryMessage = "Histórico ainda não carregado.";
        AlertMessage = "Alertas ainda não carregados.";
        ReportMessage = "Relatório ainda não gerado.";
        ReportStatusLabel = "-";
        SelectedReportTypeLabel = "Produção";
        SelectedReportMachineLabel = "Aguardando seleção";
        SelectedReportPeriodLabel = "Hoje";
        ApplyReportMachines(new List<ReportMachineOption>
        {
            new("1", "PJ_08", "PJ_08"),
            new("2", "Morno_01", "Morno_01"),
            new("3", "Morno_02", "Morno_02")
        });
        ApplyReportRows(Array.Empty<ReportPreviewRow>());
        ApplyAlertRows(Array.Empty<AlertRuleRow>());
        ActiveAlertCount = 0;
        CriticalAlertCount = 0;
        TelegramAlertStatus = "-";
        LastAlertActivity = "-";
        ApplyMachines(MachineOverviewService.CreateFallbackMachines());
        ApplyStatusMachines(MachineOverviewService.CreateFallbackMachines());
        UpdateStatusMetrics();
        OnPropertyChanged(nameof(IsAuthenticated));
        OnPropertyChanged(nameof(AuthenticatedUserDisplay));
        OnPropertyChanged(nameof(AuthenticatedUserRole));
    }

    private async Task LoadMachineOverviewAsync()
    {
        if (IsLoadingMachines) return;

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

    private async Task EnsureProductionHistoryAsync()
    {
        if (!_hasLoadedProductionHistory)
        {
            await RefreshProductionHistoryAsync();
        }
    }

    private async Task EnsureDowntimeHistoryAsync()
    {
        if (!_hasLoadedDowntimeHistory)
        {
            await RefreshDowntimeHistoryAsync();
        }
    }

    private async Task EnsureAlertsAsync()
    {
        if (!_hasLoadedAlerts)
        {
            await RefreshAlertsAsync();
        }
    }

    private async Task EnsureReportAsync()
    {
        if (!_hasLoadedReport)
        {
            await RefreshReportAsync();
        }
    }

    private async Task RefreshProductionHistoryAsync()
    {
        if (IsLoadingProductionHistory) return;

        ProductionHistoryButtonText = "Atualizando...";
        ProductionHistoryMessage = "Consultando histórico de produção...";
        IsLoadingProductionHistory = true;

        try
        {
            using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
            ProductionHistoryResult result = await _productionHistoryService.LoadHistoryAsync(
                SelectedProductionMachineId,
                SelectedProductionPeriod,
                timeout.Token);

            ApplyProductionMachines(result.Machines, result.SelectedMachineId);
            ApplyProductionRows(result.Rows);
            ProductionMachineName = result.SelectedMachineName;
            ProductionPeriodLabel = result.PeriodLabel;
            ProductionTotalProduced = result.TotalProduced;
            ProductionTotalLost = result.TotalLost;
            ProductionTotalGood = result.TotalGood;
            ProductionHistoryMessage = result.Message;
            _hasLoadedProductionHistory = true;
        }
        finally
        {
            ProductionHistoryButtonText = "Atualizar";
            IsLoadingProductionHistory = false;
        }
    }

    private async Task RefreshDowntimeHistoryAsync()
    {
        if (IsLoadingDowntimeHistory) return;

        DowntimeHistoryButtonText = "Atualizando...";
        DowntimeHistoryMessage = "Consultando histórico de paradas...";
        IsLoadingDowntimeHistory = true;

        try
        {
            using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
            DowntimeHistoryResult result = await _downtimeHistoryService.LoadAsync(
                SelectedDowntimeMachineId,
                SelectedDowntimePeriod,
                timeout.Token);

            ApplyDowntimeMachines(result.Machines, result.SelectedMachineId);
            ApplyDowntimeRows(result.Rows);
            DowntimePeriodLabel = result.PeriodLabel;
            DowntimeTotalStops = result.TotalStops;
            DowntimeTotalDuration = result.TotalDowntime;
            DowntimeMainReason = result.MainReason;
            DowntimeHistoryMessage = result.Message;
            _hasLoadedDowntimeHistory = true;
        }
        finally
        {
            DowntimeHistoryButtonText = "Atualizar";
            IsLoadingDowntimeHistory = false;
        }
    }

    private async Task RefreshAlertsAsync()
    {
        if (IsLoadingAlerts) return;

        AlertButtonText = "Atualizando...";
        AlertMessage = "Consultando alertas e Telegram...";
        IsLoadingAlerts = true;

        try
        {
            using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
            AlertOverviewResult result = await _alertService.LoadAsync(timeout.Token);
            ApplyAlertRows(result.Rows);
            ActiveAlertCount = result.ActiveAlerts;
            CriticalAlertCount = result.CriticalAlerts;
            TelegramAlertStatus = result.TelegramStatus;
            LastAlertActivity = result.LastActivity;
            AlertMessage = result.Message;
            _hasLoadedAlerts = true;
        }
        finally
        {
            AlertButtonText = "Atualizar";
            IsLoadingAlerts = false;
        }
    }

    private async Task RefreshReportAsync()
    {
        if (IsLoadingReport) return;

        ReportButtonText = "Gerando...";
        ReportMessage = "Montando prévia do relatório...";
        IsLoadingReport = true;

        try
        {
            using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
            ReportPreviewResult result = await _reportService.LoadPreviewAsync(
                SelectedReportType,
                SelectedReportMachineId,
                SelectedReportPeriod,
                timeout.Token);

            ApplyReportMachines(result.Machines);
            ApplyReportRows(result.Rows);
            SelectedReportTypeLabel = result.ReportType;
            SelectedReportMachineLabel = result.SelectedMachineName;
            SelectedReportPeriodLabel = result.PeriodLabel;
            ReportStatusLabel = result.StatusLabel;
            ReportMessage = result.Message;
            SelectedReportMachineId = result.SelectedMachineId;
            _hasLoadedReport = true;
        }
        finally
        {
            ReportButtonText = "Gerar prévia";
            IsLoadingReport = false;
        }
    }

    private async Task NavigateToPageAsync(string pageKey)
    {
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
        else if (pageKey == "ProductionHistory")
        {
            await EnsureProductionHistoryAsync();
        }
        else if (pageKey == "DowntimeHistory")
        {
            await EnsureDowntimeHistoryAsync();
        }
        else if (pageKey == "Alerts")
        {
            await EnsureAlertsAsync();
        }
        else if (pageKey == "Report")
        {
            await EnsureReportAsync();
        }
    }

    private async Task RefreshStatusAsync()
    {
        if (IsLoadingStatus) return;

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
        foreach (MachineCard machine in machines) Machines.Add(machine);
        UpdateKpis(machines);
    }

    private void ApplyStatusMachines(IReadOnlyList<MachineCard> machines)
    {
        StatusMachines.Clear();
        foreach (MachineCard machine in machines.Take(8)) StatusMachines.Add(machine);
    }

    private void ApplyProductionMachines(IReadOnlyList<ProductionMachineOption> machines, string selectedMachineId)
    {
        ProductionMachines.Clear();
        foreach (ProductionMachineOption machine in machines) ProductionMachines.Add(machine);
        SelectedProductionMachineId = selectedMachineId;
    }

    private void ApplyProductionRows(IReadOnlyList<ProductionHistoryRow> rows)
    {
        ProductionHistoryRows.Clear();
        foreach (ProductionHistoryRow row in rows) ProductionHistoryRows.Add(row);
    }

    private void ApplyDowntimeMachines(IReadOnlyList<DowntimeMachineOption> machines, string selectedMachineId)
    {
        DowntimeMachines.Clear();
        foreach (DowntimeMachineOption machine in machines) DowntimeMachines.Add(machine);
        SelectedDowntimeMachineId = selectedMachineId;
    }

    private void ApplyDowntimeRows(IReadOnlyList<DowntimeHistoryRow> rows)
    {
        DowntimeHistoryRows.Clear();
        foreach (DowntimeHistoryRow row in rows) DowntimeHistoryRows.Add(row);
    }

    private void ApplyAlertRows(IReadOnlyList<AlertRuleRow> rows)
    {
        AlertRows.Clear();
        foreach (AlertRuleRow row in rows) AlertRows.Add(row);
    }

    private void ApplyReportMachines(IReadOnlyList<ReportMachineOption> machines)
    {
        ReportMachines.Clear();
        foreach (ReportMachineOption machine in machines) ReportMachines.Add(machine);
    }

    private void ApplyReportRows(IReadOnlyList<ReportPreviewRow> rows)
    {
        ReportRows.Clear();
        foreach (ReportPreviewRow row in rows) ReportRows.Add(row);
    }

    private void OnReportSelectionChanged()
    {
        SelectedReportTypeLabel = SelectedReportType switch
        {
            "production" => "Produção",
            "status" => "Status",
            "downtime" => "Paradas",
            _ => "Relatório"
        };
        SelectedReportMachineLabel = string.IsNullOrWhiteSpace(SelectedReportMachineId)
            ? "Todas as máquinas"
            : ReportMachines.FirstOrDefault(machine => machine.Id == SelectedReportMachineId)?.DisplayName ?? "Máquina selecionada";
        SelectedReportPeriodLabel = SelectedReportPeriod;
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

    private static string FormatNumber(double value)
    {
        return value.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
    }

    private static ObservableCollection<ModuleCard> CreateModuleCards()
    {
        return new ObservableCollection<ModuleCard>
        {
            new("Sistema", "Base operacional do AnalictY Server e saúde geral do ambiente.", "Disponível", "Status"),
            new("TAGs", "Cadastro e organização dos pontos monitorados.", "Planejado", string.Empty),
            new("Weintek HTTP", "Conexão com IHMs e leitura por API local.", "Planejado", string.Empty),
            new("Alertas", "Regras de notificação e acompanhamento operacional.", "Disponível", "Alerts"),
            new("Máquinas", "Cadastro das máquinas e agrupamento por linha.", "Disponível", "Overview"),
            new("Logs", "Acesso aos registros do AnalictY Server.", "Planejado", string.Empty),
            new("Turnos", "Janelas de produção e calendário operacional.", "Planejado", string.Empty),
            new("Dashboards", "Painéis nativos para acompanhamento da fábrica.", "Disponível", "Overview"),
            new("Relatório", "Pré-visualização e geração de relatórios operacionais.", "Disponível", "Report"),
            new("Telegram", "Canal de avisos para equipes configuradas.", "Disponível", "Alerts"),
            new("Banco de Dados", "Status e rotinas do banco local.", "Planejado", string.Empty),
            new("Atualizações", "Verificação e aplicação de versões futuras.", "Planejado", string.Empty),
            new("Auditoria", "Trilha de ações administrativas e eventos do console.", "Planejado", string.Empty)
        };
    }

    private static ObservableCollection<HelpTopic> CreateHelpTopics()
    {
        return new ObservableCollection<HelpTopic>
        {
            new("Funcionamento", "O AnalictY Manager reúne as telas principais da operação em um aplicativo desktop. Você pode acompanhar máquinas, status, histórico e alertas sem usar navegador.", string.Empty),
            new("Cadastro e acesso", "Entre com o mesmo usuário do AnalictY Server. Depois de entrar, a navegação lateral libera as telas do console e mantém a sessão apenas enquanto o aplicativo estiver aberto.", string.Empty),
            new("Visão Geral", "A visão geral mostra as máquinas, os estados principais e os indicadores mais importantes para você entender rapidamente a operação.", "Overview"),
            new("Status", "A tela de status resume a saúde do ambiente, a situação do servidor e a condição das máquinas, com atualização manual quando necessário.", "Status"),
            new("Histórico de Produção", "Use essa tela para conferir a produção por período, comparar máquinas e revisar os totais de peças produzidas, boas e perdidas.", "ProductionHistory"),
            new("Histórico de Paradas", "Aqui você acompanha paradas registradas, duração, motivos e categorias para entender o que afetou a operação.", "DowntimeHistory"),
            new("Relatório", "O relatório ajuda a gerar uma prévia organizada por tipo, máquina e período. Nesta etapa, a geração é segura e apenas de visualização.", "Report"),
            new("Alertas e Telegram", "Os alertas mostram ocorrências importantes e o status do canal Telegram quando ele estiver configurado.", "Alerts"),
            new("Configurações", "A área de configurações organiza os módulos do sistema. Alguns itens já podem ser abertos para consulta, e os demais continuam em preparação.", "Settings"),
            new("Atualizações", "As rotinas de atualização ainda não fazem parte desta etapa. A área existe para orientar o usuário sobre o que virá depois.", string.Empty)
        };
    }
}
