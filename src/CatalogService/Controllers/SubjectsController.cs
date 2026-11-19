
using CatalogService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubjectsController : ControllerBase
{
    // Temporary in-memory data for this HTTP communication lab.
    // Replace with the Catalog repository/DbContext in production.
    private static readonly List<Subject> Subjects =
    [
        new Subject { Id = 1, Name = "Mathematics", IsActive = true },
        new Subject { Id = 2, Name = "English Language", IsActive = true },
        new Subject { Id = 3, Name = "Biology", IsActive = true },
        new Subject { Id = 4, Name = "Chemistry", IsActive = true },
        new Subject { Id = 5, Name = "Physics", IsActive = false }
    ];

    // GET: api/subjects
    [HttpGet]
    public ActionResult<IEnumerable<Subject>> GetAll()
    {
        return Ok(Subjects);
    }

    // GET: api/subjects/1
    [HttpGet("{id:int}")]
    public ActionResult<Subject> GetById(int id)
    {
        var subject = Subjects.FirstOrDefault(x => x.Id == id);

        if (subject is null)
        {
            return NotFound(new
            {
                message = $"Subject with ID {id} was not found."
            });
        }

        return Ok(subject);
    }

    // GET: api/subjects/validate?ids=1&ids=2
    [HttpGet("validate")]
    public ActionResult<IEnumerable<Subject>> Validate(
        [FromQuery] int[] ids)
    {
        if (ids.Length == 0)
        {
            return BadRequest(new
            {
                message = "At least one subject ID is required."
            });
        }

        var matchingSubjects = Subjects
            .Where(s => ids.Contains(s.Id))
            .ToList();

        return Ok(matchingSubjects);
    }
}
