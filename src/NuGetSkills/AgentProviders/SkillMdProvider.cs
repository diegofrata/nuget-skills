namespace NuGetSkills.AgentProviders;

public abstract class SkillMdProvider : IAgentProvider
{
    public abstract string Name { get; }
    public abstract string DisplayName { get; }
    protected abstract string SkillsSubPath { get; }

    public abstract bool DetectProject(string directory);

    public void InstallSkill(string baseDir, string skillName, string skillContent)
    {
        var skillDir = Path.Combine(baseDir, SkillsSubPath, skillName);
        Directory.CreateDirectory(skillDir);
        File.WriteAllText(Path.Combine(skillDir, Constants.SkillFileName), skillContent);
    }

    public virtual void InstallHooks(string baseDir)
    {
    }
}
