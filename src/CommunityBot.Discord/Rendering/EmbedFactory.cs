using NetCord;
using NetCord.Rest;

namespace CommunityBot.Discord.Rendering;

public static class EmbedFactory
{
    public static readonly Color PrimaryColor = new(88, 101, 242);
    public static readonly Color SuccessColor = new(87, 242, 135);
    public static readonly Color WarningColor = new(254, 231, 92);
    public static readonly Color ErrorColor = new(237, 66, 69);

    public static EmbedProperties Create(
        string title,
        string? description = null,
        Color? color = null)
        => new()
        {
            Title = title,
            Description = description,
            Color = color ?? PrimaryColor
        };

    public static EmbedProperties Success(string message, string title = "Success")
        => Create(title, message, SuccessColor);

    public static EmbedProperties Warning(string message, string title = "Warning")
        => Create(title, message, WarningColor);

    public static EmbedProperties Error(string message, string title = "Error")
        => Create(title, message, ErrorColor);
}