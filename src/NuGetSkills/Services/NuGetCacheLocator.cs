namespace NuGetSkills.Services;

public class NuGetCacheLocator
{
    private readonly IDotnetCli _cli;
    private string? _cachedPath;

    public NuGetCacheLocator(IDotnetCli cli)
    {
        _cli = cli;
    }

    public async Task<string> GetGlobalPackagesPathAsync(CancellationToken cancellationToken = default)
    {
        return _cachedPath ??= await _cli.GetGlobalPackagesPathAsync(cancellationToken);
    }

    public async Task<string> GetPackageDirectoryAsync(
        string packageId, string version, CancellationToken cancellationToken = default)
    {
        var globalPath = await GetGlobalPackagesPathAsync(cancellationToken);
        return Path.Combine(globalPath, packageId.ToLowerInvariant(), version);
    }

    public async Task<string?> FindSkillsDirectoryAsync(
        string packageId, string version, CancellationToken cancellationToken = default)
    {
        var packageDir = await GetPackageDirectoryAsync(packageId, version, cancellationToken);

        foreach (var dirName in Constants.SkillDirectoryNames)
        {
            var skillsDir = Path.Combine(packageDir, dirName);
            if (Directory.Exists(skillsDir))
                return skillsDir;
        }

        return null;
    }
}
