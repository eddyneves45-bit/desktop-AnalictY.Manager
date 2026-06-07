using System.Net;
using System.Net.Http;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.Infrastructure;

public static class AppServices
{
    private static readonly CookieContainer _cookieContainer = new();
    private static readonly HttpClient _httpClient;
    private static AuthService? _authService;

    static AppServices()
    {
        var handler = new HttpClientHandler
        {
            CookieContainer = _cookieContainer
        };
        var csrfHandler = new CsrfHandler(_cookieContainer, handler);
        _httpClient = new HttpClient(csrfHandler) { Timeout = TimeSpan.FromSeconds(10) };
    }

    public static HttpClient HttpClient => _httpClient;
    public static CookieContainer CookieContainer => _cookieContainer;

    public static AuthService AuthService
    {
        get
        {
            if (_authService == null)
            {
                _authService = new AuthService(_httpClient, _cookieContainer);
            }
            return _authService;
        }
    }

    public static void Configure(HttpClient httpClient)
    {
        // HttpClient configurado estaticamente, não precisa mais ser substituído
    }
}
