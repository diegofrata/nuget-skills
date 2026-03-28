using System.Xml.Linq;

namespace NuGetSkills.Services;

public record NuspecMetadata(
    string? Description,
    string? RepositoryUrl,
    string? RepositoryType,
    string? ProjectUrl,
    string? License);

public class NuspecParser
{
    public static NuspecMetadata? Parse(string packageCacheDir, string packageId)
    {
        var nuspecPath = Path.Combine(packageCacheDir, $"{packageId.ToLowerInvariant()}.nuspec");

        try
        {
            var doc = XDocument.Load(nuspecPath);
            var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
            var metadata = doc.Root?.Element(ns + "metadata");

            if (metadata is null)
                return null;

            var repoElement = metadata.Element(ns + "repository");

            return new NuspecMetadata(
                Description: metadata.Element(ns + "description")?.Value,
                RepositoryUrl: repoElement?.Attribute("url")?.Value,
                RepositoryType: repoElement?.Attribute("type")?.Value,
                ProjectUrl: metadata.Element(ns + "projectUrl")?.Value,
                License: metadata.Element(ns + "license")?.Value);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }
}
