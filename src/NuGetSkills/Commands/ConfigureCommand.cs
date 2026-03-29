using System.CommandLine;
using NuGetSkills.Models;
using NuGetSkills.Services;

namespace NuGetSkills.Commands;

public static class ConfigureCommand
{
    public static Command Create()
    {
        var projectOption = new Option<string?>("--project", "-p") { Description = "Path to a solution or project file" };
        var resetOption = new Option<bool>("--reset") { Description = "Remove the project configuration file" };
        var listOption = new Option<bool>("--list") { Description = "Show current configuration without modifying" };
        var addOption = new Option<string?>("--add") { Description = "Add a package to the whitelist (non-interactive)" };
        var removeOption = new Option<string?>("--remove") { Description = "Remove a package from the whitelist (non-interactive)" };

        var command = new Command("configure", "Configure which package skills to load for this project");
        command.Add(projectOption);
        command.Add(resetOption);
        command.Add(listOption);
        command.Add(addOption);
        command.Add(removeOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var project = parseResult.GetValue(projectOption);
            var reset = parseResult.GetValue(resetOption);
            var list = parseResult.GetValue(listOption);
            var add = parseResult.GetValue(addOption);
            var remove = parseResult.GetValue(removeOption);
            await ExecuteAsync(project, reset, list, add, remove, cancellationToken);
        });

