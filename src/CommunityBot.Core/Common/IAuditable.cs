namespace CommunityBot.Core.Common;

/// <summary>
/// Defines the contract for entities requiring automatic
/// persistence timestamp tracking.
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