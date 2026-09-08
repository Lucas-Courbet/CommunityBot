using CommunityBot.Discord.Models;
using NetCord;

namespace CommunityBot.Discord.Interactions;

public interface IResponseService
{
    Task RespondAsync(IInteraction interaction, ResponseRequest response);

    Task RespondErrorAsync(IInteraction interaction, string message);
}