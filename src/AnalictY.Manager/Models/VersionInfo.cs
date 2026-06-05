namespace AnalictY.Manager.Models;

using System.Text.Json.Serialization;

public sealed class VersionInfo
{
    public string Version { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    [JsonPropertyName("data_directory")]
    public string DataDirectory { get; set; } = string.Empty;
    public string? Error { get; set; }
}
