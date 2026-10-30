namespace Portfolio.DataAccess.Postgres.Repositories;

/// <summary>
/// EF Core adapter that implements the <see cref="IUserRepository"/> port
/// (defined in <c>Portfolio.Domain</c>) on top of PostgreSQL.
/// <para>
/// Responsible for translating between the persistence model
/// (<see cref="UserDbModel"/>) and the domain aggregate (<see cref="User"/>)
/// using Mapster. The domain layer knows nothing about this class — only about
/// the interface it implements.
/// </para>
/// </summary>
internal sealed class PostgresUserRepository : IUserRepository
{
    private readonly PortfolioDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresUserRepository"/> class.
    /// </summary>
    /// <param name="context">The EF Core <see cref="PortfolioDbContext"/> used to access the database.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public PostgresUserRepository(PortfolioDbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Retrieves a user by their email address.
    /// The email is normalized (trimmed and lower-cased) before querying, so lookups
    /// are case-insensitive from the caller's perspective regardless of how the
    /// value is stored in the database.
    /// Uses <c>AsNoTracking</c> because the returned instance is rehydrated into
    /// a domain entity and will not be mutated by the change tracker.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The domain <see cref="User"/> if found; otherwise, <c>null</c>.</returns>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var normalized = email.Trim().ToLowerInvariant();

        var db = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalized, ct);

        return db?.Adapt<User>();
    }

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// Uses <c>AsNoTracking</c> because the returned instance is rehydrated into
    /// a domain entity and will not be mutated by the change tracker.
    /// </summary>
    /// <param name="id">The user's identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The domain <see cref="User"/> if found; otherwise, <c>null</c>.</returns>
    public async Task<User?> GetByIdAsync(string id, CancellationToken ct)
    {
        var db = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        return db?.Adapt<User>();
    }

    /// <summary>
    /// Persists a new user.
    /// The domain aggregate is projected into <see cref="UserDbModel"/>
    /// before being added to the change tracker.
    /// </summary>
    /// <param name="user">The domain user to persist.</param>
    /// <param name="ct">The cancellation token.</param>
    public async Task AddAsync(User user, CancellationToken ct)
    {
        await _context.Users.AddAsync(user.Adapt<UserDbModel>(), ct);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Updates an existing user with the current state of the domain aggregate.
    /// Loads the tracked entity first so EF can compute the UPDATE statement.
    /// <para>
    /// Property assignment is done explicitly (instead of <c>user.Adapt(db)</c>) on purpose:
    /// it makes the persisted field set visible at a glance, prevents accidental overwrites
    /// of columns that the domain does not control, and forces a compile-time review
    /// whenever the aggregate gains a new field that needs to be persisted.
    /// </para>
    /// </summary>
    /// <param name="user">The domain user containing the updated state.</param>
    /// <param name="ct">The cancellation token.</param>
    public async Task UpdateAsync(User user, CancellationToken ct)
    {
        var db = await _context.Users.FindAsync([user.Id], ct);
        if (db is null) return;

        db.Email = user.Email;
        db.PasswordHash = user.PasswordHash;
        db.Roles = user.Roles.ToList();
        db.TwoFactorEnabled = user.TwoFactorEnabled;
        db.TwoFactorSecret = user.TwoFactorSecret;
        db.TwoFactorRecoveryCodes = user.TwoFactorRecoveryCodes.ToList();
        db.EmailConfirmed = user.EmailConfirmed;
        db.TwoFactorEnabledAt = user.TwoFactorEnabledAt;
        db.AccessFailedCount = user.AccessFailedCount;
        db.LockoutEnd = user.LockoutEnd;
        db.LastLoginAt = user.LastLoginAt;

        await _context.SaveChangesAsync(ct);
    }
}