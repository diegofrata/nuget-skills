using System.CommandLine;
using System.Reflection;
using NugetSkills.AgentProviders;
using NugetSkills.Services;

namespace NugetSkills.Commands;

public static class InitCommand
{
    public static Command Create()
    {
        var globalOption = new Option<bool>("--global", "-g")
        {
            Description = "Install skills and hooks to the global agent directory instead of project-level",
        };

        var agentOption = new Option<string?>("--agent", "-a")
        {
            Description = $"Comma-separated list of agents to install for ({string.Join(", ", AgentProviderRegistry.ValidNames)}, all). Auto-detects if omitted.",
        };

        var noRemoteOption = new Option<bool>("--no-remote")
        {
            Description = "Disable remote skill discovery (checking package source repos)",
        };

        var noReadmeOption = new Option<bool>("--no-readme")
        {
            Description = "Disable README fallback when no skill is found",
        };

        var command = new Command("init", "Install nuget-skills for AI coding agents (skills + hooks)");
        command.Add(globalOption);
        command.Add(agentOption);
        command.Add(noRemoteOption);
        command.Add(noReadmeOption);

        command.SetAction(async (parseResult, _) =>
        {
            var global = parseResult.GetValue(globalOption);
            var agent = parseResult.GetValue(agentOption);
            var noRemote = parseResult.GetValue(noRemoteOption);
            var noReadme = parseResult.GetValue(noReadmeOption);
            await ExecuteAsync(global, agent, noRemote, noReadme);
        });

        return command;
    }

    private static async Task ExecuteAsync(bool global, string? agentFlag, bool noRemote, bool noReadme)
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
            return;
        }

        var (metaSkill, builderSkill) = LoadTemplates();

        foreach (var provider in providers)
        {
            Console.WriteLine($"  [{provider.DisplayName}]");

            provider.InstallSkills(baseDir, metaSkill, builderSkill);
            Console.WriteLine($"    Installed skills");

            provider.InstallHooks(baseDir);
            Console.WriteLine($"    Installed hooks");
        }

        // Save settings only if user explicitly opted out of something
        if (noRemote || noReadme)
        {
            var settings = new NugetSkillsSettings(
                EnableRemoteScan: !noRemote,
                EnableReadmeFallback: !noReadme);
            settings.Save();
            Console.WriteLine($"  Saved settings to {NugetSkillsSettings.FilePath}");
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

    private static (string MetaSkill, string BuilderSkill) LoadTemplates()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var metaSkill = ReadResource(assembly, "NugetSkills.Templates.nuget_package_skills.SKILL.md");
        var builderSkill = ReadResource(assembly, "NugetSkills.Templates.nuget_package_skills_builder.SKILL.md");

        return (metaSkill, builderSkill);
    }

    private static string ReadResource(Assembly assembly, string name)
    {
        using var stream = assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded resource not found: {name}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
