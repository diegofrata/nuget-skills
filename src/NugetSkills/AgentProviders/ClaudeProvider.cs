using System.Text.Json.Nodes;
using NugetSkills.Services;

namespace NugetSkills.AgentProviders;

public class ClaudeProvider : SkillMdProvider
{
    public override string Name => "claude";
    public override string DisplayName => "Claude Code";
    protected override string SkillsSubPath => Path.Combine(".claude", "skills");

    public override bool DetectProject(string directory)
    {
        return Directory.Exists(Path.Combine(directory, ".claude"));
    }

    public override void InstallHooks(string baseDir)
    {
        var settingsPath = Path.Combine(baseDir, ".claude", "settings.json");

        var root = JsonHelpers.LoadOrCreateObject(settingsPath);

        var hookEntry = new JsonObject
        {
            ["hooks"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "command",
                    ["command"] = Constants.HookCommand,
                    ["timeout"] = 30000,
                },
            },
        };

        JsonHelpers.MergeHookEntry(root, "SessionStart", hookEntry);
        JsonHelpers.WriteJson(settingsPath, root);
    }
}
