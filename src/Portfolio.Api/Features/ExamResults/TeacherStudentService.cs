namespace Portfolio.Api.Features.ExamResults;

public interface ITeacherStudentService
{
    Task<bool> LinkStudentAsync(
        string teacherId,
        string studentId,
        CancellationToken cancellationToken);

    Task<bool> UnlinkStudentAsync(
        string teacherId,
        string studentId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<LinkedStudentDto>> GetLinkedStudentsAsync(
        string teacherId,
        CancellationToken cancellationToken);
}

public sealed class TeacherStudentService(
    ITeacherStudentRepository links,
    IExamAttemptRepository attempts) : ITeacherStudentService
{
    public Task<bool> LinkStudentAsync(
        string teacherId,
        string studentId,
        CancellationToken cancellationToken)
        => links.CreateLinkAsync(teacherId, studentId, cancellationToken);

    public Task<bool> UnlinkStudentAsync(
        string teacherId,
        string studentId,
        CancellationToken cancellationToken)
        => links.RemoveLinkAsync(teacherId, studentId, cancellationToken);

    public async Task<IReadOnlyList<LinkedStudentDto>> GetLinkedStudentsAsync(
        string teacherId,
        CancellationToken cancellationToken)
    {
        var students = await links.GetLinkedStudentsAsync(teacherId, cancellationToken);
        if (students.Count == 0)
            return [];

        var result = new List<LinkedStudentDto>(students.Count);

        foreach (var student in students)
        {
            var (latestAttempts, _) = await attempts.GetFinishedByStudentIdAsync(
                student.Id,
                limit: 1,
                offset: 0,
                cancellationToken);

            var lastAttempt = latestAttempts.FirstOrDefault();

            string? lastAttemptId = null;
            string? lastAccuracy = null;
            DateTime? lastFinishedAt = null;

            if (lastAttempt is not null)
            {
                var distinctAnswers = lastAttempt.Answers.DistinctBy(a => a.Index).ToList();
                var totalAnswered = distinctAnswers.Count;
                var totalCorrect = distinctAnswers.Count(a => a.IsCorrect);

                var percentage = totalAnswered > 0
                    ? (double)totalCorrect / totalAnswered * 100.0
                    : 0.0;

                lastAttemptId = lastAttempt.Id;
                lastAccuracy = percentage.ToString("F1", CultureInfo.InvariantCulture);
                lastFinishedAt = lastAttempt.FinishedAt;
            }

            result.Add(new LinkedStudentDto(
                Id: student.Id,
                Name: student.Name,
                Email: student.Email,
                LastExamAttemptId: lastAttemptId,
                LastExamAccuracy: lastAccuracy,
                LastExamFinishedAt: lastFinishedAt));
        }

        return result;
    }
}
