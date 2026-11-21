namespace Portfolio.Api.Features.ExamResults;

using Portfolio.Api.Features.ExamResults.Contracts;
using System.Net.Http;
using System.Text.Json;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/exams/result")]
[Produces("application/json")]
[Authorize]
public sealed class ExamResultsController : BaseApiController<ExamResultsController>
{
    private const int DefaultLimit = 20;
    private const int MaximumLimit = 100;
    private readonly IExamResultService _results;

    public ExamResultsController(
        IExamResultService results,
        ILogger<ExamResultsController> logger,
        IApplicationContextService appContextService)
        : base(logger, appContextService)
        => _results = results ?? throw new ArgumentNullException(nameof(results));

    /**
    /// <summary>Returns every completed result for a student, each with a page of questions.</summary>
    [HttpGet("student/{studentId}")]
    [EndpointSummary("Obtém as resoluções de um aluno com questões paginadas")]
    [EndpointDescription("Retorna cada resolução finalizada do aluno no formato completo, incluindo questões paginadas obtidas da API externa do ENEM. Disponível ao próprio aluno ou a um professor vinculado.")]
    [ProducesResponseType(typeof(IReadOnlyList<ExamResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ExamResultResponse>>> ListByStudent(
        string studentId,
        CancellationToken cancellationToken,
        [FromQuery] int limit = DefaultLimit,
        [FromQuery] int offset = 0)
    {
        var userId = GetAuthenticatedUserId();
        if (userId is null)
            return ProblemResponse(StatusCodes.Status401Unauthorized, "Autenticação necessária.",
                "Não foi possível identificar o usuário autenticado.");
        if (limit is < 1 or > MaximumLimit)
            return ProblemResponse(StatusCodes.Status400BadRequest, "Parâmetro inválido.",
                $"O parâmetro limit deve estar entre 1 e {MaximumLimit}.");
        if (offset < 0)
            return ProblemResponse(StatusCodes.Status400BadRequest, "Parâmetro inválido.",
                "O parâmetro offset não pode ser negativo.");

        try
        {
            var results = await _results.GetResultsForStudentAsync(
                studentId,
                userId,
                User.IsInRole("student"),
                User.IsInRole("teacher"),
                limit,
                offset,
                cancellationToken);
            return results is null
                ? ProblemResponse(StatusCodes.Status404NotFound, "Resultados não encontrados.",
                    "O aluno não existe ou você não tem permissão para acessar os resultados.")
                : Ok(results);
        }
        catch (HttpRequestException exception)
        {
            Logger.LogError(exception, "Failed to fetch exam questions from the ENEM API.");
            return ProblemResponse(StatusCodes.Status502BadGateway, "Falha ao obter questões.",
                "Não foi possível obter as questões na API externa neste momento.");
        }
        catch (JsonException exception)
        {
            Logger.LogError(exception, "The ENEM API returned an invalid question response.");
            return ProblemResponse(StatusCodes.Status502BadGateway, "Falha ao obter questões.",
                "A API externa retornou dados inválidos para as questões.");
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            Logger.LogError(exception, "The ENEM API request timed out.");
            return ProblemResponse(StatusCodes.Status502BadGateway, "Falha ao obter questões.",
                "A API externa demorou para responder. Tente novamente.");
        }
    }

    /// <summary>Returns the completed result and a page of its questions.</summary>
    [HttpGet("{resultId}")]
    [EndpointSummary("Obtém um resultado de simulado com questões paginadas")]
    [EndpointDescription("Retorna os dados da resolução e as questões da API pública do ENEM, paginadas por limit e offset. Disponível ao aluno autor da tentativa ou a um professor vinculado.")]
    [ProducesResponseType(typeof(ExamResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<ExamResultResponse>> GetById(
        string resultId,
        CancellationToken cancellationToken,
        [FromQuery] int limit = DefaultLimit,
        [FromQuery] int offset = 0)
    {
        if (limit is < 1 or > MaximumLimit)
            return ProblemResponse(StatusCodes.Status400BadRequest, "Parâmetro inválido.",
                $"O parâmetro limit deve estar entre 1 e {MaximumLimit}.");
        if (offset < 0)
            return ProblemResponse(StatusCodes.Status400BadRequest, "Parâmetro inválido.",
                "O parâmetro offset não pode ser negativo.");

        var userId = GetAuthenticatedUserId();
        if (userId is null)
            return ProblemResponse(StatusCodes.Status401Unauthorized, "Autenticação necessária.",
                "Não foi possível identificar o usuário autenticado.");
                
        var isStudent = User.IsInRole("student");
        var isTeacher = User.IsInRole("teacher");
        if (!isStudent && !isTeacher)
            return ProblemResponse(StatusCodes.Status403Forbidden, "Acesso negado.",
                "Somente o aluno autor da tentativa ou um professor vinculado pode acessar o resultado.");

        ExamResultResponse? result;
        try
        {
            result = await _results.GetResultByIdAsync(
                resultId,
                userId,
                isStudent,
                isTeacher,
                limit,
                offset,
                cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            Logger.LogError(exception, "Failed to fetch exam questions from the ENEM API.");
            return ProblemResponse(StatusCodes.Status502BadGateway, "Falha ao obter questões.",
                "Não foi possível obter as questões na API externa neste momento.");
        }
        catch (JsonException exception)
        {
            Logger.LogError(exception, "The ENEM API returned an invalid question response.");
            return ProblemResponse(StatusCodes.Status502BadGateway, "Falha ao obter questões.",
                "A API externa retornou dados inválidos para as questões.");
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            Logger.LogError(exception, "The ENEM API request timed out.");
            return ProblemResponse(StatusCodes.Status502BadGateway, "Falha ao obter questões.",
                "A API externa demorou para responder. Tente novamente.");
        }

        return result is null
            ? ProblemResponse(StatusCodes.Status404NotFound, "Resultado não encontrado.",
                "O resultado não existe, ainda não foi finalizado ou você não tem permissão para acessá-lo.")
            : Ok(result);
    }

    private string? GetAuthenticatedUserId()
    {
        var userId = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userId, out var parsedUserId)
            ? parsedUserId.ToString("N")
            : userId;
    }

    private ObjectResult ProblemResponse(int status, string title, string detail)
        => new(new ProblemDetails
        {
            Type = $"https://httpstatuses.io/{status}",
            Title = title,
            Status = status,
            Detail = detail,
            Instance = HttpContext.Request.Path
        })
        {
            StatusCode = status
        };

    */
}
