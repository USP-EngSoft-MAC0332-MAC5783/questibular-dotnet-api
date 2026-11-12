namespace Portfolio.Api.Infrastructure.EnemApi;

// =============================================================================
// EnemApiClient
// Orchestrate the builder an factory adding queries/body and trigger
// the dispatcher.
// =============================================================================

public sealed class EnemApiClient : IEnemApiClient
{
    private readonly IEnemApiDispatcher _dispatcher;
    private readonly IEnemApiRequestFactory _requests;
    private const int MaxApiLimit = 50;

    public EnemApiClient(IEnemApiDispatcher dispatcher, IEnemApiRequestFactory requests)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _requests   = requests   ?? throw new ArgumentNullException(nameof(requests));
    }

    public async Task<IReadOnlyList<EnemApiExamSummary>> GetAllExamsAsync(CancellationToken ct = default)
    {
        var request = _requests.GetAllExams();

        var result = await _dispatcher.SendAsync<List<EnemApiExamSummary>>(request, ct);
        
        return result ?? [];
    }

    public Task<EnemApiExam?> GetExamByYearAsync(int year, CancellationToken ct = default)
        => _dispatcher.SendAsync<EnemApiExam>(_requests.GetExamByYear(year), ct);

    public async Task<EnemApiQuestionsPage> GetQuestionsAsync(
        int year,
        string? language,
        int limit,
        int offset,
        CancellationToken ct = default)
    {
        if (limit <= MaxApiLimit)
        {
            var request = _requests.GetExamQuestions(year)
                .WithQuery("language", language)
                .WithQuery("limit", limit > 0 ? limit : null)
                .WithQuery("offset", offset > 0 ? offset : null);

            return await _dispatcher.SendAsync<EnemApiQuestionsPage>(request, ct) 
                   ?? new EnemApiQuestionsPage();
        }
        
        var allQuestions = new List<EnemApiQuestion>();
        var currentOffset = offset;
        EnemApiPagination? metadata = null;

        while (allQuestions.Count < limit)
        {
            var fetchLimit = Math.Min(limit - allQuestions.Count, MaxApiLimit);
            
            var request = _requests.GetExamQuestions(year)
                .WithQuery("language", language)
                .WithQuery("limit", fetchLimit)
                .WithQuery("offset", currentOffset);

            var page = await _dispatcher.SendAsync<EnemApiQuestionsPage>(request, ct) 
                       ?? new EnemApiQuestionsPage();
            
            metadata ??= page.Metadata;
            
            if (page.Questions.Count == 0) break; // Não há mais questões disponíveis
            
            allQuestions.AddRange(page.Questions);
            currentOffset += page.Questions.Count;
            
            if (!page.Metadata.HasMore) break; // Chegou ao fim da prova
        }

        return new EnemApiQuestionsPage
        {
            Metadata = new EnemApiPagination
            {
                Limit = limit,
                Offset = offset,
                Total = metadata?.Total ?? allQuestions.Count,
                HasMore = metadata?.HasMore ?? false
            },
            Questions = allQuestions
        };
    }
}