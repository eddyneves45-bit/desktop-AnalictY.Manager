using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AnalictY.Manager.Services;

public sealed class SecurityService
{
    private static readonly Uri ApiBaseUri = new("http://127.0.0.1:5000");
    private static readonly Uri MfaStatusEndpoint = new(ApiBaseUri, "/api/auth/mfa/status");
    private static readonly Uri MfaSetupEndpoint = new(ApiBaseUri, "/api/auth/mfa/setup");
    private static readonly Uri MfaEnableEndpoint = new(ApiBaseUri, "/api/auth/mfa/enable");
    private static readonly Uri MfaDisableEndpoint = new(ApiBaseUri, "/api/auth/mfa/disable");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public SecurityService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SecurityMfaStatus> GetMfaStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(MfaStatusEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return SecurityMfaStatus.Failed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            bool enabled = ReadBool(document.RootElement, "enabled");
            return new SecurityMfaStatus(true, enabled, string.Empty);
        }
        catch (Exception ex)
        {
            return SecurityMfaStatus.Failed($"Erro ao carregar MFA: {ex.Message}");
        }
    }

    public async Task<SecurityMfaSetup> StartMfaSetupAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsync(MfaSetupEndpoint, null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return SecurityMfaSetup.Failed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return new SecurityMfaSetup(
                true,
                ReadString(document.RootElement, "secret") ?? string.Empty,
                ReadString(document.RootElement, "otpauthUrl", "otpauth_url") ?? string.Empty,
                string.Empty);
        }
        catch (Exception ex)
        {
            return SecurityMfaSetup.Failed($"Erro ao iniciar MFA: {ex.Message}");
        }
    }

    public async Task<SecurityOperationResult> EnableMfaAsync(string code, CancellationToken cancellationToken = default)
    {
        return await SendMfaCodeAsync(MfaEnableEndpoint, code, "MFA ativado. Faca login novamente.", cancellationToken);
    }

    public async Task<SecurityOperationResult> DisableMfaAsync(string code, CancellationToken cancellationToken = default)
    {
        return await SendMfaCodeAsync(MfaDisableEndpoint, code, "MFA desativado.", cancellationToken);
    }

    private async Task<SecurityOperationResult> SendMfaCodeAsync(
        Uri endpoint,
        string code,
        string successMessage,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length != 6)
        {
            return SecurityOperationResult.Failed("Informe um codigo de 6 digitos.");
        }

        try
        {
            var payload = JsonSerializer.Serialize(new { code = code.Trim() }, JsonOptions);
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return SecurityOperationResult.Failed(await ReadErrorMessageAsync(response, cancellationToken));
            }

            return new SecurityOperationResult(true, successMessage);
        }
        catch (Exception ex)
        {
            return SecurityOperationResult.Failed($"Erro ao atualizar MFA: {ex.Message}");
        }
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(body))
        {
            return $"Erro HTTP {(int)response.StatusCode}";
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            return ReadString(document.RootElement, "message", "error", "detail") ?? body;
        }
        catch (JsonException)
        {
            return body;
        }
    }

    private static string? ReadString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (!TryGetProperty(element, name, out var value))
            {
                continue;
            }

            return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
        }

        return null;
    }

    private static bool ReadBool(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (!TryGetProperty(element, name, out var value))
            {
                continue;
            }

            return value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(value.GetString(), out var parsed) && parsed,
                _ => false
            };
        }

        return false;
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}

public sealed record SecurityMfaStatus(bool Success, bool Enabled, string Error)
{
    public static SecurityMfaStatus Failed(string error) => new(false, false, error);
}

public sealed record SecurityMfaSetup(bool Success, string Secret, string OtpAuthUrl, string Error)
{
    public static SecurityMfaSetup Failed(string error) => new(false, string.Empty, string.Empty, error);
}

public sealed record SecurityOperationResult(bool Success, string Message)
{
    public static SecurityOperationResult Failed(string message) => new(false, message);
}
