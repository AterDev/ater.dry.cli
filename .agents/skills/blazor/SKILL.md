---
name: blazor
description: "Use for Blazor Server development with Fluent UI Blazor, Razor components, layouts, pages, scoped UI services, culture switching, localization, forms, loading/empty/error states, and Studio-style dashboard workflows."
---

# Blazor And Fluent UI Development

## Workflow

1. Identify the page, component, layout, or service under `src/Apps/Dashboard`.
2. Read nearby Razor components and code-behind for local patterns.
3. Check `Program.cs` and `ServiceCollectionExtension.cs` before changing service registration or middleware.
4. Read the CoreMod service/manager before putting workflow logic in UI code.

## Conventions

- Keep Razor components focused on UI composition and user interaction.
- Put reusable workflows in CoreMod services or managers.
- Use Fluent UI Blazor components and icons consistently with surrounding pages.
- Preserve app-level providers for message bars, toasts, dialogs, tooltips, menus, and theme.
- Use scoped managers/services registered through the app startup path; avoid ad hoc service construction.
- Make loading, empty, success, and error states explicit for data and form flows.
- Keep UI text product-facing, not implementation-facing.

## Localization And Validation

- Use `@Localizer.Get(Localizer.Key)` for user-facing labels.
- Add new strings to both `src/Share/Localizer.zh-CN.resx` and `src/Share/Localizer.en-US.resx`.
- Prefer testing workflow behavior at the CoreMod service/manager layer. Run the Dashboard manually only when the UI change is risky or the user asks.
