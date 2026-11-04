namespace Portfolio.DataAccess.Mapping;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        // ── USER ───────────────────────────────────────────────────────
        TypeAdapterConfig<UserDbModel, User>.NewConfig()
            .MapWith(src => User.Reconstitute(
                src.Id, src.Email, src.Name, src.PasswordHash,
                src.Roles, src.TwoFactorEnabled, src.TwoFactorSecret,
                src.TwoFactorRecoveryCodes, src.AccessFailedCount,
                src.LockoutEnd, src.LastLoginAt, src.CreatedAt,
                src.EmailConfirmed, src.TwoFactorEnabledAt
            ));

        // Save
        TypeAdapterConfig<User, UserDbModel>.NewConfig()
            .Map(dest => dest.Roles, src => src.Roles.ToList())
            .Map(dest => dest.TwoFactorRecoveryCodes, src => src.TwoFactorRecoveryCodes.ToList());

        // ── REFRESH TOKEN ──────────────────────────────────────────────
        // Save
        TypeAdapterConfig<RefreshToken, RefreshTokenDbModel>.NewConfig();

        // Read - Factory Method
        TypeAdapterConfig<RefreshTokenDbModel, RefreshToken>.NewConfig()
            .MapWith(src => RefreshToken.Reconstitute(
                src.Id, src.UserId, src.TokenHash, src.ExpiresAt,
                src.RevokedAt, src.ReplacedByTokenHash, src.CreatedByIp,
                src.RevokedByIp, src.CreatedAt
            ));
    }

    /// <summary>
    /// Automatically runs when the assembly of the project is loaded,
    /// guaranteeing that Mapster is configured in any process
    /// (API, tests, CLI tools, CI jobs).
    /// </summary>
    [ModuleInitializer]
    internal static void Initialize() => RegisterMappings();
}