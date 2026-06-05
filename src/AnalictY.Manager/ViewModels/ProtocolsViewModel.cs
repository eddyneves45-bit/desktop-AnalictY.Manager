using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;

namespace AnalictY.Manager.ViewModels;

public sealed class ProtocolsViewModel : ObservableObject
{
    private bool _isMqttSelected = true;
    private bool _isOpcUaSelected = false;

    public ProtocolsViewModel()
    {
        MqttTopicsList = new ObservableCollection<ProtocolMqttTopicRow>();
        MqttClientsList = new ObservableCollection<ProtocolMqttClientRow>();
        OpcUaServersList = new ObservableCollection<OpcUaServerRow>();
        OpcUaNodesList = new ObservableCollection<OpcUaNodeRow>();

        SelectMqttCommand = new RelayCommand(_ =>
        {
            IsMqttSelected = true;
            IsOpcUaSelected = false;
            return Task.CompletedTask;
        });
        SelectOpcUaCommand = new RelayCommand(_ =>
        {
            IsMqttSelected = false;
            IsOpcUaSelected = true;
            return Task.CompletedTask;
        });

        InitializeMqttData();
        InitializeOpcUaData();
    }

    public bool IsMqttSelected
    {
        get => _isMqttSelected;
        set => SetProperty(ref _isMqttSelected, value);
    }

    public bool IsOpcUaSelected
    {
        get => _isOpcUaSelected;
        set => SetProperty(ref _isOpcUaSelected, value);
    }

    // MQTT Properties
    public string MqttStatus { get; } = "Online";
    public string MqttClients { get; } = "12";
    public string MqttTopics { get; } = "45";
    public string MqttMessagesPerSecond { get; } = "234";
    public string MqttPort { get; } = "1883";
    public string MqttHost { get; } = "0.0.0.0";
    public string MqttSsl { get; } = "Desabilitado";

    // OPC UA Properties
    public string OpcUaStatus { get; } = "Online";
    public string OpcUaServers { get; } = "3";
    public string OpcUaEndpoints { get; } = "5";
    public string OpcUaNodes { get; } = "1,234";
    public string OpcUaPort { get; } = "4840";
    public string OpcUaSecurity { get; } = "None";
    public string OpcUaDiscovery { get; } = "Habilitado";

    public ObservableCollection<ProtocolMqttTopicRow> MqttTopicsList { get; private set; }
    public ObservableCollection<ProtocolMqttClientRow> MqttClientsList { get; private set; }
    public ObservableCollection<OpcUaServerRow> OpcUaServersList { get; private set; }
    public ObservableCollection<OpcUaNodeRow> OpcUaNodesList { get; private set; }

    public ICommand SelectMqttCommand { get; }
    public ICommand SelectOpcUaCommand { get; }

    private void InitializeMqttData()
    {
        MqttTopicsList = new ObservableCollection<ProtocolMqttTopicRow>
        {
            new("analicty/machines/+/status", "1", "8"),
            new("analicty/machines/+/production", "0", "12"),
            new("analicty/machines/+/alarms", "2", "5"),
            new("analicty/servers/+/metrics", "1", "4"),
            new("analicty/tags/+/value", "0", "16")
        };

        MqttClientsList = new ObservableCollection<ProtocolMqttClientRow>
        {
            new("client_001", "192.168.1.10", "Sim", "3"),
            new("client_002", "192.168.1.11", "Sim", "5"),
            new("client_003", "192.168.1.12", "Não", "2"),
            new("client_004", "192.168.1.13", "Sim", "4"),
            new("client_005", "192.168.1.14", "Sim", "6")
        };
    }

    private void InitializeOpcUaData()
    {
        OpcUaServersList = new ObservableCollection<OpcUaServerRow>
        {
            new("AnalictY Server", "opc.tcp://localhost:4840", "Online"),
            new("PLC Siemens", "opc.tcp://192.168.1.50:4840", "Online"),
            new("PLC Rockwell", "opc.tcp://192.168.1.51:4840", "Offline")
        };

        OpcUaNodesList = new ObservableCollection<OpcUaNodeRow>
        {
            new("ns=2;s=Machine1.Status", "Machine1 Status", "Boolean", "Good"),
            new("ns=2;s=Machine1.Production", "Machine1 Production", "Int32", "Good"),
            new("ns=2;s=Machine1.Temperature", "Machine1 Temperature", "Float", "Good"),
            new("ns=2;s=Machine2.Status", "Machine2 Status", "Boolean", "Good"),
            new("ns=2;s=Machine2.Production", "Machine2 Production", "Int32", "Good")
        };
    }
}

public sealed record ProtocolMqttTopicRow(string Topic, string Qos, string Subscribers);
public sealed record ProtocolMqttClientRow(string ClientId, string Ip, string Connected, string Topics);
public sealed record OpcUaServerRow(string Name, string Endpoint, string Status);
public sealed record OpcUaNodeRow(string NodeId, string Name, string Type, string Quality);
