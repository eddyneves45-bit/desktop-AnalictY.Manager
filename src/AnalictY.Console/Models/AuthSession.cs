namespace AnalictY.Console.Models;

public sealed record AuthSession(AuthUser User, string? AccessToken, bool UsesCookieSession);
