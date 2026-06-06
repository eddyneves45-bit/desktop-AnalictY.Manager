using System.Net.Http;
using System.Text.Json;

namespace AnalictY.Manager.Services;

public sealed class AdminApiService
{
    private static readonly Uri BaseUri = new("http://127.0.0.1:5000");
    private readonly HttpClient _httpClient;

    public AdminApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AdminOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        using var root = await GetJsonAsync("/api/admin/server/overview", cancellationToken);
        var json = root.RootElement;

        return new AdminOverviewDto(
            ReadString(json, "serverStatus") ?? "Nao verificado",
            ReadString(json, "version") ?? "-",
            ReadString(json, "channel") ?? "-",
            ReadString(json, "environment") ?? "-",
            ReadString(json, "machineName") ?? Environment.MachineName,
            ReadString(json, "uptime") ?? "-",
            ReadString(json, "apiStatus") ?? "-",
            ReadString(json, "databaseStatus") ?? "-",
            ReadString(json, "runtimeStatus") ?? "-",
            ReadString(json, "mqttStatus") ?? "-",
            ReadString(json, "opcUaStatus") ?? "-",
            ReadString(json, "tagsTotal") ?? "0",
            ReadString(json, "tagsActive") ?? "0",
            ReadString(json, "tagsStale") ?? "0",
            ReadString(json, "eventsQueued") ?? "0",
            ReadString(json, "dataDirectory") ?? "-",
            ReadString(json, "checkedAt") ?? "-");
    }

    public async Task<AdminDatabaseDto> GetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        using var root = await GetJsonAsync("/api/admin/database/status", cancellationToken);
        var json = root.RootElement;

        return new AdminDatabaseDto(
            ReadString(json, "status") ?? "-",
            ReadString(json, "provider") ?? "-",
            ReadString(json, "databaseFile") ?? "-",
            ReadString(json, "size") ?? "-",
            ReadString(json, "tables") ?? "-",
            ReadString(json, "records") ?? "-",
            ReadString(json, "lastWriteAt") ?? "-");
    }

    public async Task<AdminMqttDto> GetMqttAsync(CancellationToken cancellationToken = default)
    {
        using var root = await GetJsonAsync("/api/admin/mqtt/status", cancellationToken);
        var json = root.RootElement;

        return new AdminMqttDto(
            ReadString(json, "status") ?? "-",
            ReadString(json, "clientsConnected") ?? "0",
            ReadString(json, "topics") ?? "0",
            ReadString(json, "messagesReceivedPerSecond") ?? "0",
            ReadString(json, "messagesSentPerSecond") ?? "0",
            ReadString(json, "retained") ?? "0");
    }

    public async Task<IReadOnlyList<AdminServiceDto>> GetServicesAsync(CancellationToken cancellationToken = default)
    {
        using var root = await GetJsonAsync("/api/admin/services", cancellationToken);
        if (!TryGetProperty(root.RootElement, "services", out var services) || services.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<AdminServiceDto>();
        }

        return services.EnumerateArray()
            .Select(item => new AdminServiceDto(
                ReadString(item, "name") ?? "-",
                ReadString(item, "status") ?? "-",
                ReadString(item, "uptime") ?? "-"))
            .ToArray();
    }

    public async Task<IReadOnlyList<AdminLogDto>> GetLogsAsync(CancellationToken cancellationToken = default)
    {
        using var root = await GetJsonAsync("/api/admin/logs", cancellationToken);
        if (!TryGetProperty(root.RootElement, "logs", out var logs) || logs.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<AdminLogDto>();
        }

        return logs.EnumerateArray()
            .Select(item => new AdminLogDto(
                FormatDate(ReadString(item, "dateTime") ?? "-"),
                ReadString(item, "level") ?? "-",
                ReadString(item, "message") ?? "-"))
            .ToArray();
    }

    public async Task<AdminTagsDto> GetTagsAsync(CancellationToken cancellationToken = default)
    {
        using var root = await GetJsonAsync("/api/admin/tags", cancellationToken);
        if (!TryGetProperty(root.RootElement, "summary", out var summary))
        {
            return new AdminTagsDto("0", "0", "0", "0");
        }

        return new AdminTagsDto(
            ReadString(summary, "total") ?? "0",
            ReadString(summary, "active") ?? "0",
            ReadString(summary, "stale") ?? "0",
            ReadString(summary, "error") ?? "0");
    }

    private async Task<JsonDocument> GetJsonAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(new Uri(BaseUri, path), cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
    }

    private static string FormatDate(string value)
    {
        return DateTimeOffset.TryParse(value, out var date)
            ? date.LocalDateTime.ToString("dd/MM/yyyy HH:mm:ss")
            : value;
    }

    private static string? ReadString(JsonElement element, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (!TryGetProperty(element, propertyName, out var value))
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

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            value = default;
            return false;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}

public sealed record AdminOverviewDto(
    string ServerStatus,
    string Version,
    string Channel,
    string Environment,
    string MachineName,
    string Uptime,
    string ApiStatus,
    string DatabaseStatus,
    string RuntimeStatus,
    string MqttStatus,
    string OpcUaStatus,
    string TagsTotal,
    string TagsActive,
    string TagsStale,
    string EventsQueued,
    string DataDirectory,
    string CheckedAt);

public sealed record AdminDatabaseDto(
    string Status,
    string Provider,
    string DatabaseFile,
    string Size,
    string Tables,
    string Records,
    string LastWriteAt);

public sealed record AdminMqttDto(
    string Status,
    string ClientsConnected,
    string Topics,
    string MessagesReceivedPerSecond,
    string MessagesSentPerSecond,
    string Retained);

public sealed record AdminServiceDto(string Name, string Status, string Uptime);

public sealed record AdminLogDto(string DateTime, string Level, string Message);

public sealed record AdminTagsDto(string Total, string Active, string Stale, string Error);
