using NetCord;

namespace CommunityBot.Discord.Interactions;

/// <summary>
/// Provides standardized response operations for Discord interactions.
/// </summary>
public interface IResponseService
{
    /// <summary>
    /// Sends the initial interaction response.
    /// </summary>
    Task RespondAsync(IInteraction interaction, ResponseRequest response);

    /// <summary>
    /// Sends an ephemeral error as the initial interaction response.
    /// </summary>
    Task RespondErrorAsync(IInteraction interaction, string message);

    /// <summary>
    /// Acknowledges a component interaction so its source message can be modified later.
    /// </summary>
    Task DeferModifyAsync(IInteraction interaction);

    /// <summary>
    /// Replaces the original interaction response.
    /// </summary>
    Task ModifyAsync(IInteraction interaction, ResponseRequest response);

    /// <summary>
    /// Replaces the original interaction response with an error.
    /// </summary>
    Task ModifyErrorAsync(IInteraction interaction, string message);
}