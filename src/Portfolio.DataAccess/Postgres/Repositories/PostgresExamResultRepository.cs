namespace Portfolio.DataAccess.Postgres.Repositories;

using System.Text.Json;

internal sealed class PostgresExamResultRepository : IExamResultRepository
{
    private static readonly JsonSerializerOptions AnswerJsonOptions = new(JsonSerializerDefaults.Web);
    private readonly PortfolioDbContext _context;

    public PostgresExamResultRepository(PortfolioDbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<IReadOnlyList<ExamResultReadModel>?> GetResultsForStudentAsync(
        string studentId,
        string requestingUserId,
        bool isStudent,
        bool isTeacher,
        CancellationToken cancellationToken)
    {
        var ownsResults = isStudent && studentId == requestingUserId;
        if (!ownsResults && (!isTeacher || !await _context.TeacherStudentLinks
                .AsNoTracking()
                .AnyAsync(
                    link => link.StudentId == studentId && link.TeacherId == requestingUserId,
                    cancellationToken)))
            return null;

        var results = await _context.ExamResults
            .AsNoTracking()
            .Where(result => result.StudentId == studentId && result.FinishAt.HasValue)
            .OrderByDescending(result => result.FinishAt)
            .ToListAsync(cancellationToken);

        return results.Select(ToReadModel).ToArray();
    }

    public async Task<ExamResultReadModel?> GetResultByIdAsync(
        Guid resultId,
        string requestingUserId,
        bool isStudent,
        bool isTeacher,
        CancellationToken cancellationToken)
    {
        var result = await _context.ExamResults
            .AsNoTracking()
            .Where(item => item.Id == resultId
                && item.FinishAt.HasValue
                && ((isStudent && item.StudentId == requestingUserId)
                    || (isTeacher && _context.TeacherStudentLinks.Any(link =>
                        link.StudentId == item.StudentId && link.TeacherId == requestingUserId))))
            .SingleOrDefaultAsync(cancellationToken);

        return result is null ? null : ToReadModel(result);
    }

    private static ExamResultReadModel ToReadModel(ExamResultDbModel result)
    {
        var answers = JsonSerializer.Deserialize<List<ExamAnswerReadModel>>(
            result.Answers,
            AnswerJsonOptions) ?? [];

        return new ExamResultReadModel(
            result.Id,
            result.ExamId,
            result.Title,
            result.Year,
            result.TimeInMinutes,
            result.QuestionCount,
            result.Language,
            result.Humanities,
            result.Nature,
            result.Languages,
            result.Math,
            result.Anonymous,
            result.AnonymousToken,
            result.StartedAt,
            result.FinishAt!.Value,
            answers);
    }
}
