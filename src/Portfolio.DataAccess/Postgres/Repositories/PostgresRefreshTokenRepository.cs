namespace Portfolio.DataAccess.Postgres.Repositories;

public sealed class PostgresRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly PortfolioDbContext _context;

    public PostgresRefreshTokenRepository(PortfolioDbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct)
    {
        var db = await _context.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);
        return db is null ? null : MapToDomain(db);
    }

    public async Task AddAsync(RefreshToken token, CancellationToken ct)
    {
        await _context.RefreshTokens.AddAsync(MapToDb(token), ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RefreshToken token, CancellationToken ct)
    {
        var db = await _context.RefreshTokens.FindAsync(new object[] { token.Id }, ct);
        if (db is null) return;

        db.RevokedAt = token.RevokedAt;
        db.RevokedByIp = token.RevokedByIp;
        db.ReplacedByTokenHash = token.ReplacedByTokenHash;

        await _context.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(string userId, string? ip, DateTime now, CancellationToken ct)
    {
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var t in tokens)
        {
            t.RevokedAt = now;
            t.RevokedByIp = ip;
        }

        await _context.SaveChangesAsync(ct);
    }

    private static RefreshToken MapToDomain(RefreshTokenDbModel db)
        => RefreshToken.Reconstitute(
            db.Id, db.UserId, db.TokenHash, db.ExpiresAt, 
            db.RevokedAt, db.ReplacedByTokenHash, db.CreatedByIp, 
            db.RevokedByIp, db.CreatedAt);

    private static RefreshTokenDbModel MapToDb(RefreshToken token)
        => new()
        {
            Id = token.Id,
            UserId = token.UserId,
            TokenHash = token.TokenHash,
            ExpiresAt = token.ExpiresAt,
            RevokedAt = token.RevokedAt,
            ReplacedByTokenHash = token.ReplacedByTokenHash,
            CreatedByIp = token.CreatedByIp,
            RevokedByIp = token.RevokedByIp,
            CreatedAt = token.CreatedAt
        };
}