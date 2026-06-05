using System.Net.Http;
using System.Text.Json;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.Services;

public sealed class HealthService
{
    private static readonly Uri HealthEndpoint = new("http://127.0.0.1:5000/api/system/health");
    private readonly HttpClient _httpClient;

    public HealthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ServerHealthStatus> CheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(HealthEndpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ServerHealthStatus(false, $"Offline ({(int)response.StatusCode})");
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (IsHealthy(body))
            {
                return new ServerHealthStatus(true, "Online");
            }

            return new ServerHealthStatus(false, "Offline (resposta inesperada)");
        }
        catch (OperationCanceledException)
        {
            return new ServerHealthStatus(false, "Offline (tempo esgotado)");
        }
        catch (HttpRequestException)
        {
            return new ServerHealthStatus(false, "Offline");
        }
        catch (Exception)
        {
            return new ServerHealthStatus(false, "Offline (erro local)");
        }
    }

    private static bool IsHealthy(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.TryGetProperty("status", out var status) &&
                string.Equals(status.GetString(), "healthy", StringComparison.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
