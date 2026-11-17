using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var jwt = builder.Configuration.GetSection("Jwt");
var signingKey = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key must be configured.");
if (Encoding.UTF8.GetByteCount(signingKey) < 32)
    throw new InvalidOperationException("Jwt:Key must contain at least 32 UTF-8 bytes.");

// The gateway validates tokens before YARP forwards requests to internal services.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = jwt["Issuer"],
        ValidateAudience = true, ValidAudience = jwt["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30),
        NameClaimType = ClaimTypes.Name, RoleClaimType = ClaimTypes.Role
    };
});
builder.Services.AddAuthorization(options =>
{
    // Permission policies are more flexible than role checks: a role is just a bundle of claims.
    options.AddPolicy("CatalogRead", policy => policy.RequireClaim("permission", "catalog.read"));
    options.AddPolicy("CatalogManage", policy => policy.RequireClaim("permission", "catalog.manage"));
    options.AddPolicy("RegistrationsRead", policy => policy.RequireClaim("permission", "registrations.read"));
    options.AddPolicy("RegistrationsCreate", policy => policy.RequireClaim("permission", "registrations.create"));
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
});
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { service = "gateway", status = "healthy" }));
app.MapReverseProxy();
app.Run();
