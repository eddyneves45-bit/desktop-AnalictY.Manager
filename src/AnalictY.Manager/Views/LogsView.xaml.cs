using System.Windows.Controls;
using AnalictY.Manager.ViewModels;

namespace AnalictY.Manager.Views;

public partial class LogsView : UserControl
{
    public LogsView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is LogsViewModel viewModel)
            {
                await viewModel.LoadAsync();
            }
        };
    }
}
