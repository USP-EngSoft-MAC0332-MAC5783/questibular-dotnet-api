namespace Portfolio.Api.Features.Auth;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthFacade _authFacade;

    public AuthController(IAuthFacade authFacade)
        => _authFacade = authFacade ?? throw new ArgumentNullException(nameof(authFacade));

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _authFacade.RegisterAsync(request, GetClientIp(), ct);
        return result.Succeeded ? Ok(MapToResponse(result)) 
            : BadRequest(BuildProblem("Falha no registro", result.ErrorMessage!, 400));
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<UserResponse> Me()
    {
        var userResponse = User.ToUserResponse();

        if (string.IsNullOrEmpty(userResponse.Id)) return Unauthorized();
        return Ok(userResponse);
    }

    private string? GetClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private static ProblemDetails BuildProblem(string title, string detail, int status)
        => new() { Title = title, Detail = detail, Status = status, Type = $"https://httpstatuses.io/{status}" };

    private static AuthResponse MapToResponse(AuthResult result)
        => new(
            AccessToken: result.AccessToken ?? string.Empty,
            RefreshToken: result.RefreshToken ?? string.Empty,
            AccessTokenExpiresAt: result.AccessTokenExpiresAt ?? DateTime.UtcNow.AddMinutes(5),
            RefreshTokenExpiresAt: result.RefreshTokenExpiresAt ?? DateTime.UtcNow.AddMinutes(5),
            User: result.ToUserResponse()
        );
}