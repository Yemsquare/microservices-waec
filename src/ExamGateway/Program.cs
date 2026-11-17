using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Register OpenAPI generation and interactive API documentation.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WAEC API Gateway API",
        Version = "v1",
        Description = "Gateway health and entry-point documentation. Downstream service APIs are listed in the aggregated Swagger UI."
    });

    // Let learners provide a JWT access token using Swagger's Authorize button.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT access token. Swagger adds the Bearer prefix."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
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

// Serve the OpenAPI document and interactive Swagger UI.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "WAEC API Gateway API V1");
    options.RoutePrefix = "swagger";
});
// Host one Swagger UI that aggregates the gateway and all downstream microservice specifications.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "WAEC API Gateway V1");
    options.SwaggerEndpoint("/swagger/auth/v1/swagger.json", "Authentication Service");
    options.SwaggerEndpoint("/swagger/catalog/v1/swagger.json", "Catalog Service");
    options.SwaggerEndpoint("/swagger/orders/v1/swagger.json", "Orders Service");
    options.SwaggerEndpoint("/swagger/registration/v1/swagger.json", "Registration Service");
    options.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { service = "gateway", status = "healthy" }));
app.MapReverseProxy();
app.Run();
