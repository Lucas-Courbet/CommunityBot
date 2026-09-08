using NetCord.Rest;

namespace CommunityBot.Discord.Models;

/// <summary>
/// Describes a Discord interaction response independently of the command handling code.
/// </summary>
public sealed class ResponseRequest
{
    public string? Content { get; init; }

    public EmbedProperties? Embed { get; init; }

    public IEnumerable<IMessageComponentProperties>? Components { get; init; }

    public bool IsEphemeral { get; init; }

    public static ResponseRequest FromContent(string content, bool isEphemeral = false)
        => new()
        {
            Content = content,
            IsEphemeral = isEphemeral
        };

    public static ResponseRequest FromEmbed(EmbedProperties embed, bool isEphemeral = false)
        => new()
        {
            Embed = embed,
            IsEphemeral = isEphemeral
        };
}