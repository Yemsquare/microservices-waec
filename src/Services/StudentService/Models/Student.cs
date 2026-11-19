namespace StudentService.Models;

public class Student
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public DateTime DateRegistered { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
