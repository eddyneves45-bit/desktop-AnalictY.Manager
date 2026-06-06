using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
