namespace AnalictY.Manager.Models;

public sealed record MachineOverviewResult(
    IReadOnlyList<MachineCard> Machines,
    bool IsFallback,
    string Message);
