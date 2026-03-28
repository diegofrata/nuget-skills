namespace NuGetSkills.AgentProviders;

public interface IAgentProvider
{
    string Name { get; }
    string DisplayName { get; }
    bool DetectProject(string directory);
    void InstallSkills(string baseDir, string metaSkillContent, string builderSkillContent);
    void InstallHooks(string baseDir);
}
