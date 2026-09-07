namespace CommunityBot.Core.Common;

/// <summary>
/// Defines the common identity contract for persisted entities.
/// </summary>
public interface IEntity<out TKey>
{
    /// <summary>
    /// Gets the entity primary key.
    /// </summary>
    TKey Id { get; }
}