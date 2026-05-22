---
description: "Use when editing Perigon.CLI C#, Razor, csproj, tests, CLI commands, CoreMod services/managers, CodeGenerator logic, Dashboard Blazor UI, localization, MCP, or module packaging/install code."
applyTo: "src/**/*.cs"
---
# Perigon.CLI Architecture And Style

## Repository Shape

- The solution is a .NET 10 multi-project CLI/tooling repository.
- `src/Apps/CommandLine` is the `perigon` dotnet tool entry point. It uses `Spectre.Console.Cli`, `Host.CreateApplicationBuilder`, DI, `OutputHelper`, and localized command descriptions.
- `src/Apps/Dashboard` is the Studio Web UI built with ASP.NET Core, Blazor Server interactive components, and `Microsoft.FluentUI.AspNetCore.Components`.
- `src/Modules/CoreMod` contains the main business workflows. `Services` orchestrate CLI/codegen/module operations; `Managers` wrap MiniDb persistence and query/update behavior.
- `src/CodeGenerator` contains Roslyn/OpenAPI/Razor template based generation logic.
- `src/Share` contains shared entities, models, helpers, constants, resource localization, context, and common framework registration.
- `tests/StudioMod.Tests` uses xUnit v3 and Moq for focused behavior tests.

## Dependency And Layering Rules

- Keep `CommandLine` thin: parse/validate command settings, initialize `SolutionContext` through `CommandSolutionHelper`, call CoreMod services, and return exit codes.
- Keep `Dashboard` UI thin: inject managers/services, use Fluent UI components, put reusable UI in `Components/Shared`, and avoid duplicating business logic in Razor components.
- Put business orchestration in `CoreMod.Services` and persistence/query behavior in `CoreMod.Managers`.
- Put reusable models, entities, helpers, constants, localization resources, and framework extension methods in `Share`.
- Put parsing/generation/template logic in `CodeGenerator`; prefer Roslyn/OpenAPI/Razor APIs over ad hoc string parsing when those APIs are available.
- Do not introduce reverse dependencies from `Share` or `CodeGenerator` back into apps.

## C# Style

- Use nullable reference types and implicit/global usings already configured by each project.
- Match the existing file-scoped namespace style.
- Primary constructors are common for services/managers/commands; use them when consistent with nearby code.
- Prefer explicit, descriptive names over abbreviations. Do not introduce one-letter variables.
- Async methods should accept/pass `CancellationToken` for I/O or external calls when the surrounding code does.
- Use `ConstVal.DefaultJsonSerializerOptions` for JSON behavior unless a local override is justified.
- Use `Mapster` methods such as `MapTo`, `Merge`, and `Adapt` where the codebase already maps DTOs/entities that way.
- Keep comments concise. Existing code has XML comments, often Chinese; add comments only when they clarify non-obvious behavior.

## CLI Conventions

- Commands inherit from `AsyncCommand<TSettings>` or `AsyncCommand`.
- Settings classes inherit `CommandSettings` and use `CommandArgument`, `CommandOption`, and `Description` attributes.
- Register commands and aliases in `src/Apps/CommandLine/Program.cs` with localized descriptions from `Localizer.<Key>`.
- Output user-facing messages through `OutputHelper` or `AnsiConsole` tables; escape table cell values with `Markup.Escape`.
- Return `0` for success and `1` for expected command failures.
- For MCP stdio-sensitive paths, preserve protocol-safe startup behavior and avoid early console output.

## Dashboard And Razor Conventions

- Use Fluent UI Blazor components and Fluent icons instead of raw HTML controls when a Fluent component exists.
- Use the existing localization service in Razor: `@Localizer.Get(Localizer.Key)`.
- For new user-facing strings, add both `Localizer.zh-CN.resx` and `Localizer.en-US.resx`, then use the generated `Localizer.<Key>` constant.
- Preserve the current localization flow: cookie culture first, then `Accept-Language`, supported cultures `zh-CN` and `en-US`.

## Testing Conventions

- Add tests when behavior changes in command parsing, module install/package flows, GitHub/API calls, code generation, JSON manipulation, or manager query behavior.
- Use xUnit v3 attributes (`[Fact]`, `[Theory]`, `[InlineData]`) and Moq where existing tests do.
- For HTTP behavior, prefer stub `HttpMessageHandler` tests over live network calls.
- For file-system tests, create unique temp directories and clean them in `finally`.

## Verification

- Follow the repository instruction: do not build the whole project unless explicitly requested.
- Prefer IDE diagnostics for quick validation.
- For metadata-only changes under `.agents`, `.github`, or docs, frontmatter/path validation is usually enough.
