using CommunityBot.Discord.Interactions;
using CommunityBot.Discord.Pagination;
using Microsoft.Extensions.Logging;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace CommunityBot.Discord.Modules;

public sealed class PaginationInteractionModule(
    IPaginationDispatcher paginationDispatcher,
    ILogger<PaginationInteractionModule> logger,
    IResponseService responseService)
    : ABaseInteractionModule(logger, responseService)
{
    [ComponentInteraction("pagination")]
    public Task HandlePaginationAsync(string source, ulong ownerId, int pageIndex)
    {
        return ExecuteInteractionAsync("pagination", async () =>
        {
            if (Context.User.Id != ownerId)
            {
                await ResponseService.RespondErrorAsync(
                    Context.Interaction,
                    "You cannot interact with this pagination.");

                return;
            }

            var page = await paginationDispatcher.GetPageAsync(
                new PaginationRequest(source, ownerId, pageIndex));

            if (page is null)
            {
                await ResponseService.RespondErrorAsync(
                    Context.Interaction,
                    "Unknown pagination source.");

                return;
            }

            await Context.Interaction.SendResponseAsync(
                InteractionCallback.ModifyMessage(properties =>
                {
                    properties.Embeds = [page.Embed];
                    properties.Components = page.Components;
                }));
        });
    }
}