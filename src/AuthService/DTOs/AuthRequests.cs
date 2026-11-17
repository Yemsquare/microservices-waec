namespace AuthService.DTOs;
public sealed record RegisterRequest(string Email,string Password);
public sealed record LoginRequest(string Email,string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record CreateStaffRequest(string Email,string TemporaryPassword,string Role);
public sealed record ChangeRoleRequest(string Role);
public sealed record ChangeStatusRequest(bool Enabled);