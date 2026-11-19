using StudentService.DTOs;
using StudentService.Models;
using StudentService.Repositories;
namespace StudentService.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repository;

    public StudentService(IStudentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<StudentResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        var students = await _repository.GetAllAsync(cancellationToken);
        return students.Select(MapToResponse).ToList();
    }

    public async Task<StudentResponseDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var student = await _repository.GetByIdAsync(id, cancellationToken);
        return student is null ? null : MapToResponse(student);
    }

    public async Task<StudentResponseDto> CreateAsync(
        CreateStudentDto dto,
        CancellationToken cancellationToken = default
    )
    {

    var normalizedEmail = dto.EmailAddress.Trim().ToLowerInvariant();
    // check for already email 
    var existingStudent = await _repository.GetByEmailAsync(
        normalizedEmail,
        cancellationToken
    );
    if (existingStudent is not null)
        {
            throw new InvalidOperationException(
                "Student with the email address exist"
            );
        }

    var student = new Student
    {
        Id = Guid.NewGuid(),
        FirstName = dto.FirstName.Trim(),
        LastName = dto.LastName.Trim(),
        EmailAddress = normalizedEmail,
        DateOfBirth = dto.DateOfBirth,
        SchoolName = dto.SchoolName.Trim(),
    };

    await _repository.AddAsync(student, cancellationToken);
    await _repository.SaveChangesAsync(cancellationToken);

    return MapToResponse(student);
    }

    public async Task<StudentResponseDto?> UpdateAsync(
        Guid id,
        UpdateStudentDto dto,
        CancellationToken cancellationToken = default
    )
    {
        var student  = await _repository.GetByIdAsync(
            id, cancellationToken);

        if (student is null)
            {
                return null;
            }

        var normalizedEmail = dto.EmailAddress.Trim().ToLowerInvariant();

        var existingStudent = await _repository.GetByEmailAsync(
            normalizedEmail,
            cancellationToken
        );

        if (existingStudent is not null && existingStudent.Id != id)
            {
                throw new InvalidOperationException(
                    "Student with the email address exist"
                );
            }

        student.FirstName = dto.FirstName.Trim();
        student.LastName = dto.LastName.Trim();
        // student.EmailAddress = normalizedEmail;
        student.DateOfBirth = dto.DateOfBirth;
        student.SchoolName = dto.SchoolName.Trim();

        await _repository.SaveChangesAsync(cancellationToken);

        return MapToResponse(student);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var student = await _repository.GetByIdAsync(id, cancellationToken);

        if (student is null)
            {
                return false;
            }

        _repository.Remove(student);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
    private static StudentResponseDto MapToResponse(Student student)
    {
        return new StudentResponseDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            EmailAddress = student.EmailAddress,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender,
            SchoolName = student.SchoolName,
        
        };
    }
}