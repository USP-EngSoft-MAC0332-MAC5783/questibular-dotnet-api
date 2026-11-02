namespace Portfolio.Api.Features.Auth.Contracts;

public static class AuthMappings
{
    // Mapeia do AuthResult para o UserResponse
    public static UserResponse ToUserResponse(this AuthResult result)
    {
        if (string.IsNullOrEmpty(result.UserId))
            throw new InvalidOperationException("UserId não pode ser nulo ao mapear um usuário com sucesso.");

        return new UserResponse(
            result.UserId,
            result.Email ?? string.Empty,
            result.Name ?? string.Empty,
            result.Roles ?? new List<string>()
        );
    }

    // Mapeia do ClaimsPrincipal (User da Controller) para o UserResponse
    public static UserResponse ToUserResponse(this ClaimsPrincipal user)
    {
        var id = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = user.FindFirst("email")?.Value;
        var name = user.FindFirst("name")?.Value;
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return new UserResponse(
            id ?? string.Empty,
            email ?? string.Empty,
            name ?? string.Empty,
            roles
        );
    }
}