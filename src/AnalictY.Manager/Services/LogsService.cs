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
            var url = LogsEndpoint;
            if (count > 0 || !string.IsNullOrEmpty(level))
            {
                var queryParams = new List<string>();
                if (count > 0) queryParams.Add($"count={count}");
                if (!string.IsNullOrEmpty(level)) queryParams.Add($"level={System.Net.WebUtility.UrlEncode(level)}");
                url = new Uri($"{LogsEndpoint}?{string.Join("&", queryParams)}");
            }

            using HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new LogsResult
                {
                    Entries = [],
                    Error = $"Erro HTTP {(int)response.StatusCode}"
                };
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var logsResult = JsonSerializer.Deserialize<LogsResult>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return logsResult ?? new LogsResult
            {
                Entries = [],
                Error = "Resposta inválida"
            };
        }
        catch (OperationCanceledException)
        {
            return new LogsResult
            {
                Entries = [],
                Error = "Tempo esgotado"
            };
        }
        catch (HttpRequestException)
        {
            return new LogsResult
            {
                Entries = [],
                Error = "Servidor não disponível"
            };
        }
        catch (JsonException ex)
        {
            return new LogsResult
            {
                Entries = [],
                Error = $"Erro ao processar resposta: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new LogsResult
            {
                Entries = [],
                Error = $"Erro: {ex.Message}"
            };
        }
    }
}
