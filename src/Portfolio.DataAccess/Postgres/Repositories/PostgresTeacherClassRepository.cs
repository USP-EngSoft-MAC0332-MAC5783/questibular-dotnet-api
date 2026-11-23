namespace Portfolio.DataAccess.Postgres.Repositories;

internal sealed class PostgresTeacherClassRepository : ITeacherClassRepository
{
    private readonly PortfolioDbContext _context;

    public PostgresTeacherClassRepository(PortfolioDbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return _context.TeacherClasses.AsNoTracking().AnyAsync(c => c.Code == normalized, ct);
    }

    public async Task<TeacherClass?> GetByCodeAsync(string code, CancellationToken ct)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var db = await _context.TeacherClasses.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == normalized, ct);
        return db?.Adapt<TeacherClass>();
    }

    public async Task<IReadOnlyList<TeacherClass>> GetByTeacherIdAsync(string teacherId, CancellationToken ct)
    {
        var rows = await _context.TeacherClasses.AsNoTracking()
            .Where(c => c.TeacherId == teacherId)
            .OrderBy(c => c.Code)
            .ToListAsync(ct);

        return rows.Select(c => c.Adapt<TeacherClass>()).ToList();
    }

    public async Task AddAsync(TeacherClass teacherClass, CancellationToken ct)
    {
        await _context.TeacherClasses.AddAsync(teacherClass.Adapt<TeacherClassDbModel>(), ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(string teacherId, string code, CancellationToken ct)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var affected = await _context.TeacherClasses
            .Where(c => c.TeacherId == teacherId && c.Code == normalized)
            .ExecuteDeleteAsync(ct);

        return affected > 0;
    }
}