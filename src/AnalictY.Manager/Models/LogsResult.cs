namespace AnalictY.Manager.Models;

public sealed class LogsResult
{
    public List<LogEntry> Entries { get; set; } = [];
    public string? Error { get; set; }
}

public sealed class LogEntry
{
    public string Timestamp { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Source { get; set; }
}
