using NugetSkills.Services;

namespace NugetSkills.Tests;

public class SkillReaderTests : IDisposable
{
    private readonly string _tempDir;

    public SkillReaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"nuget-skills-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void ReadFrontmatter_ExtractsNameAndDescription()
    {
        var path = WriteFile("SKILL.md", """
            ---
            name: my-skill
            description: A test skill
            ---

            # Content here
            """);

        var result = SkillReader.ReadFrontmatter(path);

        Assert.Equal("my-skill", result.Name);
        Assert.Equal("A test skill", result.Description);
    }

    [Fact]
    public void ReadFrontmatter_NoFrontmatter_ReturnsNulls()
    {
        var path = WriteFile("SKILL.md", "# Just content\nNo frontmatter here.");

        var result = SkillReader.ReadFrontmatter(path);

        Assert.Null(result.Name);
        Assert.Null(result.Description);
    }

    [Fact]
    public void ReadFrontmatter_EmptyFrontmatter_ReturnsNulls()
    {
        var path = WriteFile("SKILL.md", """
            ---
            ---

            # Content
            """);

        var result = SkillReader.ReadFrontmatter(path);

        Assert.Null(result.Name);
        Assert.Null(result.Description);
    }

    [Fact]
    public void ReadFrontmatter_IgnoresExtraFields()
    {
        var path = WriteFile("SKILL.md", """
            ---
            name: test
            description: desc
            author: someone
            version: 1.0
            ---
            """);

        var result = SkillReader.ReadFrontmatter(path);

        Assert.Equal("test", result.Name);
        Assert.Equal("desc", result.Description);
    }

    [Fact]
    public void ReadFrontmatter_CaseInsensitiveKeys()
    {
        var path = WriteFile("SKILL.md", """
            ---
            Name: upper
            DESCRIPTION: also upper
            ---
            """);

        var result = SkillReader.ReadFrontmatter(path);

        Assert.Equal("upper", result.Name);
        Assert.Equal("also upper", result.Description);
    }

    [Fact]
    public async Task ReadFullAsync_ReturnsEntireContent()
    {
        var content = "---\nname: test\n---\n\n# Hello\nWorld";
        var path = WriteFile("SKILL.md", content);

        var result = await SkillReader.ReadFullAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal(content, result);
    }

    [Fact]
    public void FindSkillFiles_FindsMdFiles()
    {
        WriteFile("SKILL.md", "skill");
        WriteFile("GUIDE.md", "guide");
        WriteFile("README.txt", "not this");

        var files = SkillReader.FindSkillFiles(_tempDir);

        Assert.Equal(2, files.Length);
        Assert.All(files, f => Assert.EndsWith(".md", f));
    }

    [Fact]
    public void FindSkillFiles_ReturnsSorted()
    {
        WriteFile("B.md", "b");
        WriteFile("A.md", "a");
        WriteFile("C.md", "c");

        var files = SkillReader.FindSkillFiles(_tempDir);

        Assert.Equal("A.md", Path.GetFileName(files[0]));
        Assert.Equal("B.md", Path.GetFileName(files[1]));
        Assert.Equal("C.md", Path.GetFileName(files[2]));
    }

    [Fact]
    public void FindSkillFiles_EmptyDirectory_ReturnsEmpty()
    {
        var subDir = Path.Combine(_tempDir, "empty");
        Directory.CreateDirectory(subDir);

        var files = SkillReader.FindSkillFiles(subDir);

        Assert.Empty(files);
    }

    private string WriteFile(string name, string content)
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }
}