        return command;
    }

    private static async Task ExecuteAsync(
        string? project, bool reset, bool list, string? add, string? remove,
        CancellationToken cancellationToken)
    {
        var configDir = ProjectConfigService.GetConfigDirectory(project);
        var configPath = Path.Combine(configDir, Constants.ProjectConfigFileName);

        if (reset)
        {
            ExecuteReset(configPath);
            return;
        }

        if (list)
        {
            ExecuteList(configPath);
            return;
        }

        if (add is not null)
        {
            ExecuteAdd(configPath, add);
            return;
        }

        if (remove is not null)
        {
            ExecuteRemove(configPath, remove);
            return;
        }

        await ExecuteInteractiveAsync(project, configPath, cancellationToken);
    }

    private static void ExecuteReset(string configPath)
    {
        if (File.Exists(configPath))
        {
            File.Delete(configPath);
            Console.WriteLine($"Removed {Constants.ProjectConfigFileName}");
        }
        else
        {
            Console.WriteLine("No configuration file found.");
        }
    }

    private static void ExecuteList(string configPath)
    {
        var config = ProjectConfigService.Load(configPath);
        if (config is null)
        {
            Console.WriteLine("No configuration file found. All discovered skills will be loaded.");
            return;
        }

        if (config.Packages.Length == 0)
        {
            Console.WriteLine("Configuration file exists but no packages are whitelisted.");
            return;
        }

        Console.WriteLine($"Configured packages ({config.Packages.Length}):");
        Console.WriteLine();
        foreach (var pkg in config.Packages)
            Console.WriteLine($"  {pkg}");
    }

    private static void ExecuteAdd(string configPath, string packageId)
    {
        var existing = ProjectConfigService.Load(configPath);
        var packages = new HashSet<string>(
            existing?.Packages ?? [], StringComparer.OrdinalIgnoreCase);

        if (!packages.Add(packageId))
        {
            Console.WriteLine($"{packageId} is already configured.");
            return;
        }

        ProjectConfigService.Save(configPath, new ProjectConfig([.. packages]));
        Console.WriteLine($"Added {packageId} to {Constants.ProjectConfigFileName}");
    }

    private static void ExecuteRemove(string configPath, string packageId)
    {
        var config = ProjectConfigService.Load(configPath);
        if (config is null)
        {
            Console.WriteLine("No configuration file found.");
            return;
        }

        var packages = new HashSet<string>(config.Packages, StringComparer.OrdinalIgnoreCase);
        if (!packages.Remove(packageId))
        {
            Console.WriteLine($"{packageId} is not in the configuration.");
            return;
        }

        ProjectConfigService.Save(configPath, new ProjectConfig([.. packages]));
        Console.WriteLine($"Removed {packageId} from {Constants.ProjectConfigFileName}");
    }

    private static async Task ExecuteInteractiveAsync(
        string? project, string configPath, CancellationToken cancellationToken)
    {
        if (Console.IsInputRedirected || !IsInteractiveTerminal())
        {
            Console.Error.WriteLine("Interactive mode requires a terminal. Use --add/--remove for non-interactive use.");
            Environment.Exit(1);
            return;
        }

        var allPackages = await ScanForPackagesWithSkillsAsync(project, cancellationToken);

        if (allPackages.Length == 0)
        {
            Console.WriteLine("No packages with skills found. Nothing to configure.");
            return;
        }

        var existingConfig = ProjectConfigService.Load(configPath);
        var selected = new HashSet<string>(
            existingConfig?.Packages ?? [], StringComparer.OrdinalIgnoreCase);

        // If no existing config, start with all selected
        if (existingConfig is null)
        {
            foreach (var pkg in allPackages)
                selected.Add(pkg.PackageId);
        }

        if (!RunInteractiveLoop(allPackages, selected))
            return;

        ProjectConfigService.Save(configPath, new ProjectConfig([.. selected]));
        Console.WriteLine();
        Console.WriteLine($"Saved {selected.Count} package(s) to {Constants.ProjectConfigFileName}");
    }

    /// <returns>true if the user chose to save, false if cancelled.</returns>
    internal static bool RunInteractiveLoop(PackageSkillInfo[] packages, HashSet<string> selected)
    {
        while (true)
        {
            PrintSelectionList(packages, selected);
            Console.WriteLine();
            Console.WriteLine("  Toggle: enter number(s) separated by spaces");
            Console.WriteLine("  Commands: [a]ll  [n]one  [s]ave  [q]uit");
            Console.Write("> ");

            var input = Console.ReadLine()?.Trim();
            if (input is null or "q")
            {
                Console.WriteLine("Cancelled.");
                return false;
            }

            if (input is "s")
                return true;

            if (input is "a")
            {
                foreach (var pkg in packages)
                    selected.Add(pkg.PackageId);
                continue;
            }

            if (input is "n")
            {
                selected.Clear();
                continue;
            }

            foreach (var token in input.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(token, out var num) && num >= 1 && num <= packages.Length)
                {
                    var packageId = packages[num - 1].PackageId;
                    if (!selected.Remove(packageId))
                        selected.Add(packageId);
                }
            }
        }
    }

    private static void PrintSelectionList(PackageSkillInfo[] packages, HashSet<string> selected)
    {
        Console.WriteLine();
        var maxIdLen = packages.Max(p => p.PackageId.Length);

        for (var i = 0; i < packages.Length; i++)
        {
            var pkg = packages[i];
            var check = selected.Contains(pkg.PackageId) ? "x" : " ";
            var source = pkg.Source switch
            {
                SkillSource.Local => "(local) ",
                SkillSource.Remote => "(remote)",
                _ => "(??????)",
            };
            var desc = pkg.Description ?? "";
            Console.WriteLine($"  [{check}] {i + 1,2}. {pkg.PackageId.PadRight(maxIdLen)}  {source}  {desc}");
        }
    }

    private static async Task<PackageSkillInfo[]> ScanForPackagesWithSkillsAsync(
        string? project, CancellationToken cancellationToken)
    {
        var scanner = await ScannerFactory.CreateAsync();

        var targets = ProjectDiscovery.Discover(project);
        if (targets.Length == 0)
        {
            Console.WriteLine("No solution or project files found. Use --project to specify a path.");
            return [];
        }

        var allPackages = new List<PackageSkillInfo>();
        foreach (var target in targets)
        {
            var result = await scanner.ScanAsync(target, cancellationToken: cancellationToken);
            allPackages.AddRange(result.PackagesWithSkills);
        }

        return allPackages
            .GroupBy(p => p.PackageId, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(p => p.PackageId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool IsInteractiveTerminal()
    {
        try
        {
            _ = Console.WindowHeight;
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }
}
