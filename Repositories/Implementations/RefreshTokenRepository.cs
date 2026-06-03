using BookingSystem.Data;
using BookingSystem.Entities;
using BookingSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Repositories.Implementations;

public class RefreshTokenRepository(BookingContext context) : IRefreshTokenRepository
{
    public async Task CreateAsync(RefreshToken token, CancellationToken ct = default)
    {
        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync(ct);
    }

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default) =>
        await context.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash, ct);

    public async Task RevokeAsync(RefreshToken token, CancellationToken ct = default)
    {
        token.IsRevoked = true;
        await context.SaveChangesAsync(ct);
    }

    public async Task RevokeAllAsync(Guid userId, CancellationToken ct = default)
    {
        await context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsRevoked, true), ct);
    }
}
