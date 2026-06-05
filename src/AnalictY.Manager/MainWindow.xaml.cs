using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Windows;
using AnalictY.Manager.Services;
using AnalictY.Manager.ViewModels;

namespace AnalictY.Manager;

public partial class MainWindow : Window
{
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

        var machineOverviewService = new MachineOverviewService(apiClient);
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
    }
}
