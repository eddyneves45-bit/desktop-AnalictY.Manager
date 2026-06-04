using System.Net;
using System.Net.Http;
using System.Text.Json;
using AnalictY.Console.Models;

namespace AnalictY.Console.Services;

public sealed class StatusOverviewService
{
    private static readonly Uri HealthEndpoint = new("http://127.0.0.1:5000/api/system/health");
    private static readonly Uri VersionEndpoint = new("http://127.0.0.1:5000/api/system/version");
    private static readonly Uri RuntimeEndpoint = new("http://127.0.0.1:5000/api/runtime/state");
    private readonly HttpClient _httpClient;
    private readonly MachineOverviewService _machineOverviewService;

    public StatusOverviewService(HttpClient httpClient, MachineOverviewService machineOverviewService)
    {
        _httpClient = httpClient;
        _machineOverviewService = machineOverviewService;
    }

    public async Task<StatusOverviewResult> LoadAsync(CancellationToken cancellationToken = default)
    {
        var health = await LoadHealthAsync(cancellationToken);
        var version = await LoadVersionAsync(cancellationToken);
        var runtime = await LoadRuntimeAsync(cancellationToken);
        var machines = await _machineOverviewService.LoadAsync(cancellationToken);

        string message = BuildMessage(health.Message, version.Message, runtime.Message, machines.Message);

        return new StatusOverviewResult(
            health.IsOnline,
            health.Status,
            version.Version,
            version.Channel,
            version.Source,
            health.DataDirectory != "-" ? health.DataDirectory : version.DataDirectory,
            health.DatabaseStatus,
            runtime.Status,
            health.IsOnline ? "Online" : "Offline",
            message,
            machines.Machines);
    }

    private async Task<HealthSnapshot> LoadHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(HealthEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new HealthSnapshot(false, $"Offline ({(int)response.StatusCode})", "-", "Indisponível", "Health retornou erro.");
            }

            using JsonDocument document = await ReadJsonAsync(response, cancellationToken);
            string status = ReadString(document.RootElement, "status") ?? "healthy";
            string dataDirectory = ReadString(document.RootElement, "data_directory", "dataDirectory") ?? "-";
            bool databaseExists = ReadBool(document.RootElement, "database_exists", "databaseExists");

            return new HealthSnapshot(
                string.Equals(status, "healthy", StringComparison.OrdinalIgnoreCase),
                NormalizeHealth(status),
                dataDirectory,
                databaseExists ? "Banco local encontrado" : "Banco local não confirmado",
                "Health consultado com sucesso.");
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException)
        {
            return new HealthSnapshot(false, "Offline", "-", "Indisponível", "Não foi possível consultar health.");
        }
    }

    private async Task<VersionSnapshot> LoadVersionAsync(CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(VersionEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new VersionSnapshot("-", "-", "-", "-", "Version retornou erro.");
            }

            using JsonDocument document = await ReadJsonAsync(response, cancellationToken);
            return new VersionSnapshot(
                ReadString(document.RootElement, "version") ?? "-",
                ReadString(document.RootElement, "channel") ?? "-",
                ReadString(document.RootElement, "source") ?? "-",
                ReadString(document.RootElement, "data_directory", "dataDirectory") ?? "-",
                "Version consultado com sucesso.");
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException)
        {
            return new VersionSnapshot("-", "-", "-", "-", "Não foi possível consultar version.");
        }
    }

    private async Task<RuntimeSnapshot> LoadRuntimeAsync(CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(RuntimeEndpoint, cancellationToken);
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return new RuntimeSnapshot("Requer login", "Runtime exige autenticação.");
            }

            if (!response.IsSuccessStatusCode)
            {
                return new RuntimeSnapshot($"Indisponível ({(int)response.StatusCode})", "Runtime retornou erro.");
            }

            using JsonDocument document = await ReadJsonAsync(response, cancellationToken);
            int count = CountRuntimeItems(document.RootElement);
            string status = count > 0 ? $"{count} tags em runtime" : "Runtime sem tags ativas";
            return new RuntimeSnapshot(status, "Runtime consultado com sucesso.");
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException)
        {
            return new RuntimeSnapshot("Indisponível", "Não foi possível consultar runtime.");
        }
    }

    private static string BuildMessage(params string[] messages)
    {
        return string.Join(" ", messages.Where(message => !string.IsNullOrWhiteSpace(message)).Distinct());
    }

    private static int CountRuntimeItems(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            return root.GetArrayLength();
        }

        if (root.ValueKind != JsonValueKind.Object)
        {
            return 0;
        }

        foreach (string name in new[] { "tags", "states", "data", "items", "results" })
        {
            if (TryGetProperty(root, name, out JsonElement value) && value.ValueKind == JsonValueKind.Array)
            {
                return value.GetArrayLength();
            }
        }

        return root.EnumerateObject().Count();
    }

    private static string NormalizeHealth(string status)
    {
        return string.Equals(status, "healthy", StringComparison.OrdinalIgnoreCase) ? "Saudável" : status;
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
    }

    private static string? ReadString(JsonElement element, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            if (TryGetProperty(element, propertyName, out JsonElement value))
            {
                return value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString(),
                    JsonValueKind.Number => value.ToString(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => null
                };
            }
        }

        return null;
    }

    private static bool ReadBool(JsonElement element, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            if (!TryGetProperty(element, propertyName, out JsonElement value))
            {
                continue;
            }

            return value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(value.GetString(), out bool parsed) && parsed,
                _ => false
            };
        }

        return false;
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            value = default;
            return false;
        }

        foreach (JsonProperty property in element.EnumerateObject())
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

    private sealed record HealthSnapshot(bool IsOnline, string Status, string DataDirectory, string DatabaseStatus, string Message);

    private sealed record VersionSnapshot(string Version, string Channel, string Source, string DataDirectory, string Message);

    private sealed record RuntimeSnapshot(string Status, string Message);
}
