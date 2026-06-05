using System.Windows;
using System.Windows.Controls;

namespace AnalictY.Manager.Views
{
    public partial class ConfigDetailPage : Page
    {
        public ConfigDetailPage()
        {
            InitializeComponent();
        }

        public ConfigDetailPage(string title, string description)
        {
            InitializeComponent();
            TitleText.Text = title;
            DescriptionText.Text = description;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
