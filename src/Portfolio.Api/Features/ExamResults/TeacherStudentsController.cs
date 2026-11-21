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
    [HttpPost("{studentId}")]
    [EndpointSummary("Vincula um aluno ao professor autenticado")]
    [EndpointDescription("Registra o vínculo professor-aluno usado para autorizar a consulta aos resultados do aluno.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LinkStudent(string studentId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return ProblemResponse(StatusCodes.Status400BadRequest, "Parâmetro inválido.",
                "O identificador do aluno é obrigatório.");

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

    /// <summary>Lists all students linked to the authenticated teacher with their latest exam accuracy.</summary>
    [HttpGet]
    [EndpointSummary("Lista os alunos vinculados ao professor autenticado")]
    [EndpointDescription("Retorna a lista de alunos vinculados ao professor autenticado, incluindo o nome do aluno e a taxa de acerto do seu último simulado finalizado.")]
    [ProducesResponseType(typeof(IReadOnlyList<LinkedStudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<LinkedStudentDto>>> ListStudents(CancellationToken cancellationToken)
    {
        var teacherId = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(teacherId))
            return ProblemResponse(StatusCodes.Status401Unauthorized, "Autenticação necessária.",
                "Não foi possível identificar o professor autenticado.");

        var students = await _students.GetLinkedStudentsAsync(teacherId, cancellationToken);
        return Ok(students);
    }

    /// <summary>Unlinks a student from the authenticated teacher.</summary>
    [HttpDelete("{studentId}")]
    [EndpointSummary("Remove o vínculo de um aluno com o professor autenticado")]
    [EndpointDescription("Exclui o vínculo professor-aluno, revogando a autorização do professor para consultar os resultados deste aluno.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkStudent(string studentId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(studentId) || studentId.Trim().Length is not (32 or 36))
            return ProblemResponse(StatusCodes.Status400BadRequest, "Identificador inválido.",
                "O identificador do aluno informado possui formato inválido.");

        var teacherId = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(teacherId))
            return ProblemResponse(StatusCodes.Status401Unauthorized, "Autenticação necessária.",
                "Não foi possível identificar o professor autenticado.");

        if (!await _students.UnlinkStudentAsync(teacherId, studentId, cancellationToken))
            return ProblemResponse(StatusCodes.Status404NotFound, "Vínculo não encontrado.",
                "Não foi encontrado um vínculo ativo entre você e o aluno informado.");

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
