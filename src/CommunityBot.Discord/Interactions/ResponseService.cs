using CommunityBot.Discord.Rendering;
using NetCord;
using NetCord.Rest;

namespace CommunityBot.Discord.Interactions;

public sealed class ResponseService(RestClient restClient) : IResponseService
{
    public async Task RespondAsync(IInteraction interaction, ResponseRequest response)
    {
        var properties = new InteractionMessageProperties
        {
            Content = response.Content,
            Flags = response.IsEphemeral ? MessageFlags.Ephemeral : default
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
            ResponseRequest.FromEmbed(EmbedFactory.Error(message), isEphemeral: true));

    public Task DeferModifyAsync(IInteraction interaction)
        => interaction.SendResponseAsync(InteractionCallback.DeferredModifyMessage);

    public async Task ModifyAsync(IInteraction interaction, ResponseRequest response)
    {
        await restClient.ModifyInteractionResponseAsync(
            interaction.ApplicationId,
            interaction.Token,
            properties =>
            {
                properties.Content = response.Content;
                properties.Embeds = response.Embed is null ? [] : [response.Embed];
                properties.Components = response.Components ?? [];
            });
    }

    public Task ModifyErrorAsync(IInteraction interaction, string message)
        => ModifyAsync(
            interaction,
            ResponseRequest.FromEmbed(EmbedFactory.Error(message)));
}