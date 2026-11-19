using AuthService.Common;
using AuthService.DTOs;
using AuthService.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public sealed class UserManagementService(UserManager<IdentityUser> users) : IUserManagementService
{
    public async Task<IReadOnlyList<UserSummaryResponse>> ListAsync(CancellationToken ct = default)
    {
        var all = await users.Users.OrderBy(x => x.Email).ToListAsync(ct);
        var result = new List<UserSummaryResponse>(all.Count);
        foreach (var user in all)
            result.Add(new(user.Id, user.Email ?? "", await users.GetRolesAsync(user),
                user.LockoutEnd is not null && user.LockoutEnd > DateTimeOffset.UtcNow));
        return result;
    }

    public async Task<Result<UserCreatedResponse>> CreateStaffAsync(CreateStaffRequest request, CancellationToken ct = default)
    {
        if (!Roles.Staff.Contains(request.Role))
            return Result<UserCreatedResponse>.Fail(ErrorType.Validation,
                "Only Examiner or Administrator can be provisioned here.");

        var email = request.Email.Trim();
        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var created = await users.CreateAsync(user, request.TemporaryPassword);
        if (!created.Succeeded)
            return Result<UserCreatedResponse>.Fail(ErrorType.Validation,
                errors: AuthenticationService.ToErrorDictionary(created));

        await users.AddToRoleAsync(user, request.Role);
        return Result<UserCreatedResponse>.Ok(new(user.Id, user.Email, request.Role));
    }

    public async Task<Result<UserCreatedResponse>> ChangeRoleAsync(
        string userId, string? actorId, ChangeRoleRequest request, CancellationToken ct = default)
    {
        if (!Roles.All.Contains(request.Role))
            return Result<UserCreatedResponse>.Fail(ErrorType.Validation,
                "Role must be Candidate, Examiner, or Administrator.");

        var target = await users.FindByIdAsync(userId);
        if (target is null) return Result<UserCreatedResponse>.Fail(ErrorType.NotFound);

        if (target.Id == actorId && request.Role != Roles.Administrator)
            return Result<UserCreatedResponse>.Fail(ErrorType.Validation,
                "Administrators cannot remove their own Administrator role.");

        var removed = await users.RemoveFromRolesAsync(target, await users.GetRolesAsync(target));
        if (!removed.Succeeded) return Failure(removed);
        var added = await users.AddToRoleAsync(target, request.Role);
        if (!added.Succeeded) return Failure(added);

        await users.UpdateSecurityStampAsync(target);
        return Result<UserCreatedResponse>.Ok(new(target.Id, target.Email, request.Role));
    }

    public async Task<Result<UserStatusResponse>> ChangeStatusAsync(
        string userId, string? actorId, ChangeStatusRequest request, CancellationToken ct = default)
    {
        var target = await users.FindByIdAsync(userId);
        if (target is null) return Result<UserStatusResponse>.Fail(ErrorType.NotFound);

        if (target.Id == actorId && !request.Enabled)
            return Result<UserStatusResponse>.Fail(ErrorType.Validation,
                "Administrators cannot disable their own account.");

        target.LockoutEnabled = true;
        target.LockoutEnd = request.Enabled ? null : DateTimeOffset.MaxValue;
        var updated = await users.UpdateAsync(target);
        if (!updated.Succeeded)
            return Result<UserStatusResponse>.Fail(ErrorType.Validation,
                errors: AuthenticationService.ToErrorDictionary(updated));

        await users.UpdateSecurityStampAsync(target);
        return Result<UserStatusResponse>.Ok(new(target.Id, target.Email, request.Enabled));
    }

    private static Result<UserCreatedResponse> Failure(IdentityResult r) =>
        Result<UserCreatedResponse>.Fail(ErrorType.Validation, errors: AuthenticationService.ToErrorDictionary(r));
}