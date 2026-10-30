namespace Portfolio.DataAccess.Mapping;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        // ── USER ───────────────────────────────────────────────────────
        
        // Do Domínio para o Banco (Salvar)
        TypeAdapterConfig<UserDbModel, User>.NewConfig()
            .MapWith(src => User.Reconstitute(
                src.Id, src.Email, src.PasswordHash,
                src.Roles, src.TwoFactorEnabled, src.TwoFactorSecret,
                src.TwoFactorRecoveryCodes, src.AccessFailedCount,
                src.LockoutEnd, src.LastLoginAt, src.CreatedAt
            ));

        // Do Domínio para o Banco
        TypeAdapterConfig<User, UserDbModel>.NewConfig()
            .Map(dest => dest.Roles, src => src.Roles.ToList())
            .Map(dest => dest.TwoFactorRecoveryCodes, src => src.TwoFactorRecoveryCodes.ToList());

        // ── REFRESH TOKEN ──────────────────────────────────────────────

        // Do Domínio para o Banco (Salvar)
        TypeAdapterConfig<RefreshToken, RefreshTokenDbModel>.NewConfig();

        // Do Banco para o Domínio (Ler) - Usa o seu Factory Method seguro
        TypeAdapterConfig<RefreshTokenDbModel, RefreshToken>.NewConfig()
            .MapWith(src => RefreshToken.Reconstitute(
                src.Id, src.UserId, src.TokenHash, src.ExpiresAt,
                src.RevokedAt, src.ReplacedByTokenHash, src.CreatedByIp,
                src.RevokedByIp, src.CreatedAt
            ));
    }
}