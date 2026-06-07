using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AnalictY.Manager.ViewModels;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.Views
{
    public partial class ProductionDiagnosticsPage : UserControl
    {
        private readonly ProductionDiagnosticsViewModel _viewModel;

        public ProductionDiagnosticsPage()
        {
            InitializeComponent();
            var configService = App.Current.Resources["ConfigService"] as ConfigService ?? throw new InvalidOperationException("ConfigService not found in resources");
            _viewModel = new ProductionDiagnosticsViewModel(configService);
            DataContext = _viewModel;
            Loaded += ProductionDiagnosticsPage_Loaded;
        }

        private async void ProductionDiagnosticsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadTimeZoneAsync();
            await _viewModel.LoadAsync();
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
