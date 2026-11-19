using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentService.DTOs;
using StudentService.Services;

namespace StudentService.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController(StudentService.Services.StudentService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var student = await service.GetByIdAsync(id, ct);
        return student is null ? NotFound(new { message = "Student not found." }) : Ok(student);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateStudentDto dto, CancellationToken ct)
    {
        try
        {
            var student = await service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "A student with this email or registration number already exists." });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateStudentDto dto, CancellationToken ct)
    {
        try
        {
            var updated = await service.UpdateAsync(id, dto, ct);
            return updated ? Ok(new { message = "Student updated successfully." }) : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "A student with this email already exists." });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var deactivated = await service.DeactivateAsync(id, ct);
        return deactivated ? Ok(new { message = "Student deactivated successfully." }) : NotFound();
    }
}
