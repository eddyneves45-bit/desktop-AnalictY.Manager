using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.ViewModels;

namespace AnalictY.Manager.Views
{
    public partial class OpcBrowserPage : Page
    {
        private readonly OpcUaBrowserViewModel _viewModel;

        public OpcBrowserPage()
        {
            InitializeComponent();
            var httpClient = AppServices.HttpClient;
            var configService = new ConfigService(httpClient);
            _viewModel = new OpcUaBrowserViewModel(configService);
            DataContext = _viewModel;
            Loaded += OpcBrowserPage_Loaded;
        }

        private async void OpcBrowserPage_Loaded(object sender, RoutedEventArgs e)
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

        private void RootButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.GoToRoot();
        }

        private void BackNodeButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.GoBack();
        }

        private void OpenNodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: Models.OpcUaNode node } && node.HasChildren)
            {
                _viewModel.OpenNode(node.NodeId);
            }
        }

        private void NodesList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_viewModel.SelectedNode is { HasChildren: true } node)
            {
                _viewModel.OpenNode(node.NodeId);
            }
        }
    }
}
