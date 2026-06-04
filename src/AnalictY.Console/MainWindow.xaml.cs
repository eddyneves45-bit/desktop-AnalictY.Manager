using System.Net.Http;
using System.Windows;
using AnalictY.Console.Services;
using AnalictY.Console.ViewModels;

namespace AnalictY.Console;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(
            new HealthService(new HttpClient { Timeout = TimeSpan.FromSeconds(6) }));
    }
}
