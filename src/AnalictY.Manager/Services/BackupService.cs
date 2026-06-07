using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace AnalictY.Manager.Services;

public sealed class BackupService
{
    private static readonly Uri BackupsEndpoint = new("http://127.0.0.1:5000/api/admin/backups");
    private static readonly Uri BackupStatusEndpoint = new("http://127.0.0.1:5000/api/admin/backup/status");
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public BackupService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BackupsResult> GetBackupsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(BackupsEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new BackupsResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(content);
            
            var backups = new List<BackupItem>();
            if (doc.RootElement.TryGetProperty("backups", out var backupsElement) && backupsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in backupsElement.EnumerateArray())
                {
                    backups.Add(new BackupItem(
                        ReadString(item, "FileName") ?? "-",
                        ReadString(item, "CreatedAt") ?? "-",
                        ReadString(item, "Size") ?? "-"
                    ));
                }
            }

            return new BackupsResult { Backups = backups };
        }
        catch (Exception ex)
        {
            return new BackupsResult { Error = ex.Message };
        }
    }

    public async Task<BackupOperationResult> CreateBackupAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsync(BackupsEndpoint, null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new BackupOperationResult { Success = false, Message = $"Erro HTTP {(int)response.StatusCode}" };
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(content);
            
            return new BackupOperationResult
            {
                Success = true,
                BackupId = ReadString(doc.RootElement, "backup_id") ?? "-",
                Message = ReadString(doc.RootElement, "message") ?? "Backup criado"
            };
        }
        catch (Exception ex)
        {
            return new BackupOperationResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<BackupOperationResult> RestoreBackupAsync(string backupId, CancellationToken cancellationToken = default)
    {
        try
        {
            var restoreEndpoint = new Uri(BackupsEndpoint, $"{backupId}/restore");
            using var response = await _httpClient.PostAsync(restoreEndpoint, null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new BackupOperationResult { Success = false, Message = $"Erro HTTP {(int)response.StatusCode}" };
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(content);
            
            return new BackupOperationResult
            {
                Success = true,
                Message = ReadString(doc.RootElement, "message") ?? "Backup restaurado"
            };
        }
        catch (Exception ex)
        {
            return new BackupOperationResult { Success = false, Message = ex.Message };
        }
    }

    private static string? ReadString(JsonElement element, params string[] names)
    {
        foreach (string name in names)
        {
            if (element.TryGetProperty(name, out JsonElement value))
            {
                return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
            }
        }
        return null;
    }
}

public sealed class BackupsResult
{
    public List<BackupItem> Backups { get; set; } = new();
    public string? Error { get; set; }
}

public sealed class BackupItem
{
    public BackupItem(string fileName, string createdAt, string size)
    {
        FileName = fileName;
        CreatedAt = createdAt;
        Size = size;
    }

    public string FileName { get; }
    public string CreatedAt { get; }
    public string Size { get; }
}

public sealed class BackupOperationResult
{
    public bool Success { get; set; }
    public string? BackupId { get; set; }
    public string? Message { get; set; }
}
