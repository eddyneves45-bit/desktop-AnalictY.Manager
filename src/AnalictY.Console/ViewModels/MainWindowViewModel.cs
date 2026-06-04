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
    private string _currentPage = "Overview";
    private string _serverStatus = "Não verificado";
    private string _serverStatusDetail = "Clique em Verificar servidor para consultar o AnalictY Server local.";
    private string _checkButtonText = "Verificar servidor";
    private string _lastCheckedAt = "Ainda não verificado";
    private string _selectedShift = "Turno atual";
    private string _selectedLine = "Todas as linhas";
    private bool _isServerOnline;
    private bool _isCheckingServer;

    public MainWindowViewModel(HealthService healthService)
    {
        _healthService = healthService;

        NavigateCommand = new RelayCommand(parameter =>
        {
            if (parameter is string pageKey)
            {
                CurrentPage = pageKey;
            }

            return Task.CompletedTask;
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

        Kpis = new ObservableCollection<KpiCard>
        {
            new("Total Máquinas", "8", "Máquinas cadastradas no console", Brushes.DodgerBlue),
            new("Total Em Operação", "5", "Produção ativa agora", new SolidColorBrush(Color.FromRgb(32, 180, 134))),
            new("Total em Manutenção", "1", "Aguardando intervenção", new SolidColorBrush(Color.FromRgb(239, 68, 68))),
            new("Total Ociosas", "2", "Sem ordem em execução", new SolidColorBrush(Color.FromRgb(249, 115, 22)))
        };

        Machines = new ObservableCollection<MachineCard>
        {
            new("PJ_08", "Linha Principal", "Em operação", "OP-2406-118", "92%", Brushes.White, new SolidColorBrush(Color.FromRgb(28, 139, 99))),
            new("Morno_01", "Preparação", "Em operação", "OP-2406-121", "88%", Brushes.White, new SolidColorBrush(Color.FromRgb(28, 139, 99))),
            new("Morno_02", "Preparação", "Ociosa", "Sem ordem", "0%", Brushes.White, new SolidColorBrush(Color.FromRgb(100, 116, 139))),
            new("Injetora_03", "Célula A", "Manutenção", "Bloqueada", "0%", Brushes.White, new SolidColorBrush(Color.FromRgb(217, 119, 6))),
            new("Esteira_01", "Expedição", "Em operação", "OP-2406-102", "95%", Brushes.White, new SolidColorBrush(Color.FromRgb(28, 139, 99))),
            new("Misturador_02", "Célula B", "Ociosa", "Aguardando programação", "0%", Brushes.White, new SolidColorBrush(Color.FromRgb(100, 116, 139)))
        };

        ModuleCards = new ObservableCollection<ModuleCard>
        {
            new("TAGs", "Cadastro e organização dos pontos monitorados.", "Planejado"),
            new("Weintek HTTP", "Conexão com IHMs e leitura por API local.", "Planejado"),
            new("Alertas", "Regras de notificação e acompanhamento operacional.", "Planejado"),
            new("Máquinas", "Cadastro das máquinas e agrupamento por linha.", "Mock inicial"),
            new("Logs", "Acesso aos registros do AnalictY Server.", "Planejado"),
            new("Turnos", "Janelas de produção e calendário operacional.", "Planejado"),
            new("Dashboards", "Painéis nativos para acompanhamento da fábrica.", "Planejado"),
            new("Telegram", "Canal de avisos para equipes configuradas.", "Planejado"),
            new("Banco de Dados", "Status e rotinas do banco local.", "Planejado"),
            new("Atualizações", "Verificação e aplicação de versões futuras.", "Planejado")
        };

        HelpTopics = new ObservableCollection<HelpTopic>
        {
            new("Funcionamento", "O AnalictY Console é a janela desktop para acompanhar o AnalictY Server instalado neste computador. Ele mostra o estado da operação sem abrir navegador."),
            new("Cadastro e acesso", "Nesta etapa o usuário exibido é admin/admin apenas como referência visual. O login real será conectado em uma próxima fase."),
            new("Configurações", "Os módulos de configuração aparecem como cartões para orientar o fluxo. Eles ainda não gravam dados nesta etapa."),
            new("Telegram/Alertas", "Alertas e Telegram serão usados para avisos operacionais. Por enquanto, a tela mostra apenas a organização prevista."),
            new("Produção/Histórico", "Os cards de produção são exemplos para validar a experiência. A leitura real de máquinas será integrada depois pela API."),
            new("Atualizações", "Atualizações do sistema continuam fora do escopo desta etapa. O console apenas reserva a área visual para essa função.")
        };

        ShiftFilters = new ObservableCollection<string> { "Turno atual", "Hoje", "Últimas 24 horas" };
        LineFilters = new ObservableCollection<string> { "Todas as linhas", "Linha Principal", "Preparação", "Célula A" };
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
        "Overview" => "Acompanhamento inicial da operação com dados mockados.",
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
}
