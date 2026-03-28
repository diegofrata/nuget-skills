using System.CommandLine;
using System.Reflection;
using NuGetSkills.Commands;

var rootCommand = new RootCommand("Discover and load AI coding skills bundled with NuGet packages");

rootCommand.Add(InitCommand.Create());
rootCommand.Add(ScanCommand.Create());
rootCommand.Add(LoadCommand.Create());
rootCommand.Add(InfoCommand.Create());
rootCommand.Add(DoctorCommand.Create());

var parseResult = rootCommand.Parse(args);

// Print header for interactive commands (not for load/info which output machine-readable data, or --help/--version)
var commandName = parseResult.CommandResult.Command.Name;
var quietCommands = new[] { "load", "info" };
var hasJsonFlag = args.Contains("--json");
if (!quietCommands.Contains(commandName) && !hasJsonFlag)
{
    var version = Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "dev";
    Console.WriteLine($"nuget-skills {version}");
    Console.WriteLine();
}

return await parseResult.InvokeAsync();
