namespace AnalictY.Console.Models;

public sealed record AuthResult(
    bool Success,
    AuthSession? Session,
    string Message,
    bool MfaRequired = false)
{
    public static AuthResult Failed(string message, bool mfaRequired = false)
    {
        return new AuthResult(false, null, message, mfaRequired);
    }
}
