using System.Net;

namespace Waec.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class ExamGatewayTests
{
    private readonly HttpClient _client = TestEnvironment.CreateClient("GATEWAY_BASE_URL", "http://localhost:8080/");

    [Fact]
    public async Task Gateway_health_endpoint_returns_healthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/catalog/subjects")]
    [InlineData("/api/orders")]
    [InlineData("/api/registrations")]
    public async Task Gateway_forwards_api_routes_to_their_services(string path)
    {
        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
