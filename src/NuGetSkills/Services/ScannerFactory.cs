namespace NuGetSkills.Services;

public static class ScannerFactory
{
    public static async Task<PackageScanner> CreateAsync()
    {
        var settings = NuGetSkillsSettings.Load();
        var cli = new DotnetCli();
        var cache = new NuGetCacheLocator(cli);
        var ghAvailable = settings.EnableRemoteScan && await ToolChecker.IsGhAvailableAsync();
        var github = new GitHubService();
        return new PackageScanner(cli, cache, github, ghAvailable);
    }
}
