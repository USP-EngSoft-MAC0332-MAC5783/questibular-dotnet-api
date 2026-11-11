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
        var request = _requests.GetExamQuestions(year)
            .WithQuery("language", language)
            .WithQuery("limit", limit > 0 ? limit : null)
            .WithQuery("offset", offset > 0 ? offset : null);

        var page = await _dispatcher.SendAsync<EnemApiQuestionsPage>(request, ct)
                ?? new EnemApiQuestionsPage();

        return page;
    }
}