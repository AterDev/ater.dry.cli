# Perigon.CLI Architecture Reference

## Purpose

Perigon.CLI is a .NET development assistant distributed as the `perigon` dotnet tool. It supports command-line workflows, a Blazor Server Studio UI, MCP server integration, module packaging/install, OpenAPI client generation, and Roslyn/Razor based code generation for ASP.NET Core style solutions.

## Technology Stack

- .NET 10 (`net10.0`) with nullable reference types and implicit usings.
- Central package management via `Directory.Packages.props`.
- CLI: `Spectre.Console`, `Spectre.Console.Cli`, `Microsoft.Extensions.Hosting`, dependency injection, and console localization.
- Web UI: ASP.NET Core, Blazor Server interactive components, Fluent UI Blazor.
- MCP: `ModelContextProtocol.AspNetCore` in CoreMod.
- Code analysis/generation: Roslyn (`Microsoft.CodeAnalysis.*`), `Microsoft.OpenApi`, `RazorEngineCore`, generated templates, and helper models.
- Mapping and utilities: Mapster, Humanizer, Perigon.MiniDb, shared helper classes.
- Tests: xUnit v3, Moq, `Microsoft.NET.Test.Sdk`, coverlet collector.

## Project Map

- `src/Apps/CommandLine`: the packaged dotnet tool. Registers services and Spectre commands in `Program.cs`.
- `src/Apps/Dashboard`: Studio web server. Uses Blazor Server, Fluent UI, localization middleware, static assets, and scoped managers/services.
- `src/Modules/CoreMod`: primary business module. Contains service orchestration, managers, models, MCP tools, module install/package flows, and code generation workflows.
- `src/CodeGenerator`: Roslyn/OpenAPI/Razor generation engine and templates.
- `src/Share`: shared entities, DTO support, helpers, constants, MiniDb context, `SolutionContext`, localization resources, and framework registration.
- `tests/StudioMod.Tests`: behavior tests for commands, managers, services, helpers, and generation.

## Layering Habits

- App projects depend on CoreMod. CoreMod depends on CodeGenerator and Share. Share remains foundational.
- Command classes should not own business logic. They validate arguments, set context, call services, and format output.
- Dashboard components should not own business logic. They use managers/services and Fluent UI components.
- CoreMod services orchestrate workflows such as project analysis, module package/install, official module download, codegen, and command-facing operations.
- CoreMod managers encapsulate persistence/query behavior over MiniDb `DefaultDbContext` and `ManagerBase`.
- CodeGenerator owns code parsing and emitted source content. Prefer Roslyn/OpenAPI/Razor APIs there.
- Share owns cross-project concepts and should avoid dependencies on app-specific concerns.

## Localization

- Supported cultures are `zh-CN` and `en-US`.
- CLI culture follows `DOTNET_CLI_UI_LANGUAGE` when set, otherwise current UI culture.
- Dashboard culture uses a cookie first, then `Accept-Language`.
- `Localizer` wraps `IStringLocalizer<Localizer>`.
- Razor should use `Localizer.<Key>` generated constants via `@Localizer.Get(Localizer.Key)`.
- Add user-facing strings to both `.resx` files.

## Verification Policy

The repository guidance says not to build unless explicitly requested. For most edits, use IDE diagnostics and targeted tests only when behavior changes justify them. Metadata-only changes in `.agents`, `.github`, or docs normally need frontmatter/path validation rather than a full build.
