namespace AnalictY.Console.Models;

public sealed record StatusOverviewResult(
    bool IsOnline,
    string HealthStatus,
    string Version,
    string Channel,
    string Source,
    string DataDirectory,
    string DatabaseStatus,
    string RuntimeStatus,
    string ApiStatus,
    string Message,
    IReadOnlyList<MachineCard> Machines);
