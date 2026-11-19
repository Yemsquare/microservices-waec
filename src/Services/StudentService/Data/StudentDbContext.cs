using Microsoft.EntityFrameworkCore;
using StudentService.Models;

namespace StudentService.Data;

public class StudentDbContext(DbContextOptions<StudentDbContext> options)
    : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(student => student.Id);
            entity.Property(student => student.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(student => student.LastName).HasMaxLength(100).IsRequired();
            entity.Property(student => student.Email).HasMaxLength(150).IsRequired();
            entity.Property(student => student.RegistrationNumber).HasMaxLength(50).IsRequired();
            entity.Property(student => student.ExamType).HasMaxLength(50).IsRequired();
            entity.HasIndex(student => student.Email).IsUnique();
            entity.HasIndex(student => student.RegistrationNumber).IsUnique();
        });
    }
}
