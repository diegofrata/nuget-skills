using NuGetSkills.Services;

namespace NuGetSkills.Tests;

public class ProjectDiscoveryTests
{
    [Fact]
    public void Discover_ExplicitPath_ReturnsIt()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var result = ProjectDiscovery.Discover(tempFile);
            Assert.Single(result);
            Assert.Equal(tempFile, result[0]);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Discover_ExplicitPath_NotFound_Throws()
    {
        Assert.Throws<FileNotFoundException>(() =>
            ProjectDiscovery.Discover("/nonexistent/path/to/file.sln"));
    }
}
