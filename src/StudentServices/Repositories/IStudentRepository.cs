using StudentService.Models;
namespace StudentService.Repositories;

public interface IStudentRepository
{
    
    Task<List<Student>> GetAllAsync(
        CancellationToken cancellationToken = default
    );

    Task<Student?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<Student?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        Student student,
        CancellationToken cancellationToken = default
    );

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default
    );

    Task UpdateAsync(
        CancellationToken cancellationToken = default
    );

    void Remove(Student student);
}