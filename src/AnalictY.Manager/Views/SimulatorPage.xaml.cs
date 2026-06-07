using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AnalictY.Manager.ViewModels;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.Views
{
    public partial class SimulatorPage : UserControl
    {
        private readonly SimulatorViewModel _viewModel;

        public SimulatorPage()
        {
            InitializeComponent();
            var configService = App.Current.Resources["ConfigService"] as ConfigService ?? throw new InvalidOperationException("ConfigService not found in resources");
            _viewModel = new SimulatorViewModel(configService);
            DataContext = _viewModel;
            Loaded += SimulatorPage_Loaded;
        }

        private async void SimulatorPage_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadMachinesAsync();
        }

        private void NavigateBackButton_Click(object sender, RoutedEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(this);
            while (parent != null && parent is not ConfigPage)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent is ConfigPage configPage)
            {
                configPage.ReturnToCards();
            }
        }
    }
}
