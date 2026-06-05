using System.Net.Http;
using System.Text.Json;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.Services;

public sealed class VersionService
{
    private static readonly Uri VersionEndpoint = new("http://127.0.0.1:5000/api/system/version");
    private readonly HttpClient _httpClient;

    public VersionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<VersionInfo> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(VersionEndpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new VersionInfo
                {
                    Version = "Desconhecido",
                    Channel = "Desconhecido",
                    Source = "Desconhecido",
                    Error = $"Erro HTTP {(int)response.StatusCode}"
                };
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var versionInfo = JsonSerializer.Deserialize<VersionInfo>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return versionInfo ?? new VersionInfo
            {
                Version = "Desconhecido",
                Channel = "Desconhecido",
                Source = "Desconhecido",
                Error = "Resposta inválida"
            };
        }
        catch (OperationCanceledException)
        {
            return new VersionInfo
            {
                Version = "Desconhecido",
                Channel = "Desconhecido",
                Source = "Desconhecido",
                Error = "Tempo esgotado"
            };
        }
        catch (HttpRequestException)
        {
            return new VersionInfo
            {
                Version = "Desconhecido",
                Channel = "Desconhecido",
                Source = "Desconhecido",
                Error = "Servidor não disponível"
            };
        }
        catch (JsonException ex)
        {
            return new VersionInfo
            {
                Version = "Desconhecido",
                Channel = "Desconhecido",
                Source = "Desconhecido",
                Error = $"Erro ao processar resposta: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new VersionInfo
            {
                Version = "Desconhecido",
                Channel = "Desconhecido",
                Source = "Desconhecido",
                Error = $"Erro: {ex.Message}"
            };
        }
    }
}
