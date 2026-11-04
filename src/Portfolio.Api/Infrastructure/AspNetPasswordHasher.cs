namespace Portfolio.Api.Infrastructure.Security;

public sealed class AspNetPasswordHasher : IPasswordHasher
{
    private readonly AspNetHasher _inner;

    public AspNetPasswordHasher(AspNetHasher inner)
        => _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    public string Hash(string plainPassword)
        => _inner.HashPassword(null!, plainPassword);

    public PasswordVerification Verify(string hashedPassword, string providedPassword)
    {
        var result = _inner.VerifyHashedPassword(null!, hashedPassword, providedPassword);
        return result switch
        {
            AspNetResult.SuccessRehashNeeded => PasswordVerification.SuccessRehashNeeded,
            AspNetResult.Success => PasswordVerification.Success,
            _ => PasswordVerification.Failed
        };
    }
}

/// <summary>Generic Wrapper — Identity requires TUser, we don't need it.</summary>
public sealed class GenericPasswordHasher : AspNetHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<object> _inner = new();

    public string HashPassword(object user, string password) => _inner.HashPassword(user, password);

    public AspNetResult VerifyHashedPassword(object user, string hashed, string provided)
        => _inner.VerifyHashedPassword(user, hashed, provided);
}