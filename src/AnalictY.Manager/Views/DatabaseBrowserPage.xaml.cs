using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.ViewModels;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.Views;

public partial class DatabaseBrowserPage : UserControl
{
    private DatabaseBrowserViewModel? _viewModel;

    public DatabaseBrowserPage()
    {
        InitializeComponent();
        _viewModel = new DatabaseBrowserViewModel(new ConfigService(AppServices.HttpClient));
        DataContext = _viewModel;
        Loaded += DatabaseBrowserPage_Loaded;
    }

    private async void DatabaseBrowserPage_Loaded(object sender, RoutedEventArgs e)
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

    private void TableButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null && sender is Button button && button.Tag is TableInfo table)
        {
            _viewModel.SelectedTable = table;
        }
    }

    private void ExportCsv_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.ExportCsv();
        }
    }
}
