using System.Text.Json;
using NuGetSkills.Models;

namespace NuGetSkills.Services;

public static class ProjectConfigService
{
    public static string? FindConfigPath(string? projectPath = null)
    {
        var configPath = Path.Combine(GetConfigDirectory(projectPath), Constants.ProjectConfigFileName);
        return File.Exists(configPath) ? configPath : null;
    }

    public static string GetConfigDirectory(string? projectPath = null)
    {
        return projectPath is not null
            ? Path.GetDirectoryName(Path.GetFullPath(projectPath))!
            : Directory.GetCurrentDirectory();
    }

    public static ProjectConfig? Load(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ProjectConfig>(json, Constants.JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public static void Save(string path, ProjectConfig config)
    {
        var json = JsonSerializer.Serialize(config, Constants.JsonOptions);
        File.WriteAllText(path, json);
    }

    /// <returns>The set of configured package IDs, or null if no config file exists (meaning "show everything").</returns>
    public static HashSet<string>? ResolveConfiguredPackages(string? projectPath = null)
    {
        var configPath = FindConfigPath(projectPath);
        if (configPath is null)
            return null;

        var config = Load(configPath);
        if (config is null)
            return null;

        return new HashSet<string>(config.Packages, StringComparer.OrdinalIgnoreCase);
    }
}
