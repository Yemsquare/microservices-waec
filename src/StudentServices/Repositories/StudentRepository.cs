using Microsoft.EntityFrameworkCore;
using StudentService.Data;
using StudentService.Models;

namespace StudentService.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly StudentDbContext _context;

    public StudentRepository(StudentDbContext context)
    {
        _context = context;
    }

    public async Task<List<Student>> GetAllAsync(

        CancellationToken cancellationToken = default
    )
    {
        return await _context.Students.AsNoTracking().OrderBy(e =>e.LastName).ToListAsync(cancellationToken);
    }

    public async Task<Student?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Students.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Student?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Students.AsNoTracking().FirstOrDefaultAsync(e => e.EmailAddress == email, cancellationToken);
    }

    public async Task AddAsync(
        Student student,
        CancellationToken cancellationToken = default
    )
    {
        await _context.Students.AddAsync(student, cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default
    )
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        CancellationToken cancellationToken = default
    )
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Remove(Student student)
    {
        _context.Students.Remove(student);
    }
}