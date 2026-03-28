namespace NuGetSkills.Models;

public enum SkillSource { Local, Remote, Readme }

public record PackageSkillInfo(
    string PackageId,
    string Version,
    string[] SkillFiles,
    string? Name,
    string? Description,
    SkillSource Source = SkillSource.Local,
    string? RemoteRepo = null,
    string? RemoteRef = null);
