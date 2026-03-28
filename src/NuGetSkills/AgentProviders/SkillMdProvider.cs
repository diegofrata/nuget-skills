namespace NuGetSkills.AgentProviders;

/// <summary>
/// Base class for agents that use standard SKILL.md format in a skills/ subdirectory.
/// </summary>
public abstract class SkillMdProvider : IAgentProvider
{
    public abstract string Name { get; }
    public abstract string DisplayName { get; }
    protected abstract string SkillsSubPath { get; }

    public abstract bool DetectProject(string directory);

    public void InstallSkills(string baseDir, string metaSkillContent, string builderSkillContent)
    {
        var skillsBase = Path.Combine(baseDir, SkillsSubPath);

        WriteSkill(skillsBase, Constants.MetaSkillName, metaSkillContent);
        WriteSkill(skillsBase, Constants.BuilderSkillName, builderSkillContent);
    }

    public virtual void InstallHooks(string baseDir)
    {
        // No hooks by default — override in providers that support hooks
    }

    private static void WriteSkill(string skillsBase, string skillName, string content)
    {
        var skillDir = Path.Combine(skillsBase, skillName);
        Directory.CreateDirectory(skillDir);
        File.WriteAllText(Path.Combine(skillDir, "SKILL.md"), content);
    }
}
