
using System.Net;
using System.Net.Http.Json;
using OrdersService.DTOs;

namespace OrdersService.Clients;

public sealed class CatalogClient : ICatalogClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CatalogClient> _logger;

    public CatalogClient(
        HttpClient httpClient,
        ILogger<CatalogClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<SubjectResponse>> GetSubjectsAsync(
        IEnumerable<int> subjectIds,
        CancellationToken cancellationToken)
    {
        // Remove duplicate IDs before sending the request.
        var ids = subjectIds.Distinct().ToArray();

        // Construct a query string such as:
        // api/subjects/validate?ids=1&ids=2
        var query = string.Join(
            "&",
            ids.Select(id => $"ids={id}"));

        var requestUrl = $"api/subjects/validate?{query}";

        _logger.LogInformation(
            "Calling CatalogService: {RequestUrl}",
            requestUrl);

        try
        {
            // Send the HTTP GET request to CatalogService.
            using var response = await _httpClient.GetAsync(
                requestUrl,
                cancellationToken);

            // Treat a non-success response as a remote-service error.
            if (!response.IsSuccessStatusCode)
            {
                var responseBody =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                throw new CatalogRequestException(
                    response.StatusCode,
                    responseBody);
            }

            // Convert the JSON response into C# objects.
            var subjects =
                await response.Content.ReadFromJsonAsync<
                    List<SubjectResponse>>(
                    cancellationToken: cancellationToken);

            return subjects ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Could not connect to CatalogService.");

            throw new CatalogUnavailableException(
                "CatalogService could not be reached.",
                ex);
        }
        catch (TaskCanceledException ex)
            when (!cancellationToken.IsCancellationRequested)
        {
            // A timeout is different from the caller cancelling the request.
            _logger.LogError(
                ex,
                "CatalogService request timed out.");

            throw new CatalogUnavailableException(
                "CatalogService did not respond in time.",
                ex);
        }
    }
}

// Represents a connection or availability problem.
public sealed class CatalogUnavailableException : Exception
{
    public CatalogUnavailableException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

// Represents an unsuccessful HTTP response from CatalogService.
public sealed class CatalogRequestException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public CatalogRequestException(
        HttpStatusCode statusCode,
        string message)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
