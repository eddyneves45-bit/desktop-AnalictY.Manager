using System.Net.Http;
using System.Text.Json;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.Services;

public sealed class DatabaseService
{
    private static readonly Uri DatabaseStatusEndpoint = new("http://127.0.0.1:5000/api/database/status");
    private static readonly Uri DatabaseTablesEndpoint = new("http://127.0.0.1:5000/api/database/tables");
    private readonly HttpClient _httpClient;

    public DatabaseService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DatabaseStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(DatabaseStatusEndpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new DatabaseStatus
                {
                    Type = "Desconhecido",
                    Status = "Offline",
                    Path = "Desconhecido",
                    Error = $"Erro HTTP {(int)response.StatusCode}"
                };
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var dbStatus = JsonSerializer.Deserialize<DatabaseStatus>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return dbStatus ?? new DatabaseStatus
            {
                Type = "Desconhecido",
                Status = "Offline",
                Path = "Desconhecido",
                Error = "Resposta inválida"
            };
        }
        catch (OperationCanceledException)
        {
            return new DatabaseStatus
            {
                Type = "Desconhecido",
                Status = "Offline",
                Path = "Desconhecido",
                Error = "Tempo esgotado"
            };
        }
        catch (HttpRequestException)
        {
            return new DatabaseStatus
            {
                Type = "Desconhecido",
                Status = "Offline",
                Path = "Desconhecido",
                Error = "Servidor não disponível"
            };
        }
        catch (JsonException ex)
        {
            return new DatabaseStatus
            {
                Type = "Desconhecido",
                Status = "Offline",
                Path = "Desconhecido",
                Error = $"Erro ao processar resposta: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new DatabaseStatus
            {
                Type = "Desconhecido",
                Status = "Offline",
                Path = "Desconhecido",
                Error = $"Erro: {ex.Message}"
            };
        }
    }

    public async Task<DatabaseTablesResult> GetTablesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(DatabaseTablesEndpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new DatabaseTablesResult
                {
                    Tables = [],
                    Error = $"Erro HTTP {(int)response.StatusCode}"
                };
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var tablesResult = JsonSerializer.Deserialize<DatabaseTablesResult>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return tablesResult ?? new DatabaseTablesResult
            {
                Tables = [],
                Error = "Resposta inválida"
            };
        }
        catch (OperationCanceledException)
        {
            return new DatabaseTablesResult
            {
                Tables = [],
                Error = "Tempo esgotado"
            };
        }
        catch (HttpRequestException)
        {
            return new DatabaseTablesResult
            {
                Tables = [],
                Error = "Servidor não disponível"
            };
        }
        catch (JsonException ex)
        {
            return new DatabaseTablesResult
            {
                Tables = [],
                Error = $"Erro ao processar resposta: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new DatabaseTablesResult
            {
                Tables = [],
                Error = $"Erro: {ex.Message}"
            };
        }
    }
}
