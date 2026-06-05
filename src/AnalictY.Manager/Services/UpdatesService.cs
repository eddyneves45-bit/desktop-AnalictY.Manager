using System.Net.Http;
using System.Text.Json;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.Services;

public sealed class UpdatesService
{
    private static readonly Uri UpdatesCheckEndpoint = new("http://127.0.0.1:5000/api/system/updates/check");
    private readonly HttpClient _httpClient;

    public UpdatesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(UpdatesCheckEndpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new UpdateCheckResult
                {
                    HasUpdate = false,
                    CurrentVersion = "Desconhecido",
                    LatestVersion = "Desconhecido",
                    Error = $"Erro HTTP {(int)response.StatusCode}"
                };
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var updateResult = JsonSerializer.Deserialize<UpdateCheckResult>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return updateResult ?? new UpdateCheckResult
            {
                HasUpdate = false,
                CurrentVersion = "Desconhecido",
                LatestVersion = "Desconhecido",
                Error = "Resposta inválida"
            };
        }
        catch (OperationCanceledException)
        {
            return new UpdateCheckResult
            {
                HasUpdate = false,
                CurrentVersion = "Desconhecido",
                LatestVersion = "Desconhecido",
                Error = "Tempo esgotado"
            };
        }
        catch (HttpRequestException)
        {
            return new UpdateCheckResult
            {
                HasUpdate = false,
                CurrentVersion = "Desconhecido",
                LatestVersion = "Desconhecido",
                Error = "Servidor não disponível"
            };
        }
        catch (JsonException ex)
        {
            return new UpdateCheckResult
            {
                HasUpdate = false,
                CurrentVersion = "Desconhecido",
                LatestVersion = "Desconhecido",
                Error = $"Erro ao processar resposta: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new UpdateCheckResult
            {
                HasUpdate = false,
                CurrentVersion = "Desconhecido",
                LatestVersion = "Desconhecido",
                Error = $"Erro: {ex.Message}"
            };
        }
    }
}
