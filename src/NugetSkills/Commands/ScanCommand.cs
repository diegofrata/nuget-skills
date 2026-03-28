using System.CommandLine;
using System.Text.Json;
using NuGetSkills.Models;
using NuGetSkills.Services;

namespace NuGetSkills.Commands;

public static class ScanCommand
{
    public static Command Create()
    {
        var projectOption = new Option<string?>("--project", "-p") { Description = "Path to a solution or project file" };
        var jsonOption = new Option<bool>("--json") { Description = "Output results as JSON" };
        var refreshOption = new Option<bool>("--refresh") { Description = "Re-check all remote repos (ignore cache)" };

        var command = new Command("scan", "Scan NuGet packages for bundled skills (local + remote)");
        command.Add(projectOption);
        command.Add(jsonOption);
        command.Add(refreshOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var project = parseResult.GetValue(projectOption);
            var json = parseResult.GetValue(jsonOption);
            var refresh = parseResult.GetValue(refreshOption);
            await ExecuteAsync(project, json, refresh, cancellationToken);
        });

        return command;
    }

    private static async Task ExecuteAsync(string? project, bool json, bool refresh, CancellationToken cancellationToken)
    {
        var settings = NuGetSkillsSettings.Load();
        var cli = new DotnetCli();
        var cache = new NuGetCacheLocator(cli);
        var ghAvailable = settings.EnableRemoteScan && await ToolChecker.IsGhAvailableAsync();
        var github = new GitHubService();
        var scanner = new PackageScanner(cli, cache, github, ghAvailable);

        var targets = ProjectDiscovery.Discover(project);

        if (targets.Length == 0)
        {
            if (!json)
                Console.WriteLine("No solution or project files found. Use --project to specify a path.");
            return;
        }

        var allResults = new List<ScanResult>();

        foreach (var target in targets)
        {
            var result = await scanner.ScanAsync(target, refresh, cancellationToken);
            allResults.Add(result);
        }

        if (json)
            WriteJson(allResults);
        else
            WriteHuman(allResults, ghAvailable, settings);
    }

    private static void WriteJson(List<ScanResult> results)
    {
        Console.WriteLine(JsonSerializer.Serialize(results, Constants.JsonOptions));
    }

    private static void WriteHuman(List<ScanResult> results, bool ghAvailable, NuGetSkillsSettings settings)
    {
        foreach (var result in results)
        {
            Console.WriteLine($"Scanning {Path.GetFileName(result.Source)}...");

            if (result.Problems is { Length: > 0 })
            {
                foreach (var problem in result.Problems)
                    Console.Error.WriteLine($"  Warning: {problem}");
            }

            if (result.PackagesWithSkills.Length == 0)
            {
                Console.WriteLine($"  No packages with skills found (scanned {result.TotalPackages} packages).");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine($"  Found {result.PackagesWithSkills.Length} package(s) with skills (of {result.TotalPackages} total):");
                Console.WriteLine();

                var maxIdLen = result.PackagesWithSkills.Max(p => p.PackageId.Length);
                var maxVerLen = result.PackagesWithSkills.Max(p => p.Version.Length);

                foreach (var pkg in result.PackagesWithSkills)
                {
                    var source = pkg.Source switch
                    {
                        SkillSource.Local => "[local] ",
                        SkillSource.Remote => "[remote]",
                        SkillSource.Readme => "[readme]",
                        _ => "[??????]",
                    };
                    var desc = pkg.Description ?? "(no description)";
                    var suffix = pkg.Source == SkillSource.Remote && pkg.RemoteRepo is not null
                        ? $" ({pkg.RemoteRepo.Replace("https://", "")} @ {pkg.RemoteRef ?? "HEAD"})"
                        : "";
                    Console.WriteLine($"    {pkg.PackageId.PadRight(maxIdLen)}  {pkg.Version.PadRight(maxVerLen)}  {source}  {desc}{suffix}");
                }

                Console.WriteLine();
                Console.WriteLine("  Use 'nuget-skills load <package>' to view a skill.");
                Console.WriteLine();
            }

            if (result.SkippedNonGitHub > 0)
                Console.WriteLine($"  Skipped {result.SkippedNonGitHub} package(s): repo not on GitHub.");

            if (settings.EnableRemoteScan && !ghAvailable)
                Console.WriteLine("  Note: 'gh' CLI not found. Remote skill discovery skipped. Run 'nuget-skills doctor' for details.");
        }
    }
}
