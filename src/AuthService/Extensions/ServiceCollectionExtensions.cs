using System.Security.Claims;
using System.Text;
using AuthService.Data;
using AuthService.Options;
using AuthService.Repositories;
using AuthService.Security;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthPersistence(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AuthDbContext>(o => o.UseSqlServer(
            config.GetConnectionString("Auth")
            ?? throw new InvalidOperationException("ConnectionStrings:Auth is required.")));

        services.AddIdentityCore<IdentityUser>(o =>
        {
            o.Password.RequiredLength = 10;
            o.Password.RequireDigit = true;
            o.Password.RequireUppercase = true;
            o.Password.RequireLowercase = true;
            o.Password.RequireNonAlphanumeric = true;
            o.User.RequireUniqueEmail = true;
            o.Lockout.AllowedForNewUsers = true;
            o.Lockout.MaxFailedAccessAttempts = 5;
            o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AuthDbContext>()
        .AddSignInManager();

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<AuthDbInitializer>();
        return services;
    }

    public static IServiceCollection AddAuthApplication(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        return services;
    }

    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<JwtOptions>()
            .Bind(config.GetSection(JwtOptions.SectionName))
            .Validate(o => Encoding.UTF8.GetByteCount(o.Key) >= 32, "Jwt:Key must contain at least 32 UTF-8 bytes.")
            .ValidateOnStart();
        services.Configure<BootstrapAdminOptions>(config.GetSection(BootstrapAdminOptions.SectionName));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                bearer.MapInboundClaims = false; // keep "sub"/"email" names as issued
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, ValidIssuer = jwt.Issuer,
                    ValidateAudience = true, ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = ClaimTypes.Name, RoleClaimType = ClaimTypes.Role
                };
            });

        services.AddAuthorization(o => o.AddPolicy(Permissions.Policies.AdminOnly,
            p => p.RequireClaim(Permissions.ClaimType, Permissions.UsersManage)));
        return services;
    }
}