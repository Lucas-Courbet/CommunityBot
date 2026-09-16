using System.Net;
using CommunityBot.Application.Roles;
using NetCord.Rest;

namespace CommunityBot.Discord.Roles;

public sealed class DiscordRoleAdapter(
    DiscordRoleSettings settings,
    RestClient restClient)
    : IRoleAdapter
{
    public async Task<bool> UserHasRoleAsync(
        ulong memberId,
        ulong roleId,
        CancellationToken ct = default)
    {
        try
        {
            var member = await restClient.GetGuildUserAsync(
                settings.GuildId,
                memberId,
                cancellationToken: ct);

            return member.RoleIds.Contains(roleId);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }

    public async Task<RoleOperationResult?> AssignRoleAsync(
        ulong memberId,
        ulong roleId,
        CancellationToken ct = default)
    {
        try
        {
            await restClient.AddGuildUserRoleAsync(
                settings.GuildId,
                memberId,
                roleId,
                cancellationToken: ct);

            return null;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (RestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            return RoleOperationResult.Forbidden();
        }
        catch (RestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return RoleOperationResult.NotFound();
        }
        catch (Exception ex)
        {
            return RoleOperationResult.Failure(
                $"Unexpected Discord role error: {ex.Message}");
        }
    }
}