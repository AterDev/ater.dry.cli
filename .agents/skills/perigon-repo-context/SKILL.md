---
name: perigon-repo-context
description: "Use when working in Perigon.CLI on .NET CLI commands, Blazor Studio UI, CoreMod services/managers, CodeGenerator/Roslyn/OpenAPI/Razor templates, MCP server behavior, module packaging/install, localization, or tests. Loads project architecture, layering, style, and verification guidance."
argument-hint: "task or area in Perigon.CLI"
---

# Perigon Repo Context

## When To Use

Use this skill before implementing or reviewing changes in this repository, especially when the task touches:

- CLI commands under `src/Apps/CommandLine`
- Studio UI under `src/Apps/Dashboard`
- Core workflows under `src/Modules/CoreMod`
- generation logic under `src/CodeGenerator`
- shared entities, helpers, localizer, or context under `src/Share`
- tests under `tests/StudioMod.Tests`

## Procedure

1. Load [architecture](./references/architecture.md) for the repository map, dependency direction, and technology stack.
2. Load [coding conventions](./references/coding-conventions.md) for local implementation patterns.
3. Identify the owning layer before editing. Keep command/UI code thin and put reusable behavior in CoreMod, CodeGenerator, or Share as appropriate.
4. Check localization impact for user-facing text. Add or update both `Localizer.zh-CN.resx` and `Localizer.en-US.resx` when needed.
5. Add or update focused tests for behavior changes in commands, module flows, GitHub/API access, JSON config, code generation, or managers.
6. Prefer IDE diagnostics for validation. Do not run a full build unless the user asks or the change is risky enough to justify it.

## Output Expectations

When reporting back, include:

- the layer or files changed
- any localization or tests added
- validation performed or intentionally skipped
- notable assumptions about architecture or behavior
