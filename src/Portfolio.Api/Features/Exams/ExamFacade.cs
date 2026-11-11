namespace Portfolio.Api.Features.Exams;

// =============================================================================
// ExamFacade
// Orchestrate the application logic of the Exams feature.
//
// IMPORTANTE:
//  • GetExamsAsync create "stubs" (IsSynced = false) for exams not in db.
//    Exam.ApplySyncData(...) persists the complete exam data.
// =============================================================================

public sealed class ExamFacade : IExamFacade
{
    private readonly IExamRepository _examRepository;
    private readonly IEnemApiClient _enemApi;
    private readonly IUserRepository _userRepository;
    private readonly IClock _clock;
    private readonly IExamSyncService _examSyncService;

    public ExamFacade(
        IExamRepository examRepository,
        IEnemApiClient enemApi,
        IUserRepository userRepository,
        IClock clock,
        IExamSyncService examSyncService)
    {
        _examRepository = examRepository ?? throw new ArgumentNullException(nameof(examRepository));
        _enemApi = enemApi ?? throw new ArgumentNullException(nameof(enemApi));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _examSyncService = examSyncService ?? throw new ArgumentNullException(nameof(examSyncService));
    }

    public async Task<GetExamsResponse> GetExamsAsync(GetExamsRequest request, CancellationToken ct = default)
    {
        if (request.Start > request.End)
            throw new ArgumentException("Parametro start não pode ser maior que end.");
        if (request.Start < 2000)
            throw new ArgumentException("Ano inicial inválido. Deve ser maior ou igual a 2000.");

        var existing = await _examRepository.GetByYearRangeAsync(request.Start, request.End, ct);
        var existingYears = existing.Select(e => e.Year).ToHashSet();

        var stubs = new List<Exam>();
        for (var year = request.Start; year <= request.End; year++)
        {
            var yearStr = year.ToString();
        
            if (!existingYears.Contains(yearStr))
            {
                stubs.Add(Exam.CreateStub(yearStr, _clock.UtcNow));
            }
        }

        if (stubs.Count > 0)
        {
            await _examRepository.AddRangeAsync(stubs, ct);
            existing.AddRange(stubs);
        }

        await _examSyncService.SyncIfNeededAsync(existing, ct);

        var ordered = existing.OrderBy(e => e.Year).ToList();
        
        int totalH = 0, totalN = 0, totalL = 0, totalM = 0;
        foreach (var e in ordered)
        {
            totalH += e.QuestionCountHumanities;
            totalN += e.QuestionCountNature;
            totalL += e.QuestionCountLanguages;
            totalM += e.QuestionCountMath;
        }

        return new GetExamsResponse(
            Start: request.Start,
            End: request.End,
            TotalCountHumanities: totalH,
            TotalCountNature: totalN,
            TotalCountLanguages: totalL,
            TotalCountMath: totalM,
            Exams: ordered.Select(MapExamToDto).ToList());
    }

    // ===================== Helpers =====================

    private static ExamResponseDto MapExamToDto(Exam e) => new(
        Id: e.Id,
        Title: e.Title,
        Year: e.Year,
        QuestionCountHumanities: e.QuestionCountHumanities,
        QuestionCountNature: e.QuestionCountNature,
        QuestionCountLanguages: e.QuestionCountLanguages,
        QuestionCountMath: e.QuestionCountMath,
        Languages: e.Languages);
}