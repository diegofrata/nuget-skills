using System.Text.Json;
using NuGetSkills.Models;

namespace NuGetSkills.Services;

public static class SkillCache
{
    public static readonly string CacheDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "nuget-skills", "cache");

    public static CachedSkillInfo? Read(string packageId, string version)
    {
        var path = GetCachePath(packageId, version);
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<CachedSkillInfo>(json, Constants.JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public static void Write(CachedSkillInfo info)
    {
        var path = GetCachePath(info.PackageId, info.Version);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(info, Constants.JsonOptions));
    }

    public static int CountEntries()
    {
        if (!Directory.Exists(CacheDirectory))
            return 0;

        return Directory.EnumerateFiles(CacheDirectory, "*.json", SearchOption.AllDirectories).Count();
    }

    private static string GetCachePath(string packageId, string version)
    {
        return Path.Combine(CacheDirectory, packageId.ToLowerInvariant(), $"{version}.json");
    }
}
