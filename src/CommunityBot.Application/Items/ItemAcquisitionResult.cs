using CommunityBot.Core.Items;

namespace CommunityBot.Application.Items;

public sealed record ItemAcquisitionResult(
    ItemAcquisitionStatus Status,
    InventoryItem InventoryItem);