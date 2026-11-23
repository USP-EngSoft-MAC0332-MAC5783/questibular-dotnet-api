namespace Portfolio.Api.Features.ExamResults;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/teachers/classes")]
[Produces("application/json")]
[Authorize(Roles = "teacher")]
public sealed class TeacherClassesController(
    ITeacherClassService classes,
    ILogger<TeacherClassesController> logger,
    IApplicationContextService appContextService)
    : BaseApiController<TeacherClassesController>(logger, appContextService)
{
    [HttpPost]
    [ProducesResponseType(typeof(TeacherClassResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Cria uma nova turma para o professor autenticado")]
    public async Task<ActionResult<TeacherClassResponse>> CreateClass(
        [FromBody] CreateTeacherClassRequest request,
        CancellationToken ct)
    {
        var teacherId = GetTeacherId();
        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        try
        {
            var created = await classes.CreateClassAsync(teacherId, request.Code, ct);
            return CreatedAtAction(nameof(ListClasses), new { }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(BuildProblem("Código de turma já existe.", ex.Message, 409));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(BuildProblem("Código inválido.", ex.Message, 400));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TeacherClassResponse>), StatusCodes.Status200OK)]
    [EndpointSummary("Lista todas as turmas criadas pelo professor autenticado")]
    public async Task<ActionResult<IReadOnlyList<TeacherClassResponse>>> ListClasses(CancellationToken ct)
    {
        var teacherId = GetTeacherId();
        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        return Ok(await classes.GetMyClassesAsync(teacherId, ct));
    }

    [HttpGet("{classCode}/students")]
    [ProducesResponseType(typeof(IReadOnlyList<ClassStudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Lista os estudantes cadastrados em uma turma do professor")]
    [EndpointDescription("Retorna os alunos que informaram o código da turma no cadastro/perfil, indicando via 'isLinked' se já estão vinculados ao professor.")]
    public async Task<ActionResult<IReadOnlyList<ClassStudentDto>>> GetClassStudents(
        string classCode,
        CancellationToken ct)
    {
        var teacherId = GetTeacherId();
        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        try
        {
            return Ok(await classes.GetClassStudentsAsync(teacherId, classCode, ct));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(BuildProblem("Turma não encontrada.", ex.Message, 404));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, BuildProblem("Acesso negado.", ex.Message, 403));
        }
    }

    [HttpDelete("{classCode}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Remove uma turma do professor autenticado")]
    public async Task<IActionResult> DeleteClass(string classCode, CancellationToken ct)
    {
        var teacherId = GetTeacherId();
        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        if (!await classes.DeleteClassAsync(teacherId, classCode, ct))
            return NotFound(BuildProblem("Turma não encontrada.", "Nenhuma turma com este código foi encontrada para o seu perfil.", 404));

        return NoContent();
    }

    private string? GetTeacherId()
        => User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    private ObjectResult BuildProblem(string title, string detail, int status)
        => new(new ProblemDetails
        {
            Type = $"https://httpstatuses.io/{status}",
            Title = title,
            Status = status,
            Detail = detail,
            Instance = HttpContext.Request.Path
        }) { StatusCode = status };
}