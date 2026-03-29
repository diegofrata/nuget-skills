using NuGetSkills.Models;

namespace NuGetSkills.Tests;

public class ScanFilterTests
{
    private static PackageSkillInfo MakePackage(string id, SkillSource source = SkillSource.Local) =>
        new(PackageId: id, Version: "1.0.0", SkillFiles: ["SKILL.md"], Name: null, Description: "test", Source: source);

    [Fact]
    public void NoConfig_ReturnsAllPackages()
    {
        HashSet<string>? configured = null;
        var packages = new[] { MakePackage("Serilog"), MakePackage("Polly"), MakePackage("MediatR") };

        var filtered = configured is not null
            ? packages.Where(p => configured.Contains(p.PackageId)).ToArray()
            : packages;

        Assert.Equal(3, filtered.Length);
    }

    [Fact]
    public void WithConfig_FiltersCorrectly()
    {
        var configured = new HashSet<string>(["Serilog", "MediatR"], StringComparer.OrdinalIgnoreCase);
        var packages = new[] { MakePackage("Serilog"), MakePackage("Polly"), MakePackage("MediatR") };

        var filtered = packages.Where(p => configured.Contains(p.PackageId)).ToArray();

        Assert.Equal(2, filtered.Length);
        Assert.Contains(filtered, p => p.PackageId == "Serilog");
        Assert.Contains(filtered, p => p.PackageId == "MediatR");
        Assert.DoesNotContain(filtered, p => p.PackageId == "Polly");
    }

    [Fact]
    public void WithConfig_CaseInsensitive()
    {
        var configured = new HashSet<string>(["serilog"], StringComparer.OrdinalIgnoreCase);
        var packages = new[] { MakePackage("Serilog"), MakePackage("Polly") };

        var filtered = packages.Where(p => configured.Contains(p.PackageId)).ToArray();

        Assert.Single(filtered);
        Assert.Equal("Serilog", filtered[0].PackageId);
    }

    [Fact]
    public void EmptyConfig_ReturnsNone()
    {
        var configured = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var packages = new[] { MakePackage("Serilog"), MakePackage("Polly") };

        var filtered = packages.Where(p => configured.Contains(p.PackageId)).ToArray();

        Assert.Empty(filtered);
    }

    [Fact]
    public void Config_WithUnknownPackages_IgnoresThem()
    {
        var configured = new HashSet<string>(["Serilog", "NonExistent"], StringComparer.OrdinalIgnoreCase);
        var packages = new[] { MakePackage("Serilog"), MakePackage("Polly") };

        var filtered = packages.Where(p => configured.Contains(p.PackageId)).ToArray();

        Assert.Single(filtered);
        Assert.Equal("Serilog", filtered[0].PackageId);
    }
}
