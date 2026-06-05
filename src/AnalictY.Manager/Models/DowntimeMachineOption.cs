namespace AnalictY.Manager.Models;

public sealed record DowntimeMachineOption(string Id, string Name, string Code)
{
    public string DisplayName => string.IsNullOrWhiteSpace(Code) ? Name : $"{Name} ({Code})";
}
