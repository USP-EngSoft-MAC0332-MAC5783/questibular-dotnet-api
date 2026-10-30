namespace Portfolio.DataAccess.Postgres.Repositories;

public sealed class PostgresUserRepository : IUserRepository
{
    private readonly PortfolioDbContext _context;

    public PostgresUserRepository(PortfolioDbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var db = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);
        return db?.Adapt<User>();
    }

    public async Task<User?> GetByIdAsync(string id, CancellationToken ct)
    {
        var db = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
        return db?.Adapt<User>();
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        var db = user.Adapt<UserDbModel>();
        await _context.Users.AddAsync(db, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct)
    {
        var db = await _context.Users.FindAsync([user.Id], ct);
        if (db is null) return;

        user.Adapt(db);
        await _context.SaveChangesAsync(ct);
    }
}