using System.Net.Http;
using System.Text.Json;

namespace AnalictY.Manager.Services;

public sealed class EventsService
{
    private static readonly Uri EventsEndpoint = new("http://127.0.0.1:5000/api/admin/events");
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public EventsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<EventsResult> GetEventsAsync(int limit = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = limit > 0 ? new Uri($"{EventsEndpoint}?limit={limit}") : EventsEndpoint;
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new EventsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(content);
            
            var events = new List<EventItem>();
            if (doc.RootElement.TryGetProperty("events", out var eventsElement) && eventsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in eventsElement.EnumerateArray())
                {
                    events.Add(new EventItem(
                        ReadString(item, "id") ?? "-",
                        ReadString(item, "timestamp") ?? "-",
                        ReadString(item, "level") ?? "info",
                        ReadString(item, "source") ?? "-",
                        ReadString(item, "message") ?? "-",
                        ReadBool(item, false, "acknowledged")
                    ));
                }
            }

            var total = events.Count;
            if (doc.RootElement.TryGetProperty("total", out var totalElement) && totalElement.ValueKind == JsonValueKind.Number)
            {
                total = totalElement.GetInt32();
            }

            return new EventsResult { Events = events, Total = total };
        }
        catch (Exception ex)
        {
            return new EventsResult { Error = ex.Message };
        }
    }

    private static string? ReadString(JsonElement element, params string[] names)
    {
        foreach (string name in names)
        {
            if (element.TryGetProperty(name, out JsonElement value))
            {
                return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
            }
        }
        return null;
    }

    private static bool ReadBool(JsonElement element, bool defaultValue, params string[] names)
    {
        foreach (string name in names)
        {
            if (element.TryGetProperty(name, out JsonElement value))
            {
                return value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False 
                    ? value.GetBoolean() 
                    : defaultValue;
            }
        }
        return defaultValue;
    }
}

public sealed class EventsResult
{
    public List<EventItem> Events { get; set; } = new();
    public int Total { get; set; }
    public string? Error { get; set; }
}

public sealed class EventItem
{
    public EventItem(string id, string timestamp, string level, string source, string message, bool acknowledged)
    {
        Id = id;
        Timestamp = timestamp;
        Level = level;
        Source = source;
        Message = message;
        Acknowledged = acknowledged;
    }

    public string Id { get; }
    public string Timestamp { get; }
    public string Level { get; }
    public string Source { get; }
    public string Message { get; }
    public bool Acknowledged { get; }
}
