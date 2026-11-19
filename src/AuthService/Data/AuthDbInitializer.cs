using System.Security.Claims;
using AuthService.Options;
using AuthService.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AuthService.Data;

public sealed class AuthDbInitializer(
    AuthDbContext db,
    RoleManager<IdentityRole> roles,
    UserManager<IdentityUser> users,
    IOptions<BootstrapAdminOptions> bootstrap,
    ILogger<AuthDbInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await EnsureDatabaseAsync(ct);
        await SeedRolesAsync();
        await SeedAdministratorAsync();
    }

    // Workshop shortcut: use EF migrations in production.
    private async Task EnsureDatabaseAsync(CancellationToken ct)
    {
        var attempts = 0;
        while (true)
        {
            try { await db.Database.EnsureCreatedAsync(ct); return; }
            catch when (++attempts < 40) { await Task.Delay(TimeSpan.FromSeconds(3), ct); }
        }
    }

    private async Task SeedRolesAsync()
    {
        foreach (var (roleName, permissions) in Permissions.ByRole)
        {
            var role = await roles.FindByNameAsync(roleName);
            if (role is null)
            {
                role = new IdentityRole(roleName);
                await roles.CreateAsync(role);
            }

            foreach (var claim in (await roles.GetClaimsAsync(role)).Where(c => c.Type == Permissions.ClaimType))
                await roles.RemoveClaimAsync(role, claim);
            foreach (var permission in permissions)
                await roles.AddClaimAsync(role, new Claim(Permissions.ClaimType, permission));
        }
    }

    private async Task SeedAdministratorAsync()
    {
        var (email, password) = (bootstrap.Value.Email, bootstrap.Value.Password);
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)
            || await users.FindByEmailAsync(email) is not null) return;

        var admin = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var created = await users.CreateAsync(admin, password);
        if (created.Succeeded) await users.AddToRoleAsync(admin, Roles.Administrator);
        else logger.LogError("Bootstrap administrator creation failed: {Errors}",
            string.Join("; ", created.Errors.Select(e => e.Description)));
    }
}