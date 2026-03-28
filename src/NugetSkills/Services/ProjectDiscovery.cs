namespace NugetSkills.Services;

public static class ProjectDiscovery
{
    /// <summary>
    /// Discovers the project or solution file to use for scanning.
    /// Checks for .slnx, .sln, then project files (.csproj, .fsproj, .vbproj) in the given directory.
    /// </summary>
    public static string[] Discover(string? explicitPath = null)
    {
        if (explicitPath is not null)
        {
            if (!File.Exists(explicitPath))
                throw new FileNotFoundException($"Project or solution file not found: {explicitPath}");
            return [explicitPath];
        }

        var directory = Directory.GetCurrentDirectory();

        // Prefer .slnx (modern default in .NET 10)
        var slnxFiles = Directory.GetFiles(directory, "*.slnx");
        if (slnxFiles.Length > 0)
            return slnxFiles;

        // Fall back to .sln
        var slnFiles = Directory.GetFiles(directory, "*.sln");
        if (slnFiles.Length > 0)
            return slnFiles;

        // Fall back to project files
        var projectFiles = new[] { "*.csproj", "*.fsproj", "*.vbproj" }
            .SelectMany(ext => Directory.GetFiles(directory, ext))
            .ToArray();
        if (projectFiles.Length > 0)
            return projectFiles;

        return [];
    }
}
