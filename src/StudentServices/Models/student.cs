namespace StudentService.Models;

public class Student
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    // public DateTime DateCreation { get; set; }
}

// domain model 
// Student id : Guid 
// first name : string 
// last name : string
// Email address : string 
// Date of Birth : DateTIme
// School Name : string 
// Date creation : DateTime 
