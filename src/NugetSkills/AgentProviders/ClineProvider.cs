namespace NugetSkills.AgentProviders;

public class ClineProvider : SkillMdProvider
{
    public override string Name => "cline";
    public override string DisplayName => "Cline";
    protected override string SkillsSubPath => Path.Combine(".cline", "skills");

    public override bool DetectProject(string directory)
    {
        return Directory.Exists(Path.Combine(directory, ".cline"));
    }
}
