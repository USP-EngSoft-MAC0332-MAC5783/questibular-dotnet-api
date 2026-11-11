namespace Portfolio.Api.Infrastructure.EnemApi;

// =============================================================================
// EnemApiOptions
// Bind keys from .env (ENEM_API_*). The templates uses the format
// "%placeholder%" (ex.: "exams/%year%/questions/%question%").
// =============================================================================
public sealed class EnemApiOptions
{
    public string BaseUrl { get; set; } = default!;
    public string Version { get; set; } = default!;

    public string AllExamsEndpoint        { get; set; } = default!;
    public string ExamListEndpoint        { get; set; } = default!;
    public string ExamEndpoint            { get; set; } = default!;
    public string ExamQuestionListEndpoint{ get; set; } = default!;
    public string ExamQuestionEndpoint    { get; set; } = default!;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Default Accept header sent in all requests.</summary>
    public string AcceptHeader { get; set; } = "application/json";
}