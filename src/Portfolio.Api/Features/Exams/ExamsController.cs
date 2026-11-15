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

    // ===================== Helpers =====================
    private string? GetAuthenticatedUserId()
        => User.Identity?.IsAuthenticated == true
            ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            : null;
            
    private static ProblemDetails BuildProblem(string title, string detail, int status)
        => new()
        {
            Title  = title,
            Detail = detail,
            Status = status,
            Type   = $"https://httpstatuses.io/{status}"
        };
}