using NuGetSkills.Models;
using NuGetSkills.Services;

namespace NuGetSkills.Tests;

public class ProjectConfigServiceTests : IDisposable
{
    private readonly string _tempDir;

    public ProjectConfigServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"nuget-skills-config-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void Load_ValidFile_ReturnsConfig()
    {
        var path = Path.Combine(_tempDir, ".nuget-skills.json");
        File.WriteAllText(path, """{"packages":["Serilog","Polly"]}""");

        var config = ProjectConfigService.Load(path);

        Assert.NotNull(config);
        Assert.Equal(2, config.Packages.Length);
        Assert.Contains("Serilog", config.Packages);
        Assert.Contains("Polly", config.Packages);
    }

    [Fact]
    public void Load_MissingFile_ReturnsNull()
    {
        var path = Path.Combine(_tempDir, "nonexistent.json");

        var config = ProjectConfigService.Load(path);

        Assert.Null(config);
    }

    [Fact]
    public void Load_MalformedJson_ReturnsNull()
    {
        var path = Path.Combine(_tempDir, ".nuget-skills.json");
        File.WriteAllText(path, "not json at all {{{");

        var config = ProjectConfigService.Load(path);

        Assert.Null(config);
    }

    [Fact]
    public void Load_EmptyPackages_ReturnsEmptyArray()
    {
        var path = Path.Combine(_tempDir, ".nuget-skills.json");
        File.WriteAllText(path, """{"packages":[]}""");

        var config = ProjectConfigService.Load(path);

        Assert.NotNull(config);
        Assert.Empty(config.Packages);
    }

    [Fact]
    public void Save_WritesValidJson()
    {
        var path = Path.Combine(_tempDir, ".nuget-skills.json");
        var config = new ProjectConfig(["Serilog", "MediatR"]);

        ProjectConfigService.Save(path, config);

        var json = File.ReadAllText(path);
        Assert.Contains("Serilog", json);
        Assert.Contains("MediatR", json);

        var loaded = ProjectConfigService.Load(path);
        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.Packages.Length);
    }

    [Fact]
    public void Save_OverwritesExisting()
    {
        var path = Path.Combine(_tempDir, ".nuget-skills.json");

        ProjectConfigService.Save(path, new ProjectConfig(["Serilog"]));
        ProjectConfigService.Save(path, new ProjectConfig(["Polly", "MediatR"]));

        var config = ProjectConfigService.Load(path);

        Assert.NotNull(config);
        Assert.Equal(2, config.Packages.Length);
        Assert.Contains("Polly", config.Packages);
        Assert.Contains("MediatR", config.Packages);
        Assert.DoesNotContain("Serilog", config.Packages);
    }

    [Fact]
    public void FindConfigPath_FileExists_ReturnsPath()
    {
        var configPath = Path.Combine(_tempDir, Constants.ProjectConfigFileName);
        File.WriteAllText(configPath, """{"packages":[]}""");

        // Create a dummy project file so we can pass its path
        var projectFile = Path.Combine(_tempDir, "Test.csproj");
        File.WriteAllText(projectFile, "<Project />");

        var found = ProjectConfigService.FindConfigPath(projectFile);

        Assert.NotNull(found);
        Assert.Equal(configPath, found);
    }

    [Fact]
    public void FindConfigPath_NoFile_ReturnsNull()
    {
        var projectFile = Path.Combine(_tempDir, "Test.csproj");
        File.WriteAllText(projectFile, "<Project />");

        var found = ProjectConfigService.FindConfigPath(projectFile);

        Assert.Null(found);
    }

    [Fact]
    public void ResolveConfiguredPackages_NoFile_ReturnsNull()
    {
        var projectFile = Path.Combine(_tempDir, "Test.csproj");
        File.WriteAllText(projectFile, "<Project />");

        var result = ProjectConfigService.ResolveConfiguredPackages(projectFile);

        Assert.Null(result);
    }

    [Fact]
    public void ResolveConfiguredPackages_WithFile_ReturnsHashSet()
    {
        var configPath = Path.Combine(_tempDir, Constants.ProjectConfigFileName);
        File.WriteAllText(configPath, """{"packages":["Serilog","Polly"]}""");
        var projectFile = Path.Combine(_tempDir, "Test.csproj");
        File.WriteAllText(projectFile, "<Project />");

        var result = ProjectConfigService.ResolveConfiguredPackages(projectFile);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains("Serilog", result);
        Assert.Contains("Polly", result);
    }

    [Fact]
    public void ResolveConfiguredPackages_CaseInsensitive()
    {
        var configPath = Path.Combine(_tempDir, Constants.ProjectConfigFileName);
        File.WriteAllText(configPath, """{"packages":["serilog"]}""");
        var projectFile = Path.Combine(_tempDir, "Test.csproj");
        File.WriteAllText(projectFile, "<Project />");

        var result = ProjectConfigService.ResolveConfiguredPackages(projectFile);

        Assert.NotNull(result);
        Assert.Contains("Serilog", result);
        Assert.Contains("SERILOG", result);
        Assert.Contains("serilog", result);
    }
}
