using System.Diagnostics;
using CommunityBot.Discord.Interactions;
using Microsoft.Extensions.Logging;
using NetCord.Services.ApplicationCommands;

namespace CommunityBot.Discord.Modules;

/// <summary>
/// Base class for application command modules.
/// Centralizes logging and standardized command error handling.
/// </summary>
public abstract class ABaseSlashModule(
    ILogger logger,
    IResponseService responseService)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    protected IResponseService ResponseService { get; } = responseService;

    protected async Task ExecuteCommandAsync(string commandName, Func<Task> operation)
    {
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            logger.LogDebug(
                "Command {Command} started by {UserId}.",
                commandName,
                Context.User.Id);

            await operation();

            logger.LogInformation(
                "Command {Command} completed in {ElapsedMs} ms for {UserId}.",
                commandName,
                (long)Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
                Context.User.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Command {Command} failed for {UserId}.",
                commandName,
                Context.User.Id);

            await ResponseService.RespondErrorAsync(
                Context.Interaction,
                "An internal error occurred.");
        }
    }
}