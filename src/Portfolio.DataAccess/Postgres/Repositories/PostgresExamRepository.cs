namespace Portfolio.DataAccess.Postgres.Repositories;

internal sealed class PostgresExamRepository : IExamRepository
{
    private readonly PortfolioDbContext _context;

    public PostgresExamRepository(PortfolioDbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<Exam?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var db = await _context.Exams
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct);
            
        return db?.Adapt<Exam>();
    }

    public async Task<Exam?> GetByYearAsync(string year, CancellationToken ct = default)
    {
        var db = await _context.Exams
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Year == year, ct);
            
        return db?.Adapt<Exam>();
    }

    public async Task<List<Exam>> GetByYearRangeAsync(int startYear, int endYear, CancellationToken ct = default)
    {
        var startStr = startYear.ToString();
        var endStr = endYear.ToString();

        var dbs = await _context.Exams
            .AsNoTracking()
            .Where(e => e.Year.CompareTo(startStr) >= 0 && e.Year.CompareTo(endStr) <= 0)
            .OrderBy(e => e.Year)
            .ToListAsync(ct);

        return dbs.Adapt<List<Exam>>();
    }

    public async Task<List<Exam>> GetPendingSyncAsync(int take, CancellationToken ct = default)
    {
        var dbs = await _context.Exams
            .AsNoTracking()
            .Where(e => !e.IsSynced)
            .OrderBy(e => e.Year)
            .Take(take)
            .ToListAsync(ct);

        return dbs.Adapt<List<Exam>>();
    }

    public async Task AddAsync(Exam exam, CancellationToken ct = default)
    {
        await _context.Exams.AddAsync(exam.Adapt<ExamDbModel>(), ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<Exam> exams, CancellationToken ct = default)
    {
        try
        {
            await _context.Exams.AddRangeAsync(exams.Adapt<List<ExamDbModel>>(), ct);
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _context.ChangeTracker.Clear();
        }
    }

    public async Task UpdateAsync(Exam exam, CancellationToken ct = default)
    {
        var db = await _context.Exams.FindAsync([exam.Id], ct);
        if (db is null) return;

        db.Title = exam.Title;
        db.QuestionCountHumanities = exam.QuestionCountHumanities;
        db.QuestionCountNature = exam.QuestionCountNature;
        db.QuestionCountLanguages = exam.QuestionCountLanguages;
        db.QuestionCountMath = exam.QuestionCountMath;
        db.IsSynced = exam.IsSynced;
        db.Languages = exam.Languages.ToList();
        db.Disciplines = exam.Disciplines.ToList();
        db.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
    }

    public Task IncrementStartAsync(string examId, DateTime now, CancellationToken ct = default) =>
        _context.Exams
            .Where(e => e.Id == examId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.StartCount, e => e.StartCount + 1)
                .SetProperty(e => e.LastStartedAt, now)
                .SetProperty(e => e.UpdatedAt, now), ct);

    public Task IncrementFinishAsync(string examId, int questionsFinished, DateTime now, CancellationToken ct = default) =>
        _context.Exams
            .Where(e => e.Id == examId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.FinishCount, e => e.FinishCount + 1)
                .SetProperty(e => e.TotalQuestionsFinished, e => e.TotalQuestionsFinished + questionsFinished)
                .SetProperty(e => e.LastFinishedAt, now)
                .SetProperty(e => e.UpdatedAt, now), ct);

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true ||
        ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true ||
        ex.InnerException?.Message.Contains("23505") == true;
}