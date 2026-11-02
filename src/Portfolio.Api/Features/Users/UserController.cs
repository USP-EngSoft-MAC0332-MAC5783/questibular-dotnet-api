namespace Portfolio.Api.Features.Users;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
//[Authorize]
public sealed class UserController : ControllerBase
{
    private readonly IUserService _users;

    public UserController(IUserService users)
        => _users = users ?? throw new ArgumentNullException(nameof(users));

    [HttpGet("list")]
    //[Authorize(Policy = "Admin")]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> List(CancellationToken ct)
        => Ok(await _users.ListAsync(ct));

    [HttpGet("{id}")]
    //[Authorize(Policy = "SelfOnly")]
    public async Task<ActionResult<UserResponse>> GetById(string id, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(id, ct);
        return user is null ? NotFound() : Ok(user);
    }
}