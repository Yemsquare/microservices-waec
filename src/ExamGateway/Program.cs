var builder = WebApplication.CreateBuilder(args);
// YARP loads route and destination configuration from appsettings and environment variables.
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
var app = builder.Build();
app.MapGet("/health", () => Results.Ok(new { service = "gateway", status = "healthy" }));
// Consumers call the gateway; it forwards /api/* requests to internal services.
app.MapReverseProxy();
app.Run();
