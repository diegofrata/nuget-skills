using NuGetSkills.Services;

namespace NuGetSkills.Tests;

public class VersionMatcherTests
{
    [Fact]
    public void ExactMatch_ReturnsTag()
    {
        var tags = new[] { "v1.0.0", "v2.0.0", "v3.0.0" };
        var result = VersionMatcher.FindBestTag(tags, "MyPackage", "2.0.0");
        Assert.Equal("v2.0.0", result);
    }

    [Fact]
    public void ExactMatch_WithoutPrefix_ReturnsTag()
    {
        var tags = new[] { "1.0.0", "2.0.0", "3.0.0" };
        var result = VersionMatcher.FindBestTag(tags, "MyPackage", "2.0.0");
        Assert.Equal("2.0.0", result);
    }

    [Fact]
    public void ExactMatch_PreRelease_ReturnsTag()
    {
        var tags = new[] { "v4.0.0-alpha7", "v4.0.0-alpha8", "v4.0.0" };
        var result = VersionMatcher.FindBestTag(tags, "MyPackage", "4.0.0-alpha8");
        Assert.Equal("v4.0.0-alpha8", result);
    }

    [Fact]
    public void PreRelease_DoesNotMatchStable()
    {
        var tags = new[] { "v4.0.0" };
        var result = VersionMatcher.FindBestTag(tags, "MyPackage", "4.0.0-beta1");
        // 4.0.0 > 4.0.0-beta1, so 4.0.0 is not <= target — no match
        // Actually 4.0.0 > 4.0.0-beta1, so it should NOT be returned as closest lower
        // The closest lower would need to be < 4.0.0-beta1
        Assert.Null(result);
    }

    [Fact]
    public void ClosestLower_WhenNoExactMatch()
    {
        var tags = new[] { "v1.0.0", "v2.0.0", "v3.0.0", "v5.0.0" };
        var result = VersionMatcher.FindBestTag(tags, "MyPackage", "4.0.0");
        Assert.Equal("v3.0.0", result);
    }

    [Fact]
    public void ClosestLower_PreRelease_LowerThanStable()
    {
        var tags = new[] { "v3.0.0-rc1", "v3.0.0", "v4.0.0" };
        var result = VersionMatcher.FindBestTag(tags, "MyPackage", "3.0.0");
        // Exact match on v3.0.0
        Assert.Equal("v3.0.0", result);
    }

    [Fact]
    public void NoMatch_ReturnsNull()
    {
        var tags = new[] { "v5.0.0", "v6.0.0" };
        var result = VersionMatcher.FindBestTag(tags, "MyPackage", "2.0.0");
        Assert.Null(result);
    }

    [Fact]
    public void EmptyTags_ReturnsNull()
    {
        var result = VersionMatcher.FindBestTag([], "MyPackage", "1.0.0");
        Assert.Null(result);
    }

    [Fact]
    public void StripPrefix_PackageName_Dot()
    {
        var tags = new[] { "MyPackage.1.0.0", "MyPackage.2.0.0" };
        var result = VersionMatcher.FindBestTag(tags, "MyPackage", "2.0.0");
        Assert.Equal("MyPackage.2.0.0", result);
    }

    [Fact]
    public void StripPrefix_PackageName_Slash()
    {
        var tags = new[] { "MyPackage/1.0.0", "MyPackage/2.0.0" };
        var result = VersionMatcher.FindBestTag(tags, "MyPackage", "2.0.0");
        Assert.Equal("MyPackage/2.0.0", result);
    }

    [Fact]
    public void StripPrefix_Release()
    {
        var tags = new[] { "release/1.0.0", "release/2.0.0" };
        var result = VersionMatcher.FindBestTag(tags, "MyPackage", "2.0.0");
        Assert.Equal("release/2.0.0", result);
    }

    [Fact]
    public void StripPrefix_CaseInsensitive()
    {
        var tags = new[] { "V1.0.0", "V2.0.0" };
        var result = VersionMatcher.FindBestTag(tags, "MyPackage", "2.0.0");
        Assert.Equal("V2.0.0", result);
    }

    [Fact]
    public void NonSemverTags_AreIgnored()
    {
        var tags = new[] { "latest", "stable", "v2.0.0", "not-a-version" };
        var result = VersionMatcher.FindBestTag(tags, "MyPackage", "2.0.0");
        Assert.Equal("v2.0.0", result);
    }

    [Fact]
    public void BuildMetadata_IsIgnoredForParsing()
    {
        var tags = new[] { "v2.0.0+build123" };
        var result = VersionMatcher.FindBestTag(tags, "MyPackage", "2.0.0");
        // Build metadata is part of semver but should not affect matching
        // The string won't exact-match "2.0.0", but parsed version should be 2.0.0
        Assert.Equal("v2.0.0+build123", result);
    }

    [Fact]
    public void PreRelease_Ordering()
    {
        var tags = new[] { "v1.0.0-alpha", "v1.0.0-beta", "v1.0.0-rc1" };
        var result = VersionMatcher.FindBestTag(tags, "MyPackage", "1.0.0-beta");
        Assert.Equal("v1.0.0-beta", result);
    }

    [Fact]
    public void ClosestLower_PreRelease_AlphaBeforeBeta()
    {
        var tags = new[] { "v1.0.0-alpha", "v1.0.0-gamma" };
        var result = VersionMatcher.FindBestTag(tags, "MyPackage", "1.0.0-beta");
        Assert.Equal("v1.0.0-alpha", result);
    }
}
