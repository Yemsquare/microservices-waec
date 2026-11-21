using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. JWT CONFIGURATION
// ==========================================

var jwt = builder.Configuration.GetSection("Jwt");

var signingKey = jwt["Key"]
    ?? throw new InvalidOperationException("Jwt:Key must be configured.");

if (Encoding.UTF8.GetByteCount(signingKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key must contain at least 32 UTF-8 bytes.");
}

// ==========================================
// 2. SWAGGER CONFIGURATION
// ==========================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "WAEC Exam Gateway API",
        Version = "v1",
        Description = "Central API Gateway for the WAEC Examination Management System."
    });

    // Enable JWT Bearer authentication in Swagger
    c.AddSecurityDefinition("Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Enter your JWT token."
        });

    c.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

// ==========================================
// 3. JWT AUTHENTICATION
// ==========================================

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwt["Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(signingKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

// ==========================================
// 4. AUTHORIZATION POLICIES
// ==========================================

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CatalogRead",
        policy => policy.RequireClaim("permission", "catalog.read"));

    options.AddPolicy("CatalogManage",
        policy => policy.RequireClaim("permission", "catalog.manage"));

    options.AddPolicy("RegistrationsRead",
        policy => policy.RequireClaim("permission", "registrations.read"));

    options.AddPolicy("RegistrationsCreate",
        policy => policy.RequireClaim("permission", "registrations.create"));

    options.AddPolicy("Authenticated",
        policy => policy.RequireAuthenticatedUser());
});

// ==========================================
// 5. YARP REVERSE PROXY
// ==========================================

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ==========================================
// 6. APPLICATION PIPELINE
// ==========================================

var app = builder.Build();

// Swagger
app.UseSwagger();

app.UseSwaggerUI(options =>
{
    // Gateway documentation
    options.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "Exam Gateway API");

    // Authentication Service
    options.SwaggerEndpoint(
        "/swagger/auth/v1/swagger.json",
        "Authentication Service");

    // Student Service
    options.SwaggerEndpoint(
        "/swagger/students/v1/swagger.json",
        "Student Management Service");

    // Registration Service
    options.SwaggerEndpoint(
        "/swagger/registration/v1/swagger.json",
        "Registration Service");

    // Catalog Service
    options.SwaggerEndpoint(
        "/swagger/catalog/v1/swagger.json",
        "Examination Catalog Service");

    // Orders Service
    options.SwaggerEndpoint(
        "/swagger/orders/v1/swagger.json",
        "Orders Service");

    options.RoutePrefix = "swagger";
});

// Authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Health check
app.MapGet("/health", () =>
    Results.Ok(new
    {
        service = "gateway",
        status = "healthy"
    }));

// Reverse proxy
app.MapReverseProxy();

app.Run();