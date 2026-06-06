using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AnalictY.Manager.Views
{
    public partial class LogsPage : Page
    {
        public LogsPage()
        {
            InitializeComponent();
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
