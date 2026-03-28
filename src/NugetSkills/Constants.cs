using System.Text.Json;

namespace NugetSkills;

public static class Constants
{
    public static readonly string[] SkillDirectoryNames = ["skills", ".skills"];
    public const string SkillFileName = "SKILL.md";
    public const string ReadmeFileName = "README.md";
    public const string DefaultRef = "HEAD";
    public const string HookCommand = "nuget-skills scan";
    public const string HookIdentifier = "nuget-skills";
    public const string MetaSkillName = "nuget-package-skills";
    public const string BuilderSkillName = "nuget-package-skills-builder";

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
