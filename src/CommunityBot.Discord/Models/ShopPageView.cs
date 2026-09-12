using NetCord.Rest;

namespace CommunityBot.Discord.Models;

public sealed class ShopPageView
{
    public required EmbedProperties Embed { get; init; }

    public IReadOnlyList<IMessageComponentProperties> Components { get; init; } = [];
}