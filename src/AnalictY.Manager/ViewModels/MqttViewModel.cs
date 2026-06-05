using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;

namespace AnalictY.Manager.ViewModels;

public sealed class MqttViewModel : ObservableObject
{
    private string _status = "Em execução";
    private string _port = "1883";
    private string _connectedClients = "3";
    private string _topics = "24";
    private string _messagesReceived = "1,234";
    private string _messagesSent = "856";
    private string _retained = "12";

    public MqttViewModel()
    {
        OpenDashboardCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Dashboard MQTT será aberto quando o endpoint estiver disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
        ClearRetainedCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Limpeza de mensagens retidas será conectada ao Broker quando o endpoint estiver disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
        RestartBrokerCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Reinício do Broker exigirá confirmação futura e integração com o serviço.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });

        TopicRows = new ObservableCollection<MqttTopicRow>
        {
            new("fabrica/m01/producao", "QoS 1", "142", "85"),
            new("fabrica/m01/status", "QoS 0", "56", "0"),
            new("fabrica/m02/producao", "QoS 1", "98", "42"),
            new("fabrica/m02/status", "QoS 0", "23", "0"),
            new("fabrica/pj08/ciclo", "QoS 1", "67", "31"),
            new("fabrica/celula-a/temperatura", "QoS 0", "12", "0"),
            new("analicty/commands", "QoS 2", "8", "Retained"),
            new("analicty/status", "QoS 1", "15", "Retained")
        };

        ClientRows = new ObservableCollection<MqttClientRow>
        {
            new("manager-01", "192.168.1.100", "Connected", "5m"),
            new("hmi-panel-02", "192.168.1.101", "Connected", "12m"),
            new("external-api", "192.168.1.50", "Connected", "2m")
        };
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
}

public sealed record MqttTopicRow(string Topic, string Qos, string Subscribers, string Retained);
public sealed record MqttClientRow(string ClientId, string Ip, string Status, string ConnectedFor);
