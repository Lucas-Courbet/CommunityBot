namespace CommunityBot.Discord.Pagination;

public static class PaginationSource
{
    private const char Separator = '_';

    public static string Build<TValue>(string key, TValue value)
        => $"{key}{Separator}{value}";

    public static bool TryReadEnum<TEnum>(string source, string key, out TEnum value)
        where TEnum : struct, Enum
    {
        value = default;

        var prefix = $"{key}{Separator}";

        if (!source.StartsWith(prefix, StringComparison.Ordinal))
            return false;

        return Enum.TryParse(source[prefix.Length..], out value);
    }
}