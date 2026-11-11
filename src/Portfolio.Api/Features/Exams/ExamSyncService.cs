namespace Portfolio.Api.Features.Exams;

public sealed class ExamSyncService : IExamSyncService
{
    private readonly IExamRepository _examRepository;
    private readonly IEnemApiClient _enemApi;
    private readonly IClock _clock;

    public ExamSyncService(
        IExamRepository examRepository, 
        IEnemApiClient enemApi, 
        IClock clock)
    {
        _examRepository = examRepository;
        _enemApi = enemApi;
        _clock = clock;
    }

    public async Task SyncIfNeededAsync(IEnumerable<Exam> exams, CancellationToken ct = default)
    {
        var pending = exams.Where(e => !e.IsSynced).ToList();
        
        foreach (var exam in pending)
        {
            var apiData = await _enemApi.GetExamByYearAsync(int.Parse(exam.Year), ct);
            if (apiData == null) continue;

            var questions = apiData.Questions;
            
            var humanitiesCount = questions.Count(q => q.Discipline == "ciencias-humanas");
            var natureCount     = questions.Count(q => q.Discipline == "ciencias-natureza");
            var languagesCount  = questions.Count(q => q.Discipline == "linguagens");
            var mathCount       = questions.Count(q => q.Discipline == "matematica");

            var languages = questions
                .Where(q => !string.IsNullOrWhiteSpace(q.Language))
                .Select(q => q.Language!)
                .Distinct()
                .ToList();

            exam.ApplySyncData(
                apiData.Title, humanitiesCount, natureCount, languagesCount, mathCount,
                apiData.Disciplines.Select(d => d.Value), languages, _clock.UtcNow);

            await _examRepository.UpdateAsync(exam, ct);
        }
    }
}