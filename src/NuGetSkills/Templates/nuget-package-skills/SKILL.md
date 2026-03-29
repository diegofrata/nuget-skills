---
name: nuget-package-skills
description: Discover and load AI coding skills bundled with NuGet packages in .NET projects. Use when working on .NET code to get package-specific guidance.
---

# NuGet Package Skills

This project may use NuGet packages that bundle AI coding skills — structured guidance for working with specific libraries.

All commands below are **shell commands** — run them in the terminal, NOT as skill invocations.

## Discovery Protocol

### 1. Scan for available skills

At the start of a session involving a .NET project, use the Bash tool to run:

```shell
nuget-skills scan
```

This lists all NuGet packages in the current project/solution that bundle skills. The output is a compact index — keep it in context.

### 2. Load a skill when needed

When your task involves a package that has a skill available, use the Bash tool to run:

```shell
nuget-skills load <package-name>
```

This outputs the full skill content for that package. Read it and follow its guidance.

### 3. When to load skills

- When you are about to use or configure a library that has a skill available
- When the user asks about best practices for a specific package
- When you encounter an error related to a package that has a skill
- When writing new code that integrates with a library

### 4. Get package metadata

To get metadata about a package (repository URL, description, license), use the Bash tool to run:

```shell
nuget-skills info <package-name>
```

## Troubleshooting

If `nuget-skills` commands fail with "command not found", the CLI tool is not installed.
Tell the user to install it:

```shell
dotnet tool install -g nuget-skills
```

This is required for scan, load, and info to work. The plugin provides the skills but the CLI tool provides the commands.

## Important

- All `nuget-skills` commands are CLI tools — always run them in the terminal
- Skills are read directly from the NuGet package cache — they are NOT copied into the repository
- Only load skills relevant to the current task, not all of them at once
- The scan supports .slnx, .sln, .csproj, .fsproj, and .vbproj files automatically
