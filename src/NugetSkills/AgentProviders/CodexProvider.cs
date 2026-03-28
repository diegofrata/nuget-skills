namespace NugetSkills.AgentProviders;

/// <summary>
/// Covers both OpenAI Codex and Amp (Sourcegraph) via shared .agents/skills/ path.
/// </summary>
public class CodexProvider : SkillMdProvider
{
    public override string Name => "codex";
    public override string DisplayName => "Codex / Amp";
    protected override string SkillsSubPath => Path.Combine(".agents", "skills");

    public override bool DetectProject(string directory)
    {
        return Directory.Exists(Path.Combine(directory, ".codex"))
            || Directory.Exists(Path.Combine(directory, ".agents"));
    }
}
