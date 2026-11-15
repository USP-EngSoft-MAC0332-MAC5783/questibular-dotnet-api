namespace Portfolio.Api.Features.ExamResults;

using System.Net.Http.Json;
using System.Text.Json;

public interface IEnemQuestionsClient
{
    Task<EnemQuestionPage> GetQuestionsAsync(
        int year,
        string language,
        int limit,
        int offset,
        CancellationToken cancellationToken);
}

public sealed class EnemQuestionsClient(HttpClient httpClient) : IEnemQuestionsClient
{
    public async Task<EnemQuestionPage> GetQuestionsAsync(
        int year,
        string language,
        int limit,
        int offset,
        CancellationToken cancellationToken)
    {
        var uri = $"exams/{year}/questions?limit={limit}&offset={offset}&language={Uri.EscapeDataString(language)}";
        using var response = await httpClient.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<EnemQuestionPage>(
                   cancellationToken: cancellationToken)
               ?? throw new JsonException("A API externa retornou uma resposta vazia.");
    }
}

public sealed record EnemQuestionPage(EnemQuestionMetadata Metadata, IReadOnlyList<EnemQuestion> Questions);

public sealed record EnemQuestionMetadata(int Limit, int Offset, int Total, bool HasMore);

public sealed record EnemQuestion(
    string Title,
    int Index,
    string? Discipline,
    string? Language,
    int Year,
    string? Context,
    IReadOnlyList<string> Files,
    string? CorrectAlternative,
    string? AlternativesIntroduction,
    IReadOnlyList<EnemAlternative> Alternatives);

public sealed record EnemAlternative(
    string Letter,
    string? Text,
    string? File,
    bool IsCorrect);
