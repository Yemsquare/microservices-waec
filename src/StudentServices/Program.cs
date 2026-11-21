using Microsoft.EntityFrameworkCore;
using StudentService.Data;
using StudentService.Repositories;
using StudentService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Register Student Service Database
builder.Services.AddDbContext<StudentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StudentDatabase")));

// Register Student Service Repository
builder.Services.AddScoped<IStudentRepository, StudentRepository>();

// Register API documenttaion
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c=>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Student Service API",
        Version = "v1"
    });
});

// Register application service 
builder.Services.AddScoped<IStudentService, StudentService.Services.StudentService>();

// configure standard API error 
builder.Services.AddProblemDetails();

var app = builder.Build();

// Enable swagger in development environment

app.UseSwagger();
app.UseSwaggerUI();


// convert unhandled exceptions to ProblemDetails responses
app.UseExceptionHandler();

// Map controller endpoints
app.MapControllers();

app.Run();