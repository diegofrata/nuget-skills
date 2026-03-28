namespace NuGetSkills.AgentProviders;

public class WindsurfProvider : SkillMdProvider
{
    public override string Name => "windsurf";
    public override string DisplayName => "Windsurf";
    protected override string SkillsSubPath => Path.Combine(".windsurf", "skills");

    public override bool DetectProject(string directory)
    {
        return Directory.Exists(Path.Combine(directory, ".windsurf"));
    }
}
