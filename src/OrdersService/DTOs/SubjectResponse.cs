
namespace OrdersService.DTOs;

// Represents the subject information received from CatalogService.
public class SubjectResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
