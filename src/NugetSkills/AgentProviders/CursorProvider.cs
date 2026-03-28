using System.Text.Json.Nodes;
using NugetSkills.Services;

namespace NugetSkills.AgentProviders;

public class CursorProvider : IAgentProvider
{
    public string Name => "cursor";
    public string DisplayName => "Cursor";

    public bool DetectProject(string directory)
    {
        return Directory.Exists(Path.Combine(directory, ".cursor"));
    }

    public void InstallSkills(string baseDir, string metaSkillContent, string builderSkillContent)
    {
        var rulesDir = Path.Combine(baseDir, ".cursor", "rules");
        Directory.CreateDirectory(rulesDir);

        WriteMdc(rulesDir, Constants.MetaSkillName, metaSkillContent);
        WriteMdc(rulesDir, Constants.BuilderSkillName, builderSkillContent);
    }

    public void InstallHooks(string baseDir)
    {
        var hooksPath = Path.Combine(baseDir, ".cursor", "hooks.json");

        var root = JsonHelpers.LoadOrCreateObject(hooksPath);
        root.TryAdd("version", 1);

        var hookEntry = new JsonObject { ["command"] = Constants.HookCommand };

        JsonHelpers.MergeHookEntry(root, "sessionStart", hookEntry);
        JsonHelpers.WriteJson(hooksPath, root);
    }

    private static void WriteMdc(string rulesDir, string skillName, string skillContent)
    {
        var parsed = FrontmatterParser.Parse(skillContent);
        var description = parsed.GetField("description") ?? "";

        var mdc = $"""
            ---
            description: "{description}"
            alwaysApply: false
            ---

            {parsed.Body}
            """;

        File.WriteAllText(Path.Combine(rulesDir, $"{skillName}.mdc"), mdc.TrimStart());
    }
}
