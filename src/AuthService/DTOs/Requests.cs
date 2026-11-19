using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs;

public sealed record RegisterRequest(
    [Required, EmailAddress] 
    string Email, 
    
    [Required] 
    string Password
    );
public sealed record LoginRequest(
    [Required, EmailAddress] 
    string Email, 
    
    [Required] 
    string Password
    );
public sealed record RefreshRequest(
    [Required] 
    string RefreshToken
    );
public sealed record CreateStaffRequest(
    [Required, EmailAddress] 
    string Email, 
    [Required] 
    string TemporaryPassword, 
    [Required] 
    string Role
    );
public sealed record ChangeRoleRequest([Required] string Role);
public sealed record ChangeStatusRequest(bool Enabled);