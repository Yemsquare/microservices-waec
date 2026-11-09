using System.Net.Http.Headers;

namespace Waec.IntegrationTests;

internal static class TestEnvironment
{
    public static HttpClient CreateClient(string variableName, string fallbackUrl)
    {
        var baseUrl = Environment.GetEnvironmentVariable(variableName) ?? fallbackUrl;
        return new HttpClient { BaseAddress = new Uri(baseUrl, UriKind.Absolute), Timeout = TimeSpan.FromSeconds(10) };
    }

    public static HttpClient CreateRabbitManagementClient()
    {
        var client = CreateClient("RABBITMQ_MANAGEMENT_URL", "http://localhost:15672/");
        var username = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "waec";
        var password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "training-local-only";
        var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        return client;
    }

    public static async Task<bool> WaitUntilAsync(
        Func<Task<bool>> condition,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (await condition())
                    return true;
            }
            catch (HttpRequestException)
            {
                // A dependent container may still be starting. Keep polling until the deadline.
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        }

        return false;
    }
}
