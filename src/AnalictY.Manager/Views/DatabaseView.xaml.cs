using System.Windows.Controls;
using AnalictY.Manager.ViewModels;

namespace AnalictY.Manager.Views;

public partial class DatabaseView : UserControl
{
    public DatabaseView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is DatabaseViewModel viewModel)
            {
                await viewModel.LoadAsync();
            }
        };
    }
}
