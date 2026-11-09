using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Waec.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class RabbitMqMessagingTests
{
    private readonly HttpClient _management = TestEnvironment.CreateRabbitManagementClient();
    private readonly HttpClient _registrations = TestEnvironment.CreateClient("REGISTRATION_BASE_URL", "http://localhost:8083/");

    [Fact]
    public async Task Registration_event_is_published_and_acknowledged_by_notification_worker()
    {
        var ready = await TestEnvironment.WaitUntilAsync(async () =>
        {
            using var response = await _management.GetAsync("api/queues/%2F/registration.created");
            if (!response.IsSuccessStatusCode)
                return false;

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return document.RootElement.GetProperty("consumers").GetInt32() > 0;
        }, TimeSpan.FromSeconds(45));

        Assert.True(ready, "RabbitMQ queue was not available with an active Notification Worker consumer.");

        var acknowledgementsBefore = await GetAcknowledgementCountAsync();

        var candidateNumber = $"MSG-{Guid.NewGuid():N}"[..18].ToUpperInvariant();
        var create = await _registrations.PostAsJsonAsync("/api/registrations", new
        {
            candidateNumber,
            candidateName = "RabbitMQ Integration Candidate",
            examSession = $"Messaging-{Guid.NewGuid():N}"[..22],
            subjects = new[] { "Mathematics", "English Language" }
        });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var acknowledged = await TestEnvironment.WaitUntilAsync(async () =>
            await GetAcknowledgementCountAsync() > acknowledgementsBefore,
            TimeSpan.FromSeconds(20));

        Assert.True(acknowledged, "The registration event was not acknowledged by the Notification Worker.");
    }

    private async Task<long> GetAcknowledgementCountAsync()
    {
        using var response = await _management.GetAsync("api/queues/%2F/registration.created");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (document.RootElement.TryGetProperty("message_stats", out var stats) &&
            stats.TryGetProperty("ack", out var acknowledgements))
        {
            return acknowledgements.GetInt64();
        }

        return 0;
    }
}
