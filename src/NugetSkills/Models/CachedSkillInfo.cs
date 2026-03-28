namespace NugetSkills.Models;

public record CachedSkillInfo(
    string PackageId,
    string Version,
    bool HasRemoteSkill,
    bool HasReadme,
    string? RepoUrl,
    string? RemoteRef,
    string? SkillPath,
    string? Description,
    DateTime CheckedAt);
