using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AnalictY.Manager.ViewModels;

namespace AnalictY.Manager.Views
{
    public partial class ConfigPage : Page
    {
        private ConfigPageViewModel? _viewModel;
        private DispatcherTimer? _clockTimer;

        public ConfigPage()
        {
            InitializeComponent();
            _viewModel = new ConfigPageViewModel();
            DataContext = _viewModel;
            Loaded += ConfigPage_Loaded;
        }

        private void ConfigPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.SelectSector("all");
                
                // Setup clock timer
                _clockTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                _clockTimer.Tick += (s, args) => _viewModel?.UpdateClock();
                _clockTimer.Start();
            }
        }

        private void SectorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string sectorId && _viewModel != null)
            {
                _viewModel.SelectSector(sectorId);
            }
        }

        private async void SaveTimeZone_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                await _viewModel.SaveTimeZone();
            }
        }

        private void Card_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string navigationTarget)
            {
                NavigateToPage(navigationTarget);
            }
        }

        private void NavigateToPage(string target)
        {
            FrameworkElement? page = target switch
            {
                "opc-browser" => new OpcUaView(),
                "mqtt-monitor" => new MqttMonitorPage(),
                "mysql-console" => new ConnectionsPage(),
                "database-browser" => new DatabaseBrowserPage(),
                "connections" => new ConnectionsPage(),
                "weintek-browser" => new WeintekPage(),
                "tags" => new TagsPage(),
                "machines" => new MachinesPage(),
                "shifts" => new ShiftsPage(),
                "simulator" => new SimulatorPage(),
                "production-diagnostics" => new ProductionDiagnosticsPage(),
                "alerts" => new AlertsPage(),
                "telegram-notifications" => new TelegramPage(),
                "dashboards" => new DashboardsPage(),
                "logs" => new LogsPage(),
                "local-server" => new LocalServerPage(),
                "users" => new UsersPage(),
                "security" => new SecurityPage(),
                "audit" => new AuditPage(),
                "downtime-reasons" => new DowntimeReasonsPage(),
                _ => null
            };

            if (page != null)
            {
                ConfigHeader.Visibility = Visibility.Collapsed;
                MainConfigContent.Visibility = Visibility.Collapsed;
                NavigationFrame.Visibility = Visibility.Visible;
                NavigationFrame.Navigate(page);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ReturnToCards();
        }

        public void ReturnToCards()
        {
            NavigationFrame.Content = null;
            NavigationFrame.Visibility = Visibility.Collapsed;
            ConfigHeader.Visibility = Visibility.Visible;
            MainConfigContent.Visibility = Visibility.Visible;
        }
    }
}
