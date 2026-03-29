using System.Text.RegularExpressions;

namespace NuGetSkills.Services;

public static partial class VersionMatcher
{
    [GeneratedRegex(@"^(\d+)\.(\d+)\.(\d+)(?:-([a-zA-Z0-9.]+))?(?:\+.+)?$")]
    private static partial Regex SemverRegex();

    /// <summary>
    /// Finds the best matching tag for a given package version.
    /// Returns the original tag name (not stripped) or null if no match.
    /// </summary>
    public static string? FindBestTag(IEnumerable<string> tags, string packageId, string targetVersion)
    {
        string? bestTag = null;
        SemVer? bestVersion = null;
        var target = SemVer.Parse(targetVersion);
        var prefixes = BuildPrefixes(packageId);

        foreach (var tag in tags)
        {
            var stripped = StripTagPrefix(tag, prefixes);

            // Exact string match (handles prerelease: "4.0.0-beta1" == "4.0.0-beta1")
            if (string.Equals(stripped, targetVersion, StringComparison.OrdinalIgnoreCase))
                return tag;

            // Fall back to base version comparison for closest lower
            if (target is null)
                continue;

            var parsed = SemVer.Parse(stripped);
            if (parsed is null)
                continue;

            if (parsed <= target && (bestVersion is null || parsed > bestVersion))
            {
                bestVersion = parsed;
                bestTag = tag;
            }
        }

        return bestTag;
    }

    private static string[] BuildPrefixes(string packageId) =>
    [
        $"{packageId}.",
        $"{packageId}/",
        $"{packageId}-",
        "release/",
        "releases/",
        "v",
    ];

    private static string StripTagPrefix(string tag, string[] prefixes)
    {
        foreach (var prefix in prefixes)
        {
            if (tag.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return tag[prefix.Length..];
        }

        return tag;
    }

    private record SemVer(int Major, int Minor, int Patch, string? PreRelease) : IComparable<SemVer>
    {
        public static SemVer? Parse(string input)
        {
            var match = SemverRegex().Match(input);
            if (!match.Success)
                return null;

            return new SemVer(
                int.Parse(match.Groups[1].Value),
                int.Parse(match.Groups[2].Value),
                int.Parse(match.Groups[3].Value),
                match.Groups[4].Success ? match.Groups[4].Value : null);
        }

        public int CompareTo(SemVer? other)
        {
            if (other is null) return 1;

            var cmp = Major.CompareTo(other.Major);
            if (cmp != 0) return cmp;

            cmp = Minor.CompareTo(other.Minor);
            if (cmp != 0) return cmp;

            cmp = Patch.CompareTo(other.Patch);
            if (cmp != 0) return cmp;

            // No pre-release > has pre-release (1.0.0 > 1.0.0-beta)
            if (PreRelease is null && other.PreRelease is null) return 0;
            if (PreRelease is null) return 1;
            if (other.PreRelease is null) return -1;

            return string.Compare(PreRelease, other.PreRelease, StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator <(SemVer left, SemVer right) => left.CompareTo(right) < 0;
        public static bool operator >(SemVer left, SemVer right) => left.CompareTo(right) > 0;
        public static bool operator <=(SemVer left, SemVer right) => left.CompareTo(right) <= 0;
        public static bool operator >=(SemVer left, SemVer right) => left.CompareTo(right) >= 0;
    }
}
