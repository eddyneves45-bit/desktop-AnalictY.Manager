using System.Net.Http;
using System.Text.Json;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.Services;

public sealed class DatabaseService
{
    private static readonly Uri DatabaseStatusEndpoint = new("http://127.0.0.1:5000/api/admin/database/status");
    private static readonly Uri DatabaseConnectionsEndpoint = new("http://127.0.0.1:5000/api/database-browser/connections");
    private readonly HttpClient _httpClient;

    public DatabaseService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DatabaseStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(DatabaseStatusEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Offline($"Erro HTTP {(int)response.StatusCode}");
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var root = document.RootElement;

            return new DatabaseStatus
            {
                Type = ReadString(root, "provider") ?? "SQLite",
                Status = ReadString(root, "status") ?? "Desconhecido",
                Path = ReadString(root, "databaseFile") ?? "Desconhecido",
                Size = ReadString(root, "size") ?? "-",
                Tables = ReadString(root, "tables") ?? "-",
                Records = ReadString(root, "records") ?? "-",
                LastWrite = FormatDate(ReadString(root, "lastWriteAt") ?? "-")
            };
        }
        catch (OperationCanceledException)
        {
            return Offline("Tempo esgotado");
        }
        catch (HttpRequestException)
        {
            return Offline("Servidor nao disponivel");
        }
        catch (JsonException ex)
        {
            return Offline($"Erro ao processar resposta: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Offline($"Erro: {ex.Message}");
        }
    }

    public async Task<DatabaseTablesResult> GetTablesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(DatabaseConnectionsEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new DatabaseTablesResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var items = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray()
                    .Select(item => $"{ReadString(item, "provider") ?? "Banco"} - {ReadString(item, "name") ?? "-"} ({ReadString(item, "database") ?? "-"})")
                    .ToList()
                : [];

            return new DatabaseTablesResult { Tables = items };
        }
        catch (OperationCanceledException)
        {
            return new DatabaseTablesResult { Error = "Tempo esgotado" };
        }
        catch (HttpRequestException)
        {
            return new DatabaseTablesResult { Error = "Servidor nao disponivel" };
        }
        catch (JsonException ex)
        {
            return new DatabaseTablesResult { Error = $"Erro ao processar resposta: {ex.Message}" };
        }
        catch (Exception ex)
        {
            return new DatabaseTablesResult { Error = $"Erro: {ex.Message}" };
        }
    }

    private static DatabaseStatus Offline(string error)
    {
        return new DatabaseStatus
        {
            Type = "Desconhecido",
            Status = "Offline",
            Path = "Desconhecido",
            Error = error
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

    private static string FormatDate(string value)
    {
        return DateTimeOffset.TryParse(value, out var date)
            ? date.LocalDateTime.ToString("dd/MM/yyyy HH:mm:ss")
            : value;
    }
}
