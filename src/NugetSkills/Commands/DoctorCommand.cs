using System.CommandLine;
using NugetSkills.Services;

namespace NugetSkills.Commands;

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

        var dotnet = await ToolChecker.CheckDotnetAsync();
        PrintCheck("dotnet CLI", dotnet.Available, dotnet.Version, null);

        var gh = await ToolChecker.CheckGhAsync();
        PrintCheck("gh CLI", gh.Available, gh.Version, gh.Available
            ? gh.Details
            : "Remote skill discovery will not work.\n                      Install from https://cli.github.com");

        var nugetCache = await ToolChecker.CheckNuGetCacheAsync();
        PrintCheck("NuGet cache", nugetCache.Available, null, nugetCache.Details);

        var skillsCache = ToolChecker.CheckSkillsCache();
        PrintCheck("Skills cache", skillsCache.Available, null, skillsCache.Details);

        var settings = NugetSkillsSettings.Load();
        Console.WriteLine();
        Console.WriteLine("  Settings:");
        Console.WriteLine($"    Remote scan:     {(settings.EnableRemoteScan ? "enabled" : "disabled")}");
        Console.WriteLine($"    README fallback: {(settings.EnableReadmeFallback ? "enabled" : "disabled")}");
        Console.WriteLine($"    Config:          {NugetSkillsSettings.FilePath}");

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
