using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.Common;
using AuthService.DTOs;
using AuthService.Security;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthenticationService auth) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await auth.RegisterAsync(request, ct);
        return result.Succeeded
            ? Created($"/api/auth/users/{result.Value!.Id}", result.Value)
            : this.ToProblem(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await auth.LoginAsync(request, ct);
        return result.Succeeded ? Ok(result.Value) : this.ToProblem(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken ct)
    {
        var result = await auth.RefreshAsync(request, ct);
        return result.Succeeded ? Ok(result.Value) : this.ToProblem(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest request, CancellationToken ct)
    {
        await auth.LogoutAsync(request, ct);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new MeResponse(
        User.FindFirstValue(JwtRegisteredClaimNames.Sub),
        User.FindFirstValue(JwtRegisteredClaimNames.Email),
        User.FindAll(ClaimTypes.Role).Select(c => c.Value),
        User.FindAll(Permissions.ClaimType).Select(c => c.Value)));
}