namespace AnalictY.Console.Models;

public sealed record ProductionHistoryResult(
    IReadOnlyList<ProductionMachineOption> Machines,
    IReadOnlyList<ProductionHistoryRow> Rows,
    string SelectedMachineId,
    string SelectedMachineName,
    string PeriodLabel,
    double TotalProduced,
    double TotalLost,
    double TotalGood,
    string Message,
    bool IsFallback);
