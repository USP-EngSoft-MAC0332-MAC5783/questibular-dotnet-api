namespace Portfolio.Api.Features.Exams;

public interface IExamSyncService
{
    Task SyncIfNeededAsync(IEnumerable<Exam> exams, CancellationToken ct = default);
}