using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

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
    public async Task Gateway_forwards_api_routes_to_authorized_users(string path)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAdminTokenAsync());
        var response = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Gateway_rejects_anonymous_and_wrong_role_requests()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var anonymous = await _client.GetAsync("/api/catalog/subjects");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var candidateToken = await RegisterCandidateAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", candidateToken);
        var forbidden = await _client.GetAsync("/api/registrations");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }


    [Fact]
    public async Task Refresh_tokens_rotate_once_and_logout_revokes_the_current_token()
    {
        // Each test uses a unique account so it remains repeatable against the shared CI database.
        var email = $"refresh-{Guid.NewGuid():N}@example.test";
        const string password = "Candidate#2026Pass";
        var registration = await _client.PostAsJsonAsync("/api/auth/register", new { email, password });
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        var login = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        using var loginJson = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var originalRefreshToken = loginJson.RootElement.GetProperty("refreshToken").GetString()!;

        // The first refresh call revokes the old token and returns a replacement pair.
        var refresh = await _client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = originalRefreshToken });
        refresh.EnsureSuccessStatusCode();
        using var refreshJson = JsonDocument.Parse(await refresh.Content.ReadAsStringAsync());
        var replacementRefreshToken = refreshJson.RootElement.GetProperty("refreshToken").GetString()!;
        Assert.NotEqual(originalRefreshToken, replacementRefreshToken);

        // Replaying the old refresh token must fail.
        var replay = await _client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = originalRefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);

        // Logout revokes the latest refresh token.
        var logout = await _client.PostAsJsonAsync("/api/auth/logout", new { refreshToken = replacementRefreshToken });
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        var afterLogout = await _client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = replacementRefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, afterLogout.StatusCode);
    }

    [Fact]
    public async Task Administrator_can_list_users_but_candidate_cannot()
    {
        // Administrator permission is checked by the AuthService endpoint, not just by the gateway.
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAdminTokenAsync());
        var adminResponse = await _client.GetAsync("/api/auth/users");
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);

        var candidateToken = await RegisterCandidateAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", candidateToken);
        var candidateResponse = await _client.GetAsync("/api/auth/users");
        Assert.Equal(HttpStatusCode.Forbidden, candidateResponse.StatusCode);
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var email = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@waec-training.local";
        var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "ChangeThis-Admin-Password-2026!";
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("accessToken").GetString()!;
    }

    private async Task<string> RegisterCandidateAndGetTokenAsync()
    {
        var email = $"candidate-{Guid.NewGuid():N}@example.test";
        const string password = "Candidate#2026Pass";
        var registration = await _client.PostAsJsonAsync("/api/auth/register", new { email, password });
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        var login = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("accessToken").GetString()!;
    }
}
