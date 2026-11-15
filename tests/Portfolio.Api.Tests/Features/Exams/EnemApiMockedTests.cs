namespace Portfolio.Api.Tests.Features.Exams;

// =============================================================================
// EnemApiMockedTests
// Integration tests - fast and deterministic.
// It simulates the external API to validate the URL construction, headers and JSON.
// =============================================================================
public class EnemApiMockedTests
{
    private readonly EnemApiOptions _options;
    private readonly EnemApiRequestFactory _factory;

    public EnemApiMockedTests()
    {
        _options = new EnemApiOptions
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

        _factory = new EnemApiRequestFactory(Options.Create(_options));
    }

    [Fact]
    public async Task GetAllExamsAsync_ShouldRequestCorrectUrlAndDeserializeResponse()
    {
        // Arrange
        var jsonResponse = """
        [
            {
                "title": "ENEM 2023",
                "year": 2023,
                "disciplines": [{ "label": "Ciências Humanas", "value": "ciencias-humanas" }],
                "languages": [{ "label": "Inglês", "value": "ingles" }]
            }
        ]
        """;
        
        var fakeHandler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        });
        
        var httpClient = new HttpClient(fakeHandler) { BaseAddress = new Uri("https://api.enem.dev/v1/") };
        var client = new EnemApiClient(
            new EnemApiDispatcher(httpClient, NullLogger<EnemApiDispatcher>.Instance),
            _factory);

        // Act
        var result = await client.GetAllExamsAsync();

        // Assert
        fakeHandler.LastRequest.Should().NotBeNull();
        fakeHandler.LastRequest!.RequestUri!.ToString().Should().Be("https://api.enem.dev/v1/exams");
        fakeHandler.LastRequest.Headers.Accept.ToString().Should().Be("application/json");

        result.Should().HaveCount(1);
        result.First().Year.Should().Be(2023);
        result.First().Languages.Should().ContainSingle(l => l.Value == "ingles");
    }

    [Fact]
    public async Task GetQuestionsAsync_ShouldRequestCorrectQueryParametersAndDeserialize()
    {
        // Arrange
        var jsonResponse = """
        {
            "metadata": { "limit": 9, "offset": 180, "total": 182, "hasMore": false },
            "questions": [
                {
                    "title": "Questão 180",
                    "index": 180,
                    "discipline": "matematica",
                    "language": null,
                    "year": 2020,
                    "context": "...",
                    "files": [],
                    "correctAlternative": "C",
                    "alternativesIntroduction": "...",
                    "alternatives": [{ "letter": "C", "text": "Z.", "file": null, "isCorrect": true }]
                }
            ]
        }
        """;

        var fakeHandler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        });
        
        var httpClient = new HttpClient(fakeHandler) { BaseAddress = new Uri("https://api.enem.dev/v1/") };
        var client = new EnemApiClient(
            new EnemApiDispatcher(httpClient, NullLogger<EnemApiDispatcher>.Instance),
            _factory);

        // Act
        // Nota: Removemos o parâmetro 'disciplines' da chamada e o limit/offset agora são obrigatórios
        var result = await client.GetQuestionsAsync(
            year: 2020, 
            language: "ingles", 
            limit: 9, 
            offset: 180);

        // Assert
        var uri = fakeHandler.LastRequest!.RequestUri!;
        uri.AbsolutePath.Should().Be("/v1/exams/2020/questions");
        
        // VALIDAÇÃO CORRIGIDA: Não deve conter discipline, mas deve conter language, limit e offset
        uri.Query.Should().Contain("language=ingles");
        uri.Query.Should().Contain("limit=9");
        uri.Query.Should().Contain("offset=180");
        uri.Query.Should().NotContain("discipline=");

        result.Metadata.Total.Should().Be(182);
        result.Questions.Should().HaveCount(1);
        result.Questions.First().CorrectAlternative.Should().Be("C");
    }

    [Fact]
    public async Task GetExamByYearAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        var fakeHandler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var httpClient = new HttpClient(fakeHandler) { BaseAddress = new Uri("https://api.enem.dev/v1/") };
        
        var client = new EnemApiClient(
            new EnemApiDispatcher(httpClient, NullLogger<EnemApiDispatcher>.Instance),
            _factory);

        // Act
        var result = await client.GetExamByYearAsync(1999);

        // Assert
        result.Should().BeNull();
    }
}