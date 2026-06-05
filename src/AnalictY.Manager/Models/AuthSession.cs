namespace AnalictY.Manager.Models;

public sealed record AuthSession(AuthUser User, string? AccessToken, bool UsesCookieSession);
