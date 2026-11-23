namespace Portfolio.Api.Features.Users;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> ListAsync(CancellationToken ct = default);
    Task<UserResponse?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<UserResponse> UpdateProfileAsync(string userId, UpdateUserProfileRequest request, CancellationToken ct = default);
}