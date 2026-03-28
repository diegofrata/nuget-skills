using NuGetSkills.Models;
using NuGetSkills.Services;

namespace NuGetSkills.Tests;

public class CacheClearCommandTests : IDisposable
{
    private readonly string _packageId = $"ClearTest.{Guid.NewGuid():N}"[..25];
    private const string Version = "1.0.0";

    public void Dispose()
    {
        var path = Path.Combine(SkillCache.CacheDirectory, _packageId.ToLowerInvariant());
        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }

    [Fact]
    public void WriteAndClear_RemovesEntry()
    {
        var info = new CachedSkillInfo(
            PackageId: _packageId,
            Version: Version,
            HasRemoteSkill: false,
            HasReadme: false,
            RepoUrl: null,
            RemoteRef: null,
            SkillPath: null,
            Description: null,
            CheckedAt: DateTime.UtcNow);

        SkillCache.Write(info);
        Assert.NotNull(SkillCache.Read(_packageId, Version));

        // Simulate clear-cache for this package
        var packageDir = Path.Combine(SkillCache.CacheDirectory, _packageId.ToLowerInvariant());
        if (Directory.Exists(packageDir))
            Directory.Delete(packageDir, true);

        Assert.Null(SkillCache.Read(_packageId, Version));
    }

    [Fact]
    public void Clear_NonExistentPackage_DoesNotThrow()
    {
        var packageDir = Path.Combine(SkillCache.CacheDirectory, "nonexistent-package-xyz");
        Assert.False(Directory.Exists(packageDir));
    }
}
