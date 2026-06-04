using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using AnalictY.Console.Infrastructure;
using AnalictY.Console.Models;
using AnalictY.Console.Services;

namespace AnalictY.Console.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly HealthService _healthService;
    private readonly MachineOverviewService _machineOverviewService;
    private bool _hasLoadedOverview;
    private string _currentPage = "Overview";
    private string _serverStatus = "Não verificado";
    private string _serverStatusDetail = "Clique em Verificar servidor para consultar o AnalictY Server local.";
    private string _checkButtonText = "Verificar servidor";
    private string _lastCheckedAt = "Ainda não verificado";
    private string _selectedShift = "Turno atual";
    private string _selectedLine = "Todas as linhas";
    private string _overviewDataMessage = "Carregando máquinas...";
    private bool _isServerOnline;
    private bool _isCheckingServer;
    private bool _isLoadingMachines;

    public MainWindowViewModel(HealthService healthService, MachineOverviewService machineOverviewService)
    {
        _healthService = healthService;
        _machineOverviewService = machineOverviewService;

        NavigateCommand = new RelayCommand(async parameter =>
        {
            if (parameter is string pageKey)
            {
                CurrentPage = pageKey;
                if (pageKey == "Overview")
                {
                    await LoadMachineOverviewAsync();
                }
            }
        });

        CheckServerCommand = new RelayCommand(CheckServerAsync);

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
        ModuleCards = CreateModuleCards();
        HelpTopics = CreateHelpTopics();
        ShiftFilters = new ObservableCollection<string> { "Turno atual", "Hoje", "Últimas 24 horas" };
        LineFilters = new ObservableCollection<string> { "Todas as linhas", "Linha Principal", "Preparação", "Célula A" };

        ApplyMachines(MachineOverviewService.CreateFallbackMachines());
        _ = LoadMachineOverviewAsync();
    }

    public ObservableCollection<NavigationItem> NavigationItems { get; }

    public ObservableCollection<KpiCard> Kpis { get; }

    public ObservableCollection<MachineCard> Machines { get; }

    public ObservableCollection<ModuleCard> ModuleCards { get; }

    public ObservableCollection<HelpTopic> HelpTopics { get; }

    public ObservableCollection<string> ShiftFilters { get; }

    public ObservableCollection<string> LineFilters { get; }

    public ICommand NavigateCommand { get; }

    public ICommand CheckServerCommand { get; }

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
        "Exit" => "Sair",
        _ => "AnalictY Console"
    };

    public string CurrentPageSubtitle => CurrentPage switch
    {
        "Overview" => "Acompanhamento inicial da operação com dados do AnalictY Server quando disponíveis.",
        "Status" => "Conectividade local com o AnalictY Server.",
        "Settings" => "Módulos previstos para operação e administração.",
        "Help" => "Orientações rápidas para uso do console.",
        "Exit" => "Encerramento de sessão será implementado com o login real.",
        _ => "Tela nativa reservada para a próxima etapa."
    };

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

    public string SelectedShift
    {
        get => _selectedShift;
        set => SetProperty(ref _selectedShift, value);
    }

    public string SelectedLine
    {
        get => _selectedLine;
        set => SetProperty(ref _selectedLine, value);
    }

    public string OverviewDataMessage
    {
        get => _overviewDataMessage;
        private set => SetProperty(ref _overviewDataMessage, value);
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

    public bool IsLoadingMachines
    {
        get => _isLoadingMachines;
        private set => SetProperty(ref _isLoadingMachines, value);
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

    private void ApplyMachines(IReadOnlyList<MachineCard> machines)
    {
        Machines.Clear();
        foreach (MachineCard machine in machines)
        {
            Machines.Add(machine);
        }

        UpdateKpis(machines);
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
                ? "AnalictY Server respondeu com status saudável."
                : "Não foi possível conectar ao AnalictY Server local.";
            LastCheckedAt = $"Última verificação: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        }
        finally
        {
            CheckButtonText = "Verificar servidor";
            IsCheckingServer = false;
        }
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
            new("Cadastro e acesso", "Nesta etapa o usuário exibido é admin/admin apenas como referência visual. O login real será conectado em uma próxima fase."),
            new("Configurações", "Os módulos de configuração aparecem como cartões para orientar o fluxo. Eles ainda não gravam dados nesta etapa."),
            new("Telegram/Alertas", "Alertas e Telegram serão usados para avisos operacionais. Por enquanto, a tela mostra apenas a organização prevista."),
            new("Produção/Histórico", "A Visão Geral tenta carregar máquinas reais do AnalictY Server e usa dados demonstrativos se a API não responder."),
            new("Atualizações", "Atualizações do sistema continuam fora do escopo desta etapa. O console apenas reserva a área visual para essa função.")
        };
    }
}
