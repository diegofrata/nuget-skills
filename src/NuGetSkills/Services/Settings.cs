using System.Text.Json;

namespace NuGetSkills.Services;

public record NuGetSkillsSettings(
    bool EnableRemoteScan = true)
{
    public static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "nuget-skills", "settings.json");

    private static NuGetSkillsSettings? _cached;

    public static NuGetSkillsSettings Load()
    {
        if (_cached is not null)
            return _cached;

        try
        {
            var json = File.ReadAllText(FilePath);
            _cached = JsonSerializer.Deserialize<NuGetSkillsSettings>(json, Constants.JsonOptions)
                       ?? new NuGetSkillsSettings();
        }
        catch
        {
            _cached = new NuGetSkillsSettings();
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
