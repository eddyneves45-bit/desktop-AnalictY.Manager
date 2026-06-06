using System.Net;
using System.Net.Http;
using System.Text.Json;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.Services;

public sealed class LogsService
{
    private static readonly Uri LogsEndpoint = new("http://127.0.0.1:5000/api/logs/recent");
    private readonly HttpClient _httpClient;

    public LogsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LogsResult> GetRecentLogsAsync(int count = 50, string? level = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new List<string>();
            if (count > 0) query.Add($"count={count}");
            if (!string.IsNullOrWhiteSpace(level)) query.Add($"level={WebUtility.UrlEncode(level)}");
            var url = query.Count == 0 ? LogsEndpoint : new Uri($"{LogsEndpoint}?{string.Join("&", query)}");

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new LogsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var entriesElement = ResolveEntriesElement(document.RootElement);
            var entries = entriesElement.ValueKind == JsonValueKind.Array
                ? entriesElement.EnumerateArray().Select(ToLogEntry).ToList()
                : [];

            return new LogsResult { Entries = entries };
        }
        catch (OperationCanceledException)
        {
            return new LogsResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new LogsResult { Error = "Servidor nao disponivel" };
        }
        catch (JsonException ex)
        {
            return new LogsResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
        catch (Exception ex)
        {
            return new LogsResult { Error = $"Erro: {ex.Message}" };
        }
    }

    private static JsonElement ResolveEntriesElement(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            return root;
        }

        foreach (var name in new[] { "entries", "logs", "items", "data" })
        {
            if (TryGetProperty(root, name, out var value))
            {
                return value;
            }
        }

        return default;
    }

    private static LogEntry ToLogEntry(JsonElement item)
    {
        return new LogEntry
        {
            Timestamp = FormatDate(ReadString(item, "timestamp", "dateTime", "createdAt") ?? "-"),
            Level = NormalizeLevel(ReadString(item, "level") ?? "-"),
            Source = ReadString(item, "source", "service", "category") ?? "-",
            Message = ReadString(item, "message") ?? "-"
        };
    }

    private static string NormalizeLevel(string level)
    {
        return level.Equals("Information", StringComparison.OrdinalIgnoreCase) ? "Info" : level;
    }

    private static string FormatDate(string value)
    {
        return DateTimeOffset.TryParse(value, out var date)
            ? date.LocalDateTime.ToString("dd/MM/yyyy HH:mm:ss")
            : value;
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
