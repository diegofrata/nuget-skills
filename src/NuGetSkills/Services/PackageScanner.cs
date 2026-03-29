using NuGetSkills.Models;

namespace NuGetSkills.Services;

public class PackageScanner
{
    private readonly IDotnetCli _cli;
    private readonly NuGetCacheLocator _cache;
    private readonly GitHubService _github;
    private readonly bool _ghAvailable;
    private readonly NuGetSkillsSettings _settings;

    public PackageScanner(IDotnetCli cli, NuGetCacheLocator cache, GitHubService github, bool ghAvailable)
    {
        _cli = cli;
        _cache = cache;
        _github = github;
        _ghAvailable = ghAvailable;
        _settings = NuGetSkillsSettings.Load();
    }

    public async Task<ScanResult> ScanAsync(
        string projectOrSolution, bool refresh = false, CancellationToken cancellationToken = default)
    {
        var (packages, source, problems) = await _cli.ListPackagesAsync(projectOrSolution, cancellationToken);

        var skillInfos = new List<PackageSkillInfo>();
        var packagesWithoutLocalSkills = new List<PackageInfo>();

        foreach (var package in packages)
        {
            var skillsDir = await _cache.FindSkillsDirectoryAsync(
                package.Id, package.ResolvedVersion, cancellationToken);

            if (skillsDir is not null)
            {
                var skillFiles = SkillReader.FindSkillFiles(skillsDir);
                if (skillFiles.Length > 0)
                {
                    var frontmatter = SkillReader.ReadFrontmatter(skillFiles[0]);
                    if (PackageGlobMatcher.IsMatch(package.Id, frontmatter.Packages))
                    {
                        skillInfos.Add(new PackageSkillInfo(
                            PackageId: package.Id,
                            Version: package.ResolvedVersion,
                            SkillFiles: skillFiles.Select(Path.GetFileName).ToArray()!,
                            Name: frontmatter.Name,
                            Description: frontmatter.Description,
                            Source: SkillSource.Local));
                        continue;
                    }
                    // Skill exists locally but its packages field excludes this package —
                    // fall through to remote scan in case a different skill matches.
                }
            }

            packagesWithoutLocalSkills.Add(package);
        }

        var skippedNonGitHub = 0;
        var remoteResults = await Task.WhenAll(
            packagesWithoutLocalSkills.Select(p => CheckRemoteAsync(p, refresh, cancellationToken)));

        foreach (var result in remoteResults)
        {
            if (result is null)
                continue;
            if (result.Value.Skipped)
                skippedNonGitHub++;
            if (result.Value.Info is not null)
                skillInfos.Add(result.Value.Info);
        }

        return new ScanResult(
            Source: source,
            TotalPackages: packages.Count,
            PackagesWithSkills: skillInfos.ToArray(),
            Problems: problems,
            SkippedNonGitHub: skippedNonGitHub);
    }

    private async Task<(PackageSkillInfo? Info, bool Skipped)?> CheckRemoteAsync(
        PackageInfo package, bool refresh, CancellationToken cancellationToken)
    {
        var packageDir = await _cache.GetPackageDirectoryAsync(
            package.Id, package.ResolvedVersion, cancellationToken);
        var nuspec = NuspecParser.Parse(packageDir, package.Id);

        // Check cache first (unless refresh)
        if (!refresh)
        {
            var cached = SkillCache.Read(package.Id, package.ResolvedVersion);
            if (cached is not null)
            {
                // Positive results (skill found) are cached indefinitely
                if (cached.HasRemoteSkill && _settings.EnableRemoteScan)
                {
                    return (new PackageSkillInfo(
                        PackageId: cached.PackageId,
                        Version: cached.Version,
                        SkillFiles: [cached.SkillPath ?? Constants.SkillFileName],
                        Name: null,
                        Description: cached.Description,
                        Source: SkillSource.Remote,
                        RemoteRepo: cached.RepoUrl,
                        RemoteRef: cached.RemoteRef), false);
                }

                // Negative results expire after 7 days so newly added skills get picked up
                var isStale = DateTime.UtcNow - cached.CheckedAt > TimeSpan.FromDays(7);

                if (!isStale)
                {
                    return null;
                }
            }
        }

        // Remote scan via gh
        var hasRemoteSkill = false;
        string? repoUrl = null;
        string? remoteRef = null;
        string? skillPath = null;
        string? description = null;
        var skipped = false;

        if (_settings.EnableRemoteScan && _ghAvailable)
        {
            var parsed = GitHubService.ParseGitHubUrl(nuspec?.RepositoryUrl);

            if (parsed is null)
            {
                repoUrl = nuspec?.RepositoryUrl;
                skipped = true;
            }
            else
            {
                var (owner, repo) = parsed.Value;
                repoUrl = $"https://github.com/{owner}/{repo}";

                var tags = await _github.ListTagsAsync(owner, repo, cancellationToken);
                var bestTag = VersionMatcher.FindBestTag(tags, package.Id, package.ResolvedVersion);

                // Try matched tag first, then HEAD — FindSkillAsync returns content to avoid re-fetching
                SkillSearchResult? found = null;

                if (bestTag is not null)
                {
                    found = await _github.FindSkillAsync(owner, repo, bestTag, cancellationToken);
                    if (found is not null)
                        remoteRef = bestTag;
                }

                if (found is null)
                {
                    found = await _github.FindSkillAsync(owner, repo, Constants.DefaultRef, cancellationToken);
                    if (found is not null)
                        remoteRef = Constants.DefaultRef;
                }

                if (found is not null)
                {
                    var frontmatter = FrontmatterParser.Parse(found.Content);
                    if (PackageGlobMatcher.IsMatch(package.Id, frontmatter.GetField("packages")))
                    {
                        skillPath = found.Path;
                        description = frontmatter.GetField("description");
                        hasRemoteSkill = true;
                    }
                }
            }
        }

        SkillCache.Write(new CachedSkillInfo(
            PackageId: package.Id,
            Version: package.ResolvedVersion,
            HasRemoteSkill: hasRemoteSkill,
            RepoUrl: repoUrl,
            RemoteRef: remoteRef,
            SkillPath: skillPath,
            Description: description,
            CheckedAt: DateTime.UtcNow));

        if (hasRemoteSkill)
        {
            return (new PackageSkillInfo(
                PackageId: package.Id,
                Version: package.ResolvedVersion,
                SkillFiles: [Path.GetFileName(skillPath!)],
                Name: null,
                Description: description,
                Source: SkillSource.Remote,
                RemoteRepo: repoUrl,
                RemoteRef: remoteRef), skipped);
        }

        return skipped ? (null, true) : null;
    }
}
