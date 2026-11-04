namespace Portfolio.Api.Features.Auth;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public sealed class AuthController : BaseApiController<AuthController>
{
    private readonly IAuthFacade _authFacade;

    public AuthController(
        IAuthFacade authFacade, 
        ILogger<AuthController> logger,
        IApplicationContextService appContextService) 
        : base(logger, appContextService)
        => _authFacade = authFacade ?? throw new ArgumentNullException(nameof(authFacade));

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _authFacade.RegisterAsync(request, GetClientIp(), ct);
        return result.Succeeded ? Ok(MapToResponse(result)) 
            : BadRequest(BuildProblem("Falha no registro", result.ErrorMessage!, 400));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.Code) && !string.IsNullOrWhiteSpace(request.PendingToken))
        {
            var verified = await _authFacade.VerifyTwoFactorAsync(request.PendingToken, request.Code, GetClientIp(), ct);
            return verified.Succeeded ? Ok(MapToResponse(verified)) 
                : Unauthorized(BuildProblem("Falha 2FA", verified.ErrorMessage!, 401));
        }

        var result = await _authFacade.LoginAsync(request, GetClientIp(), ct);
        
        if (result.RequiresTwoFactor)
            return Ok(MapToResponse(result));

        return result.Succeeded ? Ok(MapToResponse(result)) 
            : Unauthorized(BuildProblem("Falha na autenticação", result.ErrorMessage!, 401));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _authFacade.RefreshAsync(request.RefreshToken, GetClientIp(), ct);
        return result.Succeeded ? Ok(MapToResponse(result)) 
            : Unauthorized(BuildProblem("Falha ao renovar", result.ErrorMessage!, 401));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        await _authFacade.LogoutAsync(request.RefreshToken, GetClientIp(), ct);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<UserResponse> Me()
    {
        var userResponse = User.ToUserResponse();

        if (string.IsNullOrEmpty(userResponse.Id)) return Unauthorized();
        return Ok(userResponse);
    }

    [HttpPost("2fa/setup")]
    [Authorize]
    public async Task<ActionResult<TwoFactorSetupResponse>> SetupTwoFactor(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var response = await _authFacade.SetupTwoFactorAsync(userId, ct);
        return Ok(response);
    }

    [HttpPost("2fa/activate")]
    [Authorize]
    public async Task<ActionResult<TwoFactorActivateResponse>> ActivateTwoFactor([FromBody] TwoFactorCodeRequest request, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var response = await _authFacade.ActivateTwoFactorAsync(userId, request.Code, ct);
        return Ok(response);
    }

    [HttpPost("2fa/disable")]
    [Authorize]
    public async Task<IActionResult> DisableTwoFactor([FromBody] TwoFactorCodeRequest request, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        await _authFacade.DisableTwoFactorAsync(userId, request.Code, ct);
        return NoContent();
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