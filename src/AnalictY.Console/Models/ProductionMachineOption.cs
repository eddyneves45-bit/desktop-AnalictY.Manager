namespace AnalictY.Console.Models;

public sealed record ProductionMachineOption(string Id, string Name, string Code)
{
    public string DisplayName => string.IsNullOrWhiteSpace(Code) ? Name : $"{Name} ({Code})";
}
