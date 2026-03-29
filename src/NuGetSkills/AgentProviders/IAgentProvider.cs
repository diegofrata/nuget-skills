namespace NuGetSkills.AgentProviders;

public interface IAgentProvider
{
    string Name { get; }
    string DisplayName { get; }
    bool DetectProject(string directory);
    void InstallSkill(string baseDir, string skillName, string skillContent);
    void InstallHooks(string baseDir);
}
