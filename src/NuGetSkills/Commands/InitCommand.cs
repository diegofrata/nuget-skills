using System.CommandLine;
using System.Reflection;
using NuGetSkills.AgentProviders;
using NuGetSkills.Services;

namespace NuGetSkills.Commands;

public static class InitCommand
{
    public static Command Create()
    {
        var projectOption = new Option<bool>("--project-level")
        {
            Description = "Install to the current project directory instead of globally",
        };

        var agentOption = new Option<string?>("--agent", "-a")
        {
            Description = $"Comma-separated list of agents ({string.Join(", ", AgentProviderRegistry.ValidNames)}, all). Auto-detects if omitted.",
        };

        var noRemoteOption = new Option<bool>("--no-remote")
        {
            Description = "Disable remote skill discovery (checking package source repos)",
        };

        var noReadmeOption = new Option<bool>("--no-readme")
        {
            Description = "Disable README fallback when no skill is found",
        };

        var command = new Command("install", "Install nuget-skills discovery for AI coding agents");
        command.Add(projectOption);
        command.Add(agentOption);
        command.Add(noRemoteOption);
        command.Add(noReadmeOption);

        command.SetAction(async (parseResult, _) =>
        {
            var projectLevel = parseResult.GetValue(projectOption);
            var agent = parseResult.GetValue(agentOption);
            var noRemote = parseResult.GetValue(noRemoteOption);
            var noReadme = parseResult.GetValue(noReadmeOption);
            await ExecuteAsync(!projectLevel, agent, noRemote, noReadme);
        });

        return command;
    }

    private static async Task ExecuteAsync(bool global, string? agentFlag, bool noRemote, bool noReadme)
    {
        var (baseDir, providers) = ResolveProviders(global, agentFlag);
        var metaSkill = ReadResource("NuGetSkills.Templates.nuget_package_skills.SKILL.md");
        var builderSkill = ReadResource("NuGetSkills.Templates.nuget_package_skills_builder.SKILL.md");

        foreach (var provider in providers)
        {
            Console.WriteLine($"  [{provider.DisplayName}]");
            provider.InstallSkill(baseDir, Constants.MetaSkillName, metaSkill);
            provider.InstallSkill(baseDir, Constants.BuilderSkillName, builderSkill);
            Console.WriteLine($"    Installed skills");
            provider.InstallHooks(baseDir);
            Console.WriteLine($"    Installed hooks");
        }

        if (noRemote || noReadme)
        {
            var settings = new NuGetSkillsSettings(
                EnableRemoteScan: !noRemote,
                EnableReadmeFallback: !noReadme);
            settings.Save();
            Console.WriteLine($"  Saved settings to {NuGetSkillsSettings.FilePath}");
        }

        Console.WriteLine();
        Console.WriteLine($"Done. Installed for {providers.Count} agent(s): {string.Join(", ", providers.Select(p => p.DisplayName))}");

        if (!await ToolChecker.IsGhAvailableAsync() && !noRemote)
        {
            Console.WriteLine();
            Console.WriteLine("  Warning: 'gh' CLI not found. Remote skill discovery (scanning package repos");
            Console.WriteLine("  for skills) will not be available. Install from https://cli.github.com");
        }
    }

    internal static (string BaseDir, List<IAgentProvider> Providers) ResolveProviders(bool global, string? agentFlag)
    {
        var baseDir = global
            ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            : Directory.GetCurrentDirectory();

        List<IAgentProvider> providers;
        try
        {
            providers = AgentProviderRegistry.ResolveProviders(agentFlag, baseDir);
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine(ex.Message);
            Environment.Exit(1);
            return default;
        }

        return (baseDir, providers);
    }

    internal static string ReadResource(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded resource not found: {name}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
