namespace Portfolio.Api.Features.ExamResults;

using Portfolio.Api.Features.ExamResults.Contracts;

public sealed class ExamResultService : IExamResultService
{
    private const int ExternalPageLimit = 50;
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

        var questionCache = new Dictionary<(int Year, string Language), QuestionCache>();
        var responses = new List<ExamResultResponse>(results.Count);
        foreach (var result in results)
            responses.Add(await ToResponseAsync(result, limit, offset, questionCache, cancellationToken));

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

        return await ToResponseAsync(
            result,
            limit,
            offset,
            new Dictionary<(int Year, string Language), QuestionCache>(),
            cancellationToken);
    }

    private async Task<ExamResultResponse> ToResponseAsync(
        ExamResultReadModel result,
        int limit,
        int offset,
        Dictionary<(int Year, string Language), QuestionCache> cache,
        CancellationToken cancellationToken)
    {
        var pageIndexes = result.QuestionIndexes.Skip(offset).Take(limit).ToArray();
        var cacheKey = (result.Year, result.Language);
        if (!cache.TryGetValue(cacheKey, out var questionCache))
        {
            questionCache = new QuestionCache();
            cache.Add(cacheKey, questionCache);
        }

        await questionCache.LoadAsync(
            _questions,
            result.Year,
            result.Language,
            pageIndexes,
            cancellationToken);

        var answersByQuestion = result.Answers.ToDictionary(answer => answer.QuestionIndex);
        var questions = pageIndexes
            .Select(index => questionCache.Questions.TryGetValue(index, out var question)
                ? question
                : throw new JsonException($"A questão {index} não foi encontrada na API externa do ENEM."))
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
                limit,
                offset,
                result.QuestionIndexes.Count,
                offset + questions.Length < result.QuestionIndexes.Count),
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

    private sealed class QuestionCache
    {
        private int _nextExternalOffset;
        private bool _hasMore = true;

        public Dictionary<int, EnemQuestion> Questions { get; } = [];

        public async Task LoadAsync(
            IEnemQuestionsClient client,
            int year,
            string language,
            IReadOnlyCollection<int> requestedIndexes,
            CancellationToken cancellationToken)
        {
            while (_hasMore && requestedIndexes.Any(index => !Questions.ContainsKey(index)))
            {
                var page = await client.GetQuestionsAsync(
                    year,
                    language,
                    ExternalPageLimit,
                    _nextExternalOffset,
                    cancellationToken);

                foreach (var question in page.Questions)
                    Questions.TryAdd(question.Index, question);

                _nextExternalOffset = page.Metadata.Offset + page.Metadata.Limit;
                _hasMore = page.Metadata.HasMore;

                if (page.Questions.Count == 0 && _hasMore)
                    throw new JsonException("A API externa do ENEM retornou uma página vazia com mais questões disponíveis.");
            }
        }
    }
}
