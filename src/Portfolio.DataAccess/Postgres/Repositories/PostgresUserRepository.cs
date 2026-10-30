namespace Portfolio.DataAccess.Postgres.Repositories;

internal sealed class PostgresUserRepository : IUserRepository
{
    private readonly PortfolioDbContext _context;

    public PostgresUserRepository(PortfolioDbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var normalized = email.Trim().ToLowerInvariant();

        var db = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalized, ct);

        return db?.Adapt<User>();
    }

    public async Task<User?> GetByIdAsync(string id, CancellationToken ct)
    {
        var db = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        return db?.Adapt<User>();
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await _context.Users.AddAsync(user.Adapt<UserDbModel>(), ct);
        await _context.SaveChangesAsync(ct);
    }

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