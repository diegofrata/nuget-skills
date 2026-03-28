namespace NugetSkills.AgentProviders;

public static class AgentProviderRegistry
{
    private static readonly IAgentProvider[] AllProviders =
    [
        new ClaudeProvider(),
        new CursorProvider(),
        new CopilotProvider(),
        new CodexProvider(),
        new WindsurfProvider(),
        new ClineProvider(),
        new GooseProvider(),
    ];

    public static readonly string[] ValidNames = AllProviders.Select(p => p.Name).ToArray();

    public static IAgentProvider? GetProvider(string name)
    {
        return AllProviders.FirstOrDefault(p =>
            string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public static List<IAgentProvider> DetectAgents(string directory)
    {
        return AllProviders.Where(p => p.DetectProject(directory)).ToList();
    }

    public static List<IAgentProvider> ResolveProviders(string? agentFlag, string directory)
    {
        if (agentFlag is null)
        {
            var detected = DetectAgents(directory);
            if (detected.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No AI coding agents detected in this directory. " +
                    $"Use --agent to specify which agents to install for.\n" +
                    $"Valid values: {string.Join(", ", ValidNames)}, all");
            }
            return detected;
        }

        if (string.Equals(agentFlag, "all", StringComparison.OrdinalIgnoreCase))
            return AllProviders.ToList();

        var names = agentFlag.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var providers = new List<IAgentProvider>();

        foreach (var name in names)
        {
            var provider = GetProvider(name)
                ?? throw new InvalidOperationException(
                    $"Unknown agent: '{name}'. Valid values: {string.Join(", ", ValidNames)}, all");
            providers.Add(provider);
        }

        return providers;
    }
}
