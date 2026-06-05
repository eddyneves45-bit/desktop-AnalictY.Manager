namespace AnalictY.Manager.Models;

public sealed record AlertOverviewResult(
    IReadOnlyList<AlertRuleRow> Rows,
    int ActiveAlerts,
    int CriticalAlerts,
    string TelegramStatus,
    string LastActivity,
    string Message,
    bool IsFallback);
