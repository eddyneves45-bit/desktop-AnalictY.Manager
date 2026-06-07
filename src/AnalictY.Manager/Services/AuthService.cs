using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.Services;

public sealed class AuthService
{
    private static readonly Uri ApiBaseUri = new("http://127.0.0.1:5000");
    private static readonly Uri LoginEndpoint = new(ApiBaseUri, "/api/auth/login");
    private static readonly Uri MeEndpoint = new(ApiBaseUri, "/api/auth/me");
    private static readonly Uri LogoutEndpoint = new(ApiBaseUri, "/api/auth/logout");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly CookieContainer _cookieContainer;

    public AuthService(HttpClient httpClient, CookieContainer cookieContainer)
    {
        _httpClient = httpClient;
        _cookieContainer = cookieContainer;
    }

    public AuthSession? CurrentSession { get; private set; }

    public async Task<AuthResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        return await LoginAsync(username, password, null, cancellationToken);
    }

    public async Task<AuthResult> LoginAsync(string username, string password, string? mfaCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.Failed("Informe usuário/e-mail e senha.");
        }

        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                username,
                password,
                mfaCode = string.IsNullOrWhiteSpace(mfaCode) ? null : mfaCode.Trim()
            }, JsonOptions);
            using var request = new HttpRequestMessage(HttpMethod.Post, LoginEndpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
            using JsonDocument? loginDocument = await TryReadJsonAsync(response, cancellationToken);
            string? message = loginDocument is null ? null : ReadString(loginDocument.RootElement, "message", "detail");
            bool mfaRequired = loginDocument is not null && ReadBool(loginDocument.RootElement, "mfaRequired", "mfa_required");

            if (mfaRequired)
            {
                return AuthResult.Failed("Este usuário exige MFA. O Manager ainda não implementa MFA nesta etapa.", true);
            }

            if (!response.IsSuccessStatusCode)
            {
                return AuthResult.Failed(message ?? "Não foi possível entrar. Confira usuário e senha.");
            }

            string? token = loginDocument is null ? null : ReadString(loginDocument.RootElement, "token", "accessToken", "access_token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            AuthUser? user = loginDocument is null ? null : ReadUser(loginDocument.RootElement);
            if (user is null)
            {
                user = await LoadCurrentUserAsync(cancellationToken);
            }

            if (user is null)
            {
                return AuthResult.Failed("Login aceito, mas não foi possível carregar o usuário atual.");
            }

            CurrentSession = new AuthSession(user, token, UsesCookieSession());
            return new AuthResult(true, CurrentSession, "Login realizado com sucesso.");
        }
        catch (OperationCanceledException)
        {
            return AuthResult.Failed("Tempo esgotado ao tentar entrar.");
        }
        catch (HttpRequestException)
        {
            return AuthResult.Failed("Não foi possível conectar ao AnalictY Server.");
        }
        catch (JsonException)
        {
            return AuthResult.Failed("O servidor respondeu em um formato inesperado.");
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, LogoutEndpoint);
            string? csrfToken = GetCookieValue("csrf_token");
            if (!string.IsNullOrWhiteSpace(csrfToken))
            {
                request.Headers.TryAddWithoutValidation("X-CSRF-Token", csrfToken);
            }

            await _httpClient.SendAsync(request, cancellationToken);
        }
        catch
        {
            // Logout local ainda deve limpar a sessão em memória.
        }
        finally
        {
            CurrentSession = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
            ClearCookies();
        }
    }

    private async Task<AuthUser?> LoadCurrentUserAsync(CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync(MeEndpoint, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        using JsonDocument? document = await TryReadJsonAsync(response, cancellationToken);
        return document is null ? null : ReadUser(document.RootElement);
    }

    private bool UsesCookieSession()
    {
        return !string.IsNullOrWhiteSpace(GetCookieValue("access_token")) ||
            !string.IsNullOrWhiteSpace(GetCookieValue("session_id"));
    }

    private string? GetCookieValue(string name)
    {
        return _cookieContainer.GetCookies(ApiBaseUri)
            .Cast<Cookie>()
            .FirstOrDefault(cookie => string.Equals(cookie.Name, name, StringComparison.OrdinalIgnoreCase))
            ?.Value;
    }

    private void ClearCookies()
    {
        foreach (Cookie cookie in _cookieContainer.GetCookies(ApiBaseUri))
        {
            cookie.Expired = true;
        }
    }

    private static async Task<JsonDocument?> TryReadJsonAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        return JsonDocument.Parse(body);
    }

    private static AuthUser? ReadUser(JsonElement root)
    {
        JsonElement userElement = root;
        if (root.ValueKind == JsonValueKind.Object && TryGetProperty(root, "user", out JsonElement nestedUser))
        {
            userElement = nestedUser;
        }

        if (userElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        string? username = ReadString(userElement, "username", "userName", "nome", "name");
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        return new AuthUser(
            ReadString(userElement, "id", "userId", "sub") ?? username,
            username,
            ReadString(userElement, "email") ?? string.Empty,
            (ReadString(userElement, "role", "perfil") ?? "user").ToLowerInvariant(),
            ReadStringArray(userElement, "permissions", "permissoes", "permissões"),
            ReadBool(userElement, "mfaRequired", "mfa_required"),
            ReadBool(userElement, "mfaEnabled", "mfa_enabled"));
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            if (!TryGetProperty(element, propertyName, out JsonElement value) || value.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            return value.EnumerateArray()
                .Select(item => item.GetString())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item!)
                .ToList();
        }

        return Array.Empty<string>();
    }

    private static string? ReadString(JsonElement element, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            if (TryGetProperty(element, propertyName, out JsonElement value))
            {
                return value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString(),
                    JsonValueKind.Number => value.ToString(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => null
                };
            }
        }

        return null;
    }

    private static bool ReadBool(JsonElement element, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            if (!TryGetProperty(element, propertyName, out JsonElement value))
            {
                continue;
            }

            return value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(value.GetString(), out bool parsed) && parsed,
                _ => false
            };
        }

        return false;
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}
