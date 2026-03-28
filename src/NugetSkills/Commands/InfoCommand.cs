using System.CommandLine;
using System.Text.Json;
using NugetSkills.Services;

namespace NugetSkills.Commands;

public static class InfoCommand
{
    public static Command Create()
    {
        var packageArgument = new Argument<string>("package") { Description = "The NuGet package ID to get info for" };
        var versionOption = new Option<string?>("--version", "-v") { Description = "Package version (auto-detected from project if not specified)" };
        var projectOption = new Option<string?>("--project", "-p") { Description = "Path to a solution or project file for version resolution" };

        var command = new Command("info", "Output package metadata (repository URL, description, skill status)");
        command.Add(packageArgument);
        command.Add(versionOption);
        command.Add(projectOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var package = parseResult.GetValue(packageArgument)!;
            var version = parseResult.GetValue(versionOption);
            var project = parseResult.GetValue(projectOption);
            await ExecuteAsync(package, version, project, cancellationToken);
        });

        return command;
    }

    private static async Task ExecuteAsync(string package, string? version, string? project, CancellationToken cancellationToken)
    {
        var resolved = await PackageVersionResolver.ResolveAsync(package, version, project, cancellationToken);
        if (resolved is null) return;

        var (ver, _, cache) = resolved.Value;

        var packageDir = await cache.GetPackageDirectoryAsync(package, ver, cancellationToken);
        var nuspec = NuspecParser.Parse(packageDir, package);
        var skillsDir = await cache.FindSkillsDirectoryAsync(package, ver, cancellationToken);
        var hasSkills = skillsDir is not null && SkillReader.FindSkillFiles(skillsDir).Length > 0;

        var output = new
        {
            id = package,
            version = ver,
            description = nuspec?.Description,
            repositoryUrl = nuspec?.RepositoryUrl,
            repositoryType = nuspec?.RepositoryType,
            projectUrl = nuspec?.ProjectUrl,
            license = nuspec?.License,
            hasSkills,
            cachePath = packageDir,
        };

        Console.WriteLine(JsonSerializer.Serialize(output, Constants.JsonOptions));
    }
}
