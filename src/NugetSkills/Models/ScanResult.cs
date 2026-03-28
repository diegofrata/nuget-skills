namespace NugetSkills.Models;

/// <summary>
/// The result of scanning a project/solution for NuGet package skills.
/// </summary>
public record ScanResult(
    string Source,
    int TotalPackages,
    PackageSkillInfo[] PackagesWithSkills,
    string[]? Problems,
    int SkippedNonGitHub = 0);
