namespace CommunityBot.Discord.Pagination;

/// <summary>
/// Builds and parses stateless pagination source identifiers.
/// </summary>
/// <remarks>
/// A source carries the feature-specific context required for a pagination strategy
/// to rebuild a page without storing server-side pagination state.
/// </remarks>
public static class PaginationSource
{
    private const char Separator = '_';

    /// <summary>
    /// Builds a source using the standard <c>key_value</c> format.
    /// </summary>
    public static string Build<TValue>(string key, TValue value)
        => $"{key}{Separator}{value}";

    /// <summary>
    /// Reads an enum payload from a <c>key_value</c> source.
    /// </summary>
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