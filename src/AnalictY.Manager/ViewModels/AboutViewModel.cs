using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;

namespace AnalictY.Manager.ViewModels;

public sealed class AboutViewModel : ObservableObject
{
    private string _productName = "AnalictY Server Manager";
    private string _version = "1.0.0";
    private string _build = "2025.06.05";
    private string _author = "AnalictY Solutions";
    private string _installPath = "C:\\Program Files\\AnalictY\\Manager";
    private string _dataDirectory = "C:\\ProgramData\\AnalictY";

    public AboutViewModel()
    {
        OpenDocsCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Documentação será aberta quando o endpoint estiver disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
        OpenSupportCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Suporte será aberto quando o endpoint estiver disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
    }

    public string ProductName
    {
        get => _productName;
        set => SetProperty(ref _productName, value);
    }

    public string Version
    {
        get => _version;
        set => SetProperty(ref _version, value);
    }

    public string Build
    {
        get => _build;
        set => SetProperty(ref _build, value);
    }

    public string Author
    {
        get => _author;
        set => SetProperty(ref _author, value);
    }

    public string InstallPath
    {
        get => _installPath;
        set => SetProperty(ref _installPath, value);
    }

    public string DataDirectory
    {
        get => _dataDirectory;
        set => SetProperty(ref _dataDirectory, value);
    }

    public ICommand OpenDocsCommand { get; }
    public ICommand OpenSupportCommand { get; }
}
