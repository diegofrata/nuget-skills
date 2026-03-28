using System.CommandLine;
using NuGetSkills.Services;

namespace NuGetSkills.Commands;

public static class CacheClearCommand
{
    public static Command Create()
    {
        var packageArgument = new Argument<string?>("package")
        {
            Description = "Clear cache for a specific package (clears all if omitted)",
            Arity = ArgumentArity.ZeroOrOne,
        };

        var command = new Command("clear-cache", "Clear cached remote skill data");
        command.Add(packageArgument);

        command.SetAction((parseResult, _) =>
        {
            var package = parseResult.GetValue(packageArgument);
            Execute(package);
            return Task.CompletedTask;
        });

        return command;
    }

    private static void Execute(string? package)
    {
        if (package is not null)
        {
            var packageDir = Path.Combine(SkillCache.CacheDirectory, package.ToLowerInvariant());
            if (Directory.Exists(packageDir))
            {
                var count = Directory.GetFiles(packageDir, "*.json").Length;
                Directory.Delete(packageDir, true);
                Console.WriteLine($"Cleared {count} cached entry/entries for {package}.");
            }
            else
            {
                Console.WriteLine($"No cache entries found for {package}.");
            }
        }
        else
        {
            if (Directory.Exists(SkillCache.CacheDirectory))
            {
                var count = SkillCache.CountEntries();
                Directory.Delete(SkillCache.CacheDirectory, true);
                Console.WriteLine($"Cleared {count} cached entry/entries.");
            }
            else
            {
                Console.WriteLine("Cache is already empty.");
            }
        }
    }
}
