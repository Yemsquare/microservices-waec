
using OrdersService.DTOs;

namespace OrdersService.Clients;

// Contract for communication with CatalogService.
public interface ICatalogClient
{
    Task<List<SubjectResponse>> GetSubjectsAsync(
        IEnumerable<int> subjectIds,
        CancellationToken cancellationToken);
}
