namespace CommunityBot.Core.Common;

/// <summary>
/// Marks entities whose persistence timestamps are maintained automatically.
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// Gets or sets the timestamp at which the entity was first persisted.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp at which the entity was last updated.
    /// </summary>
    DateTime UpdatedAt { get; set; }
}