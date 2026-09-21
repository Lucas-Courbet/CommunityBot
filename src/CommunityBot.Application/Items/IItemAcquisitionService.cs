namespace CommunityBot.Application.Items;

/// <summary>
/// Applies canonical item possession rules to member inventories.
/// </summary>
public interface IItemAcquisitionService
{
    /// <summary>
    /// Acquires units of a canonical item, stacking them onto an active inventory entry when allowed.
    /// </summary>
    /// <remarks>
    /// The caller owns the surrounding transaction and must hold the member row lock.
    /// Changes are tracked but are not persisted by this service.
    /// </remarks>
    Task<ItemAcquisitionResult> AcquireAsync(
        ulong memberId,
        string itemId,
        int quantity = 1,
        CancellationToken ct = default);
}