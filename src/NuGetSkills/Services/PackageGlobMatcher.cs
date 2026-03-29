namespace NuGetSkills.Services;

public static class PackageGlobMatcher
{
    /// <returns>true if packageId matches any pattern in the comma-separated packagesField, or if the field is null/empty.</returns>
    public static bool IsMatch(string packageId, string? packagesField)
    {
        if (string.IsNullOrWhiteSpace(packagesField))
            return true;

        var patterns = packagesField.Split(',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var pattern in patterns)
        {
            if (MatchesPattern(packageId, pattern))
                return true;
        }

        return false;
    }

    internal static bool MatchesPattern(string packageId, string pattern)
    {
        // No wildcard — exact match
        if (!pattern.Contains('*'))
            return packageId.Equals(pattern, StringComparison.OrdinalIgnoreCase);

        // Single trailing wildcard (most common: "Contoso.Http*")
        if (pattern.EndsWith('*') && pattern.IndexOf('*') == pattern.Length - 1)
        {
            var prefix = pattern[..^1];
            return packageId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        // General case: split on * segments and match in order
        var segments = pattern.Split('*');
        var pos = 0;

        for (var i = 0; i < segments.Length; i++)
        {
            if (segments[i].Length == 0)
                continue;

            var idx = packageId.IndexOf(segments[i], pos, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                return false;

            // First segment must match at start
            if (i == 0 && idx != 0)
                return false;

            pos = idx + segments[i].Length;
        }

        // Last segment must match at end (unless pattern ends with *)
        if (segments[^1].Length > 0 && !packageId.EndsWith(segments[^1], StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }
}
