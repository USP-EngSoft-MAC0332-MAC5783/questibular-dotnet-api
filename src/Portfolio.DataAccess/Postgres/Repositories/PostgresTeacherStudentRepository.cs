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

    public async Task<bool> HasLinkAsync(string teacherId, string studentId, CancellationToken ct)
    {
        return await _context.Database
            .SqlQuery<bool>($"""
                SELECT EXISTS(
                    SELECT 1
                    FROM "TeacherStudentLinks"
                    WHERE "TeacherId" = {teacherId} AND "StudentId" = {studentId}
                ) AS "Value"
                """)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<(string Id, string Name, string Email)>> GetLinkedStudentsAsync(
        string teacherId,
        CancellationToken ct = default)
    {
        var rows = await _context.Database
            .SqlQuery<LinkedStudentRow>(
                $"""
                SELECT u."Id", u."Name", u."Email"
                FROM "TeacherStudentLinks" tsl
                INNER JOIN "Users" u ON u."Id" = tsl."StudentId"
                WHERE tsl."TeacherId" = {teacherId}
                ORDER BY u."Name" ASC
                """)
            .ToListAsync(ct);

        return rows.Select(r => (r.Id, r.Name, r.Email)).ToList();
    }

    public async Task<bool> RemoveLinkAsync(
        string teacherId,
        string studentId,
        CancellationToken cancellationToken)
    {
        var affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            DELETE FROM "TeacherStudentLinks"
            WHERE "TeacherId" = {teacherId} AND "StudentId" = {studentId}
            """,
            cancellationToken);

        return affectedRows > 0;
    }

    private sealed record LinkedStudentRow(string Id, string Name, string Email);
}
