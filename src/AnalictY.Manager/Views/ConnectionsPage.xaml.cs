using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.ViewModels;

namespace AnalictY.Manager.Views
{
    public partial class ConnectionsPage : Page
    {
        public ConnectionsPage()
        {
            InitializeComponent();
            DataContext = new ConnectionsPageViewModel(new ConfigService(AppServices.HttpClient));
            Loaded += ConnectionsPage_Loaded;
        }

        private async void ConnectionsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ConnectionsPageViewModel viewModel)
            {
                await viewModel.LoadAsync();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
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
