using StudentService.Models;

namespace StudentService.Repositories;

public interface IStudentRepository
{
    Task<List<Student>> GetAllAsync(CancellationToken cancellationToken);
    Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Student?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> RegistrationNumberExistsAsync(string number, CancellationToken cancellationToken);
    Task AddAsync(Student student, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
