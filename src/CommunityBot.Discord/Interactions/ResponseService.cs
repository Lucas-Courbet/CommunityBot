using CommunityBot.Discord.Models;
using CommunityBot.Discord.Rendering;
using NetCord;
using NetCord.Rest;

namespace CommunityBot.Discord.Interactions;

public sealed class ResponseService : IResponseService
{
    public async Task RespondAsync(IInteraction interaction, ResponseRequest response)
    {
        var properties = new InteractionMessageProperties
        {
            Content = response.Content,
            Flags = response.IsEphemeral
                ? MessageFlags.Ephemeral
                : default
        };

        if (response.Embed is not null)
            properties.Embeds = [response.Embed];

        if (response.Components is not null)
            properties.Components = response.Components;

        await interaction.SendResponseAsync(InteractionCallback.Message(properties));
    }

    public Task RespondErrorAsync(IInteraction interaction, string message)
        => RespondAsync(
            interaction,
            ResponseRequest.FromEmbed(
                EmbedFactory.Error(message),
                isEphemeral: true));
}