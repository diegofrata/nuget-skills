using NugetSkills.Services;

namespace NugetSkills.Tests;

public class NuspecParserTests : IDisposable
{
    private readonly string _tempDir;

    public NuspecParserTests()
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
    public void Parse_ExtractsAllMetadata()
    {
        WriteNuspec("mypackage", """
            <?xml version="1.0" encoding="utf-8"?>
            <package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
              <metadata>
                <description>A great package</description>
                <repository type="git" url="https://github.com/owner/repo" />
                <projectUrl>https://example.com</projectUrl>
                <license type="expression">MIT</license>
              </metadata>
            </package>
            """);

        var result = NuspecParser.Parse(_tempDir, "MyPackage");

        Assert.NotNull(result);
        Assert.Equal("A great package", result.Description);
        Assert.Equal("https://github.com/owner/repo", result.RepositoryUrl);
        Assert.Equal("git", result.RepositoryType);
        Assert.Equal("https://example.com", result.ProjectUrl);
        Assert.Equal("MIT", result.License);
    }

    [Fact]
    public void Parse_MissingFile_ReturnsNull()
    {
        var result = NuspecParser.Parse(_tempDir, "nonexistent");
        Assert.Null(result);
    }

    [Fact]
    public void Parse_NoRepository_ReturnsNullRepoFields()
    {
        WriteNuspec("mypackage", """
            <?xml version="1.0" encoding="utf-8"?>
            <package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
              <metadata>
                <description>No repo</description>
              </metadata>
            </package>
            """);

        var result = NuspecParser.Parse(_tempDir, "MyPackage");

        Assert.NotNull(result);
        Assert.Equal("No repo", result.Description);
        Assert.Null(result.RepositoryUrl);
        Assert.Null(result.RepositoryType);
    }

    [Fact]
    public void Parse_PackageIdIsLowercased()
    {
        WriteNuspec("generator.equals", """
            <?xml version="1.0" encoding="utf-8"?>
            <package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
              <metadata>
                <description>Test</description>
              </metadata>
            </package>
            """);

        var result = NuspecParser.Parse(_tempDir, "Generator.Equals");

        Assert.NotNull(result);
        Assert.Equal("Test", result.Description);
    }

    private void WriteNuspec(string lowerId, string content)
    {
        File.WriteAllText(Path.Combine(_tempDir, $"{lowerId}.nuspec"), content);
    }
}
