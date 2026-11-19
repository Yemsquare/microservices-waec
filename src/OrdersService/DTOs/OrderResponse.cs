
namespace OrdersService.DTOs;

// Data returned to the client after an order is created.
public class OrderResponse
{
    public Guid Id { get; set; }

    public string CandidateNumber { get; set; } = string.Empty;

    public List<int> SubjectIds { get; set; } = [];

    public string Status { get; set; } = string.Empty;
}
