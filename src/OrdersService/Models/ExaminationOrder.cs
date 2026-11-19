
namespace OrdersService.Models;

// Represents an examination order owned by OrdersService.
public class ExaminationOrder
{
    // Unique identifier for this order.
    public Guid Id { get; set; } = Guid.NewGuid();

    // Candidate who placed the order.
    public string CandidateNumber { get; set; } = string.Empty;

    // Subjects selected by the candidate.
    public List<int> SubjectIds { get; set; } = [];

    // Current order status.
    public string Status { get; set; } = "Created";
}
