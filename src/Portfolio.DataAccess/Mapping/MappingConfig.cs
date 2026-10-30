namespace Portfolio.DataAccess.Mapping;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ── USER ───────────────────────────────────────────────────────
        config.NewConfig<UserDbModel, User>()
            .MapWith(src => User.Reconstitute(
                src.Id, src.Email, src.PasswordHash,
                src.Roles, src.TwoFactorEnabled, src.TwoFactorSecret,
                src.TwoFactorRecoveryCodes, src.AccessFailedCount,
                src.LockoutEnd, src.LastLoginAt, src.CreatedAt,
                src.EmailConfirmed, src.TwoFactorEnabledAt
            ));

        // Save
        config.NewConfig<User, UserDbModel>()
            .Map(dest => dest.Roles, src => src.Roles.ToList())
            .Map(dest => dest.TwoFactorRecoveryCodes, src => src.TwoFactorRecoveryCodes.ToList());

        // ── REFRESH TOKEN ──────────────────────────────────────────────
        // Save
        config.NewConfig<RefreshToken, RefreshTokenDbModel>();

        // Read - Factory Method
        config.NewConfig<RefreshTokenDbModel, RefreshToken>()
            .MapWith(src => RefreshToken.Reconstitute(
                src.Id, src.UserId, src.TokenHash, src.ExpiresAt,
                src.RevokedAt, src.ReplacedByTokenHash, src.CreatedByIp,
                src.RevokedByIp, src.CreatedAt
            ));
    }
}