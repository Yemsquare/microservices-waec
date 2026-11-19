using StudentService.DTOs;
namespace StudentService.Services;

public interface IStudentService
{
    Task<List<StudentResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default
    );

    Task<StudentResponseDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<StudentResponseDto> CreateAsync(
        CreateStudentDto dto,
        CancellationToken cancellationToken = default
    );

    Task<StudentResponseDto?> UpdateAsync(
        Guid id,
        UpdateStudentDto dto,
        CancellationToken cancellationToken = default
    );

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
}

