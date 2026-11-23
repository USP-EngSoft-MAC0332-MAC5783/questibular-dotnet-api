namespace Portfolio.Api.Features.Users;

public sealed class UserService(
    IUserRepository _users,
    ITeacherClassRepository _classes) : IUserService
{
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

    public async Task<UserResponse> UpdateProfileAsync(string userId, UpdateUserProfileRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        var normalizedClassCode = TeacherClass.NormalizeCode(request.ClassCode);

        if (user.Roles.Contains("student", StringComparer.OrdinalIgnoreCase) && normalizedClassCode is not null)
        {
            var exists = await _classes.ExistsByCodeAsync(normalizedClassCode, ct);
            if (!exists)
                throw new ArgumentException("O código da turma informado não existe.");
        }

        user.UpdateProfile(request.Name, normalizedClassCode);
        await _users.UpdateAsync(user, ct);

        return new UserResponse(user.Id, user.Email, user.Name, user.Roles.ToList(), user.ClassCode);
    }
}