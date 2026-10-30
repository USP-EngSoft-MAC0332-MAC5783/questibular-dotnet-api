namespace Portfolio.DataAccess.Postgres.Repositories;

internal sealed class PostgresRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly PortfolioDbContext _context;

    public PostgresRefreshTokenRepository(PortfolioDbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct)
    {
        var db = await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        return db?.Adapt<RefreshToken>();
    }

    public async Task AddAsync(RefreshToken token, CancellationToken ct)
    {
        await _context.RefreshTokens.AddAsync(token.Adapt<RefreshTokenDbModel>(), ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RefreshToken token, CancellationToken ct)
    {
        var db = await _context.RefreshTokens.FindAsync([token.Id], ct);
        if (db is null) return;

        db.RevokedAt = token.RevokedAt;
        db.RevokedByIp = token.RevokedByIp;
        db.ReplacedByTokenHash = token.ReplacedByTokenHash;

        await _context.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(string userId, string? ip, DateTime now, CancellationToken ct)
    {
        await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.RevokedAt, now)
                .SetProperty(t => t.RevokedByIp, ip), ct);
    }
}