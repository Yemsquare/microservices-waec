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
    IRefreshTokenRepository refreshTokens,
    ILogger<AuthenticationService> logger) : IAuthenticationService
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
            await using var tx = await refreshTokens.BeginTransactionAsync(ct);

            var stored = await refreshTokens.GetByHashAsync(TokenHasher.Hash(request.RefreshToken), includeUser: true, ct);
            if (stored is null)
                return Result<TokenPairResponse>.Fail(ErrorType.Unauthorized);

            var now = DateTime.UtcNow;

            if (stored.RevokedAtUtc is not null)
            {
                // A rotated token was presented again: either a thief or the real client holds a stale copy.
                // Kill every descendant so neither side keeps a working session.
                if (stored.ReplacedByTokenHash is not null)
                {
                    await refreshTokens.RevokeChainAsync(stored.ReplacedByTokenHash, now, ct);
                    await tx.CommitAsync(ct);
                    logger.LogWarning("Refresh token reuse detected for user {UserId}; token chain revoked.", stored.UserId);
                }
                return Result<TokenPairResponse>.Fail(ErrorType.Unauthorized);
            }

            if (!stored.IsActive(now) || IsLockedOut(stored.User, now))
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