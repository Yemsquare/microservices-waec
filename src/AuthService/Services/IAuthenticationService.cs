using AuthService.Common;
using AuthService.DTOs;
using AuthService.Repositories;
using AuthService.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public interface IAuthenticationService
{
    Task<Result<UserCreatedResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<Result<TokenPairResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<Result<TokenPairResponse>> RefreshAsync(RefreshRequest request, CancellationToken ct = default);
    Task LogoutAsync(RefreshRequest request, CancellationToken ct = default);
}