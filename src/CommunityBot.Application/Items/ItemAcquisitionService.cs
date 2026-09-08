using CommunityBot.Core.Items;

namespace CommunityBot.Application.Items;

public sealed class ItemAcquisitionService(
    IItemRepository itemRepository,
    IInventoryRepository inventoryRepository)
    : IItemAcquisitionService
{
    public async Task<ItemAcquisitionResult> AcquireAsync(
        ulong memberId,
        string itemId,
        int quantity = 1,
        CancellationToken ct = default)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Item acquisition quantity must be positive.");

        var item = await itemRepository.GetByIdAsync(itemId, ct)
                   ?? throw new InvalidOperationException(
                       $"Canonical item '{itemId}' does not exist.");

        if (!item.IsStackable && quantity != 1)
            throw new InvalidOperationException(
                $"Non-stackable item '{itemId}' can only be acquired with quantity 1.");

        var existing = await inventoryRepository.GetActiveItemAsync(memberId, itemId, ct);

        if (existing is not null)
        {
            if (!item.IsStackable)
                return new ItemAcquisitionResult(ItemAcquisitionStatus.AlreadyOwned, existing);

            existing.Quantity += quantity;
            return new ItemAcquisitionResult(ItemAcquisitionStatus.Acquired, existing);
        }

        var inventoryItem = InventoryItem.CreateActive(memberId, itemId, quantity);

        inventoryRepository.Add(inventoryItem);

        return new ItemAcquisitionResult(ItemAcquisitionStatus.Acquired, inventoryItem);
    }
}