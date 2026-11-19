using Microsoft.EntityFrameworkCore;
using StudentService.DTOs;
using StudentService.Models;
using StudentService.Repositories;

namespace StudentService.Services;

public class StudentService(IStudentRepository repository)
{
    public Task<List<Student>> GetAllAsync(CancellationToken ct) => repository.GetAllAsync(ct);

    public Task<Student?> GetByIdAsync(Guid id, CancellationToken ct) =>
        repository.GetByIdAsync(id, ct);

    public async Task<Student> CreateAsync(CreateStudentDto dto, CancellationToken ct)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var registrationNumber = dto.RegistrationNumber.Trim();

        if (await repository.GetByEmailAsync(email, ct) is not null)
            throw new InvalidOperationException("A student with this email already exists.");

        if (await repository.RegistrationNumberExistsAsync(registrationNumber, ct))
            throw new InvalidOperationException("This registration number already exists.");

        var student = new Student
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = email,
            RegistrationNumber = registrationNumber,
            ExamType = dto.ExamType.Trim()
        };

        await repository.AddAsync(student, ct);
        await repository.SaveChangesAsync(ct);
        return student;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateStudentDto dto, CancellationToken ct)
    {
        var student = await repository.GetByIdAsync(id, ct);
        if (student is null) return false;

        var email = dto.Email.Trim().ToLowerInvariant();
        var existing = await repository.GetByEmailAsync(email, ct);
        if (existing is not null && existing.Id != id)
            throw new InvalidOperationException("Another student already uses this email.");

        student.FirstName = dto.FirstName.Trim();
        student.LastName = dto.LastName.Trim();
        student.Email = email;
        student.ExamType = dto.ExamType.Trim();
        await repository.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken ct)
    {
        var student = await repository.GetByIdAsync(id, ct);
        if (student is null) return false;

        student.IsActive = false;
        await repository.SaveChangesAsync(ct);
        return true;
    }
}
