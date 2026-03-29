---
name: nuget-package-skills-builder
description: Build AI coding skills for NuGet packages by researching their documentation, issues, and best practices. Use when asked to create or author a skill for a package.
---

# NuGet Skills Builder

Use this guide when asked to create a skill for a NuGet package.

All `nuget-skills` commands below are **shell commands** — run them in the terminal, NOT as skill invocations.

## Step 1: Get Package Metadata

Use the Bash tool to run:

```shell
nuget-skills info <package-name>
```

This gives you the repository URL, description, and whether a skill already exists.

## Step 2: Research the Package

Using the repository URL from the info output:

1. **Read the README** — understand what the package does and its core concepts
2. **Check the docs/** directory or docs site** — look for configuration guides, best practices, migration guides
3. **Review recent issues** — identify common pitfalls, frequently asked questions, and gotchas
4. **Check the wiki** if available — often has detailed guides not in the README

Focus on what an AI coding agent needs to know to use the package correctly, not general documentation.

## Step 3: Write the SKILL.md

Create a file with this structure:

```markdown
---
name: <package-name-lowercase>
description: <one-line summary of what guidance this skill provides>
packages: <optional — comma-separated glob patterns to restrict which packages this skill applies to>
---

# <Package Name>

<Brief context on what the package does and when to use it>

## Configuration

<Common setup patterns, recommended defaults>

## Best Practices

<Do this, not that — concrete patterns with code examples>

## Common Pitfalls

<Mistakes that are easy to make, with corrections>

## Examples

<Short, practical code examples for the most common use cases>
```

## What Makes a Good Skill

- **Actionable over informational** — tell the agent what to DO, not just what the library IS
- **Conventions and gotchas** — what's not obvious from the API surface alone
- **Configuration examples** — the most common setups, not every possible option
- **Concise** — agents have limited context; every line should earn its place
- **Code over prose** — show patterns, don't just describe them

## Package Filtering

If a repository produces multiple NuGet packages but a skill only applies to some of them, use the `packages` frontmatter field:

```yaml
packages: Contoso.Http*, Contoso.Core
```

Each entry is a glob pattern (`*` wildcard) matched case-insensitively against package IDs. Omitting the field means the skill applies to all packages from the repository.

## Multiple Skills Per Package

A package can ship multiple skill files in the `skills/` directory:

```
skills/
  SKILL.md              # Main skill (always loaded first)
  CONFIGURATION.md      # Configuration-specific guidance
  MIGRATION.md          # Version migration guide
```

All `.md` files in the `skills/` directory are discovered and can be loaded.

## Step 4: Save the Skill

Offer the user these options:

1. **Save to the package source and configure shipping** (recommended):
   - Save the skill file(s) to `skills/SKILL.md` in the package's source repository
   - Also add the following to the package's project file (`.csproj`, `.fsproj`, or `.vbproj`) so the skill ships with the NuGet package:
     ```xml
     <ItemGroup>
       <None Include="skills/**" Pack="true" PackagePath="skills/" />
     </ItemGroup>
     ```
   - This ensures the skill is bundled in every future `.nupkg` build

2. **Save to NuGet cache** (immediate local use, not permanent):
   Save the file to `~/.nuget/packages/<package-id-lowercase>/<version>/skills/SKILL.md`
   This makes it immediately available via `nuget-skills scan` and `nuget-skills load`,
   but will be lost if the package cache is cleared or the package is reinstalled.

3. **Output to stdout** — just print it so the user can decide what to do with it.
