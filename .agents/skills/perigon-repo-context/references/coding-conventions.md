# Perigon.CLI Coding Conventions

## General C#

- Use file-scoped namespaces.
- Match nearby primary-constructor style for commands, services, and managers.
- Keep nullable reference type behavior explicit; avoid suppressions unless the invariant is clear.
- Use descriptive names and keep methods focused.
- Prefer existing helpers and constants from `Share` before adding new utility code.
- Use `ConstVal.DefaultJsonSerializerOptions` for project JSON behavior unless a local serializer option is required.
- Use `Mapster` mapping helpers (`MapTo`, `Merge`, `Adapt`) where DTO/entity mapping follows existing patterns.
- Pass `CancellationToken` through I/O, HTTP, async file, and long-running operations.

## CLI Commands

- Commands live in `src/Apps/CommandLine/Commands`.
- Inherit from `AsyncCommand<TSettings>` or `AsyncCommand`.
- Settings inherit `CommandSettings` and use `CommandArgument`, `CommandOption`, and `Description`.
- Register new commands, branches, aliases, examples, and localized descriptions in `src/Apps/CommandLine/Program.cs`.
- Use `CommandSolutionHelper.TrySetSolutionAsync` when command behavior needs a current solution context.
- Use `OutputHelper.Error`, `OutputHelper.Warning`, `OutputHelper.Info`, `OutputHelper.Success`, or `AnsiConsole` for terminal output.
- Escape dynamic Spectre table values with `Markup.Escape`.
- Return `0` for success, `1` for expected validation or operation failures.
- Preserve MCP stdio safety by avoiding non-protocol console output for raw MCP `init`/`start` paths.

## CoreMod Services And Managers

- Put cross-step workflows and external I/O in `Services`.
- Put MiniDb query/update behavior in `Managers` and derive from `ManagerBase<DefaultDbContext, TEntity>` where appropriate.
- Managers commonly expose `CreateNewEntityAsync`, `UpdateAsync(entity, dto)`, `ToPageAsync(filter)`, `GetDetailAsync`, `IsUniqueAsync`, and delete/ownership helpers.
- For HTTP services, inject `HttpClient` through an internal/test constructor when testability is needed.
- Log exceptions with structured logging before wrapping them in localized, user-friendly exceptions.

## CodeGenerator

- Keep parsing and model extraction in helpers; keep emitted code composition in generator classes or templates.
- Prefer raw string literals for multi-line generated source/XML project templates.
- Return global using lists from generator methods when creating generated projects/files.
- Use Roslyn syntax APIs for C# structure and `Microsoft.OpenApi` APIs for OpenAPI processing.
- Avoid brittle string parsing when a structured API is available.

## Dashboard / Razor

- Use Fluent UI Blazor components and icons consistently.
- Keep Razor UI composition separate from business workflows.
- Use localized labels through `@Localizer.Get(Localizer.Key)`.
- Register UI services in `ServiceCollectionExtension` or `Program.cs` following existing patterns.
- Preserve providers in the layout: message bars, toasts, dialogs, tooltips, menus, and design theme.

## Tests

- Use xUnit v3 `[Fact]`, `[Theory]`, and `[InlineData]`.
- Use Moq for localizer or collaborator mocks when simple stubs are not enough.
- Use stub `HttpMessageHandler` for HTTP tests; do not hit live GitHub/API endpoints.
- Use unique temp directories and restore current directory in `finally` blocks for file-system tests.
- Keep tests focused on observable behavior and regression risk.
