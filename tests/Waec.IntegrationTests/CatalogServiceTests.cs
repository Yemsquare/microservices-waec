using System.Net;
using System.Net.Http.Json;

namespace Waec.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class CatalogServiceTests
{
    private readonly HttpClient _client = TestEnvironment.CreateClient("CATALOG_BASE_URL", "http://localhost:8081/");

    [Fact]
    public async Task Health_endpoint_returns_healthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.Equal("catalog", body?.Service);
        Assert.Equal("healthy", body?.Status);
    }

    [Fact]
    public async Task Subject_can_be_created_read_updated_and_deleted()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var create = await _client.PostAsJsonAsync("/api/catalog/subjects", new
        {
            code = $"TST{suffix}".ToUpperInvariant(),
            name = $"Integration Subject {suffix}",
            active = true
        });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<SubjectResponse>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);

        var read = await _client.GetFromJsonAsync<SubjectResponse>($"/api/catalog/subjects/{created.Id}");
        Assert.Equal(created.Name, read?.Name);

        var update = await _client.PutAsJsonAsync($"/api/catalog/subjects/{created.Id}", new
        {
            code = $"UPD{suffix}".ToUpperInvariant(),
            name = $"Updated Subject {suffix}",
            active = false
        });

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = await update.Content.ReadFromJsonAsync<SubjectResponse>();
        Assert.Equal($"Updated Subject {suffix}", updated?.Name);
        Assert.False(updated?.Active);

        var delete = await _client.DeleteAsync($"/api/catalog/subjects/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var missing = await _client.GetAsync($"/api/catalog/subjects/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Subject_creation_rejects_missing_code_or_name()
    {
        var response = await _client.PostAsJsonAsync("/api/catalog/subjects", new
        {
            code = "",
            name = " ",
            active = true
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record HealthResponse(string Service, string Status);
    private sealed record SubjectResponse(int Id, string Code, string Name, bool Active);
}
