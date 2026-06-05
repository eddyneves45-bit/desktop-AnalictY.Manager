namespace AnalictY.Manager.Models;

public sealed record ReportPreviewResult(
    IReadOnlyList<ReportMachineOption> Machines,
    IReadOnlyList<ReportPreviewRow> Rows,
    string SelectedMachineId,
    string SelectedMachineName,
    string ReportType,
    string PeriodLabel,
    string StatusLabel,
    string Message,
    bool IsFallback);
