
namespace CatalogService.Models;

// Represents an examination subject owned by CatalogService.
public class Subject
{
    // Unique identifier for the subject.
    public int Id { get; set; }

    // Name of the subject, e.g. Mathematics.
    public string Name { get; set; } = string.Empty;

    // Indicates whether candidates can currently select it.
    public bool IsActive { get; set; } = true;
}
