using System.Net.Http;
using System.Text.Json;

namespace AnalictY.Manager.Services;

public sealed class LocalServerService
{
    private static readonly Uri LocalServerInfoEndpoint = new("http://127.0.0.1:5000/api/admin/local-server/info");
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public LocalServerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LocalServerInfoResult> GetInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(LocalServerInfoEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new LocalServerInfoResult { Error = $"Erro HTTP {(int)response.StatusCode}" };
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(content);
            
            return new LocalServerInfoResult
            {
                Hostname = ReadString(doc.RootElement, "hostname") ?? "-",
                IpAddress = ReadString(doc.RootElement, "ip_address") ?? "-",
                Port = ReadInt(doc.RootElement, "port", 0),
                OsVersion = ReadString(doc.RootElement, "os_version") ?? "-",
                CpuUsagePercent = ReadInt(doc.RootElement, "cpu_usage_percent", 0),
                MemoryUsageMb = ReadInt(doc.RootElement, "memory_usage_mb", 0),
                DiskUsageGb = ReadInt(doc.RootElement, "disk_usage_gb", 0),
                UptimeSeconds = ReadInt(doc.RootElement, "uptime_seconds", 0),
                DataDirectory = ReadString(doc.RootElement, "data_directory") ?? "-",
                Environment = ReadString(doc.RootElement, "environment") ?? "-"
            };
        }
        catch (Exception ex)
        {
            return new LocalServerInfoResult { Error = ex.Message };
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

    private static int ReadInt(JsonElement element, string name, int defaultValue)
    {
        if (element.TryGetProperty(name, out JsonElement value))
        {
            return value.ValueKind == JsonValueKind.Number ? value.GetInt32() : defaultValue;
        }
        return defaultValue;
    }
}

public sealed class LocalServerInfoResult
{
    public string? Hostname { get; set; }
    public string? IpAddress { get; set; }
    public int Port { get; set; }
    public string? OsVersion { get; set; }
    public int CpuUsagePercent { get; set; }
    public int MemoryUsageMb { get; set; }
    public int DiskUsageGb { get; set; }
    public int UptimeSeconds { get; set; }
    public string? DataDirectory { get; set; }
    public string? Environment { get; set; }
    public string? Error { get; set; }
}
