namespace AnalictY.Manager.Models;

public sealed record DowntimeHistoryRow(
    string Start,
    string End,
    string Reason,
    string Category,
    string Duration);
