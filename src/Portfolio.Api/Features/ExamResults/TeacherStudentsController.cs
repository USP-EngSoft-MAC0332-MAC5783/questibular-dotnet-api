namespace Portfolio.Api.Features.ExamResults;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/teachers/students")]
[Produces("application/json")]
[Authorize(Roles = "teacher")]
public sealed class TeacherStudentsController : BaseApiController<TeacherStudentsController>
{
    private readonly ITeacherStudentService _students;

    public TeacherStudentsController(
        ITeacherStudentService students,
        ILogger<TeacherStudentsController> logger,
        IApplicationContextService appContextService)
        : base(logger, appContextService)
        => _students = students ?? throw new ArgumentNullException(nameof(students));

    /// <summary>Links a student to the authenticated teacher.</summary>
    [HttpPost("{studentId:guid}")]
    [EndpointSummary("Vincula um aluno ao professor autenticado")]
    [EndpointDescription("Registra o vínculo professor-aluno usado para autorizar a consulta aos resultados do aluno.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LinkStudent(Guid studentId, CancellationToken cancellationToken)
    {
        var teacherId = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(teacherId))
            return ProblemResponse(StatusCodes.Status401Unauthorized, "Autenticação necessária.",
                "Não foi possível identificar o professor autenticado.");

        if (!await _students.LinkStudentAsync(teacherId, studentId, cancellationToken))
            return ProblemResponse(StatusCodes.Status404NotFound, "Aluno não encontrado.",
                "Não foi encontrado um aluno com o identificador informado.");

        return NoContent();
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
}
