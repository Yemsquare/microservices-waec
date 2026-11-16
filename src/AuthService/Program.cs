using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var jwt = builder.Configuration.GetSection("Jwt");
var signingKey = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key must be configured.");
if (Encoding.UTF8.GetByteCount(signingKey) < 32)
    throw new InvalidOperationException("Jwt:Key must contain at least 32 UTF-8 bytes.");

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Auth")
        ?? throw new InvalidOperationException("ConnectionStrings:Auth is required.")));

builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    // Sensible workshop defaults. Production systems should also configure lockout and MFA.
    options.Password.RequiredLength = 10;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AuthDbContext>()
.AddSignInManager();

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization(options =>
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrator")));

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

// Create the Identity schema for this disposable workshop. Use EF migrations in production.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var attempts = 0;
    while (true)
    {
        try { await db.Database.EnsureCreatedAsync(); break; }
        catch when (++attempts < 40) { await Task.Delay(TimeSpan.FromSeconds(3)); }
    }

    foreach (var role in new[] { "Administrator", "Examiner", "Candidate" })
        if (!await roles.RoleExistsAsync(role))
            await roles.CreateAsync(new IdentityRole(role));

    // The first administrator is provisioned from environment variables, never from a public endpoint.
    var adminEmail = builder.Configuration["BootstrapAdmin:Email"];
    var adminPassword = builder.Configuration["BootstrapAdmin:Password"];
    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword)
        && await users.FindByEmailAsync(adminEmail) is null)
    {
        var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var created = await users.CreateAsync(admin, adminPassword);
        if (created.Succeeded) await users.AddToRoleAsync(admin, "Administrator");
        else app.Logger.LogError("Bootstrap administrator could not be created: {Errors}",
            string.Join("; ", created.Errors.Select(e => e.Description)));
    }
}

app.MapGet("/health", () => Results.Ok(new { service = "auth", status = "healthy" }));

app.MapPost("/api/auth/register", async (RegisterRequest request, UserManager<IdentityUser> users) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        return Results.BadRequest(new { message = "Email and password are required." });

    var user = new IdentityUser { UserName = request.Email.Trim(), Email = request.Email.Trim() };
    var result = await users.CreateAsync(user, request.Password);
    if (!result.Succeeded)
        return Results.ValidationProblem(result.Errors.GroupBy(e => e.Code)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));

    // Public self-registration can only create Candidates. Clients cannot choose privileged roles.
    await users.AddToRoleAsync(user, "Candidate");
    return Results.Created($"/api/auth/users/{user.Id}", new { user.Id, user.Email, role = "Candidate" });
});

app.MapPost("/api/auth/login", async (LoginRequest request, UserManager<IdentityUser> users,
    SignInManager<IdentityUser> signIn) =>
{
    var user = await users.FindByEmailAsync(request.Email.Trim());
    if (user is null || !(await signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true)).Succeeded)
        return Results.Unauthorized();

    var roles = await users.GetRolesAsync(user);
    var now = DateTime.UtcNow;
    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id),
        new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        new(ClaimTypes.Name, user.Email ?? user.UserName ?? user.Id),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };
    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

    var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
        SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(jwt["Issuer"], jwt["Audience"], claims,
        now, now.AddMinutes(int.TryParse(jwt["AccessTokenMinutes"], out var minutes) ? minutes : 60), credentials);

    return Results.Ok(new
    {
        accessToken = new JwtSecurityTokenHandler().WriteToken(token),
        tokenType = "Bearer",
        expiresAtUtc = token.ValidTo,
        user = new { user.Id, user.Email, roles }
    });
});

app.MapGet("/api/auth/me", (ClaimsPrincipal user) => Results.Ok(new
{
    id = user.FindFirstValue(JwtRegisteredClaimNames.Sub),
    email = user.FindFirstValue(JwtRegisteredClaimNames.Email),
    roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value)
})).RequireAuthorization();

app.MapPost("/api/auth/users", async (CreateStaffRequest request, UserManager<IdentityUser> users) =>
{
    if (request.Role is not ("Examiner" or "Administrator"))
        return Results.BadRequest(new { message = "Only Examiner or Administrator can be provisioned here." });

    var user = new IdentityUser { UserName = request.Email.Trim(), Email = request.Email.Trim(), EmailConfirmed = true };
    var created = await users.CreateAsync(user, request.TemporaryPassword);
    if (!created.Succeeded)
        return Results.ValidationProblem(created.Errors.GroupBy(e => e.Code)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));

    await users.AddToRoleAsync(user, request.Role);
    return Results.Created($"/api/auth/users/{user.Id}", new { user.Id, user.Email, role = request.Role });
}).RequireAuthorization("AdminOnly");

app.Run();

public sealed record RegisterRequest(string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record CreateStaffRequest(string Email, string TemporaryPassword, string Role);

// Identity owns authentication data separately from examination-registration data.
public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options)
    : IdentityDbContext<IdentityUser>(options);
