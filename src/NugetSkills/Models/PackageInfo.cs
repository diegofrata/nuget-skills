namespace NuGetSkills.Models;

/// <summary>
/// A NuGet package reference discovered in a project.
/// </summary>
public record PackageInfo(string Id, string ResolvedVersion);
