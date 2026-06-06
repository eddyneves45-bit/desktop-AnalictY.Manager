using System.Net.Http;
using System.Text.Json;

namespace AnalictY.Manager.Services;

public sealed class AuditService
{
    private static readonly Uri AuditEndpoint = new("http://127.0.0.1:5000/api/audit/logs?take=200");
    private readonly HttpClient _httpClient;

    public AuditService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<AuditRow>> GetAuditAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(AuditEndpoint, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<AuditRow>();
        }

        return document.RootElement.EnumerateArray()
            .Select(item => new AuditRow(
                FormatDate(ReadString(item, "createdAt", "timestamp") ?? "-"),
                ReadString(item, "username") ?? "-",
                ReadString(item, "role") ?? "-",
                ReadString(item, "action") ?? "-",
                ReadString(item, "path") ?? "-",
                ReadString(item, "statusCode") ?? "-",
                ReadString(item, "ipAddress") ?? "-"))
            .ToArray();
    }

    private static string? ReadString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetProperty(element, name, out var value))
            {
                return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
            }
        }

        return null;
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
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

    private static string FormatDate(string value)
    {
        return DateTimeOffset.TryParse(value, out var date)
            ? date.LocalDateTime.ToString("dd/MM/yyyy HH:mm:ss")
            : value;
    }
}

public sealed record AuditRow(string DateTime, string Username, string Role, string Action, string Path, string StatusCode, string IpAddress);
