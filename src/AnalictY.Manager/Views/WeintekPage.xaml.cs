using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.ViewModels;

namespace AnalictY.Manager.Views;

public partial class WeintekPage : UserControl
{
    private WeintekViewModel? _viewModel;

    public WeintekPage()
    {
        InitializeComponent();
        _viewModel = new WeintekViewModel(new ConfigService(AppServices.HttpClient));
        DataContext = _viewModel;
        Loaded += WeintekPage_Loaded;
    }

    private async void WeintekPage_Loaded(object sender, RoutedEventArgs e)
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

    private void CloseModal_Click(object sender, RoutedEventArgs e)
    {
        ModalOverlay.Visibility = Visibility.Collapsed;
    }

    public void ShowModal(string title, string message)
    {
        ModalTitle.Text = title;
        ModalMessage.Text = message;
        ModalOverlay.Visibility = Visibility.Visible;
    }
}
