namespace Portfolio.Api.Features.Users;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Authorize]
public sealed class UserController : BaseApiController<UserController>
{
    private readonly IUserService _users;
    private readonly IExamFacade _examFacade;

    public UserController(
        IUserService users,
        IExamFacade examFacade,
        ILogger<UserController> logger,
        IApplicationContextService appContextService)
        : base(logger, appContextService)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
        _examFacade = examFacade ?? throw new ArgumentNullException(nameof(examFacade));
    }

    [HttpGet("list")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> List(CancellationToken ct)
        => Ok(await _users.ListAsync(ct));

    [HttpGet("{id}")]
    [Authorize(Policy = "SelfOnly")]
    public async Task<ActionResult<UserResponse>> GetById(string id, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(id, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet("/api/v{version:apiVersion}/users/score")]
    [HttpGet("/api/v{version:apiVersion}/users/score/{studentId}")]
    [ProducesResponseType(typeof(StudentScoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [EndpointSummary("Obtém o resumo de pontuação e evolução do aluno")]
    [EndpointDescription("Retorna as métricas de desempenho do aluno autenticado quando studentId não é informado, ou do aluno especificado quando acessado por um professor vinculado a ele.")]
    public async Task<ActionResult<StudentScoreResponse>> GetStudentScore(
        string? studentId = null,
        CancellationToken ct = default)
    {
        var requestingUserId = User.FindFirst("sub")?.Value
                            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(requestingUserId))
            return ProblemResponse(StatusCodes.Status401Unauthorized, "Autenticação necessária.",
                "Não foi possível identificar o usuário autenticado.");

        try
        {
            var result = await _examFacade.GetStudentScoreAsync(studentId, requestingUserId, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ProblemResponse(StatusCodes.Status403Forbidden, "Acesso negado.", ex.Message);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return ProblemResponse(StatusCodes.Status400BadRequest, "Falha ao obter pontuação.", ex.Message);
        }
    }

    [HttpPut("profile")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Atualiza o perfil do usuário autenticado")]
    [EndpointDescription("Permite editar o nome e, para alunos, informar ou alterar o código da turma (ex.: ESC-3A).")]
    public async Task<ActionResult<UserResponse>> UpdateProfile(
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken ct)
    {
        var userId = User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            return ProblemResponse(StatusCodes.Status401Unauthorized, "Autenticação necessária.",
                "Não foi possível identificar o usuário autenticado.");

        try
        {
            var updated = await _users.UpdateProfileAsync(userId, request, ct);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return ProblemResponse(StatusCodes.Status404NotFound, "Usuário não encontrado.", ex.Message);
        }
        catch (ArgumentException ex)
        {
            return ProblemResponse(StatusCodes.Status400BadRequest, "Dados inválidos.", ex.Message);
        }
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