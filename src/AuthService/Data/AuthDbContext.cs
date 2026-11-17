using AuthService.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace AuthService.Data;
public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options):IdentityDbContext<IdentityUser>(options) { public DbSet<RefreshToken> RefreshTokens=>Set<RefreshToken>(); protected override void OnModelCreating(ModelBuilder b){base.OnModelCreating(b);b.Entity<RefreshToken>().HasIndex(x=>x.TokenHash).IsUnique();b.Entity<RefreshToken>().Property(x=>x.RowVersion).IsRowVersion();b.Entity<RefreshToken>().HasOne(x=>x.User).WithMany().HasForeignKey(x=>x.UserId);} }