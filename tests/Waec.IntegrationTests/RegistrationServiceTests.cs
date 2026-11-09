using System.Net;
using System.Net.Http.Json;

namespace Waec.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class RegistrationServiceTests
{
    private readonly HttpClient _client = TestEnvironment.CreateClient("REGISTRATION_BASE_URL", "http://localhost:8083/");

    [Fact]
    public async Task Health_endpoint_returns_healthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.Equal("registration", body?.Service);
        Assert.Equal("healthy", body?.Status);
    }

    [Fact]
    public async Task Registration_can_be_created_retrieved_and_duplicate_is_rejected()
    {
        var candidateNumber = $"TEST-{Guid.NewGuid():N}"[..18].ToUpperInvariant();
        var session = $"Integration-{Guid.NewGuid():N}"[..22];
        var request = new
        {
            candidateNumber,
            candidateName = "Integration Test Candidate",
            examSession = session,
            subjects = new[] { "Mathematics", "English Language", "Physics" }
        };

        var create = await _client.PostAsJsonAsync("/api/registrations", request);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<RegistrationResponse>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal(candidateNumber, created.CandidateNumber);

        var getOne = await _client.GetAsync($"/api/registrations/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getOne.StatusCode);
        var retrieved = await getOne.Content.ReadFromJsonAsync<RegistrationResponse>();
        Assert.Equal(created.Id, retrieved?.Id);

        var duplicate = await _client.PostAsJsonAsync("/api/registrations", request);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var getAll = await _client.GetFromJsonAsync<List<RegistrationResponse>>("/api/registrations");
        Assert.Contains(getAll ?? [], item => item.CandidateNumber == candidateNumber && item.ExamSession == session);
    }

    [Fact]
    public async Task Registration_rejects_missing_required_fields_and_empty_subjects()
    {
        var response = await _client.PostAsJsonAsync("/api/registrations", new
        {
            candidateNumber = "",
            candidateName = "",
            examSession = "",
            subjects = Array.Empty<string>()
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Registration_rejects_a_subject_list_containing_only_whitespace()
    {
        var response = await _client.PostAsJsonAsync("/api/registrations", new
        {
            candidateNumber = $"TEST-{Guid.NewGuid():N}"[..18],
            candidateName = "Candidate With Empty Subjects",
            examSession = $"Integration-{Guid.NewGuid():N}"[..22],
            subjects = new[] { " ", "   " }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_registration_id_returns_not_found()
    {
        var response = await _client.GetAsync("/api/registrations/2147483647");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed record HealthResponse(string Service, string Status);
    private sealed record RegistrationResponse(
        int Id,
        string CandidateNumber,
        string CandidateName,
        string ExamSession,
        string Subjects,
        DateTime CreatedAtUtc);
}
