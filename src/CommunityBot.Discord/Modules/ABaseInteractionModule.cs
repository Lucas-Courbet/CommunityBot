using System.Diagnostics;
using CommunityBot.Discord.Interactions;
using Microsoft.Extensions.Logging;
using NetCord.Services.ComponentInteractions;

namespace CommunityBot.Discord.Modules;

public abstract class ABaseInteractionModule(
    ILogger logger,
    IResponseService responseService)
    : ComponentInteractionModule<ComponentInteractionContext>
{
    protected IResponseService ResponseService { get; } = responseService;

    protected Task ExecuteInteractionAsync(string interactionName, Func<Task> operation)
        => ExecuteInteractionCoreAsync(interactionName, operation, deferModify: false);

    protected Task ExecuteDeferredInteractionAsync(string interactionName, Func<Task> operation)
        => ExecuteInteractionCoreAsync(interactionName, operation, deferModify: true);

    private async Task ExecuteInteractionCoreAsync(
        string interactionName,
        Func<Task> operation,
        bool deferModify)
    {
        var startedAt = Stopwatch.GetTimestamp();
        var deferred = false;

        try
        {
            logger.LogDebug(
                "Interaction {Interaction} started by {UserId}.",
                interactionName,
                Context.User.Id);

            if (deferModify)
            {
                await ResponseService.DeferModifyAsync(Context.Interaction);
                deferred = true;
            }

            await operation();

            logger.LogInformation(
                "Interaction {Interaction} completed in {ElapsedMs} ms for {UserId}.",
                interactionName,
                (long)Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
                Context.User.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Interaction {Interaction} failed for {UserId}.",
                interactionName,
                Context.User.Id);

            if (deferred)
            {
                await ResponseService.ModifyErrorAsync(
                    Context.Interaction,
                    "An internal error occurred.");

                return;
            }

            await ResponseService.RespondErrorAsync(
                Context.Interaction,
                "An internal error occurred.");
        }
    }
}