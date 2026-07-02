---
name: dotnet
description: "Use for .NET application development involving services, managers, dependency injection, Microsoft.Extensions.Hosting, dependency direction, MiniDb persistence, DTOs, Mapster mapping, shared entities/helpers, MCP tools, module workflows, solution analysis, cancellation, logging, and logic shared by CLI and Blazor."
---

# .NET Application Development

## Workflow

1. Identify whether the change belongs in an app surface, service, manager, MCP tool, DTO/model, CodeGenerator, or Share.
2. Read the calling surface from CLI or Blazor before changing shared behavior.
3. Check shared entities and `DefaultDbContext` before changing persistence.
4. Preserve dependency direction: app projects depend on CoreMod; CoreMod depends on CodeGenerator and Share; Share stays foundational.

## Conventions

- Put cross-step workflows and external I/O in services.
- Put MiniDb query/update behavior in managers.
- Keep app-facing service methods stable when both CLI and Blazor consume them.
- Use existing DTO patterns and Mapster helpers (`MapTo`, `Merge`, `Adapt`) where nearby code does.
- Pass `CancellationToken` through file, HTTP, and long-running async operations.
- Make HTTP services testable with injectable clients or handlers.
- Log exceptions with structured logging before wrapping them in localized, user-friendly errors.
- Keep MCP tools as protocol surfaces: stable parameter names, returned shapes, and no console output.

## Tests

- Add tests under `tests/StudioMod.Tests/Services`, `Managers`, or `Commands` depending on the observable behavior.
- Stub HTTP and isolate filesystem/MiniDb state.
