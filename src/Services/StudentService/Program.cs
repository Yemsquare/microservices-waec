using Microsoft.EntityFrameworkCore;
using StudentService.Data;
using StudentService.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("StudentDatabase")
    ?? throw new InvalidOperationException("ConnectionStrings:StudentDatabase is missing.");

builder.Services.AddDbContext<StudentDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<StudentService.Services.StudentService>();

var app = builder.Build();

// EnsureCreated is used for this introductory training container so the database/table
// is created automatically. For production, replace this with reviewed EF Core migrations.
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
    var retries = 0;
    while (true)
    {
        try
        {
            await db.Database.EnsureCreatedAsync();
            break;
        }
        catch when (retries++ < 15)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/health", () => Results.Ok(new { service = "StudentService", status = "healthy" }));
app.MapControllers();
app.Run();
