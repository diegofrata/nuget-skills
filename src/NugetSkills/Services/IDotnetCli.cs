using NugetSkills.Models;

namespace NugetSkills.Services;

public interface IDotnetCli
{
    Task<(List<PackageInfo> Packages, string Source, string[]? Problems)> ListPackagesAsync(
        string projectOrSolution, CancellationToken cancellationToken = default);

    Task<string> GetGlobalPackagesPathAsync(CancellationToken cancellationToken = default);

    Task<string?> ResolvePackageVersionAsync(
        string packageId, string projectOrSolution, CancellationToken cancellationToken = default);
}
