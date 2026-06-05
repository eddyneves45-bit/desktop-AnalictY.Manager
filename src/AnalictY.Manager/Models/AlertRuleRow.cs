namespace AnalictY.Manager.Models;

public sealed record AlertRuleRow(
    string Name,
    string Scope,
    string Condition,
    string Severity,
    string Status);
