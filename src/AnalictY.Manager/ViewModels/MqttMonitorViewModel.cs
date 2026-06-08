using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Text.Json;
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

    // Diagnostics
    private string _connectionStatus = "DISCONNECTED";
    private string _broker = "-";
    private int _brokerPort = 1883;
    private string _clientId = "-";
    private int _uptime = 0;
    private int _messageCount = 0;
    private double _messagesPerSecond = 0;
    private int _retryCount = 0;

    // Collections
    private readonly ObservableCollection<string> _subscribedTopics = new();
    private readonly ObservableCollection<MqttCacheField> _cacheFields = new();
    private readonly ObservableCollection<MqttMessageLog> _messageLog = new();
    private readonly ObservableCollection<MqttConnectionLog> _connectionLog = new();

    public MqttMonitorViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(LoadAsync);
        PublishCommand = new RelayCommand(ExecutePublish);
        SubscribeCommand = new RelayCommand(ExecuteSubscribe);
        UnsubscribeCommand = new RelayCommand(ExecuteUnsubscribe);
        CreateTagCommand = new RelayCommand(parameter => ExecuteCreateTag(parameter as MqttCacheField));
        CreateAllTagsCommand = new RelayCommand(ExecuteCreateAllTags);
        DeleteTagCommand = new RelayCommand(parameter => ExecuteDeleteTag(parameter as Tag));
        ClearMessageLogCommand = new RelayCommand(() => { MessageLog.Clear(); return Task.CompletedTask; });
        ClearConnectionLogCommand = new RelayCommand(() => { ConnectionLog.Clear(); return Task.CompletedTask; });

        Connections = new ObservableCollection<MqttConnectionDisplay>();
        DiscoveredTopics = new ObservableCollection<string>();
        Clients = new ObservableCollection<MqttClientDisplay>();
        Tags = new ObservableCollection<Tag>();
    }

    public ObservableCollection<MqttConnectionDisplay> Connections { get; }
    public ObservableCollection<string> DiscoveredTopics { get; }
    public ObservableCollection<MqttClientDisplay> Clients { get; }
    public ObservableCollection<Tag> Tags { get; }
    public ObservableCollection<string> SubscribedTopics => _subscribedTopics;
    public ObservableCollection<MqttCacheField> CacheFields => _cacheFields;
    public ObservableCollection<MqttMessageLog> MessageLog => _messageLog;
    public ObservableCollection<MqttConnectionLog> ConnectionLog => _connectionLog;

    public ICommand RefreshCommand { get; }
    public ICommand PublishCommand { get; }
    public ICommand SubscribeCommand { get; }
    public ICommand UnsubscribeCommand { get; }
    public ICommand CreateTagCommand { get; }
    public ICommand CreateAllTagsCommand { get; }
    public ICommand DeleteTagCommand { get; }
    public ICommand ClearMessageLogCommand { get; }
    public ICommand ClearConnectionLogCommand { get; }

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
        set
        {
            if (SetProperty(ref _tagFilter, value))
            {
                RefreshCacheFields();
            }
        }
    }

    // Diagnostics properties
    public string ConnectionStatus
    {
        get => _connectionStatus;
        set => SetProperty(ref _connectionStatus, value);
    }

    public string Broker
    {
        get => _broker;
        set => SetProperty(ref _broker, value);
    }

    public int BrokerPort
    {
        get => _brokerPort;
        set => SetProperty(ref _brokerPort, value);
    }

    public string ClientId
    {
        get => _clientId;
        set => SetProperty(ref _clientId, value);
    }

    public int Uptime
    {
        get => _uptime;
        set => SetProperty(ref _uptime, value);
    }

    public int MessageCount
    {
        get => _messageCount;
        set => SetProperty(ref _messageCount, value);
    }

    public double MessagesPerSecond
    {
        get => _messagesPerSecond;
        set => SetProperty(ref _messagesPerSecond, value);
    }

    public int RetryCount
    {
        get => _retryCount;
        set => SetProperty(ref _retryCount, value);
    }

    public string FormattedUptime => FormatUptime(_uptime);

    public string StatusColor => ConnectionStatus switch
    {
        "CONNECTED" => "#22C55E",
        "DEGRADED" => "#F59E0B",
        _ => "#EF4444"
    };

    public string StatusText => ConnectionStatus switch
    {
        "CONNECTED" => "Conectado",
        "DEGRADED" => "Modo Degradado",
        _ => "Desconectado"
    };

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
                LoadTagsAsync(),
                LoadDiagnosticsAsync(connectionId)
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

    private async Task LoadDiagnosticsAsync(int connectionId)
    {
        try
        {
            var result = await _configService.GetMqttDiagnosticsAsync(connectionId);
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                return;
            }

            ConnectionStatus = result.Status ?? "DISCONNECTED";
            Broker = result.Broker ?? "-";
            BrokerPort = result.Port ?? 1883;
            ClientId = result.ClientId ?? "-";
            Uptime = result.Uptime ?? 0;
            MessageCount = result.MessageCount ?? 0;
            MessagesPerSecond = result.MessagesPerSecond ?? 0;
            RetryCount = result.RetryCount ?? 0;

            _subscribedTopics.Clear();
            if (result.SubscribedTopics != null)
            {
                foreach (var topic in result.SubscribedTopics)
                {
                    _subscribedTopics.Add(topic);
                }
            }

            ConnectionLog.Clear();
            if (result.ConnectionLog != null)
            {
                foreach (var log in result.ConnectionLog)
                {
                    ConnectionLog.Add(new MqttConnectionLog
                    {
                        Timestamp = log.Timestamp,
                        Event = log.Event,
                        Message = log.Message,
                        Success = log.Success
                    });
                }
            }

            ProcessValuesCache(result.ValuesCache);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar diagnósticos: {ex.Message}";
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

    private void ProcessValuesCache(Dictionary<string, object>? valuesCache)
    {
        _cacheFields.Clear();
        if (valuesCache == null) return;

        foreach (var kvp in valuesCache)
        {
            var topic = kvp.Key;
            var value = kvp.Value;

            if (value != null && value is JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in jsonElement.EnumerateObject())
                    {
                        var fieldValue = GetJsonValue(property.Value);
                        var fieldName = property.Name;
                        var address = $"{topic}::{fieldName}";
                        var tagName = SanitizeTagName($"{topic}_{fieldName}");
                        var dataType = InferDataType(fieldValue);
                        var exists = TagExistsForAddress(address);

                        _cacheFields.Add(new MqttCacheField
                        {
                            Key = address,
                            Topic = topic,
                            Field = fieldName,
                            Label = fieldName,
                            Value = fieldValue,
                            TagName = tagName,
                            Address = address,
                            DataType = dataType,
                            Exists = exists
                        });
                    }
                }
                else
                {
                    var fieldValue = GetJsonValue(jsonElement);
                    var address = topic;
                    var tagName = SanitizeTagName(topic);
                    var dataType = InferDataType(fieldValue);
                    var exists = TagExistsForAddress(address);

                    _cacheFields.Add(new MqttCacheField
                    {
                        Key = address,
                        Topic = topic,
                        Field = "",
                        Label = "payload",
                        Value = fieldValue,
                        TagName = tagName,
                        Address = address,
                        DataType = dataType,
                        Exists = exists
                    });
                }
            }
            else
            {
                var address = topic;
                var tagName = SanitizeTagName(topic);
                var dataType = InferDataType(value);
                var exists = TagExistsForAddress(address);

                _cacheFields.Add(new MqttCacheField
                {
                    Key = address,
                    Topic = topic,
                    Field = "",
                    Label = "payload",
                    Value = value?.ToString() ?? "",
                    TagName = tagName,
                    Address = address,
                    DataType = dataType,
                    Exists = exists
                });
            }
        }

        RefreshCacheFields();
    }

    private void RefreshCacheFields()
    {
    }

    private object? GetJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }

    private string InferDataType(object? value)
    {
        if (value == null) return "String";
        if (value is bool) return "Bool";
        if (value is int || value is long) return "Int32";
        if (value is double || value is float) return "Double";
        if (value is string str)
        {
            if (bool.TryParse(str, out _)) return "Bool";
            if (int.TryParse(str, out _)) return "Int32";
            if (double.TryParse(str, out _)) return "Double";
        }
        return "String";
    }

    private string SanitizeTagName(string value)
    {
        return System.Text.RegularExpressions.Regex.Replace(value, "[^a-zA-Z0-9_]", "_")
            .Replace("_+", "_")
            .Trim('_');
    }

    private bool TagExistsForAddress(string address)
    {
        return Tags.Any(t => t.Address?.Equals(address, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private string FormatUptime(int seconds)
    {
        var hours = seconds / 3600;
        var minutes = (seconds % 3600) / 60;
        var secs = seconds % 60;
        return $"{hours}h {minutes}m {secs}s";
    }

    private async Task ExecuteCreateTag(MqttCacheField? field)
    {
        if (field == null || _selectedConnectionId == null) return;
        await CreateTagAsync(field.TagName, field.DataType, field.Address);
    }

    private async Task ExecuteCreateAllTags()
    {
        if (_selectedConnectionId == null) return;
        foreach (var field in _cacheFields.Where(f => !f.Exists))
        {
            await CreateTagAsync(field.TagName, field.DataType, field.Address);
        }
    }

    private async Task ExecuteDeleteTag(Tag? tag)
    {
        if (tag == null) return;
        if (int.TryParse(tag.Id, out var tagId))
        {
            await DeleteTagAsync(tagId);
        }
    }
}

public sealed class MqttCacheField : ObservableObject
{
    private string _tagName = string.Empty;

    public string Key { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public string Field { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public object? Value { get; init; }
    public string TagName
    {
        get => _tagName;
        set => SetProperty(ref _tagName, value);
    }
    public string Address { get; init; } = string.Empty;
    public string DataType { get; init; } = string.Empty;
    public bool Exists { get; init; }

    public string FormattedValue => Value?.ToString() ?? "";
}

public sealed class MqttMessageLog : ObservableObject
{
    private string _tagName = string.Empty;

    public string Topic { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public string Timestamp { get; init; } = string.Empty;
    public string Machine { get; init; } = string.Empty;
    public string Group { get; init; } = string.Empty;
    public string Tag { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string TagName
    {
        get => _tagName;
        set => SetProperty(ref _tagName, value);
    }
}

public sealed class MqttConnectionLog
{
    public string Timestamp { get; init; } = string.Empty;
    public string Event { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool Success { get; init; }
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
