namespace Portfolio.Api.Tests.Features.ExamResults;

using FluentAssertions;
using Moq;
using Portfolio.Api.Features.ExamResults;
using Portfolio.Api.Features.ExamResults.Contracts;
using Portfolio.Domain.ReadModels;
using Portfolio.Domain.Repositories;
using Xunit;

public sealed class ExamResultServiceTests
{
    [Fact]
    public async Task GetResultsForStudentAsync_ReturnsFullResultsAndFetchesEachExamPageOnlyOnce()
    {
        var studentId = Guid.NewGuid();
        var requestingUserId = studentId.ToString("N");
        var firstResultId = Guid.NewGuid();
        var secondResultId = Guid.NewGuid();
        var results = new[]
        {
            CreateResult(firstResultId),
            CreateResult(secondResultId)
        };
        var repository = new Mock<IExamResultRepository>();
        repository
            .Setup(value => value.GetResultsForStudentAsync(
                requestingUserId,
                requestingUserId,
                true,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(results);

        var questionPage = new EnemQuestionPage(
            new EnemQuestionMetadata(9, 180, 182, false),
            [
                new EnemQuestion(
                    "Questão 180 - ENEM 2020",
                    180,
                    "matematica",
                    null,
                    2020,
                    "Enunciado",
                    [],
                    "C",
                    null,
                    [
                        new EnemAlternative("A", "A", null, false),
                        new EnemAlternative("B", "B", null, false),
                        new EnemAlternative("C", "C", null, true),
                        new EnemAlternative("D", "D", null, false),
                        new EnemAlternative("E", "E", null, false)
                    ])
            ]);
        var questions = new Mock<IEnemQuestionsClient>();
        questions
            .Setup(value => value.GetQuestionsAsync(
                2023,
                "ingles",
                9,
                180,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(questionPage);
        var service = new ExamResultService(repository.Object, questions.Object);

        var response = await service.GetResultsForStudentAsync(
            studentId,
            requestingUserId,
            isStudent: true,
            isTeacher: false,
            limit: 9,
            offset: 180,
            CancellationToken.None);

        response.Should().HaveCount(2);
        response![0].Id.Should().Be(firstResultId);
        response[0].Metadata.Should().Be(new ExamResultMetadataResponse(9, 180, 182, false));
        response[0].Questions.Should().ContainSingle();
        response[0].Questions[0].CorrectAlternative.Should().Be("C");
        response[0].Questions[0].SelectedAlternative.Should()
            .Be(new ExamSelectedAlternativeResponse("E", 4));
        response[1].Id.Should().Be(secondResultId);
        questions.Verify(value => value.GetQuestionsAsync(
            2023,
            "ingles",
            9,
            180,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static ExamResultReadModel CreateResult(Guid resultId)
        => new(
            resultId,
            Guid.NewGuid(),
            "Simulado",
            2023,
            120,
            182,
            "ingles",
            true,
            true,
            true,
            false,
            true,
            null,
            DateTimeOffset.UtcNow.AddHours(-2),
            DateTimeOffset.UtcNow,
            [new ExamAnswerReadModel(180, "E")]);
}
