using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.DTOs;
using AuthService.Entities;
using AuthService.Options;
using AuthService.Repositories;
using AuthService.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AuthService.Services;

namespace AuthService.Services;

public sealed class TokenService(
    UserManager<IdentityUser> users,
    RoleManager<IdentityRole> roleManager,
    IRefreshTokenRepository refreshTokens,
    IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _jwt = options.Value;

    public async Task<TokenPairResponse> IssueTokenPairAsync(IdentityUser user, CancellationToken ct = default)
    {
        var roles = await users.GetRolesAsync(user);
        var permissions = await GetPermissionsAsync(roles);
        var claims = BuildClaims(user, roles, permissions);

        var now = DateTime.UtcNow;
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(_jwt.Issuer, _jwt.Audience, claims,
            now, now.AddMinutes(_jwt.AccessTokenMinutes), credentials);

        // Opaque random refresh token; only its hash is stored.
        var rawRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        await refreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = TokenHasher.Hash(rawRefreshToken),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(_jwt.RefreshTokenDays)
        }, ct);
        await refreshTokens.SaveChangesAsync(ct);

        return new TokenPairResponse(new JwtSecurityTokenHandler().WriteToken(jwt), rawRefreshToken,
            "Bearer", jwt.ValidTo, user.Id, user.Email ?? "", roles, permissions);
    }

    private async Task<List<string>> GetPermissionsAsync(IEnumerable<string> roleNames)
    {
        var permissions = new HashSet<string>();
        foreach (var roleName in roleNames)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null) continue;
            foreach (var c in (await roleManager.GetClaimsAsync(role)).Where(c => c.Type == Permissions.ClaimType))
                permissions.Add(c.Value);
        }
        return [.. permissions];
    }

    private static List<Claim> BuildClaims(IdentityUser user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(ClaimTypes.Name, user.Email ?? user.UserName ?? user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permissions.Select(p => new Claim(Permissions.ClaimType, p)));
        return claims;
    }
}