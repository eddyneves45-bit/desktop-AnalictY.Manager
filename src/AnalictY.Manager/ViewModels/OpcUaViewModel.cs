using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;

namespace AnalictY.Manager.ViewModels;

public sealed class OpcUaViewModel : ObservableObject
{
    private string _status = "Conectado";
    private string _endpoint = "opc.tcp://localhost:4840";
    private string _namespace = "ns=2";
    private string _monitoredNodes = "45";
    private string _quality = "Good";
    private string _lastRead = "2s atrás";

    public OpcUaViewModel()
    {
        TestConnectionCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Teste de conexão será conectado ao servidor OPC UA quando o endpoint estiver disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
        ReconnectCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Reconexão será conectada ao servidor OPC UA quando o endpoint estiver disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
        BrowseNodesCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Navegação de nós será conectada ao servidor OPC UA quando o endpoint estiver disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });

        ServerRows = new ObservableCollection<OpcServerRow>
        {
            new("AnalictY OPC UA Server", "Conectado", "opc.tcp://localhost:4840", "2"),
            new("Weintek HMI Server", "Aguardando", "opc.tcp://192.168.1.101:4840", "-")
        };

        NodeRows = new ObservableCollection<OpcNodeRow>
        {
            new("ns=2;s=Producao.Contador", "Double", "1.250", "Good", "2s atrás"),
            new("ns=2;s=Status.Maquina", "Int32", "1", "Good", "2s atrás"),
            new("ns=2;s=Pressao.Sistema", "Double", "4.2", "Good", "2s atrás"),
            new("ns=2;s=Temperatura.Zona1", "Double", "72.5", "Good", "3s atrás"),
            new("ns=2;s=Velocidade.Rotacao", "Int32", "1450", "Good", "2s atrás"),
            new("ns=2;s=Estado.Sistema", "Boolean", "true", "Good", "1s atrás")
        };
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string Endpoint
    {
        get => _endpoint;
        set => SetProperty(ref _endpoint, value);
    }

    public string Namespace
    {
        get => _namespace;
        set => SetProperty(ref _namespace, value);
    }

    public string MonitoredNodes
    {
        get => _monitoredNodes;
        set => SetProperty(ref _monitoredNodes, value);
    }

    public string Quality
    {
        get => _quality;
        set => SetProperty(ref _quality, value);
    }

    public string LastRead
    {
        get => _lastRead;
        set => SetProperty(ref _lastRead, value);
    }

    public ICommand TestConnectionCommand { get; }
    public ICommand ReconnectCommand { get; }
    public ICommand BrowseNodesCommand { get; }
    public ObservableCollection<OpcServerRow> ServerRows { get; }
    public ObservableCollection<OpcNodeRow> NodeRows { get; }
}

public sealed record OpcServerRow(string Name, string Status, string Endpoint, string Namespace);
public sealed record OpcNodeRow(string NodeId, string DataType, string Value, string Quality, string LastRead);
