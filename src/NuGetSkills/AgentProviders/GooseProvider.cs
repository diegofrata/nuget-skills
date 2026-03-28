namespace NuGetSkills.AgentProviders;

public class GooseProvider : SkillMdProvider
{
    public override string Name => "goose";
    public override string DisplayName => "Goose";
    protected override string SkillsSubPath => Path.Combine(".goose", "skills");

    public override bool DetectProject(string directory)
    {
        return Directory.Exists(Path.Combine(directory, ".goose"))
            || File.Exists(Path.Combine(directory, ".goosehints"));
    }
}
