namespace Portfolio.Domain.Entities;

/// <summary>
/// Represents an application user, including credentials, roles, and security states.
/// </summary>
public sealed class User : EntityMetadata
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;

    private readonly List<string> _roles = [];
    public IReadOnlyCollection<string> Roles => _roles.AsReadOnly();

    public bool EmailConfirmed { get; private set; } // NOSONAR
    public DateTime? LastLoginAt { get; private set; }
    public int AccessFailedCount { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public bool TwoFactorEnabled { get; private set; } // NOSONAR
    public string? TwoFactorSecret { get; private set; } // NOSONAR

    private readonly List<string> _twoFactorRecoveryCodes = [];
    public IReadOnlyCollection<string> TwoFactorRecoveryCodes => _twoFactorRecoveryCodes.AsReadOnly();

    public DateTime? TwoFactorEnabledAt { get; private set; } // NOSONAR

    /// <summary>
    /// Indicates whether the user account is currently locked out.
    /// </summary>
    public bool IsLockedOutAt(DateTime now) => LockoutEnd.HasValue && LockoutEnd.Value > now;

    private User() { }

    /// <summary>
    /// Creates a new User instance representing a new registration.
    /// Applies default business rules, such as generating a unique identifier, setting the creation date, and assigning the default "User" role.
    /// </summary>
    /// <param name="email">The user's email address. Must not be null or whitespace.</param>
    /// <param name="passwordHash">The securely hashed representation of the user's password.</param>
    /// <param name="now">The current UTC date and time, used to set the creation timestamp.</param>
    /// <returns>A newly created <see cref="User"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided email is null, empty, or consists only of white-space characters.</exception>
    public static User Create(string email, string passwordHash, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email inválido.");
        
        User user = new()
        {
            Id = Guid.NewGuid().ToString("N"),
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = now
        };

        user._roles.Add("User");
        return user;
    }

    /// <summary>
    /// Reconstitutes an existing User entity from persistence (e.g., database).
    /// This method is intended to be used exclusively by the Infrastructure layer to rehydrate 
    /// an object without re-executing creation validations or business rules.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="passwordHash">The securely hashed representation of the user's password.</param>
    /// <param name="roles">The collection of roles assigned to the user.</param>
    /// <param name="twoFactorEnabled">Indicates whether Two-Factor Authentication is currently active.</param>
    /// <param name="twoFactorSecret">The protected secret key used for 2FA.</param>
    /// <param name="recoveryCodes">The collection of hashed 2FA recovery codes.</param>
    /// <param name="accessFailedCount">The number of consecutive failed login attempts.</param>
    /// <param name="lockoutEnd">The UTC date and time until which the account is locked, if applicable.</param>
    /// <param name="lastLoginAt">The UTC date and time of the user's last successful login.</param>
    /// <param name="createdAt">The UTC date and time when the user was originally created.</param>
    /// <returns>The fully reconstituted <see cref="User"/> entity.</returns>
    public static User Reconstitute(
        string id, string email, string passwordHash, 
        IEnumerable<string> roles, bool twoFactorEnabled, string? twoFactorSecret, 
        IEnumerable<string> recoveryCodes, int accessFailedCount, DateTime? lockoutEnd, 
        DateTime? lastLoginAt, DateTime createdAt)
    {
        var user = new User
        {
            Id = id,
            Email = email,
            PasswordHash = passwordHash,
            TwoFactorEnabled = twoFactorEnabled,
            TwoFactorSecret = twoFactorSecret,
            AccessFailedCount = accessFailedCount,
            LockoutEnd = lockoutEnd,
            LastLoginAt = lastLoginAt,
            CreatedAt = createdAt
        };

        user._roles.AddRange(roles);
        user._twoFactorRecoveryCodes.AddRange(recoveryCodes);
        return user;
    }

    /// <summary>
    /// Registers a failed login attempt and locks the account if the threshold is reached.
    /// </summary>
    public void RegisterFailedAccess(int maxAttempts, TimeSpan lockoutDuration, DateTime now)
    {
        AccessFailedCount++;
        if (AccessFailedCount >= maxAttempts)
        {
            LockoutEnd = now.Add(lockoutDuration);
        }
    }

    /// <summary>
    /// Resets the failed access count and updates the last login timestamp upon successful authentication.
    /// </summary>
    public void RegisterSuccessfulLogin(DateTime now)
    {
        AccessFailedCount = 0;
        LockoutEnd = null;
        LastLoginAt = now;
    }

    public void SetupTwoFactor(string protectedSecret)
    {
        if (TwoFactorEnabled) throw new InvalidOperationException("2FA já está habilitado.");
        TwoFactorSecret = protectedSecret;
    }

    public void EnableTwoFactor(List<string> hashedRecoveryCodes, DateTime now)
    {
        TwoFactorEnabled = true;
        _twoFactorRecoveryCodes.Clear();
        _twoFactorRecoveryCodes.AddRange(hashedRecoveryCodes);
        TwoFactorEnabledAt = now;
    }

    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
        TwoFactorSecret = null;
        _twoFactorRecoveryCodes.Clear();
        TwoFactorEnabledAt = null;
    }

    public bool UseRecoveryCode(string hashedCode)
    {
        var code = TwoFactorRecoveryCodes.FirstOrDefault(c => c == hashedCode);
        if (code is null) return false;
        
        _twoFactorRecoveryCodes.Remove(code);
        return true;
    }
}