namespace Portfolio.DataAccess.Tests.Repositories;

using System.Collections.Generic;
using Portfolio.DataAccess.Models;
using Portfolio.Domain.ReadModels;

[Collection(PostgresCollection.Name)]
public sealed class PostgresExamResultRepositoryTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public PostgresExamResultRepositoryTests(PostgresFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetResultByIdAsync_ReturnsTheStudentsFinishedAttemptAndAnswers()
    {
        var studentId = Guid.NewGuid().ToString("N");
        var resultId = Guid.NewGuid().ToString("N");
        await SeedAttemptAsync(studentId, resultId);

        await using var context = _fixture.CreateDbContext();
        var repository = new PostgresExamResultRepository(context);

        var result = await repository.GetResultByIdAsync(
            resultId,
            studentId,
            isStudent: true,
            isTeacher: false,
            CancellationToken.None);
        var otherStudentResult = await repository.GetResultByIdAsync(
            resultId,
            Guid.NewGuid().ToString("N"),
            isStudent: true,
            isTeacher: false,
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Answers.Should().ContainSingle()
            .Which.Should().Be(new ExamAnswerReadModel(180, "E"));
        result.QuestionIndexes.Should().Equal(180);
        result.Title.Should().Be("ENEM 2023");
        result.Year.Should().Be(2023);
        result.Id.Should().Be(resultId);
        otherStudentResult.Should().BeNull();
    }

    [Fact]
    public async Task GetResultsForStudentAsync_ReturnsFinishedAttemptsOnlyToStudentOrLinkedTeacher()
    {
        var studentId = Guid.NewGuid().ToString("N");
        var teacherId = Guid.NewGuid().ToString("N");
        var completedResultId = Guid.NewGuid().ToString("N");
        await SeedAttemptAsync(studentId, completedResultId);

        await using var context = _fixture.CreateDbContext();
        context.Users.Add(CreateUser(teacherId, "teacher"));
        context.TeacherStudentLinks.Add(new TeacherStudentLinkDbModel
        {
            TeacherId = teacherId,
            StudentId = studentId
        });
        context.ExamAttempts.Add(CreateAttempt(
            Guid.NewGuid().ToString("N"),
            Guid.NewGuid().ToString("N"),
            studentId,
            finishedAt: null));
        context.ExamAttempts.Add(CreateAttempt(
            Guid.NewGuid().ToString("N"),
            Guid.NewGuid().ToString("N"),
            null,
            finishedAt: DateTime.UtcNow,
            isAnonymous: true));
        await context.SaveChangesAsync();

        var repository = new PostgresExamResultRepository(context);
        var studentResults = await repository.GetResultsForStudentAsync(
            studentId,
            studentId,
            isStudent: true,
            isTeacher: false,
            CancellationToken.None);
        var teacherResults = await repository.GetResultsForStudentAsync(
            studentId,
            teacherId,
            isStudent: false,
            isTeacher: true,
            CancellationToken.None);
        var unauthorizedResults = await repository.GetResultsForStudentAsync(
            studentId,
            Guid.NewGuid().ToString("N"),
            isStudent: false,
            isTeacher: true,
            CancellationToken.None);

        studentResults.Should().ContainSingle();
        studentResults![0].Id.Should().Be(completedResultId);
        teacherResults.Should().ContainSingle();
        unauthorizedResults.Should().BeNull();
    }

    private async Task SeedAttemptAsync(string studentId, string resultId)
    {
        await using var context = _fixture.CreateDbContext();
        var examId = Guid.NewGuid().ToString("N");
        context.Users.Add(CreateUser(studentId));
        context.Exams.Add(CreateExam(examId));
        context.ExamAttempts.Add(CreateAttempt(
            resultId,
            examId,
            studentId,
            DateTime.UtcNow,
            [
                new AttemptQuestionDbModel { Index = 180, Discipline = "matematica" }
            ],
            [
                new QuestionAnswerDbModel
                {
                    Index = 180,
                    CorrectAlternative = "C",
                    SelectedAlternative = "E"
                }
            ]));
        await context.SaveChangesAsync();
    }

    private static ExamDbModel CreateExam(string examId)
        => new()
        {
            Id = examId,
            Title = "ENEM 2023",
            Year = "2023",
            IsSynced = true,
            Languages = ["ingles"],
            Disciplines = ["matematica"],
            CreatedAt = DateTime.UtcNow
        };

    private static ExamAttemptDbModel CreateAttempt(
        string id,
        string examId,
        string? userId,
        DateTime? finishedAt,
        List<AttemptQuestionDbModel>? servedQuestions = null,
        List<QuestionAnswerDbModel>? answers = null,
        bool isAnonymous = false)
        => new()
        {
            Id = id,
            ExamId = examId,
            UserId = userId,
            IsAnonymous = isAnonymous,
            StartedAt = DateTime.UtcNow.AddHours(-2),
            PlannedFinishAt = DateTime.UtcNow,
            FinishedAt = finishedAt,
            TimeInMinutes = 120,
            QuestionCount = 182,
            SelectedLanguage = "ingles",
            IncludeHumanities = true,
            IncludeNature = true,
            IncludeLanguages = true,
            IncludeMath = false,
            ServedQuestions = servedQuestions ?? [],
            Answers = answers ?? [],
            CreatedAt = DateTime.UtcNow
        };

    private static UserDbModel CreateUser(string id, string role = "student")
        => new()
        {
            Id = id,
            Email = $"{id}@example.com",
            Name = "Teste",
            PasswordHash = "hash",
            Roles = [role],
            TwoFactorRecoveryCodes = [],
            CreatedAt = DateTime.UtcNow
        };
}
