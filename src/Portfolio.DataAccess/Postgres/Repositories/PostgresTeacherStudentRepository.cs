namespace Portfolio.DataAccess.Postgres.Repositories;

internal sealed class PostgresTeacherStudentRepository : ITeacherStudentRepository
{
    private readonly PortfolioDbContext _context;

    public PostgresTeacherStudentRepository(PortfolioDbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<bool> CreateLinkAsync(
        string teacherId,
        string studentId,
        CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .AsNoTracking()
            .Where(user => user.Id == teacherId || user.Id == studentId)
            .Select(user => new { user.Id, user.Roles })
            .ToListAsync(cancellationToken);

        var isTeacher = users.Any(user => user.Id == teacherId && user.Roles.Contains("teacher"));
        var isStudent = users.Any(user => user.Id == studentId && user.Roles.Contains("student"));
        if (!isTeacher || !isStudent)
            return false;

        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO "TeacherStudentLinks" ("TeacherId", "StudentId")
            VALUES ({teacherId}, {studentId})
            ON CONFLICT ("TeacherId", "StudentId") DO NOTHING
            """,
            cancellationToken);

        return true;
    }
}
