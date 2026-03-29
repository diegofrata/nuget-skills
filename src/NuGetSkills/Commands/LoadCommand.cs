using System.CommandLine;
using NuGetSkills.Services;

namespace NuGetSkills.Commands;

public static class LoadCommand
{
    public static Command Create()
    {
        var packageArgument = new Argument<string>("package") { Description = "The NuGet package ID to load the skill for" };
        var versionOption = new Option<string?>("--version", "-v") { Description = "Package version (auto-detected from project if not specified)" };
        var projectOption = new Option<string?>("--project", "-p") { Description = "Path to a solution or project file for version resolution" };

        var command = new Command("load", "Output the full skill content for a NuGet package");
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

        // Try local first
        var skillsDir = await cache.FindSkillsDirectoryAsync(package, ver, cancellationToken);
        if (skillsDir is not null)
        {
            var skillFiles = SkillReader.FindSkillFiles(skillsDir);
            if (skillFiles.Length > 0)
            {
                await OutputSkillFiles(skillFiles, cancellationToken);
                return;
            }
        }

        // Fall back to remote (from cache)
        var cached = SkillCache.Read(package, ver);
        if (cached is { HasRemoteSkill: true, RepoUrl: not null, SkillPath: not null, RemoteRef: not null })
        {
            var parsed = GitHubService.ParseGitHubUrl(cached.RepoUrl);
            if (parsed is not null)
            {
                var (owner, repo) = parsed.Value;
                var github = new GitHubService();
                var content = await github.FetchFileContentAsync(owner, repo, cached.SkillPath, cached.RemoteRef, cancellationToken);

                if (content is not null)
                {
                    Console.Write(content);
                    return;
                }
            }
        }

        Console.Error.WriteLine($"No skills found for {package} {ver} (local or remote).");
        Environment.Exit(1);
    }

    private static async Task OutputSkillFiles(string[] skillFiles, CancellationToken cancellationToken)
    {
        for (var i = 0; i < skillFiles.Length; i++)
        {
            if (i > 0)
                Console.WriteLine("\n---\n");

            var content = await SkillReader.ReadFullAsync(skillFiles[i], cancellationToken);
            Console.Write(content);
        }
    }
}
