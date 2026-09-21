namespace CommunityBot.Core.Common;

/// <summary>
/// Defines the common identity contract for persisted entities.
/// </summary>
public interface IEntity<out TKey>
{
    TKey Id { get; }
}