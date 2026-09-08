namespace CommunityBot.Application.Items;

public sealed record InventoryRequest(
    ulong MemberId,
    int PageIndex,
    int PageSize);