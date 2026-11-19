using Microsoft.AspNetCore.Identity;

namespace AuthService.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = "";
    public IdentityUser User { get; set; } = null!;
    public string TokenHash { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public bool IsActive(DateTime nowUtc) => RevokedAtUtc is null && ExpiresAtUtc > nowUtc;
}