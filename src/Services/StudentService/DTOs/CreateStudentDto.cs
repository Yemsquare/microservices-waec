using System.ComponentModel.DataAnnotations;

namespace StudentService.DTOs;

public class CreateStudentDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string ExamType { get; set; } = string.Empty;
}
