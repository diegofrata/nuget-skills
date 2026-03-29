namespace NuGetSkills.Services;

public record SkillFrontmatter(string? Name, string? Description, string? Packages);

public static class SkillReader
{
    public static SkillFrontmatter ReadFrontmatter(string filePath)
    {
        var content = File.ReadAllText(filePath);
        var parsed = FrontmatterParser.Parse(content);
        return new SkillFrontmatter(parsed.GetField("name"), parsed.GetField("description"), parsed.GetField("packages"));
    }

    public static async Task<string> ReadFullAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await File.ReadAllTextAsync(filePath, cancellationToken);
    }

    public static string[] FindSkillFiles(string skillsDirectory)
    {
        return Directory.GetFiles(skillsDirectory, "*.md", SearchOption.TopDirectoryOnly)
            .OrderBy(f => f)
            .ToArray();
    }
}
