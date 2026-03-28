using System.Text.Json;
using NuGetSkills.Models;

namespace NuGetSkills.Services;

public class DotnetCli : IDotnetCli
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

    public async Task<(List<PackageInfo> Packages, string Source, string[]? Problems)> ListPackagesAsync(
        string projectOrSolution, CancellationToken cancellationToken = default)
    {
        var result = await ProcessRunner.RunAsync(
            "dotnet",
            $"list \"{projectOrSolution}\" package --format json --output-version 1",
            DefaultTimeout,
            cancellationToken);

        if (!result.Success)
            throw new InvalidOperationException(
                $"dotnet list package failed (exit code {result.ExitCode}): {result.Stderr}");

        var packages = new List<PackageInfo>();
        var problems = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var doc = JsonDocument.Parse(result.Stdout);
        var root = doc.RootElement;

        if (root.TryGetProperty("problems", out var problemsArray))
        {
            foreach (var problem in problemsArray.EnumerateArray())
            {
                var text = problem.GetProperty("text").GetString();
                if (text is not null)
                    problems.Add(text);
            }
        }

        if (root.TryGetProperty("projects", out var projects))
        {
            foreach (var project in projects.EnumerateArray())
            {
                if (!project.TryGetProperty("frameworks", out var frameworks))
                    continue;

                foreach (var framework in frameworks.EnumerateArray())
                {
                    if (!framework.TryGetProperty("topLevelPackages", out var topLevel))
                        continue;

                    foreach (var pkg in topLevel.EnumerateArray())
                    {
                        var id = pkg.GetProperty("id").GetString()!;
                        var resolvedVersion = pkg.GetProperty("resolvedVersion").GetString()!;
                        var key = $"{id}/{resolvedVersion}";

                        if (seen.Add(key))
                            packages.Add(new PackageInfo(id, resolvedVersion));
                    }
                }
            }
        }

        return (packages, projectOrSolution, problems.Count > 0 ? problems.ToArray() : null);
    }

    public async Task<string> GetGlobalPackagesPathAsync(CancellationToken cancellationToken = default)
    {
        var result = await ProcessRunner.RunAsync(
            "dotnet", "nuget locals global-packages -l", DefaultTimeout, cancellationToken);

        if (!result.Success)
            throw new InvalidOperationException(
                $"dotnet nuget locals failed (exit code {result.ExitCode}): {result.Stderr}");

        return ParseGlobalPackagesPath(result.Stdout);
    }

    public async Task<string?> ResolvePackageVersionAsync(
        string packageId, string projectOrSolution, CancellationToken cancellationToken = default)
    {
        var (packages, _, _) = await ListPackagesAsync(projectOrSolution, cancellationToken);
        return packages
            .FirstOrDefault(p => string.Equals(p.Id, packageId, StringComparison.OrdinalIgnoreCase))
            ?.ResolvedVersion;
    }

    internal static string ParseGlobalPackagesPath(string output)
    {
        var prefix = "global-packages:";
        var line = output.Trim();
        if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return line[prefix.Length..].Trim();

        throw new InvalidOperationException($"Unexpected output from 'dotnet nuget locals': {line}");
    }
}
