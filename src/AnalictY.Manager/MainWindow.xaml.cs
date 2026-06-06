using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Media;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.ViewModels;
using AnalictY.Manager.Views;

namespace AnalictY.Manager;

public partial class MainWindow : Window
{
    private readonly AdminDashboardViewModel _adminDashboardViewModel;

    public MainWindow()
    {
        InitializeComponent();

        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            UseCookies = true
        };
        var apiClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
        AppServices.Configure(apiClient);

        var machineOverviewService = new MachineOverviewService(apiClient);
        var healthService = new HealthService(apiClient);
        var versionService = new VersionService(apiClient);
        var logsService = new LogsService(apiClient);
        var updatesService = new UpdatesService(apiClient);
        var databaseService = new DatabaseService(apiClient);
        var adminApiService = new AdminApiService(apiClient);

        var viewModel = new MainWindowViewModel(
            new AuthService(apiClient, cookieContainer),
            machineOverviewService,
            new StatusOverviewService(apiClient, machineOverviewService),
            new ProductionHistoryService(apiClient),
            new DowntimeHistoryService(apiClient),
            new AlertService(apiClient),
            new ReportService(apiClient));
        viewModel.PropertyChanged += ViewModel_OnPropertyChanged;
        DataContext = viewModel;
        LoginPasswordBox.Password = viewModel.LoginPassword;

        _adminDashboardViewModel = new AdminDashboardViewModel(adminApiService);
        AdminDashboardViewHost.DataContext = _adminDashboardViewModel;
        ServerViewHost.DataContext = new ServerViewModel(healthService, versionService);
        LogsViewHost.DataContext = new LogsViewModel(logsService);
        UpdatesViewHost.DataContext = new UpdatesViewModel(updatesService, versionService);
        var configService = new ConfigService(apiClient);
        DatabaseViewHost.DataContext = new DatabaseViewModel(databaseService, configService);
        RuntimeViewHost.DataContext = new RuntimeViewModel();
        TagsViewHost.DataContext = new TagsViewModel(configService);
        ProtocolsViewHost.DataContext = new ProtocolsViewModel(configService);
        ServicesViewHost.DataContext = new ServicesViewModel();
        EventsViewHost.DataContext = new EventsViewModel();
        BackupViewHost.DataContext = new BackupViewModel();
        AboutViewHost.DataContext = new AboutViewModel();
        Loaded += MainWindow_OnLoaded;
        ApplyTheme(viewModel.IsDarkTheme);
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        await _adminDashboardViewModel.LoadAsync();
    }

    private void LoginPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.LoginPassword = LoginPasswordBox.Password;
        }
    }

    private void OpenLoginModalButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.OpenLoginModal();
        }

        LoginModalOverlay.Visibility = Visibility.Visible;
    }

    private void CloseLoginModalButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.CloseLoginModal();
        }

        LoginModalOverlay.Visibility = Visibility.Collapsed;
    }

    private void ViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is MainWindowViewModel { LoginPassword.Length: 0 } &&
            e.PropertyName == nameof(MainWindowViewModel.LoginPassword) &&
            !string.IsNullOrEmpty(LoginPasswordBox.Password))
        {
            LoginPasswordBox.Clear();
        }

        if (sender is MainWindowViewModel viewModel &&
            e.PropertyName == nameof(MainWindowViewModel.IsLoginModalOpen))
        {
            LoginModalOverlay.Visibility = viewModel.IsLoginModalOpen
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        if (sender is MainWindowViewModel themeViewModel &&
            e.PropertyName == nameof(MainWindowViewModel.IsDarkTheme))
        {
            ApplyTheme(themeViewModel.IsDarkTheme);
        }
    }

    private void ApplyTheme(bool isDarkTheme)
    {
        SetBrush("SurfaceBrush", isDarkTheme ? "#101820" : "#F3F5F7");
        SetBrush("CardBrush", isDarkTheme ? "#17212B" : "#FFFFFF");
        SetBrush("ContentTextBrush", isDarkTheme ? "#F8FAFC" : "#18212B");
        SetBrush("MutedTextBrush", isDarkTheme ? "#AAB7C5" : "#64707D");
        SetBrush("BorderBrushSoft", isDarkTheme ? "#2A3848" : "#DDE4EA");
        SetBrush("SidebarBrush", isDarkTheme ? "#0B1220" : "#FFFFFF");
        SetBrush("SidebarPanelBrush", isDarkTheme ? "#17212B" : "#F8FAFC");
        SetBrush("LoginBgBrush", isDarkTheme ? "#101820" : "#EEF3F7");
        SetBrush("LoginPanelBrush", isDarkTheme ? "#17212B" : "#FFFFFF");
        SetBrush("LoginFieldBrush", isDarkTheme ? "#162431" : "#F8FAFC");
        SetBrush("LoginPrimaryTextBrush", isDarkTheme ? "#F8FAFC" : "#111827");
        SetBrush("LoginMutedTextBrush", isDarkTheme ? "#AAB7C5" : "#5B6877");
        SetBrush("LoginBorderBrush", isDarkTheme ? "#334456" : "#CBD5E1");
        Background = (Brush)FindResource("SurfaceBrush");
    }

    private void SetBrush(string key, string hexColor)
    {
        var color = (Color)ColorConverter.ConvertFromString(hexColor);

        if (Resources[key] is SolidColorBrush brush)
        {
            if (brush.IsFrozen)
            {
                Resources[key] = new SolidColorBrush(color);
            }
            else
            {
                brush.Color = color;
            }
        }

        if (Application.Current.Resources[key] is SolidColorBrush appBrush)
        {
            if (appBrush.IsFrozen)
            {
                Application.Current.Resources[key] = new SolidColorBrush(color);
            }
            else
            {
                appBrush.Color = color;
            }
        }
    }
}
