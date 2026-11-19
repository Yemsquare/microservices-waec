using System.ComponentModel.DataAnnotations;
namespace StudentService.DTOs;

public class CreateStudentDto
{
    [Required]
    [StringLength(60, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;
   
    [Required] 
    [StringLength(60, MinimumLength = 2)] 
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(100, MinimumLength = 2)]
    public string EmailAddress { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    
    public string Gender { get; set; } = string.Empty;
    [Required]
    [StringLength(100)]
    public string SchoolName { get; set; } = string.Empty;

    // public DateTime DateCreation { get; set; } = DateTime.UtcNow;
}