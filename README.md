# NuGetSkills

A .NET CLI tool that discovers and loads AI coding skills bundled with NuGet packages. Works with Claude Code, Cursor, Copilot, Codex, Windsurf, Cline, and Goose.

Instead of copying skills into your repository, the tool reads them directly from the NuGet package cache or from the package's source repository on GitHub. Your AI agent gets library-specific guidance automatically.

## Why does it matter?

When you ask an AI coding agent for help with a NuGet package, it has no structured way to access that library's conventions, patterns, or pitfalls. It falls back on training data that may be outdated or generic.

**nuget-skills** fixes this by creating a distribution channel for library-specific knowledge through NuGet itself. Package authors ship a `skills/SKILL.md` alongside their code, and AI agents discover and load it automatically. The result: your agent knows the right way to use Serilog, Polly, or MediatR — not just the API surface, but the conventions, gotchas, and patterns that matter.

For the .NET ecosystem, this is the equivalent of what [npm-agentskills](https://github.com/onmax/npm-agentskills) and [vercel-labs/skills](https://github.com/vercel-labs/skills) do for JavaScript — but designed to work with NuGet's package cache instead of copying files into your repo.

## How it works

1. **`nuget-skills init`** installs a meta-skill and a session-start hook for your AI agent
2. When you start a session, the hook runs **`nuget-skills scan`** automatically
3. For each NuGet package, it checks three sources in order:
   - **Local** &mdash; `skills/SKILL.md` bundled inside the NuGet package
   - **Remote** &mdash; `skills/SKILL.md` in the package's GitHub repo (via `gh` CLI)
   - **README** &mdash; `README.md` from the NuGet package (fallback)
4. Your agent sees which packages have skills and loads them on demand with **`nuget-skills load`**

## Quick start

```shell
# Install the tool
dotnet tool install -g NuGetSkills

# Initialize in your project (auto-detects installed agents)
cd your-dotnet-project
nuget-skills init

# Or specify agents explicitly
nuget-skills init --agent claude,cursor

# Verify your setup
nuget-skills doctor
```

That's it. Your AI agent will now discover NuGet package skills on session start.

## Commands

### `nuget-skills init`

Installs skills and hooks for your AI coding agent(s).

```shell
nuget-skills init                          # Auto-detect agents
nuget-skills init --agent claude,cursor    # Specific agents
nuget-skills init --agent all              # All supported agents
nuget-skills init --global                 # Install to user-level (~/) instead of project
nuget-skills init --no-remote              # Disable remote skill discovery
nuget-skills init --no-readme              # Disable README fallback
```

**Auto-detection** checks for agent config directories (`.claude/`, `.cursor/`, etc.) in your project. If none are found, you must specify `--agent`.

### `nuget-skills scan`

Scans all NuGet packages in your project for available skills.

```shell
nuget-skills scan                    # Human-readable output
nuget-skills scan --json             # Machine-readable JSON
nuget-skills scan --project path.sln # Specific solution/project
nuget-skills scan --refresh          # Bypass cache, re-check remote repos
```

Output:

```
Scanning MyProject.sln...
Found 3 package(s) with skills (of 47 total):

  Serilog          3.1.1   [local]   Structured logging patterns
  Polly            8.3.0   [remote]  Resilience policies (github.com/App-vNext/Polly @ v8.3.0)
  Newtonsoft.Json  13.0.3  [readme]  JSON framework (from package README)

Use 'nuget-skills load <package>' to view a skill.
```

Skills are discovered from three sources:
- **[local]** &mdash; `skills/SKILL.md` bundled in the NuGet package
- **[remote]** &mdash; `skills/SKILL.md` found in the package's GitHub repo (requires `gh` CLI)
- **[readme]** &mdash; `README.md` from the NuGet package (fallback)

### `nuget-skills load <package>`

Outputs the full skill content for a package.

```shell
nuget-skills load Serilog                     # Auto-detect version from project
nuget-skills load Serilog --version 3.1.1     # Specific version
```

### `nuget-skills info <package>`

Outputs package metadata as JSON (repository URL, description, skill status).

```shell
nuget-skills info Generator.Equals
```

```json
{
  "id": "Generator.Equals",
  "version": "4.0.0",
  "description": "A source generator for generating Equals and GetHashCode methods",
  "repositoryUrl": "https://github.com/diegofrata/Generator.Equals",
  "hasSkills": true,
  "cachePath": "/Users/you/.nuget/packages/generator.equals/4.0.0"
}
```

### `nuget-skills doctor`

Validates your setup.

```
  ✓  dotnet CLI       10.0.100
  ✓  gh CLI           gh version 2.79.0
                      authenticated as yourusername
  ✓  NuGet cache      /Users/you/.nuget/packages/
  ✓  Skills cache     /Users/you/Library/Application Support/nuget-skills/cache (12 entries)

  Settings:
    Remote scan:     enabled
    README fallback: enabled
```

## Supported agents

| Agent | Skills format | Hooks | Detection |
|-------|--------------|-------|-----------|
| Claude Code | `.claude/skills/` SKILL.md | `.claude/settings.json` SessionStart | `.claude/` dir |
| Cursor | `.cursor/rules/` .mdc | `.cursor/hooks.json` sessionStart | `.cursor/` dir |
| GitHub Copilot | `.github/instructions/` .md | `.github/hooks/` SessionStart | `.github/copilot-instructions.md` |
| Codex / Amp | `.agents/skills/` SKILL.md | &mdash; | `.codex/` or `.agents/` dir |
| Windsurf | `.windsurf/skills/` SKILL.md | &mdash; | `.windsurf/` dir |
| Cline | `.cline/skills/` SKILL.md | &mdash; | `.cline/` dir |
| Goose | `.goose/skills/` SKILL.md | &mdash; | `.goose/` dir or `.goosehints` |

Agents with hooks get automatic scanning on session start. Agents without hooks rely on the meta-skill instructing the agent to run `nuget-skills scan` manually.

## For package authors

Ship skills with your NuGet package so AI agents can use them.

### 1. Create a skill

Add a `skills/SKILL.md` to your project:

````markdown
---
name: your-package
description: One-line summary of what this skill teaches
---

# Your Package

## Best Practices

- Do this, not that
- Configuration examples
- Common pitfalls

## Examples

```csharp
// Practical code examples
```
````

A package can include multiple skill files:

```
skills/
  SKILL.md              # Main skill
  CONFIGURATION.md      # Configuration guide
  MIGRATION.md          # Migration guide
```

### 2. Include in your package

Add this to your `.csproj` (or `.fsproj` / `.vbproj`):

```xml
<ItemGroup>
  <None Include="skills/**" Pack="true" PackagePath="skills/" />
</ItemGroup>
```

That's it. When users install your package, the skill is available immediately via `nuget-skills scan`.

### What makes a good skill

- **Actionable over informational** &mdash; tell the agent what to DO, not what the library IS
- **Conventions and gotchas** &mdash; what's not obvious from the API alone
- **Code over prose** &mdash; show patterns, don't just describe them
- **Concise** &mdash; agents have limited context; every line should earn its place

### Using the builder skill

Ask your AI agent: *"Build me a skill for Serilog"*

The built-in builder skill teaches the agent to research a package's docs, issues, and wiki, then generate a well-structured SKILL.md.

## Remote skill discovery

For packages that don't ship skills yet, the tool checks the package's source repository on GitHub for a `skills/SKILL.md`. This requires the `gh` CLI:

```shell
# Install gh: https://cli.github.com
gh auth login
```

The tool matches the installed package version to a git tag (handling `v1.0.0`, `release/1.0.0`, `PackageName.1.0.0` formats) and checks for skills at that tag, falling back to the default branch.

Remote results are cached per package version at:
- macOS: `~/Library/Application Support/nuget-skills/cache/`
- Linux: `~/.local/share/nuget-skills/cache/`
- Windows: `%LOCALAPPDATA%\nuget-skills\cache\`

Use `nuget-skills scan --refresh` to bypass the cache.

## Settings

Settings are stored alongside the cache. The file is only created when you explicitly opt out of features:

```shell
nuget-skills init --no-remote    # Disables remote scan
nuget-skills init --no-readme    # Disables README fallback
```

Or edit directly:

```json
{
  "enableRemoteScan": true,
  "enableReadmeFallback": true
}
```

## Claude Code plugin

For Claude Code users, the tool is also available as a plugin:

```
/plugin marketplace add diegofrata/nuget-skills
```

This installs the skills and hooks without needing `nuget-skills init --agent claude`.

## Project types

Supports all .NET project types:
- `.slnx` (default in .NET 10)
- `.sln`
- `.csproj`, `.fsproj`, `.vbproj`

