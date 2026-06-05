namespace AnalictY.Manager.Models;

public sealed class DatabaseStatus
{
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? Error { get; set; }
}

public sealed class DatabaseTablesResult
{
    public List<string> Tables { get; set; } = [];
    public string? Error { get; set; }
}
