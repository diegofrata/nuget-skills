using System.CommandLine;
using NuGetSkills.Services;

namespace NuGetSkills.Commands;

public static class DoctorCommand
{
    public static Command Create()
    {
        var command = new Command("doctor", "Validate your nuget-skills setup");

        command.SetAction(async (_, cancellationToken) =>
        {
            await ExecuteAsync(cancellationToken);
        });

        return command;
    }

    private static async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine();

        var dotnetTask = ToolChecker.CheckDotnetAsync();
        var ghTask = ToolChecker.CheckGhAsync();
        var nugetCacheTask = ToolChecker.CheckNuGetCacheAsync();
        await Task.WhenAll(dotnetTask, ghTask, nugetCacheTask);

        var dotnet = dotnetTask.Result;
        PrintCheck("dotnet CLI", dotnet.Available, dotnet.Version, null);

        var gh = ghTask.Result;
        PrintCheck("gh CLI", gh.Available, gh.Version, gh.Available
            ? gh.Details
            : "Remote skill discovery will not work.\n                      Install from https://cli.github.com");

        var nugetCache = nugetCacheTask.Result;
        PrintCheck("NuGet cache", nugetCache.Available, null, nugetCache.Details);

        var skillsCache = ToolChecker.CheckSkillsCache();
        PrintCheck("Skills cache", skillsCache.Available, null, skillsCache.Details);

        var settings = NuGetSkillsSettings.Load();
        Console.WriteLine();
        Console.WriteLine("  Settings:");
        Console.WriteLine($"    Remote scan:     {(settings.EnableRemoteScan ? "enabled" : "disabled")}");
        Console.WriteLine($"    Config:          {NuGetSkillsSettings.FilePath}");

        var projectConfigPath = ProjectConfigService.FindConfigPath();
        Console.WriteLine();
        Console.WriteLine("  Project config:");
        if (projectConfigPath is not null)
        {
            var projectConfig = ProjectConfigService.Load(projectConfigPath);
            if (projectConfig is { Packages.Length: > 0 })
                Console.WriteLine($"    Packages:        {projectConfig.Packages.Length} configured ({string.Join(", ", projectConfig.Packages)})");
            else
                Console.WriteLine("    Packages:        none configured (no skills will be loaded)");
            Console.WriteLine($"    Config file:     {projectConfigPath}");
        }
        else
        {
            Console.WriteLine("    Not configured   (all discovered skills will be loaded)");
        }

        Console.WriteLine();
    }

    private static void PrintCheck(string name, bool ok, string? version, string? details)
    {
        var symbol = ok ? "\u2713" : "\u2717";
        var label = name.PadRight(16);

        if (version is not null)
            Console.WriteLine($"  {symbol}  {label} {version}");
        else
            Console.WriteLine($"  {symbol}  {label}");

        if (details is not null)
        {
            foreach (var line in details.Split('\n'))
                Console.WriteLine($"                      {line.Trim()}");
        }
    }
}
