namespace Portfolio.Domain.Repositories;

// =============================================================================
// IExamRepository
// Contract persistence for Exam. Implemented by Infrastructure (EF Core).
// =============================================================================
public interface IExamRepository
{
    Task<Exam?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Exam?> GetByYearAsync(string year, CancellationToken ct = default);

    Task<List<Exam>> GetByYearRangeAsync(int startYear, int endYear, CancellationToken ct = default);
    Task<List<Exam>> GetPendingSyncAsync(int take, CancellationToken ct = default);

    Task AddAsync(Exam exam, CancellationToken ct = default);

    Task AddRangeAsync(IEnumerable<Exam> exams, CancellationToken ct = default);

    Task UpdateAsync(Exam exam, CancellationToken ct = default);
    Task IncrementStartAsync(string examId, DateTime now, CancellationToken ct = default);
    Task IncrementFinishAsync(string examId, int questionsFinished, DateTime now, CancellationToken ct = default);
}