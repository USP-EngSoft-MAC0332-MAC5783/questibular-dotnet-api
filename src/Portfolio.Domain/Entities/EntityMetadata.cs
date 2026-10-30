namespace Portfolio.Domain.Entities;

/// <summary>
/// Base class for all domain entities, providing common auditing and tracking properties.
/// </summary>
public abstract class EntityMetadata
{
    /// <summary>
    /// The unique identifier for the entity.
    /// </summary>
    public string Id { get; protected set; } = default!;

    /// <summary>
    /// The UTC date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// The UTC date and time when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// The identifier of the user or system that created the entity.
    /// </summary>
    public string? CreatedBy { get; protected set; }

    /// <summary>
    /// The identifier of the user or system that last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; protected set; }
}