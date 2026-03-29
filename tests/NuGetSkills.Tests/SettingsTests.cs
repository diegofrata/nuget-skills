using NuGetSkills.Services;

namespace NuGetSkills.Tests;

public class SettingsTests
{
    [Fact]
    public void DefaultSettings_RemoteScanEnabled()
    {
        var settings = new NuGetSkillsSettings();

        Assert.True(settings.EnableRemoteScan);
    }

    [Fact]
    public void Settings_CanDisableRemote()
    {
        var settings = new NuGetSkillsSettings(EnableRemoteScan: false);

        Assert.False(settings.EnableRemoteScan);
    }

    [Fact]
    public void Load_NoFile_ReturnsDefaults()
    {
        // Load always works — returns defaults when no file exists
        var settings = NuGetSkillsSettings.Load();

        Assert.NotNull(settings);
        Assert.True(settings.EnableRemoteScan);
    }

    [Fact]
    public void FilePath_ContainsNugetSkills()
    {
        Assert.Contains("nuget-skills", NuGetSkillsSettings.FilePath);
        Assert.Contains("settings.json", NuGetSkillsSettings.FilePath);
    }
}
