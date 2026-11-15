namespace Portfolio.Api.Infrastructure.EnemApi;

// =============================================================================
// EnemApiRequestFactory
// Knows the endpoints from .env and returns a pre-configured EnemApiRequestBuilder.
// The consumer only adds query params/body when necessary.
// =============================================================================
public interface IEnemApiRequestFactory
{
    EnemApiRequestBuilder GetAllExams();
    EnemApiRequestBuilder GetExamByYear(int year);
    EnemApiRequestBuilder GetExamQuestions(int year);
    EnemApiRequestBuilder GetExamQuestion(int year, int questionIndex);
}

public sealed class EnemApiRequestFactory : IEnemApiRequestFactory
{
    private readonly EnemApiOptions _options;

    public EnemApiRequestFactory(IOptions<EnemApiOptions> options)
        => _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    
    public EnemApiRequestBuilder GetAllExams() =>
        Base().WithPath(_options.AllExamsEndpoint);

    public EnemApiRequestBuilder GetExamByYear(int year) =>
        Base()
            .WithPath(_options.ExamEndpoint, ("year", year));

    public EnemApiRequestBuilder GetExamQuestions(int year) =>
        Base()
            .WithPath(_options.ExamQuestionListEndpoint, ("year", year));

    public EnemApiRequestBuilder GetExamQuestion(int year, int questionIndex) =>
        Base()
            .WithPath(_options.ExamQuestionEndpoint, ("year", year), ("question", questionIndex));

    private EnemApiRequestBuilder Base() =>
        EnemApiRequestBuilder.Create()
            .WithGet()
            .WithHeader("Accept", _options.AcceptHeader)
            .WithTimeout(_options.Timeout);
}