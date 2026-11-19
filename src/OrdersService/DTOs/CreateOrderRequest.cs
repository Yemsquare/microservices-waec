
using System.ComponentModel.DataAnnotations;

namespace OrdersService.DTOs;

// Data sent by a client when creating an examination order.
public class CreateOrderRequest
{
    [Required]
    [StringLength(30, MinimumLength = 3)]
    public string CandidateNumber { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "Select at least one subject.")]
    public List<int> SubjectIds { get; set; } = [];
}
