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

    // ===================== Helpers =====================

    private static ProblemDetails BuildProblem(string title, string detail, int status)
        => new()
        {
            Title  = title,
            Detail = detail,
            Status = status,
            Type   = $"https://httpstatuses.io/{status}"
        };
}