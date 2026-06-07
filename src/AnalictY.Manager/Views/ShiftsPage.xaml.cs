using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AnalictY.Manager.ViewModels;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.Views
{
    public partial class ShiftsPage : UserControl
    {
        private readonly ShiftsViewModel _viewModel;

        public ShiftsPage()
        {
            InitializeComponent();
            var configService = App.Current.Resources["ConfigService"] as ConfigService ?? throw new InvalidOperationException("ConfigService not found in resources");
            _viewModel = new ShiftsViewModel(configService);
            DataContext = _viewModel;
            Loaded += ShiftsPage_Loaded;
        }

        private async void ShiftsPage_Loaded(object sender, RoutedEventArgs e)
        {
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
