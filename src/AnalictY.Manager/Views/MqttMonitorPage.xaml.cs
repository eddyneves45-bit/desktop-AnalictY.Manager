using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.ViewModels;

namespace AnalictY.Manager.Views;

public partial class MqttMonitorPage : UserControl
{
    private MqttMonitorViewModel? _viewModel;

    public MqttMonitorPage()
    {
        InitializeComponent();
        _viewModel = new MqttMonitorViewModel(new ConfigService(AppServices.HttpClient));
        DataContext = _viewModel;
        Loaded += MqttMonitorPage_Loaded;
    }

    private async void MqttMonitorPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
            await _viewModel.LoadAsync();
        }
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

    private void ConnectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel != null && sender is ComboBox comboBox)
        {
            if (comboBox.SelectedValue is int connectionId)
            {
                _viewModel.SelectedConnectionId = connectionId;
            }
        }
    }

    private void CloseNotification_Click(object sender, RoutedEventArgs e)
    {
        NotificationOverlay.Visibility = Visibility.Collapsed;
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorBorder.Visibility = Visibility.Visible;
    }

    private void ShowNotification(string title, string message)
    {
        NotificationTitle.Text = title;
        NotificationMessage.Text = message;
        NotificationOverlay.Visibility = Visibility.Visible;
    }
}
