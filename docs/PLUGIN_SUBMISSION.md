# Plugin Submission Guide

How to submit NuGetSkills as a plugin/skill to each AI coding agent's official ecosystem.

## Universal Distribution (all agents)

The primary distribution mechanism for all agents is the dotnet tool:

```bash
dotnet tool install -g NuGetSkills
nuget-skills init --agent <agent-name>
```

This works for every supported agent. The platform-specific submissions below are supplementary — they make discovery easier but are not required.

---

## Claude Code — Plugin Marketplace

**Status**: Ready to submit. Repo already has plugin structure.

**Files in place**:
- `.claude-plugin/plugin.json` — plugin manifest
- `src/NuGetSkills/Templates/` — SKILL.md files for both skills
- `hooks/hooks.json` — SessionStart hook

**Submission options**:

1. **Independent marketplace** (recommended for now):
   - Users add via: `/plugin marketplace add diegofrata/nuget-skills`
   - No approval process needed — any GitHub repo with `.claude-plugin/` works

2. **Official marketplace** (anthropics/claude-plugins-official):
   - Submit a PR to `anthropics/claude-plugins-official` adding an entry to `marketplace.json`
   - Requires Anthropic review and approval

**Docs**: https://code.claude.com/docs/en/plugin-marketplaces

---

## Cursor — Marketplace

**Steps**:
1. Create a Cursor plugin manifest following the Cursor plugin spec
2. Bundle the `.mdc` rules and `hooks.json`
3. Apply to the Cursor Marketplace partner program at `cursor.com/marketplace`
4. Alternatively, share as a community plugin via `/add-plugin` in Cursor

**Docs**: https://cursor.com/docs/reference/plugins

---

## OpenAI Codex — Skills Catalog

**Steps**:
1. Submit a PR to `openai/skills` GitHub repository
2. Create a directory: `skills/nuget-package-skills/`
3. Include `SKILL.md` with the meta-skill content

**Docs**: https://developers.openai.com/codex/skills

---

## GitHub Copilot

No plugin marketplace. Distribute via `nuget-skills init --agent copilot`.

---

## Windsurf

No plugin marketplace. Distribute via `nuget-skills init --agent windsurf`.

---

## Cline

No plugin marketplace. Distribute via `nuget-skills init --agent cline`.

---

## Amp (Sourcegraph)

Covered by Codex provider (shared `.agents/skills/` path). Distribute via `nuget-skills init --agent codex`.

---

## Goose (Block)

No plugin marketplace. Distribute via `nuget-skills init --agent goose`.

---

## Priority order for submissions

1. **Claude Code** — already structured as plugin, submit immediately
2. **Codex** — submit PR to `openai/skills`, standard format
3. **Cursor** — apply to marketplace when ready
4. All others — dotnet tool is the distribution mechanism
