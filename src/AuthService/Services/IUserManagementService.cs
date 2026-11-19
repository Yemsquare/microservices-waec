using AuthService.Common;
using AuthService.DTOs;
using AuthService.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public interface IUserManagementService
{
    Task<IReadOnlyList<UserSummaryResponse>> ListAsync(CancellationToken ct = default);
    Task<Result<UserCreatedResponse>> CreateStaffAsync(CreateStaffRequest request, CancellationToken ct = default);
    Task<Result<UserCreatedResponse>> ChangeRoleAsync(string userId, string? actorId, ChangeRoleRequest request, CancellationToken ct = default);
    Task<Result<UserStatusResponse>> ChangeStatusAsync(string userId, string? actorId, ChangeStatusRequest request, CancellationToken ct = default);
}