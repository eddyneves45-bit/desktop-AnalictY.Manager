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
    private static readonly Uri OpcUaUpdateEndpoint = new(ApiBaseUri, "/api/config/opcua");
    private static readonly Uri OpcUaDeleteEndpoint = new(ApiBaseUri, "/api/config/opcua/{id}");
    private static readonly Uri OpcUaTestEndpoint = new(ApiBaseUri, "/api/config/opcua/{id}/test");
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
    private static readonly Uri RuntimeStateEndpoint = new(ApiBaseUri, "/api/runtime/state");
    private static readonly Uri MachineFoldersEndpoint = new(ApiBaseUri, "/api/machine-folders");
    private static readonly Uri MachinesEndpoint = new(ApiBaseUri, "/api/machines");
    private static readonly Uri WeintekEndpoint = new(ApiBaseUri, "/api/config/weintek");
    private static readonly Uri WeintekBrowserEndpoint = new(ApiBaseUri, "/api/config/weintek/browser");
    private static readonly Uri WeintekTokenEndpoint = new(ApiBaseUri, "/api/config/weintek/token");
    private static readonly Uri AlertsEndpoint = new(ApiBaseUri, "/api/alerts");
    private static readonly Uri AlertRulesEndpoint = new(ApiBaseUri, "/api/alert-rules");
    private static readonly Uri AlertsRetentionEndpoint = new(ApiBaseUri, "/api/alerts/retention");
    private static readonly Uri UsersEndpoint = new(ApiBaseUri, "/api/users");
    private static readonly Uri AuditLogsEndpoint = new(ApiBaseUri, "/api/audit/logs");
    private static readonly Uri RecentLogsEndpoint = new(ApiBaseUri, "/api/logs/recent");
    private static readonly Uri DowntimesEndpoint = new(ApiBaseUri, "/api/downtimes");
    private static readonly Uri DowntimeReasonsCatalogEndpoint = new(ApiBaseUri, "/api/downtime-reasons/catalog");
    private static readonly Uri DowntimeRetentionEndpoint = new(ApiBaseUri, "/api/downtimes/retention");
    private static readonly Uri ShiftsEndpoint = new(ApiBaseUri, "/api/config/shifts");
    private static readonly Uri ShiftsUpdateEndpoint = new(ApiBaseUri, "/api/config/shifts");
    private static readonly Uri ShiftsDeleteEndpoint = new(ApiBaseUri, "/api/config/shifts/{id}");
    private static readonly Uri DashboardConfigsEndpoint = new(ApiBaseUri, "/api/dashboard/configs");
    private static readonly Uri DashboardConfigDeleteEndpoint = new(ApiBaseUri, "/api/dashboard/configs/{id}");
    private static readonly Uri ProductionDiagnosticsEndpoint = new(ApiBaseUri, "/api/diagnostics/production");
    private static readonly Uri SystemTimezoneEndpoint = new(ApiBaseUri, "/api/config/system/timezone");
    private static readonly Uri SimulatorMachinesEndpoint = new(ApiBaseUri, "/api/simulator/machines");
    private static readonly Uri SimulatorMachineEndpoint = new(ApiBaseUri, "/api/simulator/machines/{id}");
    private static readonly Uri SimulatorPublishEndpoint = new(ApiBaseUri, "/api/simulator/machines/{id}/publish");
    private static readonly Uri SimulatorStartEndpoint = new(ApiBaseUri, "/api/simulator/machines/{id}/start");
    private static readonly Uri SimulatorStopEndpoint = new(ApiBaseUri, "/api/simulator/machines/{id}/stop");
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
    private static readonly Uri FtpExportEndpoint = new(ApiBaseUri, "/api/config/ftp-export");
    private static readonly Uri FtpTestEndpoint = new(ApiBaseUri, "/api/config/ftp-export/test-request");
    private static readonly Uri FtpSendNowEndpoint = new(ApiBaseUri, "/api/config/ftp-export/send-now");
    private static readonly Uri MqttCertUploadEndpoint = new(ApiBaseUri, "/api/config/mqtt/certificates/upload");
    private static readonly Uri TagDeleteEndpoint = new(ApiBaseUri, "/api/config/tags");
    private static readonly Uri DatabaseBrowserConnectionsEndpoint = new(ApiBaseUri, "/api/database-browser/connections");

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
            return new OpcUaConnectionsResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new OpcUaConnectionsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<OpcUaBrowseResult> BrowseOpcUaAsync(int connectionId, string nodeId = "", CancellationToken cancellationToken = default)
    {
        try
        {
            var nodeIdParam = string.IsNullOrWhiteSpace(nodeId) ? "" : $"&node_id={Uri.EscapeDataString(nodeId)}";
            var endpoint = new Uri(ApiBaseUri, $"/api/config/opcua/browse?connection_id={connectionId}{nodeIdParam}");
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new OpcUaBrowseResult { Error = await ReadErrorMessageAsync(response, cancellationToken) };
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
            return new OpcUaBrowseResult { Error = "Servidor nÃ£o disponÃ­vel" };
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
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("ConexÃ£o OPC UA estabelecida com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> CreateOpcUaAsync(OpcUaConnectionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(ToOpcUaPayload(null, request), JsonOptions);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, OpcUaUpdateEndpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("ConexÃ£o OPC UA criada com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateOpcUaAsync(string id, OpcUaConnectionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(ToOpcUaPayload(ParseNullableInt(id), request), JsonOptions);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, OpcUaUpdateEndpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("ConexÃ£o OPC UA atualizada com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteOpcUaAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/config/opcua/{id}");
            using var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("ConexÃ£o OPC UA excluÃ­da com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> TestOpcUaAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/config/opcua/{id}/test");
            using var response = await _httpClient.PostAsync(endpoint, null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("Teste de conexÃ£o OPC UA realizado com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> TestOpcUaEndpointAsync(string endpointUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(endpointUrl))
        {
            return OperationResult.CreateFailed("Endpoint OPC UA nao informado.");
        }

        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/drivers/opcua/status?endpointUrl={Uri.EscapeDataString(endpointUrl)}");
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("Teste de conexao OPC UA realizado com sucesso");
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
            return new MqttConnectionsResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new MqttConnectionsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<MqttTopicsResult> GetMqttTopicsAsync(int connectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/config/mqtt/cache/topics?connection_id={connectionId}");
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
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
            return new MqttTopicsResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new MqttTopicsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<MqttClientsResult> GetMqttClientsAsync(int connectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/config/mqtt/clients?connection_id={connectionId}");
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
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
            return new MqttClientsResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new MqttClientsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }


    public async Task<MqttDiagnosticsResult> GetMqttDiagnosticsAsync(int connectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/config/mqtt/diagnostics?connection_id={connectionId}");
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new MqttDiagnosticsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            
            var result = new MqttDiagnosticsResult
            {
                Status = ReadString(document.RootElement, "status"),
                Broker = ReadString(document.RootElement, "broker"),
                Port = ReadNullableInt(document.RootElement, "port"),
                ClientId = ReadString(document.RootElement, "client_id", "clientId"),
                Uptime = ReadNullableInt(document.RootElement, "uptime"),
                MessageCount = ReadNullableInt(document.RootElement, "message_count", "messageCount"),
                MessagesPerSecond = ReadNullableDouble(document.RootElement, "messages_per_second", "messagesPerSecond"),
                RetryCount = ReadNullableInt(document.RootElement, "retry_count", "retryCount")
            };

            if (TryGetProperty(document.RootElement, "subscribed_topics", out var topicsProp) && topicsProp.ValueKind == JsonValueKind.Array)
            {
                result.SubscribedTopics = topicsProp.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList();
            }

            if (TryGetProperty(document.RootElement, "values_cache", out var cacheProp) && cacheProp.ValueKind == JsonValueKind.Object)
            {
                result.ValuesCache = new Dictionary<string, object>();
                foreach (var prop in cacheProp.EnumerateObject())
                {
                    result.ValuesCache[prop.Name] = ParseJsonValue(prop.Value);
                }
            }

            if (TryGetProperty(document.RootElement, "connection_log", out var logProp) && logProp.ValueKind == JsonValueKind.Array)
            {
                result.ConnectionLog = logProp.EnumerateArray().Select(e => new MqttConnectionLogEntry
                {
                    Timestamp = ReadString(e, "timestamp") ?? "",
                    Event = ReadString(e, "event") ?? "",
                    Message = ReadString(e, "message") ?? "",
                    Success = ReadBool(e, "success")
                }).ToList();
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return new MqttDiagnosticsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new MqttDiagnosticsResult { Error = "Servidor não disponível" };
        }
        catch (JsonException ex)
        {
            return new MqttDiagnosticsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }    public async Task<OperationResult> PublishMqttAsync(int connectionId, string topic, string payload, int qos, bool retain, CancellationToken cancellationToken = default)
    {
        try
        {
            var payloadObj = new
            {
                connection_id = connectionId,
                topic,
                payload,
                qos,
                retain
            };

            var json = JsonSerializer.Serialize(payloadObj, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(MqttPublishEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("Mensagem publicada com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> SubscribeMqttAsync(int connectionId, string topic, CancellationToken cancellationToken = default)
    {
        try
        {
            var payloadObj = new
            {
                connection_id = connectionId,
                topic
            };

            var json = JsonSerializer.Serialize(payloadObj, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(MqttSubscribeEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("TÃ³pico inscrito com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> UnsubscribeMqttAsync(int connectionId, string topic, CancellationToken cancellationToken = default)
    {
        try
        {
            var payloadObj = new
            {
                connection_id = connectionId,
                topic
            };

            var json = JsonSerializer.Serialize(payloadObj, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(MqttUnsubscribeEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("TÃ³pico removido com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteTagAsync(int tagId, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/config/tags/{tagId}");
            using var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("TAG excluÃ­da com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> CreateMqttAsync(MqttConnectionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(ToMqttPayload(null, request), JsonOptions);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, MqttUpdateEndpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("ConexÃ£o MQTT criada com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateMqttAsync(string id, MqttConnectionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(ToMqttPayload(ParseNullableInt(id), request), JsonOptions);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, MqttUpdateEndpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("ConexÃ£o MQTT atualizada com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteMqttAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/config/mqtt/{id}");
            using var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("ConexÃ£o MQTT excluÃ­da com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> TestMqttAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/config/mqtt/{id}/test");
            using var response = await _httpClient.PostAsync(endpoint, null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("Teste de conexÃ£o MQTT realizado com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
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
            return new MysqlConnectionsResult { Error = "Servidor nÃ£o disponÃ­vel" };
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
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("Teste de conexÃ£o MySQL realizado com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> TestMysqlAsync(MysqlConnectionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(ToMysqlPayload(null, request), JsonOptions);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, MysqlTestEndpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            var provider = string.IsNullOrWhiteSpace(request.Provider) ? "MySQL" : request.Provider;
            return OperationResult.CreateSuccess($"Teste de conexao {provider} realizado com sucesso");
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
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("MySQL definido como primÃ¡rio com sucesso");
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

    public async Task<OperationResult> CreateMysqlAsync(MysqlConnectionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(ToMysqlPayload(null, request), JsonOptions);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, MysqlUpdateEndpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("ConexÃ£o MySQL criada com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateMysqlAsync(string id, MysqlConnectionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(ToMysqlPayload(ParseNullableInt(id), request), JsonOptions);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, MysqlUpdateEndpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("ConexÃ£o MySQL atualizada com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteMysqlAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/config/mysql/{id}");
            using var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("ConexÃ£o MySQL excluÃ­da com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> TestMysqlByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/config/mysql/{id}/test");
            using var response = await _httpClient.PostAsync(endpoint, null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("Teste de conexÃ£o MySQL realizado com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    // FTP/SFTP
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
            var element = document.RootElement;

            return new FtpExportResult
            {
                Enabled = ReadBool(element, "enabled"),
                Name = ReadString(element, "name") ?? "Exportacao FTP/SFTP",
                Protocol = ReadString(element, "protocol") ?? "SFTP",
                Host = ReadString(element, "host") ?? string.Empty,
                Port = ReadString(element, "port") ?? string.Empty,
                Username = ReadString(element, "username") ?? string.Empty,
                PasswordConfigured = ReadBool(element, "password_configured"),
                PrivateKeyPath = ReadString(element, "private_key_path") ?? string.Empty,
                Directory = ReadString(element, "destination_path", "directory") ?? string.Empty,
                Frequency = ReadString(element, "frequency") ?? "manual",
                DataType = ReadString(element, "data_type") ?? "production",
                FileFormat = ReadString(element, "file_format") ?? "CSV"
            };
        }
        catch (OperationCanceledException)
        {
            return new FtpExportResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new FtpExportResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new FtpExportResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<OperationResult> UpdateFtpExportAsync(FtpExportRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(request, JsonOptions);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, FtpExportEndpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("ConfiguraÃ§Ã£o FTP/SFTP salva com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> TestFtpExportAsync(FtpExportRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(request, JsonOptions);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, FtpTestEndpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("Teste de conexÃ£o FTP/SFTP realizado com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> SendFtpNowAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsync(FtpSendNowEndpoint, null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("Envio FTP/SFTP iniciado com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    // MQTT Certificate Upload
    public async Task<OperationResult> UploadMqttCertificateAsync(byte[] fileBytes, string kind, CancellationToken cancellationToken = default)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(fileBytes), "file", "cert.pem");
            content.Add(new StringContent(kind), "kind");

            using var response = await _httpClient.PostAsync(MqttCertUploadEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var path = ReadString(document.RootElement, "path") ?? string.Empty;
            return OperationResult.CreateSuccess($"Certificado importado: {path}");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
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
            return new TagsResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new TagsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<OperationResult> CreateTagAsync(TagRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                tag_name = request.TagName,
                data_type = request.DataType,
                driver_type = request.DriverType,
                address = request.Address,
                opcua_connection_id = request.OpcUaConnectionId,
                mqtt_connection_id = request.MqttConnectionId,
                folder_id = request.FolderId,
                poll_interval_ms = request.PollIntervalMs,
                is_active = request.IsActive,
                persistence_mode = request.PersistenceMode
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(TagsEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("TAG criada com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateTagAsync(int tagId, TagRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                tag_name = request.TagName,
                data_type = request.DataType,
                driver_type = request.DriverType,
                address = request.Address,
                opcua_connection_id = request.OpcUaConnectionId,
                mqtt_connection_id = request.MqttConnectionId,
                folder_id = request.FolderId,
                poll_interval_ms = request.PollIntervalMs,
                is_active = request.IsActive,
                persistence_mode = request.PersistenceMode
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var endpoint = new Uri(ApiBaseUri, $"/api/config/tags/{tagId}");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = content
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("TAG atualizada com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<List<TagRuntimeState>> GetTagRuntimeStatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(RuntimeStateEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new List<TagRuntimeState>();
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return ReadArray(document.RootElement, "tags", "states", "data", "items", "results")
                .Select(ParseTagRuntimeState)
                .ToList();
        }
        catch
        {
            return new List<TagRuntimeState>();
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
                ? document.RootElement.EnumerateArray().Select(ParseMachineFolder).ToList()
                : new List<MachineFolder>();

            return new MachineFoldersResult { Folders = folders };
        }
        catch (OperationCanceledException)
        {
            return new MachineFoldersResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new MachineFoldersResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new MachineFoldersResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    private MachineFolder ParseMachineFolder(JsonElement element)
    {
        var id = TryGetProperty(element, "id", out var idProp) && idProp.ValueKind == JsonValueKind.Number
            ? idProp.GetInt32()
            : 0;

        var name = TryGetProperty(element, "name", out var nameProp)
            ? ReadString(nameProp) ?? string.Empty
            : string.Empty;

        var parentFolderId = ReadNullableInt(element, "parent_folder_id", "parentFolderId");

        var isSector = TryGetProperty(element, "is_sector", out var sectorProp) && sectorProp.ValueKind == JsonValueKind.True
            ? true
            : false;

        return new MachineFolder
        {
            Id = id,
            Name = name,
            ParentFolderId = parentFolderId,
            IsSector = isSector
        };
    }

    // Machines
    public async Task<MachinesResult> GetMachinesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(MachinesEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new MachinesResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var machines = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray().Select(ParseMachine).ToList()
                : new List<Machine>();

            return new MachinesResult { Machines = machines };
        }
        catch (Exception ex)
        {
            return new MachinesResult { Error = ex.Message };
        }
    }

    private Machine ParseMachine(JsonElement element)
    {
        return new Machine
        {
            Id = ReadString(element, "id") ?? string.Empty,
            Name = ReadString(element, "name") ?? string.Empty,
            Code = ReadString(element, "code") ?? string.Empty,
            CostCenter = ReadString(element, "cost_center", "costCenter") ?? string.Empty,
            Location = ReadString(element, "location") ?? string.Empty,
            FolderId = ReadString(element, "folder_id", "folderId") ?? string.Empty,
            IsActive = ReadBoolDefaultTrue(element, "is_active", "isActive")
        };
    }

    // Machines API Actions
    public async Task<OperationResult> CreateMachineAsync(MachineRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                name = request.Name,
                code = request.Code,
                cost_center = request.CostCenter,
                location = request.Location,
                folder_id = request.FolderId,
                is_active = request.IsActive
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(MachinesEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("MÃ¡quina criada com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateMachineAsync(int machineId, MachineRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                name = request.Name,
                code = request.Code,
                cost_center = request.CostCenter,
                location = request.Location,
                folder_id = request.FolderId,
                is_active = request.IsActive
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var endpoint = new Uri(ApiBaseUri, $"/api/machines/{machineId}");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = content
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("MÃ¡quina atualizada com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteMachineAsync(int machineId, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/machines/{machineId}");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("MÃ¡quina excluÃ­da com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> CreateMachineFolderAsync(MachineFolderRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                name = request.Name,
                parent_folder_id = request.ParentFolderId,
                is_sector = request.IsSector
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(MachineFoldersEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("Pasta criada com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateMachineFolderAsync(int folderId, MachineFolderRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                name = request.Name,
                parent_folder_id = request.ParentFolderId,
                is_sector = request.IsSector
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var endpoint = new Uri(ApiBaseUri, $"/api/machine-folders/{folderId}");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = content
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("Pasta atualizada com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteMachineFolderAsync(int folderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/machine-folders/{folderId}");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("Pasta excluÃ­da com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
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
            return new ShiftsResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new ShiftsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<OperationResult> CreateShiftAsync(ShiftRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(request, JsonOptions);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ShiftsUpdateEndpoint)
            {
                Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json")
            };
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }
            return OperationResult.CreateSuccess("Turno criado com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateShiftAsync(string id, ShiftRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(request, JsonOptions);
            var endpoint = new Uri(ApiBaseUri, $"/api/config/shifts/{id}");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json")
            };
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }
            return OperationResult.CreateSuccess("Turno atualizado com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteShiftAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/config/shifts/{id}");
            using var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }
            return OperationResult.CreateSuccess("Turno excluÃ­do com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    // Dashboards
    public async Task<DashboardConfigsResult> GetDashboardConfigsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(DashboardConfigsEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new DashboardConfigsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var configs = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray().Select(ParseDashboardConfig).ToList()
                : new List<DashboardConfig>();

            return new DashboardConfigsResult { Configs = configs };
        }
        catch (OperationCanceledException)
        {
            return new DashboardConfigsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new DashboardConfigsResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new DashboardConfigsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<OperationResult> SaveDashboardConfigAsync(DashboardConfigRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(request, JsonOptions);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, DashboardConfigsEndpoint)
            {
                Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json")
            };
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }
            return OperationResult.CreateSuccess("Dashboard salvo com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteDashboardConfigAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/dashboard/configs/{id}");
            using var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }
            return OperationResult.CreateSuccess("Dashboard excluÃ­do com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    // Production Diagnostics
    public async Task<DiagnosticResult> GetProductionDiagnosticsAsync(string? machineId, string from, string to, CancellationToken cancellationToken = default)
    {
        try
        {
            var builder = new UriBuilder(ProductionDiagnosticsEndpoint);
            var query = System.Web.HttpUtility.ParseQueryString(builder.Query);
            if (!string.IsNullOrWhiteSpace(machineId))
            {
                query["machine_id"] = machineId;
            }
            query["from"] = from;
            query["to"] = to;
            builder.Query = query.ToString();

            using var response = await _httpClient.GetAsync(builder.Uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new DiagnosticResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var snapshot = ParseDiagnosticSnapshot(document.RootElement);
            return new DiagnosticResult { Snapshot = snapshot };
        }
        catch (OperationCanceledException)
        {
            return new DiagnosticResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new DiagnosticResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new DiagnosticResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<SystemTimezoneResult> GetSystemTimezoneAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(SystemTimezoneEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new SystemTimezoneResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return new SystemTimezoneResult
            {
                TimeZoneId = ReadString(document.RootElement, "timeZoneId", "time_zone_id") ?? "America/Sao_Paulo",
                Label = ReadString(document.RootElement, "label") ?? "Brasil - BrasÃ­lia (GMT-3)"
            };
        }
        catch (OperationCanceledException)
        {
            return new SystemTimezoneResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new SystemTimezoneResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new SystemTimezoneResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    // Simulator
    public async Task<VirtualMachinesResult> GetVirtualMachinesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(SimulatorMachinesEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new VirtualMachinesResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var machines = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray().Select(ParseVirtualMachineSummary).ToList()
                : new List<VirtualMachineSummary>();

            return new VirtualMachinesResult { Machines = machines };
        }
        catch (OperationCanceledException)
        {
            return new VirtualMachinesResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new VirtualMachinesResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new VirtualMachinesResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<OperationResult> CreateVirtualMachineAsync(VirtualMachineRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(request, JsonOptions);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, SimulatorMachinesEndpoint)
            {
                Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json")
            };
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }
            return OperationResult.CreateSuccess("MÃ¡quina virtual criada com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<VirtualConsoleResult> GetVirtualConsoleAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/simulator/machines/{id}");
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new VirtualConsoleResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var console = ParseVirtualConsole(document.RootElement);
            return new VirtualConsoleResult { Console = console };
        }
        catch (OperationCanceledException)
        {
            return new VirtualConsoleResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new VirtualConsoleResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new VirtualConsoleResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<OperationResult> PublishVirtualMachineAsync(int id, VirtualMachinePublishRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/simulator/machines/{id}/publish");
            var payload = JsonSerializer.Serialize(request, JsonOptions);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json")
            };
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }
            return OperationResult.CreateSuccess("Valores publicados com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> StartVirtualMachineAsync(int id, VirtualMachineStartRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/simulator/machines/{id}/start");
            var payload = JsonSerializer.Serialize(request, JsonOptions);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json")
            };
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }
            return OperationResult.CreateSuccess("SimulaÃ§Ã£o iniciada com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    public async Task<OperationResult> StopVirtualMachineAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/simulator/machines/{id}/stop");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }
            return OperationResult.CreateSuccess("SimulaÃ§Ã£o parada com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    // Weintek
    public async Task<WeintekConfig> GetWeintekConfigAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(WeintekEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new WeintekConfig();
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return ParseWeintekConfig(document.RootElement);
        }
        catch
        {
            return new WeintekConfig();
        }
    }

    public async Task<OperationResult> UpdateWeintekConfigAsync(WeintekConfig config, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                name = config.Name,
                gateway = config.Gateway,
                fhdx_ip = config.FhdxIp,
                endpoint_path = config.EndpointPath,
                enabled = config.Enabled,
                enforce_source_ip = config.EnforceSourceIp,
                gateway_token_required = config.GatewayTokenRequired
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using var response = await _httpClient.PutAsync(WeintekEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("ConfiguraÃ§Ã£o Weintek salva com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<WeintekBrowserResult> GetWeintekBrowserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(WeintekBrowserEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new WeintekBrowserResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var tags = TryGetProperty(document.RootElement, "tags", out var tagsProp) && tagsProp.ValueKind == JsonValueKind.Array
                ? tagsProp.EnumerateArray().Select(ParseDiscoveredTag).ToList()
                : new List<DiscoveredTag>();

            return new WeintekBrowserResult { Tags = tags };
        }
        catch (OperationCanceledException)
        {
            return new WeintekBrowserResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new WeintekBrowserResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new WeintekBrowserResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<OperationResult> GenerateWeintekTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsync(WeintekTokenEndpoint, null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("Token gerado com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> RevokeWeintekTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, WeintekTokenEndpoint);
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("Token revogado com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> CreateWeintekTagAsync(string tagName, string address, string dataType, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                tag_name = tagName,
                address = address,
                data_type = dataType,
                persistence_mode = "telemetry"
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var endpoint = new Uri(WeintekEndpoint, "/tags");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("TAG criada com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    private WeintekConfig ParseWeintekConfig(JsonElement element)
    {
        return new WeintekConfig
        {
            Name = TryGetProperty(element, "name", out var nameProp) ? ReadString(nameProp) ?? string.Empty : string.Empty,
            Gateway = TryGetProperty(element, "gateway", out var gatewayProp) ? ReadString(gatewayProp) ?? string.Empty : string.Empty,
            FhdxIp = TryGetProperty(element, "fhdx_ip", out var fhdxProp) ? ReadString(fhdxProp) ?? string.Empty : string.Empty,
            EndpointPath = TryGetProperty(element, "endpoint_path", out var pathProp) ? ReadString(pathProp) ?? string.Empty : string.Empty,
            Enabled = TryGetProperty(element, "enabled", out var enabledProp) && enabledProp.ValueKind == JsonValueKind.True,
            EnforceSourceIp = TryGetProperty(element, "enforce_source_ip", out var enforceProp) && enforceProp.ValueKind == JsonValueKind.True,
            GatewayTokenRequired = TryGetProperty(element, "gateway_token_required", out var tokenReqProp) && tokenReqProp.ValueKind == JsonValueKind.True,
            TokenConfigured = TryGetProperty(element, "token_configured", out var tokenConfigProp) && tokenConfigProp.ValueKind == JsonValueKind.True,
            TokenPrefix = TryGetProperty(element, "token_prefix", out var tokenPrefixProp) ? ReadString(tokenPrefixProp) : null,
            TokenCreatedAt = TryGetProperty(element, "token_created_at", out var tokenCreatedProp) ? ReadString(tokenCreatedProp) : null,
            LastAccessAt = TryGetProperty(element, "last_access_at", out var lastAccessProp) ? ReadString(lastAccessProp) : null,
            LastSourceIp = TryGetProperty(element, "last_source_ip", out var lastIpProp) ? ReadString(lastIpProp) : null
        };
    }

    private DiscoveredTag ParseDiscoveredTag(JsonElement element)
    {
        return new DiscoveredTag
        {
            Gateway = TryGetProperty(element, "gateway", out var gatewayProp) ? ReadString(gatewayProp) ?? string.Empty : string.Empty,
            CostCenter = TryGetProperty(element, "cost_center", out var costProp) ? ReadString(costProp) ?? string.Empty : string.Empty,
            Machine = TryGetProperty(element, "machine", out var machineProp) ? ReadString(machineProp) ?? string.Empty : string.Empty,
            Tag = TryGetProperty(element, "tag", out var tagProp) ? ReadString(tagProp) ?? string.Empty : string.Empty,
            Address = TryGetProperty(element, "address", out var addressProp) ? ReadString(addressProp) ?? string.Empty : string.Empty,
            Value = TryGetProperty(element, "value", out var valueProp) ? valueProp : null,
            DataType = TryGetProperty(element, "data_type", out var dataTypeProp) ? ReadString(dataTypeProp) ?? string.Empty : string.Empty,
            FirstSeen = TryGetProperty(element, "first_seen", out var firstSeenProp) ? ReadString(firstSeenProp) ?? string.Empty : string.Empty,
            LastSeen = TryGetProperty(element, "last_seen", out var lastSeenProp) ? ReadString(lastSeenProp) ?? string.Empty : string.Empty,
            SourceIp = TryGetProperty(element, "source_ip", out var sourceIpProp) ? ReadString(sourceIpProp) ?? string.Empty : string.Empty,
            Created = TryGetProperty(element, "created", out var createdProp) && createdProp.ValueKind == JsonValueKind.True
        };
    }

    // Alerts
    public async Task<AlertsResult> GetAlertsAsync(int limit = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri($"{AlertsEndpoint}?limit={limit}");
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new AlertsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var alerts = TryGetProperty(document.RootElement, "alerts", out var alertsProp) && alertsProp.ValueKind == JsonValueKind.Array
                ? alertsProp.EnumerateArray().Select(ParseAlertItem).ToList()
                : new List<AlertItem>();

            return new AlertsResult { Alerts = alerts };
        }
        catch (OperationCanceledException)
        {
            return new AlertsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new AlertsResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new AlertsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<AlertRulesResult> GetAlertRulesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(AlertRulesEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new AlertRulesResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var rules = TryGetProperty(document.RootElement, "rules", out var rulesProp) && rulesProp.ValueKind == JsonValueKind.Array
                ? rulesProp.EnumerateArray().Select(ParseAlertRule).ToList()
                : new List<AlertRule>();

            return new AlertRulesResult { Rules = rules };
        }
        catch (OperationCanceledException)
        {
            return new AlertRulesResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new AlertRulesResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new AlertRulesResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<OperationResult> CreateAlertRuleAsync(AlertRuleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                name = request.Name,
                tag_config_id = request.TagConfigId,
                @operator = request.Operator,
                limit_value = request.LimitValue,
                severity = request.Severity,
                message = request.Message,
                telegram_connection_id = request.TelegramConnectionId,
                telegram_recipient_ids = request.TelegramRecipientIds,
                is_active = request.IsActive
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(AlertRulesEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("Regra de alerta criada com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateAlertRuleAsync(int ruleId, AlertRuleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                name = request.Name,
                tag_config_id = request.TagConfigId,
                @operator = request.Operator,
                limit_value = request.LimitValue,
                severity = request.Severity,
                message = request.Message,
                telegram_connection_id = request.TelegramConnectionId,
                telegram_recipient_ids = request.TelegramRecipientIds,
                is_active = request.IsActive
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var endpoint = new Uri(ApiBaseUri, $"/api/alert-rules/{ruleId}");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = content
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("Regra de alerta atualizada com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteAlertRuleAsync(int ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/alert-rules/{ruleId}");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("Regra de alerta excluÃ­da com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> AcknowledgeAlertAsync(int alertId, string acknowledgedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/alerts/{alertId}/acknowledge?acknowledged_by={acknowledgedBy}");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("Alerta reconhecido com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateAlertRetentionAsync(int retentionDays, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new { retention_days = retentionDays };
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using var response = await _httpClient.PutAsync(AlertsRetentionEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("RetenÃ§Ã£o de alertas atualizada com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    private AlertItem ParseAlertItem(JsonElement element)
    {
        return new AlertItem
        {
            Id = TryGetProperty(element, "id", out var idProp) && idProp.ValueKind == JsonValueKind.Number ? idProp.GetInt32() : 0,
            AlertType = TryGetProperty(element, "alertType", out var alertTypeProp) ? ReadString(alertTypeProp) ?? string.Empty : string.Empty,
            Severity = TryGetProperty(element, "severity", out var severityProp) ? ReadString(severityProp) ?? string.Empty : string.Empty,
            Title = TryGetProperty(element, "title", out var titleProp) ? ReadString(titleProp) ?? string.Empty : string.Empty,
            Message = TryGetProperty(element, "message", out var messageProp) ? ReadString(messageProp) ?? string.Empty : string.Empty,
            MachineId = TryGetProperty(element, "machineId", out var machineIdProp) ? ReadString(machineIdProp) : null,
            Metadata = TryGetProperty(element, "metadata", out var metadataProp) ? ReadString(metadataProp) : null,
            IsAcknowledged = TryGetProperty(element, "isAcknowledged", out var ackProp) && ackProp.ValueKind == JsonValueKind.True,
            AcknowledgedBy = TryGetProperty(element, "acknowledgedBy", out var ackByProp) ? ReadString(ackByProp) : null,
            AcknowledgedAt = TryGetProperty(element, "acknowledgedAt", out var ackAtProp) ? ReadString(ackAtProp) : null,
            CreatedAt = TryGetProperty(element, "createdAt", out var createdAtProp) ? ReadString(createdAtProp) ?? string.Empty : string.Empty
        };
    }

    private AlertRule ParseAlertRule(JsonElement element)
    {
        return new AlertRule
        {
            Id = TryGetProperty(element, "id", out var idProp) && idProp.ValueKind == JsonValueKind.Number ? idProp.GetInt32() : 0,
            Name = TryGetProperty(element, "name", out var nameProp) ? ReadString(nameProp) ?? string.Empty : string.Empty,
            TagConfigId = TryGetProperty(element, "tagConfigId", out var tagConfigIdProp) && tagConfigIdProp.ValueKind == JsonValueKind.Number ? tagConfigIdProp.GetInt32() : 0,
            TagName = TryGetProperty(element, "tagName", out var tagNameProp) ? ReadString(tagNameProp) : null,
            Operator = TryGetProperty(element, "operator", out var operatorProp) ? ReadString(operatorProp) ?? ">" : ">",
            LimitValue = TryGetProperty(element, "limitValue", out var limitProp) && limitProp.ValueKind == JsonValueKind.Number ? limitProp.GetDouble() : 0,
            Severity = TryGetProperty(element, "severity", out var severityProp) ? ReadString(severityProp) ?? "medium" : "medium",
            Message = TryGetProperty(element, "message", out var messageProp) ? ReadString(messageProp) ?? string.Empty : string.Empty,
            TelegramConnectionId = TryGetProperty(element, "telegramConnectionId", out var telegramConnProp) && telegramConnProp.ValueKind == JsonValueKind.Number ? telegramConnProp.GetInt32() as int? : null,
            TelegramRecipientIds = TryGetProperty(element, "telegramRecipientIds", out var recipProp) && recipProp.ValueKind == JsonValueKind.Array
                ? recipProp.EnumerateArray().Select(x => x.ValueKind == JsonValueKind.Number ? x.GetInt32() : 0).ToList()
                : new List<int>(),
            IsActive = TryGetProperty(element, "isActive", out var activeProp) && activeProp.ValueKind == JsonValueKind.True
        };
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
            return new TelegramStatusResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new TelegramStatusResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<TelegramConnectionsResult> GetTelegramConnectionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(TelegramConnectionsEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new TelegramConnectionsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var connections = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray().Select(ParseTelegramConnection).ToList()
                : new List<TelegramConnection>();

            return new TelegramConnectionsResult { Connections = connections };
        }
        catch (OperationCanceledException)
        {
            return new TelegramConnectionsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new TelegramConnectionsResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new TelegramConnectionsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<TelegramRecipientsResult> GetTelegramRecipientsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(TelegramRecipientsEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new TelegramRecipientsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var recipients = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray().Select(ParseTelegramRecipient).ToList()
                : new List<TelegramRecipient>();

            return new TelegramRecipientsResult { Recipients = recipients };
        }
        catch (OperationCanceledException)
        {
            return new TelegramRecipientsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new TelegramRecipientsResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new TelegramRecipientsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<OperationResult> CreateTelegramConnectionAsync(TelegramConnectionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                name = request.Name,
                bot_token = request.BotToken,
                default_chat_id = request.DefaultChatId,
                cooldown_minutes = request.CooldownMinutes,
                is_active = request.IsActive
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(TelegramConnectionsCreateEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("ConexÃ£o Telegram criada com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateTelegramConnectionAsync(int connectionId, TelegramConnectionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                name = request.Name,
                bot_token = request.BotToken,
                default_chat_id = request.DefaultChatId,
                cooldown_minutes = request.CooldownMinutes,
                is_active = request.IsActive
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var endpoint = new Uri(TelegramConnectionsUpdateEndpoint.ToString().Replace("{id}", connectionId.ToString()));
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = content
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("ConexÃ£o Telegram atualizada com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteTelegramConnectionAsync(int connectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(TelegramConnectionsDeleteEndpoint.ToString().Replace("{id}", connectionId.ToString()));
            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("ConexÃ£o Telegram excluÃ­da com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> CreateTelegramRecipientAsync(TelegramRecipientRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                connection_id = request.ConnectionId,
                name = request.Name,
                chat_id = request.ChatId,
                destination_type = request.DestinationType,
                is_active = request.IsActive
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(TelegramRecipientsCreateEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("DestinatÃ¡rio Telegram criado com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateTelegramRecipientAsync(int recipientId, TelegramRecipientRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                connection_id = request.ConnectionId,
                name = request.Name,
                chat_id = request.ChatId,
                destination_type = request.DestinationType,
                is_active = request.IsActive
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var endpoint = new Uri(TelegramRecipientsUpdateEndpoint.ToString().Replace("{id}", recipientId.ToString()));
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = content
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("DestinatÃ¡rio Telegram atualizado com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteTelegramRecipientAsync(int recipientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(TelegramRecipientsDeleteEndpoint.ToString().Replace("{id}", recipientId.ToString()));
            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("DestinatÃ¡rio Telegram excluÃ­do com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> TestTelegramAsync(int? connectionId = null, int? recipientId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                connection_id = connectionId,
                recipient_id = recipientId,
                message = "Teste de notificaÃ§Ã£o iioT AnalictY"
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(TelegramTestEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("Teste Telegram enviado com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    private TelegramConnection ParseTelegramConnection(JsonElement element)
    {
        return new TelegramConnection
        {
            Id = TryGetProperty(element, "id", out var idProp) && idProp.ValueKind == JsonValueKind.Number ? idProp.GetInt32() : 0,
            Name = TryGetProperty(element, "name", out var nameProp) ? ReadString(nameProp) ?? string.Empty : string.Empty,
            BotTokenConfigured = TryGetProperty(element, "bot_token_configured", out var tokenConfigProp) && tokenConfigProp.ValueKind == JsonValueKind.True,
            BotTokenMasked = TryGetProperty(element, "bot_token_masked", out var tokenMaskedProp) ? ReadString(tokenMaskedProp) : null,
            DefaultChatId = TryGetProperty(element, "default_chat_id", out var chatIdProp) ? ReadString(chatIdProp) : null,
            IsActive = TryGetProperty(element, "is_active", out var activeProp) && activeProp.ValueKind == JsonValueKind.True,
            CooldownMinutes = TryGetProperty(element, "cooldown_minutes", out var cooldownProp) && cooldownProp.ValueKind == JsonValueKind.Number ? cooldownProp.GetInt32() : 15,
            Recipients = TryGetProperty(element, "recipients", out var recipProp) && recipProp.ValueKind == JsonValueKind.Number ? recipProp.GetInt32() : 0,
            ActiveRecipients = TryGetProperty(element, "active_recipients", out var activeRecipProp) && activeRecipProp.ValueKind == JsonValueKind.Number ? activeRecipProp.GetInt32() : 0
        };
    }

    private TelegramRecipient ParseTelegramRecipient(JsonElement element)
    {
        return new TelegramRecipient
        {
            Id = TryGetProperty(element, "id", out var idProp) && idProp.ValueKind == JsonValueKind.Number ? idProp.GetInt32() : 0,
            ConnectionId = TryGetProperty(element, "connection_id", out var connIdProp) && connIdProp.ValueKind == JsonValueKind.Number ? connIdProp.GetInt32() : 0,
            Name = TryGetProperty(element, "name", out var nameProp) ? ReadString(nameProp) ?? string.Empty : string.Empty,
            ChatId = TryGetProperty(element, "chat_id", out var chatIdProp) ? ReadString(chatIdProp) ?? string.Empty : string.Empty,
            DestinationType = TryGetProperty(element, "destination_type", out var destTypeProp) ? ReadString(destTypeProp) ?? string.Empty : string.Empty,
            IsActive = TryGetProperty(element, "is_active", out var activeProp) && activeProp.ValueKind == JsonValueKind.True
        };
    }

    // Users
    public async Task<UsersResult> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(UsersEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new UsersResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var users = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray().Select(ParseUser).ToList()
                : new List<User>();

            return new UsersResult { Users = users };
        }
        catch (OperationCanceledException)
        {
            return new UsersResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new UsersResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new UsersResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<OperationResult> CreateUserAsync(UserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                username = request.Username,
                email = request.Email,
                password = request.Password,
                role = request.Role,
                is_active = request.IsActive,
                permissions = request.Role == "custom" ? request.Permissions : new List<string>(),
                mfa_required = request.MfaRequired
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(UsersEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("UsuÃ¡rio criado com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateUserAsync(int userId, UserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                email = request.Email,
                password = string.IsNullOrWhiteSpace(request.Password) ? null : request.Password,
                role = request.Role,
                is_active = request.IsActive,
                permissions = request.Role == "custom" ? request.Permissions : new List<string>(),
                mfa_required = request.MfaRequired
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var endpoint = new Uri(ApiBaseUri, $"/api/users/{userId}");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = content
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("UsuÃ¡rio atualizado com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/users/{userId}");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("UsuÃ¡rio excluÃ­do com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    private User ParseUser(JsonElement element)
    {
        return new User
        {
            Id = TryGetProperty(element, "id", out var idProp) && idProp.ValueKind == JsonValueKind.Number ? idProp.GetInt32() : 0,
            Username = TryGetProperty(element, "username", out var usernameProp) ? ReadString(usernameProp) ?? string.Empty : string.Empty,
            Email = TryGetProperty(element, "email", out var emailProp) ? ReadString(emailProp) ?? string.Empty : string.Empty,
            Role = TryGetProperty(element, "role", out var roleProp) ? ReadString(roleProp) ?? "user" : "user",
            Permissions = TryGetProperty(element, "permissions", out var permProp) && permProp.ValueKind == JsonValueKind.Array
                ? permProp.EnumerateArray().Select(x => x.ValueKind == JsonValueKind.String ? x.GetString() ?? string.Empty : string.Empty).ToList()
                : new List<string>(),
            IsActive = TryGetProperty(element, "is_active", out var activeProp) && activeProp.ValueKind == JsonValueKind.True,
            MfaRequired = TryGetProperty(element, "mfa_required", out var mfaReqProp) && mfaReqProp.ValueKind == JsonValueKind.True,
            MfaEnabled = TryGetProperty(element, "mfa_enabled", out var mfaEnabledProp) && mfaEnabledProp.ValueKind == JsonValueKind.True
        };
    }

    // Audit
    public async Task<AuditLogsResult> GetAuditLogsAsync(int take = 200, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri($"{AuditLogsEndpoint}?take={take}");
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new AuditLogsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var logs = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray().Select(ParseAuditLog).ToList()
                : new List<AuditLog>();

            return new AuditLogsResult { Logs = logs };
        }
        catch (OperationCanceledException)
        {
            return new AuditLogsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new AuditLogsResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new AuditLogsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    private AuditLog ParseAuditLog(JsonElement element)
    {
        return new AuditLog
        {
            Id = TryGetProperty(element, "id", out var idProp) && idProp.ValueKind == JsonValueKind.Number ? idProp.GetInt32() : 0,
            Username = TryGetProperty(element, "username", out var usernameProp) ? ReadString(usernameProp) ?? string.Empty : string.Empty,
            Role = TryGetProperty(element, "role", out var roleProp) ? ReadString(roleProp) ?? string.Empty : string.Empty,
            Action = TryGetProperty(element, "action", out var actionProp) ? ReadString(actionProp) ?? string.Empty : string.Empty,
            Path = TryGetProperty(element, "path", out var pathProp) ? ReadString(pathProp) ?? string.Empty : string.Empty,
            EntityType = TryGetProperty(element, "entityType", out var entityTypeProp) ? ReadString(entityTypeProp) : null,
            EntityId = TryGetProperty(element, "entityId", out var entityIdProp) ? ReadString(entityIdProp) : null,
            StatusCode = TryGetProperty(element, "statusCode", out var statusCodeProp) && statusCodeProp.ValueKind == JsonValueKind.Number ? statusCodeProp.GetInt32() : 0,
            IpAddress = TryGetProperty(element, "ipAddress", out var ipAddressProp) ? ReadString(ipAddressProp) : null,
            CreatedAt = TryGetProperty(element, "createdAt", out var createdAtProp) ? ReadString(createdAtProp) ?? string.Empty : string.Empty
        };
    }

    // Logs
    public async Task<RecentLogsResult> GetRecentLogsAsync(int take = 300, string? level = null, string? search = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = $"take={take}";
            if (!string.IsNullOrWhiteSpace(level) && level != "Todos")
            {
                queryParams += $"&level={level}";
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                queryParams += $"&search={search}";
            }

            var endpoint = new Uri($"{RecentLogsEndpoint}?{queryParams}");
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new RecentLogsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var logs = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray().Select(ParseRecentLog).ToList()
                : new List<RecentLog>();

            return new RecentLogsResult { Logs = logs };
        }
        catch (OperationCanceledException)
        {
            return new RecentLogsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new RecentLogsResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new RecentLogsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    private RecentLog ParseRecentLog(JsonElement element)
    {
        return new RecentLog
        {
            Timestamp = TryGetProperty(element, "timestamp", out var timestampProp) ? ReadString(timestampProp) ?? string.Empty : string.Empty,
            Level = TryGetProperty(element, "level", out var levelProp) ? ReadString(levelProp) ?? string.Empty : string.Empty,
            Category = TryGetProperty(element, "category", out var categoryProp) ? ReadString(categoryProp) ?? string.Empty : string.Empty,
            Message = TryGetProperty(element, "message", out var messageProp) ? ReadString(messageProp) ?? string.Empty : string.Empty,
            Exception = TryGetProperty(element, "exception", out var exceptionProp) ? ReadString(exceptionProp) : null
        };
    }

    // Downtime Reasons
    public async Task<DowntimesResult> GetDowntimesAsync(string? machineId = null, string? from = null, string? to = null, int limit = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = $"limit={limit}";
            if (!string.IsNullOrWhiteSpace(machineId)) queryParams += $"&machine_id={machineId}";
            if (!string.IsNullOrWhiteSpace(from)) queryParams += $"&from={from}";
            if (!string.IsNullOrWhiteSpace(to)) queryParams += $"&to={to}";

            var endpoint = new Uri($"{DowntimesEndpoint}?{queryParams}");
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new DowntimesResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var downtimes = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray().Select(ParseDowntime).ToList()
                : new List<Downtime>();

            return new DowntimesResult { Downtimes = downtimes };
        }
        catch (OperationCanceledException)
        {
            return new DowntimesResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new DowntimesResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new DowntimesResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<DowntimeReasonsResult> GetDowntimeReasonsCatalogAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(DowntimeReasonsCatalogEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new DowntimeReasonsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var reasons = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray().Select(ParseDowntimeReason).ToList()
                : new List<DowntimeReason>();

            return new DowntimeReasonsResult { Reasons = reasons };
        }
        catch (OperationCanceledException)
        {
            return new DowntimeReasonsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new DowntimeReasonsResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new DowntimeReasonsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<OperationResult> ClassifyDowntimeAsync(int downtimeId, int? reasonId, string? informedReason, string? observation, string acknowledgedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                reason_id = reasonId,
                motivo_informado = informedReason,
                observacao = observation,
                reconhecida_por = acknowledgedBy
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var endpoint = new Uri(ApiBaseUri, $"/api/downtimes/{downtimeId}/classify");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("Parada classificada com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateDowntimeRetentionAsync(int retentionDays, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new { retention_days = retentionDays };
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using var response = await _httpClient.PutAsync(DowntimeRetentionEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.CreateFailed($"Erro HTTP {(int)response.StatusCode}: {errorText}");
            }

            return OperationResult.CreateSuccess("RetenÃ§Ã£o de paradas atualizada com sucesso");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.CreateFailed("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return OperationResult.CreateFailed("Servidor nÃ£o disponÃ­vel");
        }
        catch (JsonException ex)
        {
            return OperationResult.CreateFailed($"Erro ao processar resposta: {ex.Message}");
        }
    }

    private Downtime ParseDowntime(JsonElement element)
    {
        return new Downtime
        {
            Id = TryGetProperty(element, "id", out var idProp) && idProp.ValueKind == JsonValueKind.Number ? idProp.GetInt32() : 0,
            DowntimeId = TryGetProperty(element, "downtime_id", out var downtimeIdProp) && downtimeIdProp.ValueKind == JsonValueKind.Number ? downtimeIdProp.GetInt32() as int? : null,
            MachineId = TryGetProperty(element, "machine_id", out var machineIdProp) ? ReadString(machineIdProp) ?? string.Empty : string.Empty,
            MachineName = TryGetProperty(element, "machine_name", out var machineNameProp) ? ReadString(machineNameProp) : null,
            MachineCode = TryGetProperty(element, "machine_code", out var machineCodeProp) ? ReadString(machineCodeProp) : null,
            StartTime = TryGetProperty(element, "start_time", out var startTimeProp) ? ReadString(startTimeProp) ?? string.Empty : string.Empty,
            EndTime = TryGetProperty(element, "end_time", out var endTimeProp) ? ReadString(endTimeProp) : null,
            DurationSeconds = TryGetProperty(element, "duration_seconds", out var durationProp) && durationProp.ValueKind == JsonValueKind.Number ? durationProp.GetInt32() as int? : null,
            StatusOrigin = TryGetProperty(element, "status_origin", out var statusOriginProp) && statusOriginProp.ValueKind == JsonValueKind.Number ? statusOriginProp.GetInt32() as int? : null,
            StatusOriginDescription = TryGetProperty(element, "status_origin_description", out var statusOriginDescProp) ? ReadString(statusOriginDescProp) : null,
            CanClassify = TryGetProperty(element, "can_classify", out var canClassifyProp) && canClassifyProp.ValueKind == JsonValueKind.True,
            ReasonId = TryGetProperty(element, "reason_id", out var reasonIdProp) && reasonIdProp.ValueKind == JsonValueKind.Number ? reasonIdProp.GetInt32() as int? : null,
            Reason = TryGetProperty(element, "reason", out var reasonProp) ? ReadString(reasonProp) : null,
            Category = TryGetProperty(element, "category", out var categoryProp) ? ReadString(categoryProp) : null,
            InformedReason = TryGetProperty(element, "informed_reason", out var informedReasonProp) ? ReadString(informedReasonProp) : null,
            Observation = TryGetProperty(element, "observation", out var observationProp) ? ReadString(observationProp) : null,
            AcknowledgedBy = TryGetProperty(element, "acknowledged_by", out var ackByProp) ? ReadString(ackByProp) : null
        };
    }

    private DowntimeReason ParseDowntimeReason(JsonElement element)
    {
        return new DowntimeReason
        {
            Id = TryGetProperty(element, "id", out var idProp) && idProp.ValueKind == JsonValueKind.Number ? idProp.GetInt32() : 0,
            Code = TryGetProperty(element, "code", out var codeProp) ? ReadString(codeProp) ?? string.Empty : string.Empty,
            Description = TryGetProperty(element, "description", out var descProp) ? ReadString(descProp) ?? string.Empty : string.Empty,
            Category = TryGetProperty(element, "category", out var categoryProp) ? ReadString(categoryProp) : null
        };
    }

    public async Task<OperationResult> SaveFtpExportAsync(FtpExportRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(ToFtpPayload(request), JsonOptions);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, FtpExportEndpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return OperationResult.CreateSuccess("Configuracao FTP/SFTP atualizada com sucesso");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailed($"Erro: {ex.Message}");
        }
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var fallback = $"Erro HTTP {(int)response.StatusCode}";
        try
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(body))
            {
                return fallback;
            }

            using var document = JsonDocument.Parse(body);
            return ReadString(document.RootElement, "error", "message", "detail", "title") ??
                ReadNestedString(document.RootElement, "errors") ??
                body;
        }
        catch
        {
            return fallback;
        }
    }

    private static string? ReadNestedString(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }

        if (property.ValueKind == JsonValueKind.Object)
        {
            foreach (var item in property.EnumerateObject())
            {
                if (item.Value.ValueKind == JsonValueKind.Array)
                {
                    var messages = item.Value.EnumerateArray()
                        .Select(value => value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString())
                        .Where(value => !string.IsNullOrWhiteSpace(value));
                    return $"{item.Name}: {string.Join("; ", messages)}";
                }
            }
        }

        return property.ToString();
    }

    // Helpers
    private static object ToOpcUaPayload(int? id, OpcUaConnectionRequest request)
    {
        var securityPolicy = string.IsNullOrWhiteSpace(request.SecurityPolicy) ? "None" : request.SecurityPolicy.Trim();
        var securityMode = string.IsNullOrWhiteSpace(request.SecurityMode) ? "None" : request.SecurityMode.Trim();
        var usesSecurity = !string.Equals(securityPolicy, "None", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(securityMode, "None", StringComparison.OrdinalIgnoreCase);

        return new
        {
            id,
            name = string.IsNullOrWhiteSpace(request.Name) ? "OPC UA" : request.Name.Trim(),
            server_url = request.ServerUrl.Trim(),
            security_policy = securityPolicy,
            security_mode = securityMode,
            username = request.Username ?? string.Empty,
            password = request.Password ?? string.Empty,
            certificate_path = usesSecurity ? request.CertificatePath ?? string.Empty : string.Empty,
            private_key_path = usesSecurity ? request.PrivateKeyPath ?? string.Empty : string.Empty,
            update_interval = ParsePort(request.UpdateInterval, 1000),
            is_active = request.IsActive
        };
    }

    private static object ToMqttPayload(int? id, MqttConnectionRequest request)
    {
        return new
        {
            id,
            name = request.Name,
            client_id = request.ClientId ?? string.Empty,
            broker_host = request.BrokerHost,
            broker_port = ParsePort(request.BrokerPort, 1883),
            username = request.Username ?? string.Empty,
            password = request.Password ?? string.Empty,
            tls_enabled = request.TlsEnabled,
            ca_cert_path = request.CaCertPath ?? string.Empty,
            client_cert_path = request.ClientCertPath ?? string.Empty,
            client_key_path = request.ClientKeyPath ?? string.Empty,
            topics = request.Topics ?? string.Empty,
            qos = ParsePort(request.Qos, 0),
            is_active = request.IsActive
        };
    }

    private static object ToMysqlPayload(int? id, MysqlConnectionRequest request)
    {
        return new
        {
            id,
            name = request.Name,
            host = request.Host,
            port = ParsePort(request.Port, 3306),
            user = request.Username ?? string.Empty,
            password = request.Password ?? string.Empty,
            database = request.Database,
            pool_size = ParsePort(request.PoolSize, 10),
            is_active = request.IsActive,
            is_primary = request.IsPrimary,
            is_local = request.IsLocal || string.Equals(request.Host, "localhost", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(request.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase),
            provider = string.IsNullOrWhiteSpace(request.Provider) ? "MySQL" : request.Provider
        };
    }

    private static object ToFtpPayload(FtpExportRequest request)
    {
        return new
        {
            name = request.Name,
            enabled = request.Enabled,
            protocol = string.IsNullOrWhiteSpace(request.Protocol) ? "SFTP" : request.Protocol,
            host = request.Host,
            port = ParsePort(request.Port, string.Equals(request.Protocol, "FTP", StringComparison.OrdinalIgnoreCase) ? 21 : 22),
            username = request.Username ?? string.Empty,
            password = request.Password ?? string.Empty,
            private_key_path = request.PrivateKeyPath ?? string.Empty,
            destination_path = string.IsNullOrWhiteSpace(request.Directory) ? "/" : request.Directory,
            frequency = string.IsNullOrWhiteSpace(request.Frequency) ? "manual" : request.Frequency,
            data_type = string.IsNullOrWhiteSpace(request.DataType) ? "production" : request.DataType,
            file_format = string.IsNullOrWhiteSpace(request.FileFormat) ? "CSV" : request.FileFormat
        };
    }



    private static double? ReadNullableDouble(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetProperty(element, name, out var property))
            {
                if (property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var parsed))
                {
                    return parsed;
                }

                if (property.ValueKind == JsonValueKind.String && double.TryParse(property.GetString(), out parsed))
                {
                    return parsed;
                }
            }
        }

        return null;
    }
    private static object ParseJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? "",
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Object => element.ToString(),
            JsonValueKind.Array => element.ToString(),
            _ => element.ToString()
        };
    }    private static int? ParseNullableInt(string value)
    {
        return int.TryParse(value, out var parsed) ? parsed : null;
    }

    private static int? ReadNullableInt(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (!TryGetProperty(element, name, out var value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
            {
                return number;
            }

            if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    private static int ParsePort(string value, int fallback)
    {
        return int.TryParse(value, out var parsed) ? parsed : fallback;
    }

    private static OpcUaConnection ParseOpcUaConnection(JsonElement element)
    {
        return new OpcUaConnection
        {
            Id = ReadString(element, "id") ?? string.Empty,
            Name = ReadString(element, "name") ?? string.Empty,
            ServerUrl = ReadString(element, "endpoint", "serverUrl", "server_url") ?? string.Empty,
            SecurityPolicy = ReadString(element, "securityPolicy", "security_policy") ?? "None",
            SecurityMode = ReadString(element, "securityMode", "security_mode") ?? "None",
            Username = ReadString(element, "username") ?? string.Empty,
            CertificatePath = ReadString(element, "certificatePath", "certificate_path") ?? string.Empty,
            PrivateKeyPath = ReadString(element, "privateKeyPath", "private_key_path") ?? string.Empty,
            UpdateInterval = ReadString(element, "updateInterval", "update_interval") ?? "1000",
            IsActive = ReadBoolDefaultTrue(element, "isActive", "is_active"),
            Status = ReadString(element, "status") ?? "Desconhecido"
        };
    }

    private static OpcUaNode ParseOpcUaNode(JsonElement element)
    {
        return new OpcUaNode
        {
            NodeId = ReadString(element, "nodeId", "node_id") ?? string.Empty,
            Name = ReadString(element, "name", "displayName", "display_name", "browseName", "browse_name") ?? string.Empty,
            Type = ReadString(element, "type", "nodeClass", "node_class") ?? string.Empty,
            Quality = ReadString(element, "quality") ?? "Desconhecido",
            HasChildren = ReadBool(element, "hasChildren", "has_children")
        };
    }

    private static MqttConnection ParseMqttConnection(JsonElement element)
    {
        return new MqttConnection
        {
            Id = ReadString(element, "id") ?? string.Empty,
            Name = ReadString(element, "name") ?? string.Empty,
            ClientId = ReadString(element, "clientId", "client_id") ?? string.Empty,
            BrokerHost = ReadString(element, "host", "brokerHost", "broker_host") ?? string.Empty,
            BrokerPort = ReadString(element, "port", "brokerPort", "broker_port") ?? string.Empty,
            Username = ReadString(element, "username") ?? string.Empty,
            TlsEnabled = ReadBool(element, "tlsEnabled", "tls_enabled"),
            CaCertPath = ReadString(element, "caCertPath", "ca_cert_path") ?? string.Empty,
            ClientCertPath = ReadString(element, "clientCertPath", "client_cert_path") ?? string.Empty,
            ClientKeyPath = ReadString(element, "clientKeyPath", "client_key_path") ?? string.Empty,
            Topics = ReadString(element, "topics") ?? string.Empty,
            Qos = ReadString(element, "qos") ?? "0",
            IsActive = ReadBoolDefaultTrue(element, "isActive", "is_active"),
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
            Connected = ReadBool(element, "connected") ? "Sim" : "NÃ£o",
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
            Provider = ReadString(element, "type", "provider") ?? "MySQL",
            Username = ReadString(element, "username", "user") ?? string.Empty,
            PoolSize = ReadString(element, "poolSize", "pool_size") ?? "10",
            IsActive = ReadBoolDefaultTrue(element, "isActive", "is_active"),
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
            ScanRate = ReadString(element, "scanRate", "scan_rate", "pollingInterval", "polling_interval", "pollIntervalMs", "poll_interval_ms") ?? string.Empty,
            DriverType = ReadString(element, "driverType", "driver_type", "driver") ?? string.Empty,
            FolderId = ReadNullableInt(element, "folderId", "folder_id"),
            OpcUaConnectionId = ReadNullableInt(element, "opcUaConnectionId", "opcuaConnectionId", "opcua_connection_id"),
            MqttConnectionId = ReadNullableInt(element, "mqttConnectionId", "mqtt_connection_id"),
            IsActive = ReadBoolDefaultTrue(element, "isActive", "is_active", "active", "enabled"),
            PersistenceMode = ReadString(element, "persistenceMode", "persistence_mode"),
            CurrentValue = ReadString(element, "currentValue", "current_value", "value") ?? "-",
            Quality = ReadString(element, "quality") ?? "-",
            LastTimestamp = ReadString(element, "timestamp", "sourceTimestamp", "source_timestamp", "updatedAt", "updated_at") ?? "-",
            RuntimeConnected = ReadBool(element, "connected", "runtimeConnected", "runtime_connected")
        };
    }

    private static TagRuntimeState ParseTagRuntimeState(JsonElement element)
    {
        return new TagRuntimeState
        {
            TagId = ReadString(element, "tagId", "tag_id", "id") ?? string.Empty,
            TagName = ReadString(element, "tagName", "tag_name", "name") ?? string.Empty,
            Value = ReadString(element, "value", "currentValue", "current_value") ?? "-",
            Quality = ReadString(element, "quality") ?? "-",
            Timestamp = ReadString(element, "timestamp", "sourceTimestamp", "source_timestamp") ?? "-",
            Connected = ReadBool(element, "connected")
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
                return OperationResult.CreateFailed(await ReadErrorMessageAsync(response, cancellationToken));
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
            EndTime = ReadString(element, "endTime") ?? string.Empty,
            DaysOfWeek = ReadString(element, "daysOfWeek"),
            Description = ReadString(element, "description"),
            IsActive = ReadBoolDefaultTrue(element, "isActive")
        };
    }

    private static DashboardConfig ParseDashboardConfig(JsonElement element)
    {
        var rawWidgets = element.TryGetProperty("widgets", out var widgetsProp) ? widgetsProp : default;
        var widgets = rawWidgets.ValueKind == JsonValueKind.Array
            ? rawWidgets.EnumerateArray().Select(ParseDashboardWidget).ToList()
            : new List<DashboardWidget>();

        return new DashboardConfig
        {
            Id = TryGetProperty(element, "id", out var idProp) && idProp.ValueKind == JsonValueKind.Number ? idProp.GetInt32() : null,
            Name = ReadString(element, "name") ?? string.Empty,
            MachineId = ReadString(element, "machineId", "machine_id") ?? string.Empty,
            PeriodPreset = ReadString(element, "periodPreset", "period_preset") ?? "today",
            RefreshInterval = ReadString(element, "refreshInterval", "refresh_interval") ?? "10",
            IsDefault = ReadBool(element, "isDefault", "is_default"),
            Widgets = widgets
        };
    }

    private static DashboardWidget ParseDashboardWidget(JsonElement element)
    {
        return new DashboardWidget
        {
            Id = ReadString(element, "id") ?? string.Empty,
            Type = ReadString(element, "type") ?? string.Empty,
            Metric = ReadString(element, "metric") ?? string.Empty,
            Title = ReadString(element, "title") ?? string.Empty,
            Color = ReadString(element, "color"),
            X = TryGetProperty(element, "x", out var xProp) && xProp.ValueKind == JsonValueKind.Number ? xProp.GetInt32() : null,
            Y = TryGetProperty(element, "y", out var yProp) && yProp.ValueKind == JsonValueKind.Number ? yProp.GetInt32() : null,
            W = TryGetProperty(element, "w", out var wProp) && wProp.ValueKind == JsonValueKind.Number ? wProp.GetInt32() : null,
            H = TryGetProperty(element, "h", out var hProp) && hProp.ValueKind == JsonValueKind.Number ? hProp.GetInt32() : null
        };
    }

    private static DiagnosticSnapshot ParseDiagnosticSnapshot(JsonElement element)
    {
        var machines = TryGetProperty(element, "machines", out var machinesProp) && machinesProp.ValueKind == JsonValueKind.Array
            ? machinesProp.EnumerateArray().Select(ParseMachineSimple).ToList()
            : new List<Machine>();

        var pipeline = TryGetProperty(element, "pipeline", out var pipelineProp) && pipelineProp.ValueKind == JsonValueKind.Array
            ? pipelineProp.EnumerateArray().Select(item => item.EnumerateObject().ToDictionary(p => p.Name, p => GetJsonValueStatic(p.Value) ?? string.Empty)).ToList()
            : new List<Dictionary<string, object>>();

        var queues = TryGetProperty(element, "queues", out var queuesProp) && queuesProp.ValueKind == JsonValueKind.Object
            ? queuesProp.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.TryGetInt32(out var val) ? val : 0)
            : new Dictionary<string, int>();

        return new DiagnosticSnapshot
        {
            GeneratedAt = ReadString(element, "generated_at") ?? string.Empty,
            MachineId = ReadString(element, "machine_id"),
            Machines = machines,
            Pipeline = pipeline,
            Queues = queues,
            Sqlite = ParseDiagnosticSqlite(element),
            Mysql = ParseDiagnosticMysql(element)
        };
    }

    private static Machine ParseMachineSimple(JsonElement element)
    {
        return new Machine
        {
            Id = ReadString(element, "id") ?? string.Empty,
            Name = ReadString(element, "name") ?? string.Empty,
            Code = ReadString(element, "code") ?? string.Empty,
            CostCenter = ReadString(element, "costCenter", "cost_center") ?? string.Empty,
            Location = ReadString(element, "location") ?? string.Empty
        };
    }

    private static object? GetJsonValueStatic(JsonElement element)
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

    private static DiagnosticSqlite ParseDiagnosticSqlite(JsonElement element)
    {
        var sqliteProp = TryGetProperty(element, "sqlite", out var prop) ? prop : default;
        var tags = TryGetProperty(sqliteProp, "tags", out var tagsProp) && tagsProp.ValueKind == JsonValueKind.Array
            ? tagsProp.EnumerateArray().Select(ParseDiagnosticTag).ToList()
            : new List<DiagnosticTag>();

        var pendingEnvelopes = TryGetProperty(sqliteProp, "pending_envelopes", out var envelopesProp) && envelopesProp.ValueKind == JsonValueKind.Array
            ? envelopesProp.EnumerateArray().Select(item => item.EnumerateObject().ToDictionary(p => p.Name, p => GetJsonValueStatic(p.Value) ?? string.Empty)).ToList()
            : new List<Dictionary<string, object>>();

        return new DiagnosticSqlite
        {
            Tags = tags,
            PendingEnvelopes = pendingEnvelopes,
            PendingCount = TryGetProperty(sqliteProp, "pending_count", out var pendingProp) && pendingProp.ValueKind == JsonValueKind.Number ? pendingProp.GetInt32() : 0,
            ProcessedCount = TryGetProperty(sqliteProp, "processed_count", out var processedProp) && processedProp.ValueKind == JsonValueKind.Number ? processedProp.GetInt32() : 0,
            FailedCount = TryGetProperty(sqliteProp, "failed_count", out var failedProp) && failedProp.ValueKind == JsonValueKind.Number ? failedProp.GetInt32() : 0
        };
    }

    private static DiagnosticMysql ParseDiagnosticMysql(JsonElement element)
    {
        var mysqlProp = TryGetProperty(element, "mysql", out var prop) ? prop : default;
        var totals = TryGetProperty(mysqlProp, "totals", out var totalsProp) ? ParseDiagnosticTotals(totalsProp) : null;

        var productionEvents = TryGetProperty(mysqlProp, "production_events", out var prodProp) && prodProp.ValueKind == JsonValueKind.Array
            ? prodProp.EnumerateArray().Select(item => item.EnumerateObject().ToDictionary(p => p.Name, p => GetJsonValueStatic(p.Value) ?? string.Empty)).ToList()
            : new List<Dictionary<string, object>>();

        var lossEvents = TryGetProperty(mysqlProp, "loss_events", out var lossProp) && lossProp.ValueKind == JsonValueKind.Array
            ? lossProp.EnumerateArray().Select(item => item.EnumerateObject().ToDictionary(p => p.Name, p => GetJsonValueStatic(p.Value) ?? string.Empty)).ToList()
            : new List<Dictionary<string, object>>();

        var statusEvents = TryGetProperty(mysqlProp, "status_events", out var statusProp) && statusProp.ValueKind == JsonValueKind.Array
            ? statusProp.EnumerateArray().Select(item => item.EnumerateObject().ToDictionary(p => p.Name, p => GetJsonValueStatic(p.Value) ?? string.Empty)).ToList()
            : new List<Dictionary<string, object>>();

        var downtimeEvents = TryGetProperty(mysqlProp, "downtime_events", out var downtimeProp) && downtimeProp.ValueKind == JsonValueKind.Array
            ? downtimeProp.EnumerateArray().Select(item => item.EnumerateObject().ToDictionary(p => p.Name, p => GetJsonValueStatic(p.Value) ?? string.Empty)).ToList()
            : new List<Dictionary<string, object>>();

        var hourlySummary = TryGetProperty(mysqlProp, "hourly_summary", out var hourlyProp) && hourlyProp.ValueKind == JsonValueKind.Array
            ? hourlyProp.EnumerateArray().Select(item => item.EnumerateObject().ToDictionary(p => p.Name, p => GetJsonValueStatic(p.Value) ?? string.Empty)).ToList()
            : new List<Dictionary<string, object>>();

        return new DiagnosticMysql
        {
            Available = TryGetProperty(mysqlProp, "available", out var availProp) && availProp.ValueKind == JsonValueKind.True ? true : false,
            Message = ReadString(mysqlProp, "message"),
            Totals = totals,
            ProductionEvents = productionEvents,
            LossEvents = lossEvents,
            StatusEvents = statusEvents,
            DowntimeEvents = downtimeEvents,
            HourlySummary = hourlySummary
        };
    }

    private static DiagnosticTotals ParseDiagnosticTotals(JsonElement element)
    {
        return new DiagnosticTotals
        {
            Produced = TryGetProperty(element, "produced", out var prodProp) && prodProp.ValueKind == JsonValueKind.Number ? prodProp.GetDouble() : 0,
            Losses = TryGetProperty(element, "losses", out var lossProp) && lossProp.ValueKind == JsonValueKind.Number ? lossProp.GetDouble() : 0,
            Good = TryGetProperty(element, "good", out var goodProp) && goodProp.ValueKind == JsonValueKind.Number ? goodProp.GetDouble() : 0,
            QualityPercent = TryGetProperty(element, "quality_percent", out var qualProp) && qualProp.ValueKind == JsonValueKind.Number ? qualProp.GetDouble() : 0,
            StatusEvents = TryGetProperty(element, "status_events", out var statusProp) && statusProp.ValueKind == JsonValueKind.Number ? statusProp.GetInt32() : 0,
            DowntimeEvents = TryGetProperty(element, "downtime_events", out var downtimeProp) && downtimeProp.ValueKind == JsonValueKind.Number ? downtimeProp.GetInt32() : 0
        };
    }

    private static DiagnosticTag ParseDiagnosticTag(JsonElement element)
    {
        return new DiagnosticTag
        {
            Alias = ReadString(element, "alias") ?? string.Empty,
            TagId = TryGetProperty(element, "tag_id", out var tagIdProp) && tagIdProp.ValueKind == JsonValueKind.Number ? tagIdProp.GetInt32() : 0,
            Name = ReadString(element, "name"),
            Address = ReadString(element, "address"),
            Driver = ReadString(element, "driver"),
            PersistenceMode = ReadString(element, "persistence_mode"),
            Value = ReadString(element, "value"),
            Quality = ReadString(element, "quality"),
            SourceTimestamp = ReadString(element, "source_timestamp"),
            LastPersistedAt = ReadString(element, "last_persisted_at")
        };
    }

    private static VirtualMachineSummary ParseVirtualMachineSummary(JsonElement element)
    {
        return new VirtualMachineSummary
        {
            Id = TryGetProperty(element, "id", out var idProp) && idProp.ValueKind == JsonValueKind.Number ? idProp.GetInt32() : 0,
            Name = ReadString(element, "name") ?? string.Empty,
            Code = ReadString(element, "code") ?? string.Empty,
            CostCenter = ReadString(element, "costCenter", "cost_center") ?? string.Empty,
            Location = ReadString(element, "location") ?? string.Empty,
            IsActive = ReadBool(element, "isActive", "is_active")
        };
    }

    private static VirtualConsole ParseVirtualConsole(JsonElement element)
    {
        var machineProp = TryGetProperty(element, "machine", out var mProp) ? mProp : default;
        var machine = ParseMachineSimple(machineProp);

        var tagsProp = TryGetProperty(element, "tags", out var tProp) ? tProp : default;
        var tags = tagsProp.ValueKind == JsonValueKind.Object
            ? tagsProp.EnumerateObject().ToDictionary(p => p.Name, p => ParseVirtualTag(p.Value))
            : new Dictionary<string, VirtualTag>();

        var reasonsProp = TryGetProperty(element, "reasons", out var rProp) ? rProp : default;
        var reasons = reasonsProp.ValueKind == JsonValueKind.Array
            ? reasonsProp.EnumerateArray().Select(ParseVirtualReason).ToList()
            : new List<VirtualReason>();

        var simulatorProp = TryGetProperty(element, "simulator", out var sProp) ? sProp : default;
        var simulator = ParseVirtualMachineRuntime(simulatorProp);

        return new VirtualConsole
        {
            Machine = machine,
            Tags = tags,
            Reasons = reasons,
            Simulator = simulator
        };
    }

    private static VirtualTag ParseVirtualTag(JsonElement element)
    {
        return new VirtualTag
        {
            Id = TryGetProperty(element, "id", out var idProp) && idProp.ValueKind == JsonValueKind.Number ? idProp.GetInt32() : 0,
            Name = ReadString(element, "name") ?? string.Empty,
            Address = ReadString(element, "address") ?? string.Empty
        };
    }

    private static VirtualReason ParseVirtualReason(JsonElement element)
    {
        return new VirtualReason
        {
            Code = TryGetProperty(element, "code", out var codeProp) && codeProp.ValueKind == JsonValueKind.Number ? codeProp.GetInt32() : 0,
            Description = ReadString(element, "description") ?? string.Empty,
            Category = ReadString(element, "category")
        };
    }

    private static VirtualMachineRuntime ParseVirtualMachineRuntime(JsonElement element)
    {
        return new VirtualMachineRuntime
        {
            MachineId = TryGetProperty(element, "machineId", out var idProp) && idProp.ValueKind == JsonValueKind.Number ? idProp.GetInt32() : 0,
            Status = TryGetProperty(element, "status", out var statusProp) && statusProp.ValueKind == JsonValueKind.Number ? statusProp.GetInt32() : 0,
            DowntimeReasonCode = TryGetProperty(element, "downtimeReasonCode", out var reasonProp) && reasonProp.ValueKind == JsonValueKind.Number ? reasonProp.GetInt32() : 0,
            ProductionCounter = TryGetProperty(element, "productionCounter", out var prodProp) && prodProp.ValueKind == JsonValueKind.Number ? prodProp.GetInt32() : 0,
            LossCounter = TryGetProperty(element, "lossCounter", out var lossProp) && lossProp.ValueKind == JsonValueKind.Number ? lossProp.GetInt32() : 0,
            PiecesPerMinute = TryGetProperty(element, "piecesPerMinute", out var ppmProp) && ppmProp.ValueKind == JsonValueKind.Number ? ppmProp.GetInt32() : 0,
            Running = ReadBool(element, "running")
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

    private static bool ReadBoolDefaultTrue(JsonElement element, params string[] names)
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
                JsonValueKind.String => bool.TryParse(value.GetString(), out var parsed) ? parsed : true,
                _ => true
            };
        }

        return true;
    }

    // Database Browser
    public async Task<DatabaseConnectionsResult> GetDatabaseConnectionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(DatabaseBrowserConnectionsEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new DatabaseConnectionsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var connections = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray().Select(ParseDatabaseConnection).ToList()
                : new List<DatabaseConnection>();

            return new DatabaseConnectionsResult { Connections = connections };
        }
        catch (OperationCanceledException)
        {
            return new DatabaseConnectionsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new DatabaseConnectionsResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new DatabaseConnectionsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<DatabasesResult> GetDatabasesAsync(int connectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/database-browser/connections/{connectionId}/databases");
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new DatabasesResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var databases = ReadArray(document.RootElement, "databases")
                .Select(item => new DatabaseInfo
                {
                    Name = ReadString(item, "name") ?? string.Empty,
                    IsDefault = ReadBool(item, "is_default", "isDefault")
                })
                .ToList();

            return new DatabasesResult { Databases = databases };
        }
        catch (OperationCanceledException)
        {
            return new DatabasesResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new DatabasesResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new DatabasesResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<TablesResult> GetTablesAsync(int connectionId, string database, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/database-browser/connections/{connectionId}/tables?database={Uri.EscapeDataString(database)}");
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new TablesResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var tables = ReadArray(document.RootElement, "tables")
                .Select(ParseTableInfo)
                .ToList();

            return new TablesResult { Tables = tables };
        }
        catch (OperationCanceledException)
        {
            return new TablesResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new TablesResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new TablesResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<ColumnsResult> GetColumnsAsync(int connectionId, string database, string schema, string table, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(ApiBaseUri, $"/api/database-browser/connections/{connectionId}/columns?database={Uri.EscapeDataString(database)}&schema={Uri.EscapeDataString(schema)}&table={Uri.EscapeDataString(table)}");
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new ColumnsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var columns = ReadArray(document.RootElement, "columns")
                .Select(ParseColumnInfo)
                .ToList();

            return new ColumnsResult { Columns = columns };
        }
        catch (OperationCanceledException)
        {
            return new ColumnsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new ColumnsResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new ColumnsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    public async Task<RowsResult> GetRowsAsync(int connectionId, string database, string schema, string table, int limit, int offset, string? search = null, int? machineId = null, string? machineCode = null, string? costCenter = null, string? dateFrom = null, string? dateTo = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"database={Uri.EscapeDataString(database)}",
                $"schema={Uri.EscapeDataString(schema)}",
                $"table={Uri.EscapeDataString(table)}",
                $"limit={limit}",
                $"offset={offset}"
            };

            if (!string.IsNullOrWhiteSpace(search))
                queryParams.Add($"q={Uri.EscapeDataString(search)}");
            if (machineId.HasValue)
                queryParams.Add($"machine_id={machineId.Value}");
            if (!string.IsNullOrWhiteSpace(machineCode))
                queryParams.Add($"machine_code={Uri.EscapeDataString(machineCode)}");
            if (!string.IsNullOrWhiteSpace(costCenter))
                queryParams.Add($"cost_center={Uri.EscapeDataString(costCenter)}");
            if (!string.IsNullOrWhiteSpace(dateFrom))
                queryParams.Add($"date_from={Uri.EscapeDataString(dateFrom)}");
            if (!string.IsNullOrWhiteSpace(dateTo))
                queryParams.Add($"date_to={Uri.EscapeDataString(dateTo)}");

            var endpoint = new Uri(ApiBaseUri, $"/api/database-browser/connections/{connectionId}/rows?{string.Join('&', queryParams)}");
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new RowsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var columns = ReadArray(document.RootElement, "columns")
                .Select(item => ReadString(item) ?? string.Empty)
                .ToList();

            var rows = ReadArray(document.RootElement, "rows")
                .Select(item => item.ValueKind == JsonValueKind.Object
                    ? item.EnumerateObject().ToDictionary(p => p.Name, p => GetJsonValue(p.Value) ?? string.Empty)
                    : new Dictionary<string, object>())
                .ToList();

            return new RowsResult { Columns = columns, Rows = rows };
        }
        catch (OperationCanceledException)
        {
            return new RowsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new RowsResult { Error = "Servidor nÃ£o disponÃ­vel" };
        }
        catch (JsonException ex)
        {
            return new RowsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    private DatabaseConnection ParseDatabaseConnection(JsonElement element)
    {
        return new DatabaseConnection
        {
            Id = TryGetProperty(element, "id", out var idValue) && idValue.ValueKind == JsonValueKind.Number ? idValue.GetInt32() : 0,
            Name = ReadString(element, "name") ?? string.Empty,
            Provider = ReadString(element, "provider") ?? string.Empty,
            Host = ReadString(element, "host") ?? string.Empty,
            Port = ReadString(element, "port") ?? string.Empty,
            Database = ReadString(element, "database") ?? string.Empty,
            IsActive = ReadBool(element, "is_active", "isActive"),
            IsPrimary = ReadBool(element, "is_primary", "isPrimary"),
            IsLocal = ReadBool(element, "is_local", "isLocal")
        };
    }

    private TableInfo ParseTableInfo(JsonElement element)
    {
        return new TableInfo
        {
            Schema = ReadString(element, "schema") ?? string.Empty,
            Name = ReadString(element, "name") ?? string.Empty,
            Type = ReadString(element, "type") ?? string.Empty,
            Rows = TryGetProperty(element, "rows", out var rowsValue) && rowsValue.ValueKind == JsonValueKind.Number ? rowsValue.GetInt32() : null
        };
    }

    private ColumnInfo ParseColumnInfo(JsonElement element)
    {
        return new ColumnInfo
        {
            Name = ReadString(element, "name") ?? string.Empty,
            DataType = ReadString(element, "data_type", "dataType") ?? string.Empty,
            FullType = ReadString(element, "full_type", "fullType") ?? string.Empty,
            Nullable = ReadBool(element, "nullable"),
            Key = ReadString(element, "key") ?? string.Empty,
            Ordinal = TryGetProperty(element, "ordinal", out var ordValue) && ordValue.ValueKind == JsonValueKind.Number ? ordValue.GetInt32() : 0
        };
    }

    private object? GetJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var longVal) ? longVal : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
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


