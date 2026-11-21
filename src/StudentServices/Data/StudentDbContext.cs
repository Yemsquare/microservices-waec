using Microsoft.EntityFrameworkCore;
using StudentService.Models;

namespace StudentService.Data;

public class StudentDbContext : DbContext
{
    public StudentDbContext(
        DbContextOptions<StudentDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Student>(entity =>
        {
            // set the primary key 
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.EmailAddress).IsUnique();
            entity.Property(e => e.DateOfBirth).IsRequired();
        });
    }
}


