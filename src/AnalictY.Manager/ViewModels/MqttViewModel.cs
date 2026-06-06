using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class MqttViewModel : ObservableObject
{
    private readonly AdminApiService _adminApiService;
    private readonly ConfigService _configService;
    private string _status = "Aguardando";
    private string _port = "1883";
    private string _connectedClients = "0";
    private string _topics = "0";
    private string _messagesReceived = "0";
    private string _messagesSent = "0";
    private string _retained = "0";

    public MqttViewModel()
        : this(new AdminApiService(AppServices.HttpClient), new ConfigService(AppServices.HttpClient))
    {
    }

    public MqttViewModel(AdminApiService adminApiService, ConfigService configService)
    {
        _adminApiService = adminApiService;
        _configService = configService;
        OpenDashboardCommand = new RelayCommand(LoadAsync);
        ClearRetainedCommand = new RelayCommand(ShowNotImplementedAsync);
        RestartBrokerCommand = new RelayCommand(ShowProtectedActionAsync);

        TopicRows = new ObservableCollection<MqttTopicRow>();
        ClientRows = new ObservableCollection<MqttClientRow>();

        _ = LoadAsync();
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }

    public string ConnectedClients
    {
        get => _connectedClients;
        set => SetProperty(ref _connectedClients, value);
    }

    public string Topics
    {
        get => _topics;
        set => SetProperty(ref _topics, value);
    }

    public string MessagesReceived
    {
        get => _messagesReceived;
        set => SetProperty(ref _messagesReceived, value);
    }

    public string MessagesSent
    {
        get => _messagesSent;
        set => SetProperty(ref _messagesSent, value);
    }

    public string Retained
    {
        get => _retained;
        set => SetProperty(ref _retained, value);
    }

    public ICommand OpenDashboardCommand { get; }
    public ICommand ClearRetainedCommand { get; }
    public ICommand RestartBrokerCommand { get; }
    public ObservableCollection<MqttTopicRow> TopicRows { get; }
    public ObservableCollection<MqttClientRow> ClientRows { get; }

    private async Task LoadAsync()
    {
        try
        {
            var status = await _adminApiService.GetMqttAsync();
            Status = status.Status;
            ConnectedClients = status.ClientsConnected;
            Topics = status.Topics;
            MessagesReceived = status.MessagesReceivedPerSecond;
            MessagesSent = status.MessagesSentPerSecond;
            Retained = status.Retained;

            var connections = await _configService.GetMqttConnectionsAsync();
            var activeConnection = connections.Connections.FirstOrDefault(connection => connection.IsActive) ??
                connections.Connections.FirstOrDefault();
            Port = activeConnection?.BrokerPort ?? "1883";

            int connectionId = int.TryParse(activeConnection?.Id, out var id) ? id : 0;
            var topics = await _configService.GetMqttTopicsAsync(connectionId);
            TopicRows.Clear();
            foreach (var topic in topics.Topics)
            {
                TopicRows.Add(new MqttTopicRow(topic.Topic, $"QoS {topic.Qos}", topic.Subscribers, "-"));
            }

            var clients = await _configService.GetMqttClientsAsync(connectionId);
            ClientRows.Clear();
            foreach (var client in clients.Clients)
            {
                ClientRows.Add(new MqttClientRow(client.ClientId, client.Ip, client.Connected, client.Topics));
            }
        }
        catch (Exception ex)
        {
            Status = $"Erro: {ex.Message}";
        }
    }

    private static Task ShowNotImplementedAsync()
    {
        MessageBox.Show("Limpeza de mensagens retidas ainda depende de endpoint seguro no AnalictY Server.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
        return Task.CompletedTask;
    }

    private static Task ShowProtectedActionAsync()
    {
        MessageBox.Show("Reinicio do broker sera habilitado somente com confirmacao administrativa.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
        return Task.CompletedTask;
    }
}

public sealed record MqttTopicRow(string Topic, string Qos, string Subscribers, string Retained);
public sealed record MqttClientRow(string ClientId, string Ip, string Status, string ConnectedFor);
