namespace StudentService.DTOs;

public class StudentResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
}