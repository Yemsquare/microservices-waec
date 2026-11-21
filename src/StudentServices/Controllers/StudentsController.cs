using Microsoft.AspNetCore.Mvc;
using StudentService.Services;
using StudentService.DTOs;

namespace StudentService.Controllers;

[ApiController]
[Route("api/[controller]")]

public class StudentController : ControllerBase
{
   private readonly IStudentService _studentService;

   public StudentController(IStudentService studentService)
   {
      _studentService = studentService;
   }

//    GET method api/students
    [HttpGet]
    public async Task<ActionResult<List<StudentResponseDto>>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        var students = await _studentService.GetAllAsync(cancellationToken);
        return Ok(students);
    }

    // Get: api/students/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudentResponseDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var student = await _studentService.GetByIdAsync(
            id, 
            cancellationToken
        );

        if (student is null)
        {
            return NotFound();
        }

        return Ok(student);
    }

    // POST: api/students
    [HttpPost]
    public async Task<ActionResult<StudentResponseDto>> create(
        CreateStudentDto dto,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var student = await _studentService.CreateAsync(
            dto,
            cancellationToken
        );
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
        }
        catch(InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Student registration conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    // PUT: api/students/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StudentResponseDto>> Update(
        Guid id,
        UpdateStudentDto dto,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var student = await _studentService.UpdateAsync(
                id,
                dto,
                cancellationToken
            );

            if(student is null)
            {
                return NotFound();
            }
            return Ok(student);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Student update conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    // DELETE: api/students/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var deleted = await _studentService.DeleteAsync(
            id,
            cancellationToken
        );

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}