using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.ViewModels;

namespace AnalictY.Manager.Views;

public partial class AuditPage : UserControl
{
    private AuditViewModel? _viewModel;

    public AuditPage()
    {
        InitializeComponent();
        _viewModel = new AuditViewModel(new ConfigService(AppServices.HttpClient));
        DataContext = _viewModel;
        Loaded += AuditPage_Loaded;
    }

    private async void AuditPage_Loaded(object sender, RoutedEventArgs e)
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

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorBorder.Visibility = Visibility.Visible;
    }
}
