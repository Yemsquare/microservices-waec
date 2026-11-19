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

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default) =>
        db.Database.BeginTransactionAsync(ct);
}