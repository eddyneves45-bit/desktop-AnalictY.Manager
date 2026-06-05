using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;

namespace AnalictY.Manager.ViewModels;

public sealed class LicenseViewModel : ObservableObject
{
    private string _licenseType = "Professional";
    private string _status = "Ativa";
    private string _client = "Empresa Exemplo Ltda";
    private string _licenseKey = "ANAL-XXXX-XXXX-XXXX-XXXX";
    private string _validity = "31/12/2025";
    private string _features = "Runtime, MQTT, OPC UA, Database, API";

    public LicenseViewModel()
    {
        ActivateCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Ativação de licença será conectada ao endpoint quando disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
        UpdateCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Atualização de licença será conectada ao endpoint quando disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
    }

    public string LicenseType
    {
        get => _licenseType;
        set => SetProperty(ref _licenseType, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string Client
    {
        get => _client;
        set => SetProperty(ref _client, value);
    }

    public string LicenseKey
    {
        get => _licenseKey;
        set => SetProperty(ref _licenseKey, value);
    }

    public string Validity
    {
        get => _validity;
        set => SetProperty(ref _validity, value);
    }

    public string Features
    {
        get => _features;
        set => SetProperty(ref _features, value);
    }

    public ICommand ActivateCommand { get; }
    public ICommand UpdateCommand { get; }
}
