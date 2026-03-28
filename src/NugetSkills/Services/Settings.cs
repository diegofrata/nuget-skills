using System.Text.Json;

namespace NugetSkills.Services;

public record NugetSkillsSettings(
    bool EnableRemoteScan = true,
    bool EnableReadmeFallback = true)
{
    public static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "nuget-skills", "settings.json");

    private static NugetSkillsSettings? _cached;

    public static NugetSkillsSettings Load()
    {
        if (_cached is not null)
            return _cached;

        try
        {
            var json = File.ReadAllText(FilePath);
            _cached = JsonSerializer.Deserialize<NugetSkillsSettings>(json, Constants.JsonOptions)
                       ?? new NugetSkillsSettings();
        }
        catch
        {
            _cached = new NugetSkillsSettings();
        }

        return _cached;
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, Constants.JsonOptions));
        _cached = this;
    }
}
