namespace Portfolio.Api.Features.ExamResults;

using Portfolio.Api.Features.ExamResults.Contracts;

public sealed class ExamResultService : IExamResultService
{
    private readonly IExamResultRepository _results;
    private readonly IEnemQuestionsClient _questions;

    public ExamResultService(IExamResultRepository results, IEnemQuestionsClient questions)
    {
        _results = results ?? throw new ArgumentNullException(nameof(results));
        _questions = questions ?? throw new ArgumentNullException(nameof(questions));
    }

    public async Task<IReadOnlyList<ExamResultResponse>?> GetResultsForStudentAsync(
        Guid studentId,
        string requestingUserId,
        bool isStudent,
        bool isTeacher,
        int limit,
        int offset,
        CancellationToken cancellationToken)
    {
        var results = await _results.GetResultsForStudentAsync(
            studentId.ToString("N"),
            requestingUserId,
            isStudent,
            isTeacher,
            cancellationToken);

        if (results is null)
            return null;

        var pages = new Dictionary<(int Year, string Language), EnemQuestionPage>();
        var responses = new List<ExamResultResponse>(results.Count);
        foreach (var result in results)
        {
            var pageKey = (result.Year, result.Language);
            if (!pages.TryGetValue(pageKey, out var page))
            {
                page = await _questions.GetQuestionsAsync(
                    result.Year,
                    result.Language,
                    limit,
                    offset,
                    cancellationToken);
                pages.Add(pageKey, page);
            }

            responses.Add(ToResponse(result, page));
        }

        return responses;
    }

    public async Task<ExamResultResponse?> GetResultByIdAsync(
        Guid resultId,
        string requestingUserId,
        bool isStudent,
        bool isTeacher,
        int limit,
        int offset,
        CancellationToken cancellationToken)
    {
        var result = await _results.GetResultByIdAsync(
            resultId,
            requestingUserId,
            isStudent,
            isTeacher,
            cancellationToken);

        if (result is null)
            return null;

        var page = await _questions.GetQuestionsAsync(
            result.Year,
            result.Language,
            limit,
            offset,
            cancellationToken);
        return ToResponse(result, page);
    }

    private static ExamResultResponse ToResponse(ExamResultReadModel result, EnemQuestionPage page)
    {
        var answersByQuestion = result.Answers.ToDictionary(answer => answer.QuestionIndex);
        var questions = page.Questions
            .Select(question => new ExamResultQuestionResponse(
                question.Title,
                question.Index,
                question.Discipline,
                question.Language,
                question.Year,
                question.Context,
                question.Files,
                question.CorrectAlternative,
                question.AlternativesIntroduction,
                question.Alternatives
                    .Select(alternative => new ExamAlternativeResponse(
                        alternative.Letter,
                        alternative.Text,
                        alternative.File,
                        alternative.IsCorrect))
                    .ToArray(),
                GetSelectedAlternative(question, answersByQuestion)))
            .ToArray();

        return new ExamResultResponse(
            result.Id,
            result.ExamId,
            result.Title,
            result.Year,
            result.TimeInMinutes,
            result.QuestionCount,
            result.Language,
            result.Humanities,
            result.Nature,
            result.Languages,
            result.Math,
            result.Anonymous,
            result.AnonymousToken,
            result.StartedAt,
            result.FinishAt,
            new ExamResultMetadataResponse(
                page.Metadata.Limit,
                page.Metadata.Offset,
                page.Metadata.Total,
                page.Metadata.HasMore),
            questions);
    }

    private static ExamSelectedAlternativeResponse? GetSelectedAlternative(
        EnemQuestion question,
        IReadOnlyDictionary<int, ExamAnswerReadModel> answers)
    {
        if (!answers.TryGetValue(question.Index, out var answer)
            || answer.SelectedAlternativeLetter is null)
            return null;

        for (var index = 0; index < question.Alternatives.Count; index++)
        {
            if (question.Alternatives[index].Letter == answer.SelectedAlternativeLetter)
                return new ExamSelectedAlternativeResponse(answer.SelectedAlternativeLetter, index);
        }

        return null;
    }
}
