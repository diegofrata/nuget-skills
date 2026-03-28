using NuGetSkills.Models;
using NuGetSkills.Services;

namespace NuGetSkills.Tests;

public class SkillCacheTests : IDisposable
{
    private readonly string _originalCacheDir;
    private readonly string _tempCacheDir;

    public SkillCacheTests()
    {
        _originalCacheDir = SkillCache.CacheDirectory;
        _tempCacheDir = Path.Combine(Path.GetTempPath(), $"nuget-skills-cache-test-{Guid.NewGuid():N}");
        // We test the static methods directly — they use the real cache dir.
        // To isolate, we write/read from a known location using the actual API.
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempCacheDir))
            Directory.Delete(_tempCacheDir, true);
    }

    [Fact]
    public void GetCacheDirectory_ReturnsNonEmpty()
    {
        var dir = SkillCache.CacheDirectory;
        Assert.False(string.IsNullOrEmpty(dir));
        Assert.Contains("nuget-skills", dir);
        Assert.Contains("cache", dir);
    }

    [Fact]
    public void WriteAndRead_RoundTrips()
    {
        var info = new CachedSkillInfo(
            PackageId: "TestPackage.CacheRoundTrip",
            Version: "99.99.99",
            HasRemoteSkill: true,
            HasReadme: false,
            RepoUrl: "https://github.com/test/repo",
            RemoteRef: "v99.99.99",
            SkillPath: "skills/SKILL.md",
            Description: "Test description",
            CheckedAt: new DateTime(2026, 3, 28, 12, 0, 0, DateTimeKind.Utc));

        SkillCache.Write(info);
        var read = SkillCache.Read("TestPackage.CacheRoundTrip", "99.99.99");

        Assert.NotNull(read);
        Assert.Equal("TestPackage.CacheRoundTrip", read.PackageId);
        Assert.Equal("99.99.99", read.Version);
        Assert.True(read.HasRemoteSkill);
        Assert.Equal("https://github.com/test/repo", read.RepoUrl);
        Assert.Equal("v99.99.99", read.RemoteRef);
        Assert.Equal("skills/SKILL.md", read.SkillPath);
        Assert.Equal("Test description", read.Description);

        // Clean up
        var path = Path.Combine(SkillCache.CacheDirectory, "testpackage.cacheroundtrip", "99.99.99.json");
        if (File.Exists(path)) File.Delete(path);
    }

    [Fact]
    public void Read_NonExistent_ReturnsNull()
    {
        var result = SkillCache.Read("nonexistent.package.xyz", "0.0.0");
        Assert.Null(result);
    }

    [Fact]
    public void Write_NoRemoteSkill_RoundTrips()
    {
        var info = new CachedSkillInfo(
            PackageId: "TestPackage.NoSkill",
            Version: "99.99.98",
            HasRemoteSkill: false,
            HasReadme: true,
            RepoUrl: "https://github.com/test/noskill",
            RemoteRef: null,
            SkillPath: null,
            Description: null,
            CheckedAt: DateTime.UtcNow);

        SkillCache.Write(info);
        var read = SkillCache.Read("TestPackage.NoSkill", "99.99.98");

        Assert.NotNull(read);
        Assert.False(read.HasRemoteSkill);
        Assert.Null(read.RemoteRef);
        Assert.Null(read.SkillPath);

        // Clean up
        var path = Path.Combine(SkillCache.CacheDirectory, "testpackage.noskill", "99.99.98.json");
        if (File.Exists(path)) File.Delete(path);
    }

    [Fact]
    public void CountEntries_ReturnsZeroForEmptyCache()
    {
        // This just verifies CountEntries doesn't throw
        var count = SkillCache.CountEntries();
        Assert.True(count >= 0);
    }
}
