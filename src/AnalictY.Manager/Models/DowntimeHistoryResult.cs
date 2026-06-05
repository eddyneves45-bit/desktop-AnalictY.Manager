namespace AnalictY.Manager.Models;

public sealed record DowntimeHistoryResult(
    IReadOnlyList<DowntimeMachineOption> Machines,
    IReadOnlyList<DowntimeHistoryRow> Rows,
    string SelectedMachineId,
    string SelectedMachineName,
    string PeriodLabel,
    int TotalStops,
    string TotalDowntime,
    string MainReason,
    string Message,
    bool IsFallback);
