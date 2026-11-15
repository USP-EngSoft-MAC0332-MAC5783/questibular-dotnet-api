namespace Portfolio.DataAccess.Tests.Repositories;

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
    public async Task GetResultByIdAsync_ReturnsOnlyTheStudentsFinishedResultAndStoredAnswers()
    {
        var studentId = Guid.NewGuid().ToString("N");
        var resultId = Guid.NewGuid();
        await SeedResultAsync(studentId, resultId);

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
        result.Title.Should().Be("ENEM 2023");
        result.Year.Should().Be(2023);
        otherStudentResult.Should().BeNull();
    }

    [Fact]
    public async Task GetResultsForStudentAsync_ReturnsCompletedResultsOnlyToStudentOrLinkedTeacher()
    {
        var studentId = Guid.NewGuid().ToString("N");
        var teacherId = Guid.NewGuid().ToString("N");
        var completedResultId = Guid.NewGuid();
        await SeedResultAsync(studentId, completedResultId);

        await using var context = _fixture.CreateDbContext();
        context.Users.Add(CreateUser(teacherId, "teacher"));
        context.TeacherStudentLinks.Add(new TeacherStudentLinkDbModel
        {
            TeacherId = teacherId,
            StudentId = studentId
        });
        context.ExamResults.Add(new ExamResultDbModel
        {
            Id = Guid.NewGuid(),
            ExamId = Guid.NewGuid(),
            StudentId = studentId,
            Title = "Em andamento",
            Year = 2023,
            Language = "ingles",
            StartedAt = DateTimeOffset.UtcNow,
            Answers = "[]"
        });
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

    private async Task SeedResultAsync(string studentId, Guid resultId)
    {
        await using var context = _fixture.CreateDbContext();
        context.Users.Add(CreateUser(studentId));
        context.ExamResults.Add(new ExamResultDbModel
        {
            Id = resultId,
            ExamId = Guid.NewGuid(),
            StudentId = studentId,
            Title = "ENEM 2023",
            Year = 2023,
            TimeInMinutes = 120,
            QuestionCount = 182,
            Language = "ingles",
            Humanities = true,
            Nature = true,
            Languages = true,
            Math = false,
            Anonymous = true,
            StartedAt = DateTimeOffset.UtcNow.AddHours(-2),
            FinishAt = DateTimeOffset.UtcNow,
            Answers = """[{"questionIndex":180,"selectedAlternativeLetter":"E"}]"""
        });
        await context.SaveChangesAsync();
    }

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
