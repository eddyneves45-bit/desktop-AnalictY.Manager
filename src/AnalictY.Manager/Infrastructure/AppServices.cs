using System.Net.Http;

namespace AnalictY.Manager.Infrastructure;

public static class AppServices
{
    public static HttpClient HttpClient { get; private set; } = new() { Timeout = TimeSpan.FromSeconds(10) };

    public static void Configure(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }
}
