using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.ViewModels;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.Views;

public partial class MachinesPage : UserControl
{
    private MachinesViewModel? _viewModel;

    public MachinesPage()
    {
        InitializeComponent();
        _viewModel = new MachinesViewModel(new ConfigService(AppServices.HttpClient));
        DataContext = _viewModel;
        Loaded += MachinesPage_Loaded;
    }

    private async void MachinesPage_Loaded(object sender, RoutedEventArgs e)
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

    private void OnFolderSelected(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null && sender is TreeViewItem tvi && tvi.DataContext is MachineFolder folder)
        {
            if (_viewModel.SelectFolderCommand.CanExecute(folder))
            {
                _viewModel.SelectFolderCommand.Execute(folder);
            }
        }
    }
}
