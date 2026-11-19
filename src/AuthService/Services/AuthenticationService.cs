using AuthService.Common;
using AuthService.DTOs;
using AuthService.Repositories;
using AuthService.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public sealed class AuthenticationService(
    UserManager<IdentityUser> users,
    SignInManager<IdentityUser> signIn,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokens) : IAuthenticationService
{
    public async Task<Result<UserCreatedResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim();
        var user = new IdentityUser { UserName = email, Email = email };

        var created = await users.CreateAsync(user, request.Password);
        if (!created.Succeeded)
            return Result<UserCreatedResponse>.Fail(ErrorType.Validation, errors: ToErrorDictionary(created));

        // Public registration always assigns Candidate to prevent privilege escalation.
        await users.AddToRoleAsync(user, Roles.Candidate);
        return Result<UserCreatedResponse>.Ok(new(user.Id, user.Email, Roles.Candidate));
    }

    public async Task<Result<TokenPairResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user is null ||
            !(await signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true)).Succeeded)
            return Result<TokenPairResponse>.Fail(ErrorType.Unauthorized);

        return Result<TokenPairResponse>.Ok(await tokenService.IssueTokenPairAsync(user, ct));
    }

    public async Task<Result<TokenPairResponse>> RefreshAsync(RefreshRequest request, CancellationToken ct = default)
    {
        try
        {
            // Lookup, revoke, and replace in one transaction to block parallel token reuse.
            await using var tx = await refreshTokens.BeginTransactionAsync(ct);

            var stored = await refreshTokens.GetByHashAsync(TokenHasher.Hash(request.RefreshToken), includeUser: true, ct);
            var now = DateTime.UtcNow;
            if (stored is null || !stored.IsActive(now) || IsLockedOut(stored.User, now))
                return Result<TokenPairResponse>.Fail(ErrorType.Unauthorized);

            stored.RevokedAtUtc = now;
            var pair = await tokenService.IssueTokenPairAsync(stored.User, ct);
            stored.ReplacedByTokenHash = TokenHasher.Hash(pair.RefreshToken);
            await refreshTokens.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return Result<TokenPairResponse>.Ok(pair);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another request already rotated this token.
            return Result<TokenPairResponse>.Fail(ErrorType.Unauthorized);
        }
    }

    public async Task LogoutAsync(RefreshRequest request, CancellationToken ct = default)
    {
        var stored = await refreshTokens.GetByHashAsync(TokenHasher.Hash(request.RefreshToken), includeUser: false, ct);
        if (stored is not null && stored.RevokedAtUtc is null)
        {
            stored.RevokedAtUtc = DateTime.UtcNow;
            await refreshTokens.SaveChangesAsync(ct);
        }
    }

    private static bool IsLockedOut(IdentityUser user, DateTime now) =>
        user.LockoutEnabled && user.LockoutEnd is not null && user.LockoutEnd > now;

    internal static Dictionary<string, string[]> ToErrorDictionary(IdentityResult result) =>
        result.Errors.GroupBy(e => e.Code)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
}