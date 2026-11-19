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
[Route("api/auth/users")]
[Authorize(Policy = Permissions.Policies.AdminOnly)]
public sealed class UsersController(IUserManagementService management) : ControllerBase
{
    private string? ActorId => User.FindFirstValue(JwtRegisteredClaimNames.Sub);

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await management.ListAsync(ct));

    [HttpPost]
    public async Task<IActionResult> CreateStaff(CreateStaffRequest request, CancellationToken ct)
    {
        var result = await management.CreateStaffAsync(request, ct);
        return result.Succeeded
            ? Created($"/api/auth/users/{result.Value!.Id}", result.Value)
            : this.ToProblem(result);
    }

    [HttpPut("{userId}/role")]
    public async Task<IActionResult> ChangeRole(string userId, ChangeRoleRequest request, CancellationToken ct)
    {
        var result = await management.ChangeRoleAsync(userId, ActorId, request, ct);
        return result.Succeeded ? Ok(result.Value) : this.ToProblem(result);
    }

    [HttpPut("{userId}/status")]
    public async Task<IActionResult> ChangeStatus(string userId, ChangeStatusRequest request, CancellationToken ct)
    {
        var result = await management.ChangeStatusAsync(userId, ActorId, request, ct);
        return result.Succeeded ? Ok(result.Value) : this.ToProblem(result);
    }
}