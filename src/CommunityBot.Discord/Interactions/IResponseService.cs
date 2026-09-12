using CommunityBot.Discord.Models;
using NetCord;

namespace CommunityBot.Discord.Interactions;

public interface IResponseService
{
    Task RespondAsync(IInteraction interaction, ResponseRequest response);

    Task RespondErrorAsync(IInteraction interaction, string message);

    Task DeferModifyAsync(IInteraction interaction);

    Task ModifyAsync(IInteraction interaction, ResponseRequest response);

    Task ModifyErrorAsync(IInteraction interaction, string message);
}