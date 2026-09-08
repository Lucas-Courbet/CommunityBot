namespace CommunityBot.Application.Items;

public interface IItemAcquisitionService
{
    Task<ItemAcquisitionResult> AcquireAsync(
        ulong memberId,
        string itemId,
        int quantity = 1,
        CancellationToken ct = default);
}