using System.CommandLine;
using NugetSkills.Commands;

var rootCommand = new RootCommand("Discover and load AI coding skills bundled with NuGet packages");

rootCommand.Add(InitCommand.Create());
rootCommand.Add(ScanCommand.Create());
rootCommand.Add(LoadCommand.Create());
rootCommand.Add(InfoCommand.Create());
rootCommand.Add(DoctorCommand.Create());

var parseResult = rootCommand.Parse(args);
return await parseResult.InvokeAsync();
