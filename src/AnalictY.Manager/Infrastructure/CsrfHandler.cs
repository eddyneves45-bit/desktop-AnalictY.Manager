using System.Net;
using System.Net.Http;

namespace AnalictY.Manager.Infrastructure;

public sealed class CsrfHandler : DelegatingHandler
{
    private readonly CookieContainer _cookieContainer;

    public CsrfHandler(CookieContainer cookieContainer, HttpMessageHandler? innerHandler = null)
    {
        _cookieContainer = cookieContainer;
        InnerHandler = innerHandler ?? new HttpClientHandler();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri is not null &&
            (request.Method == HttpMethod.Post ||
            request.Method == HttpMethod.Put || 
            request.Method == HttpMethod.Delete))
        {
            var csrfToken = _cookieContainer.GetCookies(request.RequestUri)
                .Cast<Cookie>()
                .FirstOrDefault(c => c.Name == "csrf_token")?.Value;

            if (!string.IsNullOrWhiteSpace(csrfToken))
            {
                request.Headers.TryAddWithoutValidation("X-CSRF-Token", csrfToken);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
