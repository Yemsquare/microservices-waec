using AuthService.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthService.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    Task<RefreshToken?> GetByHashAsync(string tokenHash, bool includeUser, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
}