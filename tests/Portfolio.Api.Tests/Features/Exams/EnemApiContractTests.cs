namespace Portfolio.Api.Tests.Features.Exams;

// =============================================================================
// EnemApiContractTests
// Test real contracts against the ENEM API. These tests are slower and depend 
// on the external API.
// Command to run: dotnet test --filter Category=Contract
// Command to ignore: dotnet test --filter Category!=Contract
// =============================================================================

[Trait("Category", "Contract")]
public class EnemApiContractTests
{
    private readonly EnemApiClient _client;

    public EnemApiContractTests()
    {
        // Usa as configurações reais da API do ENEM
        var options = new EnemApiOptions
        {
            BaseUrl = "https://api.enem.dev",
            Version = "v1",
            AllExamsEndpoint = "exams",
            ExamEndpoint = "exams/%year%",
            ExamQuestionListEndpoint = "exams/%year%/questions",
            ExamQuestionEndpoint = "exams/%year%/questions/%question%",
            Timeout = TimeSpan.FromSeconds(30),
            AcceptHeader = "application/json"
        };

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.enem.dev/v1/")
        };

        var factory = new EnemApiRequestFactory(Options.Create(options));
        var dispatcher = new EnemApiDispatcher(httpClient, NullLogger<EnemApiDispatcher>.Instance);
        
        _client = new EnemApiClient(dispatcher, factory);
    }

    [Fact]
    public async Task GetAllExams_RealApi_ShouldReturnValidData()
    {
        // Act
        var result = await _client.GetAllExamsAsync();

        // Assert
        result.Should().NotBeNull("a API do ENEM deve retornar uma lista válida");
        result.Should().NotBeEmpty("a lista de provas não deve estar vazia");
        
        var latestExam = result.OrderByDescending(e => e.Year).First();
        latestExam.Year.Should().BeGreaterThan(2000);
        latestExam.Title.Should().NotBeNullOrWhiteSpace();
        latestExam.Disciplines.Should().NotBeEmpty("toda prova deve ter disciplinas");
        latestExam.Languages.Should().NotBeEmpty("toda prova deve ter idiomas");
    }

    [Fact]
    public async Task GetQuestions_RealApi_ShouldReturnValidData()
    {
        // Act - Pede apenas 1 questão do ENEM 2020 (sem filtro de disciplina)
        var result = await _client.GetQuestionsAsync(
            year: 2020, 
            language: "ingles", 
            limit: 1, 
            offset: 0);

        // Assert
        result.Should().NotBeNull();
        result.Metadata.Should().NotBeNull();
        result.Metadata.Total.Should().BeGreaterThan(0);
        
        result.Questions.Should().NotBeEmpty();
        var question = result.Questions.First();
        
        question.Title.Should().NotBeNullOrWhiteSpace();
        question.Year.Should().Be(2020);
        question.Discipline.Should().NotBeNullOrWhiteSpace("a API sempre retorna a disciplina da questão");
        
        question.Alternatives.Should().HaveCount(5, "questões do ENEM têm 5 alternativas");
        question.Alternatives.Count(a => a.IsCorrect).Should().Be(1);
    }
}