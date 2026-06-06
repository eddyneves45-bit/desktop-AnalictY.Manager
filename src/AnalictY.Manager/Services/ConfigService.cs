using System.Net.Http;
using System.Text;
using System.Text.Json;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.Services;

public sealed class ConfigService
{
    private static readonly Uri ApiBaseUri = new("http://127.0.0.1:5000");
    private static readonly Uri OpcUaAllEndpoint = new(ApiBaseUri, "/api/config/opcua/all");
    private static readonly Uri OpcUaBrowseEndpoint = new(ApiBaseUri, "/api/config/opcua/browse");
    private static readonly Uri OpcUaConnectEndpoint = new(ApiBaseUri, "/api/drivers/opcua/connect");
    private static readonly Uri MqttAllEndpoint = new(ApiBaseUri, "/api/config/mqtt/all");
    private static readonly Uri MqttTopicsEndpoint = new(ApiBaseUri, "/api/config/mqtt/cache/topics");
    private static readonly Uri MqttClientsEndpoint = new(ApiBaseUri, "/api/config/mqtt/clients");
    private static readonly Uri MqttTestEndpoint = new(ApiBaseUri, "/api/config/mqtt/{id}/test");
    private static readonly Uri MqttPublishEndpoint = new(ApiBaseUri, "/api/config/mqtt/publish");
    private static readonly Uri MqttSubscribeEndpoint = new(ApiBaseUri, "/api/config/mqtt/subscribe");
    private static readonly Uri MqttUnsubscribeEndpoint = new(ApiBaseUri, "/api/config/mqtt/unsubscribe");
    private static readonly Uri MqttUpdateEndpoint = new(ApiBaseUri, "/api/config/mqtt");
    private static readonly Uri MqttDeleteEndpoint = new(ApiBaseUri, "/api/config/mqtt/{id}");
    private static readonly Uri MysqlAllEndpoint = new(ApiBaseUri, "/api/config/mysql/all");
    private static readonly Uri MysqlTestEndpoint = new(ApiBaseUri, "/api/config/mysql/test");
    private static readonly Uri MysqlTestIdEndpoint = new(ApiBaseUri, "/api/config/mysql/{id}/test");
    private static readonly Uri MysqlUpdateEndpoint = new(ApiBaseUri, "/api/config/mysql");
    private static readonly Uri MysqlDeleteEndpoint = new(ApiBaseUri, "/api/config/mysql/{id}");
    private static readonly Uri MysqlSetPrimaryEndpoint = new(ApiBaseUri, "/api/config/mysql/{id}/set-primary");
    private static readonly Uri MysqlSetLocalEndpoint = new(ApiBaseUri, "/api/config/mysql/{id}/set-local");
    private static readonly Uri MysqlSetRemoteEndpoint = new(ApiBaseUri, "/api/config/mysql/{id}/set-remote");
    private static readonly Uri MysqlInitEndpoint = new(ApiBaseUri, "/api/config/mysql/{id}/init");
    private static readonly Uri TagsEndpoint = new(ApiBaseUri, "/api/config/tags");
    private static readonly Uri TagsUpdateEndpoint = new(ApiBaseUri, "/api/config/tags/{id}");
    private static readonly Uri TagsDeleteEndpoint = new(ApiBaseUri, "/api/config/tags/{id}");
    private static readonly Uri MachineFoldersEndpoint = new(ApiBaseUri, "/api/machine-folders");
    private static readonly Uri ShiftsEndpoint = new(ApiBaseUri, "/api/config/shifts");
    private static readonly Uri ShiftsUpdateEndpoint = new(ApiBaseUri, "/api/config/shifts");
    private static readonly Uri ShiftsDeleteEndpoint = new(ApiBaseUri, "/api/config/shifts/{id}");
    private static readonly Uri TelegramStatusEndpoint = new(ApiBaseUri, "/api/notifications/telegram/status");
    private static readonly Uri TelegramConnectionsEndpoint = new(ApiBaseUri, "/api/notifications/telegram/connections");
    private static readonly Uri TelegramConnectionsCreateEndpoint = new(ApiBaseUri, "/api/notifications/telegram/connections");
    private static readonly Uri TelegramConnectionsUpdateEndpoint = new(ApiBaseUri, "/api/notifications/telegram/connections/{id}");
    private static readonly Uri TelegramConnectionsDeleteEndpoint = new(ApiBaseUri, "/api/notifications/telegram/connections/{id}");
    private static readonly Uri TelegramRecipientsEndpoint = new(ApiBaseUri, "/api/notifications/telegram/recipients");
    private static readonly Uri TelegramRecipientsCreateEndpoint = new(ApiBaseUri, "/api/notifications/telegram/recipients");
    private static readonly Uri TelegramRecipientsUpdateEndpoint = new(ApiBaseUri, "/api/notifications/telegram/recipients/{id}");
    private static readonly Uri TelegramRecipientsDeleteEndpoint = new(ApiBaseUri, "/api/notifications/telegram/recipients/{id}");
    private static readonly Uri TelegramTestEndpoint = new(ApiBaseUri, "/api/notifications/telegram/test");
    private static readonly Uri TelegramCandidatesEndpoint = new(ApiBaseUri, "/api/notifications/telegram/candidates");
    private static readonly Uri WeintekEndpoint = new(ApiBaseUri, "/api/config/weintek");
    private static readonly Uri WeintekUpdateEndpoint = new(ApiBaseUri, "/api/config/weintek");
    private static readonly Uri WeintekBrowserEndpoint = new(ApiBaseUri, "/api/config/weintek/browser");
    private static readonly Uri WeintekTokenEndpoint = new(ApiBaseUri, "/api/config/weintek/token");
    private static readonly Uri WeintekTokenDeleteEndpoint = new(ApiBaseUri, "/api/config/weintek/token");
    private static readonly Uri WeintekTagsEndpoint = new(ApiBaseUri, "/api/config/weintek/tags");
    private static readonly Uri FtpExportEndpoint = new(ApiBaseUri, "/api/config/ftp-export");

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public ConfigService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // OPC UA
    public async Task<OpcUaConnectionsResult> GetOpcUaConnectionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(OpcUaAllEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new OpcUaConnectionsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var connections = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray().Select(ParseOpcUaConnection).ToList()
                : new List<OpcUaConnection>();

            return new OpcUaConnectionsResult { Connections = connections };
        }
        catch (OperationCanceledException)
        {
            return new OpcUaConnectionsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new OpcUaConnectionsResult { Error = "Servidor não disponível" };
        }
        catch (JsonException ex)
        {
            return new OpcUaConnectionsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<OpcUaBrowseResult> BrowseOpcUaAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = string.IsNullOrWhiteSpace(connectionId)
                ? OpcUaBrowseEndpoint
                : new Uri(ApiBaseUri, $"/api/config/opcua/browse?connection_id={Uri.EscapeDataString(connectionId)}");
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new OpcUaBrowseResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var nodes = ReadArray(document.RootElement, "nodes", "items", "data")
                .Select(ParseOpcUaNode)
                .ToList();

            return new OpcUaBrowseResult { Nodes = nodes };
        }
        catch (OperationCanceledException)
        {
            return new OpcUaBrowseResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new OpcUaBrowseResult { Error = "Servidor não disponível" };
        }
        catch (JsonException ex)
        {
            return new OpcUaBrowseResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<OperationResult> ConnectOpcUaAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new { connectionId }, JsonOptions);
            using var request = new HttpRequestMessage(HttpMethod.Post, OpcUaConnectEndpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}");
            }

            return OperationResult.CreateSuccess("Conexão OPC UA estabelecida com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    // MQTT
    public async Task<MqttConnectionsResult> GetMqttConnectionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(MqttAllEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new MqttConnectionsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var connections = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray().Select(ParseMqttConnection).ToList()
                : new List<MqttConnection>();

            return new MqttConnectionsResult { Connections = connections };
        }
        catch (OperationCanceledException)
        {
            return new MqttConnectionsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new MqttConnectionsResult { Error = "Servidor não disponível" };
        }
        catch (JsonException ex)
        {
            return new MqttConnectionsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<MqttTopicsResult> GetMqttTopicsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(MqttTopicsEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new MqttTopicsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var topics = ReadArray(document.RootElement, "topics")
                .Select(ParseMqttTopic)
                .Where(topic => !string.IsNullOrWhiteSpace(topic.Topic))
                .ToList();

            return new MqttTopicsResult { Topics = topics };
        }
        catch (OperationCanceledException)
        {
            return new MqttTopicsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new MqttTopicsResult { Error = "Servidor não disponível" };
        }
        catch (JsonException ex)
        {
            return new MqttTopicsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<MqttClientsResult> GetMqttClientsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(MqttClientsEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new MqttClientsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var clients = ReadArray(document.RootElement, "clients")
                .Select(ParseMqttClient)
                .Where(client => !string.IsNullOrWhiteSpace(client.ClientId))
                .ToList();

            return new MqttClientsResult { Clients = clients };
        }
        catch (OperationCanceledException)
        {
            return new MqttClientsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new MqttClientsResult { Error = "Servidor não disponível" };
        }
        catch (JsonException ex)
        {
            return new MqttClientsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    // MySQL
    public async Task<MysqlConnectionsResult> GetMysqlConnectionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(MysqlAllEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new MysqlConnectionsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var connections = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray().Select(ParseMysqlConnection).ToList()
                : new List<MysqlConnection>();

            return new MysqlConnectionsResult { Connections = connections };
        }
        catch (OperationCanceledException)
        {
            return new MysqlConnectionsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new MysqlConnectionsResult { Error = "Servidor não disponível" };
        }
        catch (JsonException ex)
        {
            return new MysqlConnectionsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<OperationResult> TestMysqlAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsync(MysqlTestEndpoint, null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}");
            }

            return OperationResult.CreateSuccess("Teste de conexão MySQL realizado com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> SetMysqlPrimaryAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/config/mysql/{id}/set-primary");
            using var response = await _httpClient.PostAsync(endpoint, null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}");
            }

            return OperationResult.CreateSuccess("MySQL definido como primário com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> SetMysqlLocalAsync(string id, CancellationToken cancellationToken = default)
    {
        return await PostMysqlActionAsync(id, "set-local", "MySQL definido como local com sucesso", cancellationToken);
    }

    public async Task<OperationResult> SetMysqlRemoteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await PostMysqlActionAsync(id, "set-remote", "MySQL definido como remoto com sucesso", cancellationToken);
    }

    public async Task<OperationResult> InitMysqlAsync(string id, CancellationToken cancellationToken = default)
    {
        return await PostMysqlActionAsync(id, "init", "MySQL inicializado com sucesso", cancellationToken);
    }

    // Tags
    public async Task<TagsResult> GetTagsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(TagsEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new TagsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            List<Tag> tags;

            if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                tags = document.RootElement.EnumerateArray().Select(ParseTag).ToList();
            }
            else if (document.RootElement.ValueKind == JsonValueKind.Object && document.RootElement.TryGetProperty("tags", out var tagsProp))
            {
                tags = tagsProp.ValueKind == JsonValueKind.Array
                    ? tagsProp.EnumerateArray().Select(ParseTag).ToList()
                    : new List<Tag>();
            }
            else
            {
                tags = new List<Tag>();
            }

            return new TagsResult { Tags = tags };
        }
        catch (OperationCanceledException)
        {
            return new TagsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new TagsResult { Error = "Servidor não disponível" };
        }
        catch (JsonException ex)
        {
            return new TagsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<MachineFoldersResult> GetMachineFoldersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(MachineFoldersEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new MachineFoldersResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var folders = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray().Select(item => ReadString(item) ?? "-").ToList()
                : new List<string>();

            return new MachineFoldersResult { Folders = folders };
        }
        catch (OperationCanceledException)
        {
            return new MachineFoldersResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new MachineFoldersResult { Error = "Servidor não disponível" };
        }
        catch (JsonException ex)
        {
            return new MachineFoldersResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    // Shifts
    public async Task<ShiftsResult> GetShiftsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(ShiftsEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new ShiftsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var shifts = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray().Select(ParseShift).ToList()
                : new List<Shift>();

            return new ShiftsResult { Shifts = shifts };
        }
        catch (OperationCanceledException)
        {
            return new ShiftsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new ShiftsResult { Error = "Servidor não disponível" };
        }
        catch (JsonException ex)
        {
            return new ShiftsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    // Telegram
    public async Task<TelegramStatusResult> GetTelegramStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(TelegramStatusEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new TelegramStatusResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return new TelegramStatusResult
            {
                Enabled = ReadBool(document.RootElement, "enabled"),
                BotToken = ReadString(document.RootElement, "botToken") ?? string.Empty,
                ChatId = ReadString(document.RootElement, "chatId") ?? string.Empty
            };
        }
        catch (OperationCanceledException)
        {
            return new TelegramStatusResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new TelegramStatusResult { Error = "Servidor não disponível" };
        }
        catch (JsonException ex)
        {
            return new TelegramStatusResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    // FTP Export
    public async Task<FtpExportResult> GetFtpExportAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(FtpExportEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new FtpExportResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return new FtpExportResult
            {
                Enabled = ReadBool(document.RootElement, "enabled"),
                Host = ReadString(document.RootElement, "host") ?? string.Empty,
                Port = ReadString(document.RootElement, "port") ?? string.Empty,
                Username = ReadString(document.RootElement, "username") ?? string.Empty,
                Directory = ReadString(document.RootElement, "directory", "destinationPath", "destination_path") ?? string.Empty
            };
        }
        catch (OperationCanceledException)
        {
            return new FtpExportResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new FtpExportResult { Error = "Servidor não disponível" };
        }
        catch (JsonException ex)
        {
            return new FtpExportResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    // Helpers
    private static OpcUaConnection ParseOpcUaConnection(JsonElement element)
    {
        return new OpcUaConnection
        {
            Id = ReadString(element, "id") ?? string.Empty,
            Name = ReadString(element, "name") ?? string.Empty,
            Endpoint = ReadString(element, "endpoint", "serverUrl", "server_url") ?? string.Empty,
            Status = ReadString(element, "status") ?? "Desconhecido"
        };
    }

    private static OpcUaNode ParseOpcUaNode(JsonElement element)
    {
        return new OpcUaNode
        {
            NodeId = ReadString(element, "nodeId") ?? string.Empty,
            Name = ReadString(element, "name") ?? string.Empty,
            Type = ReadString(element, "type") ?? string.Empty,
            Quality = ReadString(element, "quality") ?? "Desconhecido"
        };
    }

    private static MqttConnection ParseMqttConnection(JsonElement element)
    {
        return new MqttConnection
        {
            Id = ReadString(element, "id") ?? string.Empty,
            Name = ReadString(element, "name") ?? string.Empty,
            Host = ReadString(element, "host", "brokerHost", "broker_host") ?? string.Empty,
            Port = ReadString(element, "port", "brokerPort", "broker_port") ?? string.Empty,
            Status = ReadString(element, "status") ?? "Desconhecido"
        };
    }

    private static MqttTopic ParseMqttTopic(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            return new MqttTopic
            {
                Topic = element.GetString() ?? string.Empty,
                Qos = "0",
                Subscribers = "0"
            };
        }

        return new MqttTopic
        {
            Topic = ReadString(element, "topic", "name") ?? string.Empty,
            Qos = ReadString(element, "qos") ?? "0",
            Subscribers = ReadString(element, "subscribers", "subscriptions") ?? "0"
        };
    }

    private static MqttClient ParseMqttClient(JsonElement element)
    {
        return new MqttClient
        {
            ClientId = ReadString(element, "clientId", "client_id") ?? string.Empty,
            Ip = ReadString(element, "ip", "host", "address") ?? string.Empty,
            Connected = ReadBool(element, "connected") ? "Sim" : "Não",
            Topics = ReadString(element, "topics", "topic_count") ?? "0"
        };
    }

    private static MysqlConnection ParseMysqlConnection(JsonElement element)
    {
        var isLocal = ReadBool(element, "isLocal", "is_local");

        return new MysqlConnection
        {
            Id = ReadString(element, "id") ?? string.Empty,
            Name = ReadString(element, "name") ?? string.Empty,
            Host = ReadString(element, "host") ?? string.Empty,
            Port = ReadString(element, "port") ?? string.Empty,
            Database = ReadString(element, "database") ?? string.Empty,
            Type = ReadString(element, "type", "provider") ?? "MySQL",
            IsPrimary = ReadBool(element, "isPrimary", "is_primary"),
            IsLocal = isLocal,
            IsRemote = ReadBool(element, "isRemote", "is_remote") || !isLocal,
            Status = ReadString(element, "status") ?? "Desconhecido"
        };
    }

    private static Tag ParseTag(JsonElement element)
    {
        return new Tag
        {
            Id = ReadString(element, "id") ?? string.Empty,
            Name = ReadString(element, "name", "tagName", "tag_name") ?? string.Empty,
            Address = ReadString(element, "address", "nodeId", "node_id", "topic") ?? string.Empty,
            DataType = ReadString(element, "dataType", "data_type", "type") ?? string.Empty,
            ScanRate = ReadString(element, "scanRate", "scan_rate", "pollingInterval", "polling_interval") ?? string.Empty
        };
    }

    private async Task<OperationResult> PostMysqlActionAsync(
        string id,
        string action,
        string successMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/config/mysql/{id}/{action}");
            using var response = await _httpClient.PostAsync(endpoint, null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}");
            }

            return OperationResult.CreateSuccess(successMessage);
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    private static Shift ParseShift(JsonElement element)
    {
        return new Shift
        {
            Id = ReadString(element, "id") ?? string.Empty,
            Name = ReadString(element, "name") ?? string.Empty,
            StartTime = ReadString(element, "startTime") ?? string.Empty,
            EndTime = ReadString(element, "endTime") ?? string.Empty
        };
    }

    private static string? ReadString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (!TryGetProperty(element, name, out var value))
            {
                continue;
            }

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => null
            };
        }

        return null;
    }

    private static IEnumerable<JsonElement> ReadArray(JsonElement root, params string[] propertyNames)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            return root.EnumerateArray();
        }

        if (root.ValueKind != JsonValueKind.Object)
        {
            return Array.Empty<JsonElement>();
        }

        foreach (var propertyName in propertyNames)
        {
            if (TryGetProperty(root, propertyName, out var value) && value.ValueKind == JsonValueKind.Array)
            {
                return value.EnumerateArray();
            }
        }

        return Array.Empty<JsonElement>();
    }

    private static bool ReadBool(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (!TryGetProperty(element, name, out var value))
            {
                continue;
            }

            return value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(value.GetString(), out var parsed) && parsed,
                _ => false
            };
        }

        return false;
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            value = default;
            return false;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}
