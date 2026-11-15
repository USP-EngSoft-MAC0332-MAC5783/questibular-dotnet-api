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

        var attempts = await _context.ExamAttempts
            .AsNoTracking()
            .Where(attempt => !attempt.IsAnonymous
                && attempt.UserId == studentId
                && attempt.FinishedAt.HasValue)
            .OrderByDescending(attempt => attempt.FinishedAt)
            .ToListAsync(cancellationToken);

        return await ToReadModelsAsync(attempts, cancellationToken);
    }

    public async Task<ExamResultReadModel?> GetResultByIdAsync(
        Guid resultId,
        string requestingUserId,
        bool isStudent,
        bool isTeacher,
        CancellationToken cancellationToken)
    {
        var attempt = await _context.ExamAttempts
            .AsNoTracking()
            .Where(item => item.Id == resultId.ToString("N")
                && item.FinishedAt.HasValue
                && !item.IsAnonymous
                && ((isStudent && item.UserId == requestingUserId)
                    || (isTeacher && _context.TeacherStudentLinks.Any(link =>
                        link.StudentId == item.UserId && link.TeacherId == requestingUserId))))
            .SingleOrDefaultAsync(cancellationToken);

        if (attempt is null)
            return null;

        var exam = await _context.Exams
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == attempt.ExamId, cancellationToken)
            ?? throw new InvalidOperationException($"Exam '{attempt.ExamId}' for attempt '{attempt.Id}' was not found.");

        return ToReadModel(attempt, exam);
    }

    private async Task<IReadOnlyList<ExamResultReadModel>> ToReadModelsAsync(
        IReadOnlyList<ExamAttemptDbModel> attempts,
        CancellationToken cancellationToken)
    {
        if (attempts.Count == 0)
            return [];

        var examIds = attempts.Select(attempt => attempt.ExamId).Distinct().ToArray();
        var exams = await _context.Exams
            .AsNoTracking()
            .Where(exam => examIds.Contains(exam.Id))
            .ToDictionaryAsync(exam => exam.Id, cancellationToken);

        return attempts.Select(attempt =>
        {
            if (!exams.TryGetValue(attempt.ExamId, out var exam))
                throw new InvalidOperationException(
                    $"Exam '{attempt.ExamId}' for attempt '{attempt.Id}' was not found.");

            return ToReadModel(attempt, exam);
        }).ToArray();
    }

    private static ExamResultReadModel ToReadModel(ExamAttemptDbModel attempt, ExamDbModel exam)
    {
        if (!Guid.TryParse(attempt.Id, out var resultId))
            throw new InvalidOperationException($"Attempt ID '{attempt.Id}' is not a valid GUID.");
        if (!Guid.TryParse(exam.Id, out var examId))
            throw new InvalidOperationException($"Exam ID '{exam.Id}' is not a valid GUID.");
        if (!int.TryParse(exam.Year, out var year))
            throw new InvalidOperationException($"Exam year '{exam.Year}' is not valid.");

        var questionIndexes = attempt.ServedQuestions.Count > 0
            ? attempt.ServedQuestions.Select(question => question.Index).Distinct().ToArray()
            : attempt.Answers.Select(answer => answer.Index).Distinct().ToArray();
        var answers = attempt.Answers
            .Select(answer => new ExamAnswerReadModel(answer.Index, answer.SelectedAlternative))
            .ToArray();

        return new ExamResultReadModel(
            resultId,
            examId,
            exam.Title,
            year,
            attempt.TimeInMinutes,
            attempt.QuestionCount,
            attempt.SelectedLanguage ?? "ingles",
            attempt.IncludeHumanities,
            attempt.IncludeNature,
            attempt.IncludeLanguages,
            attempt.IncludeMath,
            attempt.IsAnonymous,
            attempt.AnonymousToken,
            new DateTimeOffset(DateTime.SpecifyKind(attempt.StartedAt, DateTimeKind.Utc)),
            new DateTimeOffset(DateTime.SpecifyKind(attempt.FinishedAt!.Value, DateTimeKind.Utc)),
            questionIndexes,
            answers);
    }
}
