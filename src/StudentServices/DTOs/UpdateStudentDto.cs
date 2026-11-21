using System.ComponentModel.DataAnnotations;
namespace StudentService.DTOs;

public class UpdateStudentDto
{
    [Required]
    [StringLength(60, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(60, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;
    
    [EmailAddress]
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string EmailAddress { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    
    public string Gender { get; set; } = string.Empty;
    [Required]
    [StringLength(100)]
    public string SchoolName { get; set; } = string.Empty;
}