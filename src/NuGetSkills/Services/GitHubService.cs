namespace NuGetSkills.Services;

public record SkillSearchResult(string Path, string Content);

public class GitHubService
{
    private static readonly TimeSpan GhTimeout = TimeSpan.FromSeconds(15);

    public static (string Owner, string Repo)? ParseGitHubUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        var uri = url.Replace(".git", "").TrimEnd('/');

        var githubIndex = uri.IndexOf("github.com/", StringComparison.OrdinalIgnoreCase);
        if (githubIndex < 0)
            return null;

        var path = uri[(githubIndex + "github.com/".Length)..];
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length >= 2 ? (parts[0], parts[1]) : null;
    }

    public async Task<List<string>> ListTagsAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        var result = await ProcessRunner.RunAsync(
            "gh",
            $"api repos/{owner}/{repo}/tags --paginate --jq \".[].name\"",
            GhTimeout,
            cancellationToken);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Stdout))
            return [];

        return result.Stdout
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    /// <summary>
    /// Checks skills/ then .skills/ for SKILL.md at a given ref.
    /// Returns the path and content if found, avoiding a second fetch.
    /// </summary>
    public async Task<SkillSearchResult?> FindSkillAsync(string owner, string repo, string gitRef, CancellationToken cancellationToken = default)
    {
        foreach (var dir in Constants.SkillDirectoryNames)
        {
            var path = $"{dir}/{Constants.SkillFileName}";
            var content = await FetchFileContentAsync(owner, repo, path, gitRef, cancellationToken);
            if (content is not null)
                return new SkillSearchResult(path, content);
        }

        return null;
    }

    public async Task<string?> FetchFileContentAsync(string owner, string repo, string path, string gitRef, CancellationToken cancellationToken = default)
    {
        var result = await ProcessRunner.RunAsync(
            "gh",
            $"api repos/{owner}/{repo}/contents/{path}?ref={gitRef} --jq .content",
            GhTimeout,
            cancellationToken);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Stdout))
            return null;

        try
        {
            var base64 = result.Stdout.Trim().Replace("\n", "");
            var bytes = Convert.FromBase64String(base64);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return null;
        }
    }
}
