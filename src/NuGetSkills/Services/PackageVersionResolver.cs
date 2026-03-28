namespace NuGetSkills.Services;

public static class PackageVersionResolver
{
    public static async Task<(string Version, IDotnetCli Cli, NuGetCacheLocator Cache)?> ResolveAsync(
        string packageId, string? version, string? project, CancellationToken cancellationToken)
    {
        var cli = new DotnetCli();
        var cache = new NuGetCacheLocator(cli);

        if (version is not null)
            return (version, cli, cache);

        var targets = ProjectDiscovery.Discover(project);
        foreach (var target in targets)
        {
            version = await cli.ResolvePackageVersionAsync(packageId, target, cancellationToken);
            if (version is not null)
                return (version, cli, cache);
        }

        Console.Error.WriteLine($"Package '{packageId}' not found in project references.");
        Environment.Exit(1);
        return null; // unreachable
    }
}
