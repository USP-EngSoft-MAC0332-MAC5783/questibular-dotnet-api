namespace Portfolio.Api.Features.Exams;

// =============================================================================
// ExamsController
// Endpoints REST Exams feature (US05 + US10).
// =============================================================================

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public sealed class ExamsController : BaseApiController<ExamsController>
{
    private readonly IExamFacade _examFacade;

    public ExamsController(
        IExamFacade examFacade,
        ILogger<ExamsController> logger,
        IApplicationContextService appContextService)
        : base(logger, appContextService)
    {
        _examFacade = examFacade ?? throw new ArgumentNullException(nameof(examFacade));
    }

    // ===================== US05 — GET /api/v1/exams?start=2012&end=2020 =====================
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetExamsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GetExamsResponse>> GetExams(
        [FromQuery] int start,
        [FromQuery] int end,
        CancellationToken ct)
    {
        try
        {
            var result = await _examFacade.GetExamsAsync(new GetExamsRequest(start, end), ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(BuildProblem("Falha ao obter lista", ex.Message, 400));
        }
    }

    // ===================== US10 — POST /api/v1/exams/start =====================
    [HttpPost("start")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(StartExamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<StartExamResponse>> StartExam(
        [FromBody] StartExamRequest request,
        CancellationToken ct)
    {
        var userId = GetAuthenticatedUserId();
        try
        {
            var result = await _examFacade.StartExamAsync(request, userId, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(BuildProblem("Acesso negado", ex.Message, 401));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(BuildProblem("Falha ao iniciar simulado", ex.Message, 400));
        }
    }


    [HttpGet("{id}/questions")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(StartExamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<StartExamResponse>> GetNextQuestions(
        string id,
        [FromHeader(Name = "X-Anonymous-Token")] string? anonymousToken, 
        CancellationToken ct)
    {
        var userId = GetAuthenticatedUserId();

        try
        {
            var result = await _examFacade.GetNextQuestionsAsync(id, userId, anonymousToken, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(BuildProblem("Acesso negado", ex.Message, 401));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(BuildProblem("Falha ao obter questões", ex.Message, 400));
        }
    }

    // ===================== US10 — POST /api/v1/exams/finish =====================
    [HttpPost("finish")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FinishExamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FinishExamResponse>> FinishExam(
        [FromBody] FinishExamRequest request,
        CancellationToken ct)
    {
        var userId = GetAuthenticatedUserId();
        try
        {
            var result = await _examFacade.FinishExamAsync(request, userId, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(BuildProblem("Acesso negado", ex.Message, 401));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(BuildProblem("Falha ao finalizar simulado", ex.Message, 400));
        }
    }

    // ===================== US10 — GET /api/v1/exams/report/{id} =====================
    [HttpGet("report/{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FinishExamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FinishExamResponse>> GetReport(
        string id,
        CancellationToken ct)
    {
        var userId = GetAuthenticatedUserId();
        try
        {
            var result = await _examFacade.GetReportAsync(id, userId, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(BuildProblem("Acesso negado", ex.Message, 401));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(BuildProblem("Falha ao obter relatório", ex.Message, 400));
        }
    }

    [HttpGet("{id}/results")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ExamResultsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ExamResultsResponse>> GetResults(
        string id,
        [FromQuery] int limit = 10,
        [FromQuery] int offset = 0,
        [FromHeader(Name = "X-Anonymous-Token")] string? anonymousToken = null,
        CancellationToken ct = default)
    {
        var userId = GetAuthenticatedUserId();

        // Validação básica de paginação
        if (limit <= 0 || limit > 50) limit = 10;
        if (offset < 0) offset = 0;

        try
        {
            var result = await _examFacade.GetResultsAsync(id, userId, anonymousToken, limit, offset, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(BuildProblem("Acesso negado", ex.Message, 401));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(BuildProblem("Falha ao obter resultados", ex.Message, 400));
        }
    }


    [HttpGet("list/{studentId}")]
    [Authorize]
    [ProducesResponseType(typeof(StudentExamHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Obtém o histórico de simulados de um aluno")]
    [EndpointDescription("Retorna a lista paginada de simulados finalizados do aluno, ordenados do mais recente para o mais antigo. Acessível apenas pelo próprio aluno ou por um professor vinculado a ele.")]
    public async Task<ActionResult<StudentExamHistoryResponse>> GetStudentHistory(
        string studentId,
        [FromQuery] int limit = 10,
        [FromQuery] int offset = 0,
        CancellationToken ct = default)
    {
        var requestingUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(requestingUserId))
            return Unauthorized(BuildProblem("Acesso negado", "Não foi possível identificar o usuário autenticado.", 401));

        // Sanitização básica da paginação
        if (limit <= 0 || limit > 50) limit = 10;
        if (offset < 0) offset = 0;

        try
        {
            var result = await _examFacade.GetStudentHistoryAsync(studentId, requestingUserId, limit, offset, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(BuildProblem("Acesso negado", ex.Message, 401));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(BuildProblem("Falha ao obter histórico", ex.Message, 400));
        }
    }

    // ===================== Helpers =====================
    private string? GetAuthenticatedUserId()
    {
        if (User.Identity?.IsAuthenticated != true)
            return null;

        return User.FindFirst("sub")?.Value 
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }
            
    private static ProblemDetails BuildProblem(string title, string detail, int status)
        => new()
        {
            Title  = title,
            Detail = detail,
            Status = status,
            Type   = $"https://httpstatuses.io/{status}"
        };
}