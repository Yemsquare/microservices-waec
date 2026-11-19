using AuthService.Data;
using AuthService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthService.Repositories;

public sealed class RefreshTokenRepository(AuthDbContext db) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken token, CancellationToken ct = default) =>
        await db.RefreshTokens.AddAsync(token, ct);

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, bool includeUser, CancellationToken ct = default)
    {
        IQueryable<RefreshToken> query = db.RefreshTokens;
        if (includeUser) query = query.Include(x => x.User);
        return query.SingleOrDefaultAsync(x => x.TokenHash == tokenHash, ct);
    }

     // Follows ReplacedByTokenHash forward and revokes each descendant. The cap guards against a corrupted (cyclic) chain.
    public async Task RevokeChainAsync(string firstTokenHash, DateTime nowUtc, CancellationToken ct = default)
    {
        string? hash = firstTokenHash;
        for (var i = 0; i < 100 && hash is not null; i++)
        {
            var token = await db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == hash, ct);
            if (token is null) break;
            token.RevokedAtUtc ??= nowUtc;
            hash = token.ReplacedByTokenHash;
        }
        await db.SaveChangesAsync(ct);
    }
    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default) =>
        db.Database.BeginTransactionAsync(ct);
}