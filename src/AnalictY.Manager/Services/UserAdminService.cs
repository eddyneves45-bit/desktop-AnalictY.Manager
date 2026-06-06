using System.Net.Http;
using System.Text.Json;

namespace AnalictY.Manager.Services;

public sealed class UserAdminService
{
    private static readonly Uri UsersEndpoint = new("http://127.0.0.1:5000/api/users");
    private readonly HttpClient _httpClient;

    public UserAdminService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<UserAdminRow>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(UsersEndpoint, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<UserAdminRow>();
        }

        return document.RootElement.EnumerateArray()
            .Select(item => new UserAdminRow(
                ReadString(item, "id") ?? "-",
                ReadString(item, "username") ?? "-",
                ReadString(item, "email") ?? "-",
                ReadString(item, "role") ?? "-",
                ReadBool(item, "isActive") ? "Ativo" : "Inativo",
                ReadBool(item, "mfaEnabled") ? "Sim" : "Nao",
                FormatDate(ReadString(item, "createdAt") ?? "-")))
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

public sealed record UserAdminRow(string Id, string Username, string Email, string Role, string Status, string Mfa, string CreatedAt);
