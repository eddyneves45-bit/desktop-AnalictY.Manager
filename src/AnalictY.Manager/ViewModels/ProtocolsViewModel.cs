using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Models;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class ProtocolsViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isMqttSelected = true;
    private bool _isOpcUaSelected = false;
    private bool _isLoading;

    // MQTT Properties
    private string _mqttStatus = "Carregando...";
    private string _mqttClients = "-";
    private string _mqttTopics = "-";
    private string _mqttMessagesPerSecond = "-";
    private string _mqttPort = "-";
    private string _mqttHost = "-";
    private string _mqttSsl = "-";

    // OPC UA Properties
    private string _opcUaStatus = "Carregando...";
    private string _opcUaServers = "-";
    private string _opcUaEndpoints = "-";
    private string _opcUaNodes = "-";
    private string _opcUaPort = "-";
    private string _opcUaSecurity = "-";
    private string _opcUaDiscovery = "-";

    public ProtocolsViewModel(ConfigService configService)
    {
        _configService = configService;
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

        RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());

        _ = LoadDataAsync();
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

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    // MQTT Properties
    public string MqttStatus
    {
        get => _mqttStatus;
        set => SetProperty(ref _mqttStatus, value);
    }

    public string MqttClients
    {
        get => _mqttClients;
        set => SetProperty(ref _mqttClients, value);
    }

    public string MqttTopics
    {
        get => _mqttTopics;
        set => SetProperty(ref _mqttTopics, value);
    }

    public string MqttMessagesPerSecond
    {
        get => _mqttMessagesPerSecond;
        set => SetProperty(ref _mqttMessagesPerSecond, value);
    }

    public string MqttPort
    {
        get => _mqttPort;
        set => SetProperty(ref _mqttPort, value);
    }

    public string MqttHost
    {
        get => _mqttHost;
        set => SetProperty(ref _mqttHost, value);
    }

    public string MqttSsl
    {
        get => _mqttSsl;
        set => SetProperty(ref _mqttSsl, value);
    }

    // OPC UA Properties
    public string OpcUaStatus
    {
        get => _opcUaStatus;
        set => SetProperty(ref _opcUaStatus, value);
    }

    public string OpcUaServers
    {
        get => _opcUaServers;
        set => SetProperty(ref _opcUaServers, value);
    }

    public string OpcUaEndpoints
    {
        get => _opcUaEndpoints;
        set => SetProperty(ref _opcUaEndpoints, value);
    }

    public string OpcUaNodes
    {
        get => _opcUaNodes;
        set => SetProperty(ref _opcUaNodes, value);
    }

    public string OpcUaPort
    {
        get => _opcUaPort;
        set => SetProperty(ref _opcUaPort, value);
    }

    public string OpcUaSecurity
    {
        get => _opcUaSecurity;
        set => SetProperty(ref _opcUaSecurity, value);
    }

    public string OpcUaDiscovery
    {
        get => _opcUaDiscovery;
        set => SetProperty(ref _opcUaDiscovery, value);
    }

    public ObservableCollection<ProtocolMqttTopicRow> MqttTopicsList { get; private set; }
    public ObservableCollection<ProtocolMqttClientRow> MqttClientsList { get; private set; }
    public ObservableCollection<OpcUaServerRow> OpcUaServersList { get; private set; }
    public ObservableCollection<OpcUaNodeRow> OpcUaNodesList { get; private set; }

    public ICommand SelectMqttCommand { get; }
    public ICommand SelectOpcUaCommand { get; }
    public ICommand RefreshCommand { get; }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            await LoadMqttDataAsync();
            await LoadOpcUaDataAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadMqttDataAsync()
    {
        var connectionsResult = await _configService.GetMqttConnectionsAsync();
        if (connectionsResult.Error != null)
        {
            MqttStatus = "Erro";
            return;
        }

        if (connectionsResult.Connections.Count > 0)
        {
            var firstConnection = connectionsResult.Connections[0];
            MqttStatus = connectionsResult.Connections.Count == 1
                ? "1 conexao cadastrada"
                : $"{connectionsResult.Connections.Count} conexoes cadastradas";
            MqttHost = firstConnection.Host;
            MqttPort = firstConnection.Port;
            MqttClients = connectionsResult.Connections.Count.ToString();
        }
        else
        {
            MqttStatus = "Nenhuma conexão";
        }

        var topicsResult = await _configService.GetMqttTopicsAsync();
        if (topicsResult.Error == null)
        {
            MqttTopics = topicsResult.Topics.Count.ToString();
            MqttTopicsList.Clear();
            foreach (var topic in topicsResult.Topics)
            {
                MqttTopicsList.Add(new ProtocolMqttTopicRow(topic.Topic, topic.Qos, topic.Subscribers));
            }
        }

        var clientsResult = await _configService.GetMqttClientsAsync();
        if (clientsResult.Error == null)
        {
            MqttClientsList.Clear();
            foreach (var client in clientsResult.Clients)
            {
                MqttClientsList.Add(new ProtocolMqttClientRow(client.ClientId, client.Ip, client.Connected, client.Topics));
            }
        }
    }

    private async Task LoadOpcUaDataAsync()
    {
        var connectionsResult = await _configService.GetOpcUaConnectionsAsync();
        if (connectionsResult.Error != null)
        {
            OpcUaStatus = "Erro";
            return;
        }

        if (connectionsResult.Connections.Count > 0)
        {
            var firstConnection = connectionsResult.Connections[0];
            OpcUaStatus = connectionsResult.Connections.Count == 1
                ? "1 conexao cadastrada"
                : $"{connectionsResult.Connections.Count} conexoes cadastradas";
            OpcUaServers = connectionsResult.Connections.Count.ToString();
            OpcUaEndpoints = connectionsResult.Connections.Count.ToString();
            
            // Extract port from endpoint if possible
            var endpointParts = firstConnection.Endpoint.Split(':');
            if (endpointParts.Length > 1)
            {
                OpcUaPort = endpointParts[^1];
            }
        }
        else
        {
            OpcUaStatus = "Nenhuma conexão";
        }

        OpcUaServersList.Clear();
        foreach (var connection in connectionsResult.Connections)
        {
            OpcUaServersList.Add(new OpcUaServerRow(connection.Name, connection.Endpoint, connection.Status));
        }

        // Try to browse nodes from first connection
        if (connectionsResult.Connections.Count > 0)
        {
            var browseResult = await _configService.BrowseOpcUaAsync(connectionsResult.Connections[0].Id);
            if (browseResult.Error == null)
            {
                OpcUaNodes = browseResult.Nodes.Count.ToString();
                OpcUaNodesList.Clear();
                foreach (var node in browseResult.Nodes.Take(50)) // Limit to 50 for performance
                {
                    OpcUaNodesList.Add(new OpcUaNodeRow(node.NodeId, node.Name, node.Type, node.Quality));
                }
            }
        }
    }
}

public sealed record ProtocolMqttTopicRow(string Topic, string Qos, string Subscribers);
public sealed record ProtocolMqttClientRow(string ClientId, string Ip, string Connected, string Topics);
public sealed record OpcUaServerRow(string Name, string Endpoint, string Status);
public sealed record OpcUaNodeRow(string NodeId, string Name, string Type, string Quality);
