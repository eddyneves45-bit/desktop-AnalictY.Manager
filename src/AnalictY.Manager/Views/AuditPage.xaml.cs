using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.ViewModels;

namespace AnalictY.Manager.Views;

public partial class AuditPage : Page
{
    private readonly AuditPageViewModel _viewModel;

    public AuditPage()
    {
        InitializeComponent();
        _viewModel = new AuditPageViewModel(new AuditService(AppServices.HttpClient));
        DataContext = _viewModel;
        Loaded += async (_, _) => await _viewModel.LoadAsync();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService != null && NavigationService.CanGoBack)
        {
            NavigationService.GoBack();
            return;
        }

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
