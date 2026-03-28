using NugetSkills.AgentProviders;

namespace NugetSkills.Tests;

public class AgentProviderRegistryTests : IDisposable
{
    private readonly string _tempDir;

    public AgentProviderRegistryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"nuget-skills-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Theory]
    [InlineData("claude")]
    [InlineData("cursor")]
    [InlineData("copilot")]
    [InlineData("codex")]
    [InlineData("windsurf")]
    [InlineData("cline")]
    [InlineData("goose")]
    public void GetProvider_ValidNames_ReturnsProvider(string name)
    {
        var provider = AgentProviderRegistry.GetProvider(name);
        Assert.NotNull(provider);
        Assert.Equal(name, provider.Name);
    }

    [Fact]
    public void GetProvider_CaseInsensitive()
    {
        var provider = AgentProviderRegistry.GetProvider("CLAUDE");
        Assert.NotNull(provider);
        Assert.Equal("claude", provider.Name);
    }

    [Fact]
    public void GetProvider_Unknown_ReturnsNull()
    {
        var provider = AgentProviderRegistry.GetProvider("unknown");
        Assert.Null(provider);
    }

    [Fact]
    public void DetectAgents_EmptyDir_ReturnsEmpty()
    {
        var detected = AgentProviderRegistry.DetectAgents(_tempDir);
        Assert.Empty(detected);
    }

    [Fact]
    public void DetectAgents_WithClaudeDir_DetectsClaude()
    {
        Directory.CreateDirectory(Path.Combine(_tempDir, ".claude"));

        var detected = AgentProviderRegistry.DetectAgents(_tempDir);

        Assert.Single(detected);
        Assert.Equal("claude", detected[0].Name);
    }

    [Fact]
    public void DetectAgents_WithMultipleDirs_DetectsAll()
    {
        Directory.CreateDirectory(Path.Combine(_tempDir, ".claude"));
        Directory.CreateDirectory(Path.Combine(_tempDir, ".cursor"));
        Directory.CreateDirectory(Path.Combine(_tempDir, ".windsurf"));

        var detected = AgentProviderRegistry.DetectAgents(_tempDir);

        Assert.Equal(3, detected.Count);
        Assert.Contains(detected, p => p.Name == "claude");
        Assert.Contains(detected, p => p.Name == "cursor");
        Assert.Contains(detected, p => p.Name == "windsurf");
    }

    [Fact]
    public void DetectAgents_Goose_DetectedByGoosehints()
    {
        File.WriteAllText(Path.Combine(_tempDir, ".goosehints"), "hints");

        var detected = AgentProviderRegistry.DetectAgents(_tempDir);

        Assert.Single(detected);
        Assert.Equal("goose", detected[0].Name);
    }

    [Fact]
    public void DetectAgents_Copilot_DetectedByCopilotInstructions()
    {
        var githubDir = Path.Combine(_tempDir, ".github");
        Directory.CreateDirectory(githubDir);
        File.WriteAllText(Path.Combine(githubDir, "copilot-instructions.md"), "instructions");

        var detected = AgentProviderRegistry.DetectAgents(_tempDir);

        Assert.Single(detected);
        Assert.Equal("copilot", detected[0].Name);
    }

    [Fact]
    public void DetectAgents_Copilot_NotDetectedByGithubDirAlone()
    {
        // .github/ alone should NOT trigger copilot detection
        Directory.CreateDirectory(Path.Combine(_tempDir, ".github"));

        var detected = AgentProviderRegistry.DetectAgents(_tempDir);

        Assert.DoesNotContain(detected, p => p.Name == "copilot");
    }

    [Fact]
    public void ResolveProviders_All_ReturnsAllProviders()
    {
        var providers = AgentProviderRegistry.ResolveProviders("all", _tempDir);
        Assert.Equal(AgentProviderRegistry.ValidNames.Length, providers.Count);
    }

    [Fact]
    public void ResolveProviders_CommaSeparated_ReturnsSpecified()
    {
        var providers = AgentProviderRegistry.ResolveProviders("claude,cursor", _tempDir);

        Assert.Equal(2, providers.Count);
        Assert.Contains(providers, p => p.Name == "claude");
        Assert.Contains(providers, p => p.Name == "cursor");
    }

    [Fact]
    public void ResolveProviders_Unknown_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            AgentProviderRegistry.ResolveProviders("unknown", _tempDir));
    }

    [Fact]
    public void ResolveProviders_NoFlag_NoAgents_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            AgentProviderRegistry.ResolveProviders(null, _tempDir));
    }

    [Fact]
    public void ResolveProviders_NoFlag_WithAgents_ReturnsDetected()
    {
        Directory.CreateDirectory(Path.Combine(_tempDir, ".claude"));

        var providers = AgentProviderRegistry.ResolveProviders(null, _tempDir);

        Assert.Single(providers);
        Assert.Equal("claude", providers[0].Name);
    }

    [Fact]
    public void ValidNames_ContainsAllExpected()
    {
        Assert.Contains("claude", AgentProviderRegistry.ValidNames);
        Assert.Contains("cursor", AgentProviderRegistry.ValidNames);
        Assert.Contains("copilot", AgentProviderRegistry.ValidNames);
        Assert.Contains("codex", AgentProviderRegistry.ValidNames);
        Assert.Contains("windsurf", AgentProviderRegistry.ValidNames);
        Assert.Contains("cline", AgentProviderRegistry.ValidNames);
        Assert.Contains("goose", AgentProviderRegistry.ValidNames);
    }
}
