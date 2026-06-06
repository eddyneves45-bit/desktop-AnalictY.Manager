using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.ViewModels;

namespace AnalictY.Manager.Views
{
    public partial class DatabaseBrowserPage : Page
    {
        private readonly DatabaseViewModel _viewModel;

        public DatabaseBrowserPage()
        {
            InitializeComponent();
            _viewModel = new DatabaseViewModel(
                new DatabaseService(AppServices.HttpClient),
                new ConfigService(AppServices.HttpClient));
            DatabaseViewHost.DataContext = _viewModel;
            Loaded += DatabaseBrowserPage_Loaded;
        }

        private async void DatabaseBrowserPage_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadAsync();
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
