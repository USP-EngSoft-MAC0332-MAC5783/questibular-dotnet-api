namespace Portfolio.DataAccess.Postgres.Repositories;

/// <summary>
/// EF Core adapter that implements the <see cref="IRefreshTokenRepository"/> port
/// (defined in <c>Portfolio.Domain</c>) on top of PostgreSQL.
/// <para>
/// Responsible for translating between the persistence model
/// (<see cref="RefreshTokenDbModel"/>) and the domain aggregate
/// (<see cref="RefreshToken"/>) using Mapster. The domain layer knows
/// nothing about this class — only about the interface it implements.
/// </para>
/// </summary>
internal sealed class PostgresRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly PortfolioDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresRefreshTokenRepository"/> class.
    /// </summary>
    /// <param name="context">The EF Core <see cref="PortfolioDbContext"/> used to access the database.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public PostgresRefreshTokenRepository(PortfolioDbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Retrieves a refresh token by its cryptographic hash.
    /// Uses <c>AsNoTracking</c> because the returned instance is rehydrated into
    /// a domain entity and will not be mutated by the change tracker.
    /// </summary>
    /// <param name="tokenHash">The hashed refresh token.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The domain <see cref="RefreshToken"/> if found; otherwise, <c>null</c>.</returns>
    public async Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct)
    {
        var db = await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        return db?.Adapt<RefreshToken>();
    }

    /// <summary>
    /// Persists a new refresh token.
    /// The domain aggregate is projected into <see cref="RefreshTokenDbModel"/>
    /// before being added to the change tracker.
    /// </summary>
    /// <param name="token">The domain refresh token to persist.</param>
    /// <param name="ct">The cancellation token.</param>
    public async Task AddAsync(RefreshToken token, CancellationToken ct)
    {
        await _context.RefreshTokens.AddAsync(token.Adapt<RefreshTokenDbModel>(), ct);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Updates the mutable fields of an existing refresh token.
    /// Only revocation-related fields are updated here because that is the only
    /// state transition allowed by the domain after issuance
    /// (see <see cref="RefreshToken.Revoke"/>); immutable fields are intentionally ignored.
    /// </summary>
    /// <param name="token">The domain refresh token containing the new state.</param>
    /// <param name="ct">The cancellation token.</param>
    public async Task UpdateAsync(RefreshToken token, CancellationToken ct)
    {
        var db = await _context.RefreshTokens.FindAsync([token.Id], ct);
        if (db is null) return;

        db.RevokedAt = token.RevokedAt;
        db.RevokedByIp = token.RevokedByIp;
        db.ReplacedByTokenHash = token.ReplacedByTokenHash;

        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Revokes all active refresh tokens for a given user in a single database round-trip.
    /// Uses <c>ExecuteUpdateAsync</c> to issue a bulk UPDATE without materializing entities in memory.
    /// </summary>
    /// <param name="userId">The identifier of the user whose tokens should be revoked.</param>
    /// <param name="ip">The IP address from which the revocation was requested (for audit).</param>
    /// <param name="now">The UTC date and time of revocation.</param>
    /// <param name="ct">The cancellation token.</param>
    public async Task RevokeAllForUserAsync(string userId, string? ip, DateTime now, CancellationToken ct)
    {
        await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.RevokedAt, now)
                .SetProperty(t => t.RevokedByIp, ip), ct);
    }
}