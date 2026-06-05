namespace AnalictY.Manager.Models;

public sealed record AuthUser(
    string Id,
    string Username,
    string Email,
    string Role,
    IReadOnlyList<string> Permissions,
    bool MfaRequired,
    bool MfaEnabled);
