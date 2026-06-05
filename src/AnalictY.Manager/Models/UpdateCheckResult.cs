namespace AnalictY.Manager.Models;

public sealed class UpdateCheckResult
{
    public bool HasUpdate { get; set; }
    public string CurrentVersion { get; set; } = string.Empty;
    public string LatestVersion { get; set; } = string.Empty;
    public string? ReleaseNotes { get; set; }
    public string? Error { get; set; }
}
