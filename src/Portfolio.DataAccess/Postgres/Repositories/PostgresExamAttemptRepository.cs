namespace Portfolio.DataAccess.Postgres.Repositories;

internal sealed class PostgresExamAttemptRepository : IExamAttemptRepository
{
    private readonly PortfolioDbContext _context;

    public PostgresExamAttemptRepository(PortfolioDbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<ExamAttempt?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var db = await _context.ExamAttempts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);
            
        return db?.Adapt<ExamAttempt>();
    }

    public async Task AddAsync(ExamAttempt attempt, CancellationToken ct = default)
    {
        await _context.ExamAttempts.AddAsync(attempt.Adapt<ExamAttemptDbModel>(), ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ExamAttempt attempt, CancellationToken ct = default)
    {
        var db = await _context.ExamAttempts.FindAsync([attempt.Id], ct);
        if (db is null) return;

        db.FinishedAt = attempt.FinishedAt;
        db.Answers = attempt.Answers.Adapt<List<QuestionAnswerDbModel>>();
        db.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
    }

    public Task<int> DeleteExpiredAnonymousAttemptsAsync(DateTime now, CancellationToken ct = default) =>
        _context.ExamAttempts
            .Where(a => a.IsAnonymous && a.ExpiresAt != null && a.ExpiresAt < now)
            .ExecuteDeleteAsync(ct);
}