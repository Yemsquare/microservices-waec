using System.Net;
using System.Net.Http.Json;

namespace Waec.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class OrdersServiceTests
{
    private readonly HttpClient _client = TestEnvironment.CreateClient("ORDERS_BASE_URL", "http://localhost:8082/");

    [Fact]
    public async Task Health_endpoint_returns_healthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.Equal("orders", body?.Service);
        Assert.Equal("healthy", body?.Status);
    }

    [Fact]
    public async Task Order_creation_uses_catalog_service_to_resolve_subject()
    {
        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            subjectId = 1,
            candidateName = "Integration Test Candidate",
            examSession = $"Test-{Guid.NewGuid():N}"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(order);
        Assert.Equal(1, order!.SubjectId);
        Assert.Equal("English Language", order.SubjectName);
        Assert.Equal("Integration Test Candidate", order.CandidateName);
    }

    [Fact]
    public async Task Order_creation_rejects_a_subject_that_catalog_does_not_know()
    {
        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            subjectId = 999999,
            candidateName = "Unknown Subject Candidate",
            examSession = $"Test-{Guid.NewGuid():N}"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Order_creation_rejects_invalid_required_fields()
    {
        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            subjectId = 0,
            candidateName = "",
            examSession = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record HealthResponse(string Service, string Status);
    private sealed record OrderResponse(int Id, int SubjectId, string SubjectName, string CandidateName, string ExamSession);
}
