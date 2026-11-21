// // Register Yarp reverse services
// var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddReverseProxy()
//     .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


// var app = builder.Build();

// app.MapGet("/health", () => "Health status");

// app.MapReverseProxy();

// app.Run();
