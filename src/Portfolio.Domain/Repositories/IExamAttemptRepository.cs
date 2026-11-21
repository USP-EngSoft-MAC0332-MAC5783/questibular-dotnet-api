namespace Portfolio.Domain.Repositories;

// =============================================================================
// IExamAttemptRepository
// Contract persistence for ExamAttempt. Implemented by Infrastructure.
// =============================================================================
public interface IExamAttemptRepository
{
    Task<ExamAttempt?> GetByIdAsync(string id, CancellationToken ct = default);

    Task AddAsync(ExamAttempt attempt, CancellationToken ct = default);

    Task UpdateAsync(ExamAttempt attempt, CancellationToken ct = default);

    /// <summary>
    /// Remove expired attempts of anonymous users (ExpiresAt &lt; now).
    /// Need to run as a background job.
    /// </summary>
    Task<int> DeleteExpiredAnonymousAttemptsAsync(DateTime now, CancellationToken ct = default);
    Task<(List<ExamAttempt> Attempts, int TotalCount)> GetFinishedByStudentIdAsync(string studentId, int limit, int offset, CancellationToken ct = default);
}