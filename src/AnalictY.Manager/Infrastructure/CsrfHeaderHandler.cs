using System.Net;
using System.Net.Http;

namespace AnalictY.Manager.Infrastructure;

public sealed class CsrfHeaderHandler : DelegatingHandler
{
    private static readonly Uri ApiBaseUri = new("http://127.0.0.1:5000");
    private readonly CookieContainer _cookieContainer;

    public CsrfHeaderHandler(CookieContainer cookieContainer)
    {
        _cookieContainer = cookieContainer;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (RequiresCsrf(request.Method) && !request.Headers.Contains("X-CSRF-Token"))
        {
            var csrfToken = _cookieContainer.GetCookies(ApiBaseUri)
                .Cast<Cookie>()
                .FirstOrDefault(cookie => string.Equals(cookie.Name, "csrf_token", StringComparison.OrdinalIgnoreCase))
                ?.Value;

            if (!string.IsNullOrWhiteSpace(csrfToken))
            {
                request.Headers.TryAddWithoutValidation("X-CSRF-Token", Uri.UnescapeDataString(csrfToken));
            }
        }

        return base.SendAsync(request, cancellationToken);
    }

    private static bool RequiresCsrf(HttpMethod method) =>
        method == HttpMethod.Post ||
        method == HttpMethod.Put ||
        method == HttpMethod.Delete ||
        method == HttpMethod.Patch;
}
