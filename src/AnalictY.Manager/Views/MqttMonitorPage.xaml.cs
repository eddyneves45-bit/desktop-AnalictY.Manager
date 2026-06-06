using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.ViewModels;

namespace AnalictY.Manager.Views
{
    public partial class MqttMonitorPage : Page
    {
        public MqttMonitorPage()
        {
            InitializeComponent();
            MqttViewHost.DataContext = new MqttViewModel(
                new AdminApiService(AppServices.HttpClient),
                new ConfigService(AppServices.HttpClient));
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
