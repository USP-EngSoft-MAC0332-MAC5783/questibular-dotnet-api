namespace Portfolio.Api.Features.Users;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;

    public UserService(IUserRepository users)
        => _users = users ?? throw new ArgumentNullException(nameof(users));

    public Task<IReadOnlyList<UserResponse>> ListAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<UserResponse>>(Array.Empty<UserResponse>());
    }

    public async Task<UserResponse?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(id, ct);
        
        return user is null 
            ? null 
            : new UserResponse(user.Id, user.Email, user.Name, user.Roles.ToList());
    }
}