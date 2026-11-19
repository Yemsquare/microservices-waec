using Microsoft.EntityFrameworkCore;
using StudentService.Data;
using StudentService.Models;

namespace StudentService.Repositories;

public class StudentRepository(StudentDbContext db) : IStudentRepository
{
    public Task<List<Student>> GetAllAsync(CancellationToken cancellationToken) =>
        db.Students.AsNoTracking().OrderBy(student => student.LastName).ToListAsync(cancellationToken);

    public Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Students.FirstOrDefaultAsync(student => student.Id == id, cancellationToken);

    public Task<Student?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        db.Students.FirstOrDefaultAsync(student => student.Email == email, cancellationToken);

    public Task<bool> RegistrationNumberExistsAsync(string number, CancellationToken cancellationToken) =>
        db.Students.AnyAsync(student => student.RegistrationNumber == number, cancellationToken);

    public async Task AddAsync(Student student, CancellationToken cancellationToken) =>
        await db.Students.AddAsync(student, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
