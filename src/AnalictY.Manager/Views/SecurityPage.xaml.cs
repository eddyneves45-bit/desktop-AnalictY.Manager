using System.Windows.Controls;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.ViewModels;

namespace AnalictY.Manager.Views;

public partial class SecurityPage : Page
{
    private readonly SecurityViewModel _viewModel;

    public SecurityPage()
    {
        InitializeComponent();
        _viewModel = new SecurityViewModel(new SecurityService(AppServices.HttpClient));
        DataContext = _viewModel;
        Loaded += async (_, _) => await _viewModel.LoadAsync();
    }
}
