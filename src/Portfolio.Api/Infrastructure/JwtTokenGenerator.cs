namespace Portfolio.Api.Infrastructure.Security;

public sealed class JwtTokenGenerator : ITokenGenerator
{
    private const string TwoFactorPendingScope = "2fa:pending";

    private readonly JwtSettings _settings;
    private readonly IClock _clock;
    private readonly SigningCredentials _signingCredentials;
    private readonly TokenValidationParameters _validationParameters;

    public JwtTokenGenerator(IOptions<JwtSettings> options, IClock clock)
    {
        _settings = options.Value;
        _clock = clock;

        if (string.IsNullOrWhiteSpace(_settings.Secret) || _settings.Secret.Length < 32)
            throw new InvalidOperationException("Jwt:Secret precisa ter no mínimo 32 caracteres.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _settings.Issuer,
            ValidAudience = _settings.Audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        };
    }

    public AccessToken GenerateAccessToken(User user)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };
        foreach (var role in user.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer, audience: _settings.Audience,
            claims: claims, notBefore: now, expires: expires,
            signingCredentials: _signingCredentials);

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public RefreshTokenResult GenerateRefreshToken(string userId, string? createdByIp = null)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');
        
        var hash = HashRefreshToken(raw);
        var now = _clock.UtcNow;

        var entity = RefreshToken.Create(
            userId: userId,
            tokenHash: hash,
            expiresAt: now.AddDays(_settings.RefreshTokenDays),
            createdByIp: createdByIp,
            now: now);

        return new RefreshTokenResult(entity, raw);
    }

    public string HashRefreshToken(string rawToken)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

    public string GeneratePendingTwoFactorToken(string userId)
    {
        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new("scope", TwoFactorPendingScope),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer, audience: _settings.Audience,
            claims: claims, notBefore: now, expires: now.AddMinutes(5),
            signingCredentials: _signingCredentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool TryGetUserIdFromPendingToken(string token, out string userId)
    {
        userId = string.Empty;
        try
        {
            var principal = new JwtSecurityTokenHandler { MapInboundClaims = false }
                .ValidateToken(token, _validationParameters, out _);
            if (principal.FindFirst("scope")?.Value != TwoFactorPendingScope) return false;
            var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(sub)) return false;
            userId = sub;
            return true;
        }
        catch { return false; }
    }
}