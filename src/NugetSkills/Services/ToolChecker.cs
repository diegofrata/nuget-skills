namespace NugetSkills.Services;

public record ToolCheckResult(bool Available, string? Version, string? Details);

public static class ToolChecker
{
    private static bool? _ghAvailableCache;

    public static async Task<ToolCheckResult> CheckDotnetAsync()
    {
        var result = await ProcessRunner.RunAsync("dotnet", "--version");
        return result.Success
            ? new ToolCheckResult(true, result.Stdout.Trim().Split('\n')[0].Trim(), null)
            : new ToolCheckResult(false, null, null);
    }

    public static async Task<ToolCheckResult> CheckGhAsync()
    {
        var result = await ProcessRunner.RunAsync("gh", "--version");
        if (!result.Success)
        {
            _ghAvailableCache = false;
            return new ToolCheckResult(false, null, null);
        }

        _ghAvailableCache = true;

        var auth = await ProcessRunner.RunAsync("gh", "auth status");
        if (!auth.Success)
            return new ToolCheckResult(true, result.Stdout.Trim().Split('\n')[0].Trim(), "installed but not authenticated");

        var username = auth.Stdout
            .Split('\n')
            .Select(l => l.Trim())
            .FirstOrDefault(l => l.Contains("Logged in to"))
            ?.Split("as ")
            .LastOrDefault()
            ?.Trim()
            .TrimEnd(')');

        return new ToolCheckResult(true, result.Stdout.Trim().Split('\n')[0].Trim(),
            username is not null ? $"authenticated as {username}" : "authenticated");
    }

    public static async Task<ToolCheckResult> CheckNuGetCacheAsync()
    {
        try
        {
            var cli = new DotnetCli();
            var path = await cli.GetGlobalPackagesPathAsync();
            return new ToolCheckResult(Directory.Exists(path), null, path);
        }
        catch
        {
            return new ToolCheckResult(false, null, "dotnet not available");
        }
    }

    public static ToolCheckResult CheckSkillsCache()
    {
        var cacheDir = SkillCache.CacheDirectory;
        if (!Directory.Exists(cacheDir))
            return new ToolCheckResult(true, null, $"{cacheDir} (empty)");

        var count = SkillCache.CountEntries();
        return new ToolCheckResult(true, null, $"{cacheDir} ({count} entries)");
    }

    public static async Task<bool> IsGhAvailableAsync()
    {
        if (_ghAvailableCache.HasValue)
            return _ghAvailableCache.Value;

        var result = await ProcessRunner.RunAsync("gh", "--version");
        _ghAvailableCache = result.Success;
        return result.Success;
    }
}
