using CommunityBot.Application.Common.Persistence;
using CommunityBot.Core.Members;

namespace CommunityBot.Application.Members;

/// <summary>
/// Defines persistence operations specific to members.
/// </summary>
public interface IMemberRepository : IBaseRepository<Member, ulong>
{
    /// <summary>
    /// Retrieves a member by its exact Discord username.
    /// </summary>
    Task<Member?> GetByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a member while acquiring a row lock for the current database transaction.
    /// </summary>
    Task<Member?> GetByIdForUpdateAsync(ulong memberId, CancellationToken ct = default);
}