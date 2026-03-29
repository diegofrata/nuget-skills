namespace NuGetSkills.Models;

public record CachedSkillInfo(
    string PackageId,
    string Version,
    bool HasRemoteSkill,
    string? RepoUrl,
    string? RemoteRef,
    string? SkillPath,
    string? Description,
    DateTime CheckedAt);
