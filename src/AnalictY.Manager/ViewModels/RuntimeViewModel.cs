using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;

namespace AnalictY.Manager.ViewModels;

public sealed class RuntimeViewModel : ObservableObject
{
    private string _status = "Em execução";
    private string _uptime = "2d 14h 32m";
    private string _version = "1.0.0";
    private string _queuedEvents = "0";
    private string _processingRate = "150 evt/s";
    private string _lastCycle = "2s atrás";
    private string _totalCycles = "1,234,567";

    public RuntimeViewModel()
    {
        RefreshCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Atualização de dados do Runtime será conectada ao AnalictY Server quando o endpoint estiver disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
        RestartCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Reinício do Runtime exigirá confirmação futura e integração com o serviço Windows.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });

        InternalServices = new ObservableCollection<RuntimeServiceRow>
        {
            new("Event Processor", "Em execução", "150 evt/s", "0 erros"),
            new("Tag Scanner", "Em execução", "45 tags/s", "0 erros"),
            new("MQTT Publisher", "Em execução", "12 msg/s", "0 erros"),
            new("OPC UA Client", "Em execução", "28 nós/s", "0 erros"),
            new("Database Writer", "Em execução", "85 rec/s", "0 erros"),
            new("WebSocket Server", "Em execução", "1 cliente", "0 erros")
        };

        RecentCycles = new ObservableCollection<RuntimeCycleRow>
        {
            new("2s atrás", "142", "0", "12ms"),
            new("4s atrás", "138", "0", "11ms"),
            new("6s atrás", "145", "0", "13ms"),
            new("8s atrás", "140", "0", "12ms"),
            new("10s atrás", "139", "0", "11ms")
        };
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string Uptime
    {
        get => _uptime;
        set => SetProperty(ref _uptime, value);
    }

    public string Version
    {
        get => _version;
        set => SetProperty(ref _version, value);
    }

    public string QueuedEvents
    {
        get => _queuedEvents;
        set => SetProperty(ref _queuedEvents, value);
    }

    public string ProcessingRate
    {
        get => _processingRate;
        set => SetProperty(ref _processingRate, value);
    }

    public string LastCycle
    {
        get => _lastCycle;
        set => SetProperty(ref _lastCycle, value);
    }

    public string TotalCycles
    {
        get => _totalCycles;
        set => SetProperty(ref _totalCycles, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand RestartCommand { get; }
    public ObservableCollection<RuntimeServiceRow> InternalServices { get; }
    public ObservableCollection<RuntimeCycleRow> RecentCycles { get; }
}

public sealed record RuntimeServiceRow(string Name, string Status, string Rate, string Errors);
public sealed record RuntimeCycleRow(string Time, string Events, string Errors, string Duration);
