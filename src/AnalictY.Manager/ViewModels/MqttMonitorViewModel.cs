using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.ViewModels;

public sealed class MqttMonitorViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando conexões MQTT...";
    private string _errorMessage = string.Empty;
    private int? _selectedConnectionId;

    // Publish form
    private string _publishTopic = string.Empty;
    private string _publishPayload = string.Empty;
    private int _publishQos = 0;
    private bool _publishRetain;

    // Subscribe form
    private string _subscribeTopic = string.Empty;

    // Tag form
    private string _tagName = string.Empty;
    private string _tagFilter = string.Empty;

    public MqttMonitorViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(LoadAsync);
        PublishCommand = new RelayCommand(ExecutePublish);
        SubscribeCommand = new RelayCommand(ExecuteSubscribe);
        UnsubscribeCommand = new RelayCommand(ExecuteUnsubscribe);

        Connections = new ObservableCollection<MqttConnectionDisplay>();
        DiscoveredTopics = new ObservableCollection<string>();
        Clients = new ObservableCollection<MqttClientDisplay>();
        Tags = new ObservableCollection<Tag>();
    }

    public ObservableCollection<MqttConnectionDisplay> Connections { get; }
    public ObservableCollection<string> DiscoveredTopics { get; }
    public ObservableCollection<MqttClientDisplay> Clients { get; }
    public ObservableCollection<Tag> Tags { get; }

    public ICommand RefreshCommand { get; }
    public ICommand PublishCommand { get; }
    public ICommand SubscribeCommand { get; }
    public ICommand UnsubscribeCommand { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public int? SelectedConnectionId
    {
        get => _selectedConnectionId;
        set
        {
            if (SetProperty(ref _selectedConnectionId, value) && value.HasValue)
            {
                _ = LoadConnectionDataAsync(value.Value);
            }
        }
    }

    // Publish form
    public string PublishTopic
    {
        get => _publishTopic;
        set => SetProperty(ref _publishTopic, value);
    }

    public string PublishPayload
    {
        get => _publishPayload;
        set => SetProperty(ref _publishPayload, value);
    }

    public int PublishQos
    {
        get => _publishQos;
        set => SetProperty(ref _publishQos, value);
    }

    public bool PublishRetain
    {
        get => _publishRetain;
        set => SetProperty(ref _publishRetain, value);
    }

    // Subscribe form
    public string SubscribeTopic
    {
        get => _subscribeTopic;
        set => SetProperty(ref _subscribeTopic, value);
    }

    // Tag form
    public string TagName
    {
        get => _tagName;
        set => SetProperty(ref _tagName, value);
    }

    public string TagFilter
    {
        get => _tagFilter;
        set => SetProperty(ref _tagFilter, value);
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando conexões MQTT...";

        try
        {
            var result = await _configService.GetMqttConnectionsAsync();
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                ErrorMessage = result.Error;
                StatusMessage = "Erro ao carregar conexões.";
                return;
            }

            Connections.Clear();
            foreach (var conn in result.Connections)
            {
                Connections.Add(new MqttConnectionDisplay
                {
                    Id = int.TryParse(conn.Id, out var id) ? id : 0,
                    Name = conn.Name,
                    BrokerHost = conn.BrokerHost,
                    BrokerPort = int.TryParse(conn.BrokerPort, out var port) ? port : 1883,
                    DisplayName = $"{conn.Name} ({conn.BrokerHost}:{conn.BrokerPort})"
                });
            }

            if (Connections.Count > 0)
            {
                SelectedConnectionId = Connections[0].Id;
            }

            StatusMessage = Connections.Count == 0
                ? "Nenhuma conexão MQTT configurada."
                : $"{Connections.Count} conexão(ões) MQTT carregada(s).";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadConnectionDataAsync(int connectionId)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando dados da conexão...";

        try
        {
            await Task.WhenAll(
                LoadTopicsAsync(connectionId),
                LoadClientsAsync(connectionId),
                LoadTagsAsync()
            );

            StatusMessage = "Dados carregados com sucesso.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadTopicsAsync(int connectionId)
    {
        var result = await _configService.GetMqttTopicsAsync(connectionId);
        if (!string.IsNullOrWhiteSpace(result.Error))
        {
            StatusMessage = $"Erro ao carregar tópicos: {result.Error}";
            return;
        }

        DiscoveredTopics.Clear();
        foreach (var topic in result.Topics)
        {
            DiscoveredTopics.Add(topic.Topic);
        }
    }

    private async Task LoadClientsAsync(int connectionId)
    {
        var result = await _configService.GetMqttClientsAsync(connectionId);
        if (!string.IsNullOrWhiteSpace(result.Error))
        {
            StatusMessage = $"Erro ao carregar clientes: {result.Error}";
            return;
        }

        Clients.Clear();
        foreach (var client in result.Clients)
        {
            Clients.Add(new MqttClientDisplay
            {
                ClientId = client.ClientId,
                Connected = string.Equals(client.Connected, "Sim", StringComparison.OrdinalIgnoreCase) || string.Equals(client.Connected, "Yes", StringComparison.OrdinalIgnoreCase),
                LastSeen = client.Ip
            });
        }
    }

    private async Task LoadTagsAsync()
    {
        var result = await _configService.GetTagsAsync();
        if (!string.IsNullOrWhiteSpace(result.Error))
        {
            StatusMessage = $"Erro ao carregar TAGs: {result.Error}";
            return;
        }

        Tags.Clear();
        foreach (var tag in result.Tags)
        {
            Tags.Add(tag);
        }
    }

    private async Task ExecutePublish()
    {
        if (!_selectedConnectionId.HasValue || string.IsNullOrWhiteSpace(PublishTopic) || string.IsNullOrWhiteSpace(PublishPayload))
        {
            ErrorMessage = "Selecione uma conexão e preencha tópico e payload.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.PublishMqttAsync(
                _selectedConnectionId.Value,
                PublishTopic,
                PublishPayload,
                PublishQos,
                PublishRetain);

            if (result.Success)
            {
                StatusMessage = result.Message;
                PublishPayload = string.Empty;
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteSubscribe()
    {
        if (!_selectedConnectionId.HasValue || string.IsNullOrWhiteSpace(SubscribeTopic))
        {
            ErrorMessage = "Selecione uma conexão e preencha o tópico.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.SubscribeMqttAsync(
                _selectedConnectionId.Value,
                SubscribeTopic);

            if (result.Success)
            {
                StatusMessage = result.Message;
                SubscribeTopic = string.Empty;
                await LoadTopicsAsync(_selectedConnectionId.Value);
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteUnsubscribe()
    {
        if (!_selectedConnectionId.HasValue || string.IsNullOrWhiteSpace(SubscribeTopic))
        {
            ErrorMessage = "Selecione uma conexão e preencha o tópico.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.UnsubscribeMqttAsync(
                _selectedConnectionId.Value,
                SubscribeTopic);

            if (result.Success)
            {
                StatusMessage = result.Message;
                SubscribeTopic = string.Empty;
                await LoadTopicsAsync(_selectedConnectionId.Value);
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task CreateTagAsync(string tagName, string dataType, string address)
    {
        if (!_selectedConnectionId.HasValue)
        {
            ErrorMessage = "Selecione uma conexão MQTT.";
            return;
        }

        var request = new TagRequest
        {
            TagName = tagName,
            DataType = dataType,
            DriverType = "MQTT",
            Address = address,
            OpcUaConnectionId = _selectedConnectionId.Value,
            PollIntervalMs = 1000,
            IsActive = true
        };

        var result = await _configService.CreateTagAsync(request);
        if (result.Success)
        {
            StatusMessage = $"TAG '{tagName}' criada com sucesso.";
            await LoadTagsAsync();
        }
        else
        {
            ErrorMessage = result.Message ?? "Erro ao criar TAG.";
        }
    }

    public async Task DeleteTagAsync(int tagId)
    {
        var result = await _configService.DeleteTagAsync(tagId);
        if (result.Success)
        {
            StatusMessage = "TAG excluída com sucesso.";
            await LoadTagsAsync();
        }
        else
        {
            ErrorMessage = result.Message ?? "Erro ao excluir TAG.";
        }
    }
}

public sealed record MqttConnectionDisplay
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string BrokerHost { get; init; } = string.Empty;
    public int BrokerPort { get; init; }
    public string DisplayName { get; init; } = string.Empty;
}

public sealed record MqttClientDisplay
{
    public string ClientId { get; init; } = string.Empty;
    public bool Connected { get; init; }
    public string LastSeen { get; init; } = string.Empty;
}
