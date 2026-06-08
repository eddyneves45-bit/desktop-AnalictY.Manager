namespace AnalictY.Manager.Models;

public class MqttDiagnosticsResult
{
    public string? Error { get; set; }
    public string? Status { get; set; }
    public string? Broker { get; set; }
    public int? Port { get; set; }
    public string? ClientId { get; set; }
    public int? Uptime { get; set; }
    public int? MessageCount { get; set; }
    public double? MessagesPerSecond { get; set; }
    public int? RetryCount { get; set; }
    public List<string>? SubscribedTopics { get; set; }
    public Dictionary<string, object>? ValuesCache { get; set; }
    public List<MqttConnectionLogEntry>? ConnectionLog { get; set; }
}

public class MqttConnectionLogEntry
{
    public string Timestamp { get; set; } = string.Empty;
    public string Event { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
}
