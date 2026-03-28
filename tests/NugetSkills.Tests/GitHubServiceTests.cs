using NugetSkills.Services;

namespace NugetSkills.Tests;

public class GitHubServiceTests
{
    [Theory]
    [InlineData("https://github.com/owner/repo", "owner", "repo")]
    [InlineData("https://github.com/owner/repo.git", "owner", "repo")]
    [InlineData("https://github.com/owner/repo/", "owner", "repo")]
    [InlineData("https://github.com/owner/repo.git/", "owner", "repo")]
    [InlineData("git://github.com/owner/repo", "owner", "repo")]
    [InlineData("https://github.com/My-Org/My.Package", "My-Org", "My.Package")]
    [InlineData("https://github.com/dotnet/dotnet", "dotnet", "dotnet")]
    public void ParseGitHubUrl_ValidUrls(string url, string expectedOwner, string expectedRepo)
    {
        var result = GitHubService.ParseGitHubUrl(url);
        Assert.NotNull(result);
        Assert.Equal(expectedOwner, result.Value.Owner);
        Assert.Equal(expectedRepo, result.Value.Repo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://gitlab.com/owner/repo")]
    [InlineData("https://bitbucket.org/owner/repo")]
    [InlineData("not-a-url")]
    [InlineData("https://github.com/")]
    [InlineData("https://github.com/owner")]
    public void ParseGitHubUrl_InvalidUrls_ReturnsNull(string? url)
    {
        var result = GitHubService.ParseGitHubUrl(url);
        Assert.Null(result);
    }

    [Fact]
    public void ParseGitHubUrl_WithExtraPathSegments_TakesFirstTwo()
    {
        var result = GitHubService.ParseGitHubUrl("https://github.com/owner/repo/tree/main/src");
        Assert.NotNull(result);
        Assert.Equal("owner", result.Value.Owner);
        Assert.Equal("repo", result.Value.Repo);
    }
}
