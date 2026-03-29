using System.Text.Json.Nodes;
using NuGetSkills.Services;

namespace NuGetSkills.AgentProviders;

public class CopilotProvider : IAgentProvider
{
    public string Name => "copilot";
    public string DisplayName => "GitHub Copilot";

    public bool DetectProject(string directory)
    {
        return File.Exists(Path.Combine(directory, ".github", "copilot-instructions.md"))
            || Directory.Exists(Path.Combine(directory, ".github", "instructions"))
            || Directory.Exists(Path.Combine(directory, ".github", "hooks"));
    }

    public void InstallSkill(string baseDir, string skillName, string skillContent)
    {
        var instructionsDir = Path.Combine(baseDir, ".github", "instructions");
        Directory.CreateDirectory(instructionsDir);
        WriteInstructions(instructionsDir, skillName, skillContent);
    }

    public void InstallHooks(string baseDir)
    {
        var hooksDir = Path.Combine(baseDir, ".github", "hooks");
        Directory.CreateDirectory(hooksDir);

        var hook = new JsonObject
        {
            ["event"] = "SessionStart",
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

        var hookPath = Path.Combine(hooksDir, "nuget-skills-scan.json");
        JsonHelpers.WriteJson(hookPath, hook);
    }

    private static void WriteInstructions(string dir, string skillName, string skillContent)
    {
        var parsed = FrontmatterParser.Parse(skillContent);
        File.WriteAllText(Path.Combine(dir, $"{skillName}.instructions.md"), parsed.Body);
    }
}
