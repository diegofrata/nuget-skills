using NuGetSkills.Models;
using NuGetSkills.Services;

namespace NuGetSkills.Tests;

public class CacheTtlTests : IDisposable
{
    private readonly string _packageId = $"TtlTestPkg.{Guid.NewGuid():N}"[..30];
    private const string Version = "1.0.0";

    public void Dispose()
    {
        var path = Path.Combine(SkillCache.CacheDirectory, _packageId.ToLowerInvariant());
        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }

    [Fact]
    public void FreshNegativeCache_IsRead()
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
        var read = SkillCache.Read(_packageId, Version);

        Assert.NotNull(read);
        Assert.False(read.HasRemoteSkill);
    }

    [Fact]
    public void StaleNegativeCache_CheckedAtIsOld()
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
            CheckedAt: DateTime.UtcNow.AddDays(-8));

        SkillCache.Write(info);
        var read = SkillCache.Read(_packageId, Version);

        Assert.NotNull(read);
        var isStale = DateTime.UtcNow - read.CheckedAt > TimeSpan.FromDays(7);
        Assert.True(isStale);
    }

    [Fact]
    public void PositiveCache_NeverStale()
    {
        var info = new CachedSkillInfo(
            PackageId: _packageId,
            Version: Version,
            HasRemoteSkill: true,
            HasReadme: false,
            RepoUrl: "https://github.com/test/repo",
            RemoteRef: "v1.0.0",
            SkillPath: "skills/SKILL.md",
            Description: "test",
            CheckedAt: DateTime.UtcNow.AddDays(-30));

        SkillCache.Write(info);
        var read = SkillCache.Read(_packageId, Version);

        Assert.NotNull(read);
        Assert.True(read.HasRemoteSkill);
        // Positive results should be used regardless of age
    }

    [Fact]
    public void NegativeCache_WithinTtl_NotStale()
    {
        var info = new CachedSkillInfo(
            PackageId: _packageId,
            Version: Version,
            HasRemoteSkill: false,
            HasReadme: true,
            RepoUrl: null,
            RemoteRef: null,
            SkillPath: null,
            Description: null,
            CheckedAt: DateTime.UtcNow.AddDays(-3));

        SkillCache.Write(info);
        var read = SkillCache.Read(_packageId, Version);

        Assert.NotNull(read);
        var isStale = DateTime.UtcNow - read.CheckedAt > TimeSpan.FromDays(7);
        Assert.False(isStale);
    }
}
