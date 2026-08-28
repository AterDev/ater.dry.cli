using CoreMod.Services;
using Share;
using Spectre.Console.Rendering;

namespace CommandLine.Commands;

/// <summary>
/// Tracks the focused file and the files selected for a template update.
/// </summary>
public sealed class TemplateUpdateSelectionState
{
    private readonly bool[] _selected;

    public TemplateUpdateSelectionState(IReadOnlyList<TemplateFileChange> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);
        if (changes.Count == 0)
        {
            throw new ArgumentException("At least one template change is required.", nameof(changes));
        }

        Changes = changes.ToArray();
        _selected = new bool[Changes.Count];
    }

    public IReadOnlyList<TemplateFileChange> Changes { get; }

    public int CurrentIndex { get; private set; }

    public int SelectedCount => _selected.Count(selected => selected);

    public bool IsSelected(int index)
    {
        ValidateIndex(index);
        return _selected[index];
    }

    public void MovePrevious()
    {
        CurrentIndex = Math.Max(0, CurrentIndex - 1);
    }

    public void MoveNext()
    {
        CurrentIndex = Math.Min(Changes.Count - 1, CurrentIndex + 1);
    }

    public void MoveFirst()
    {
        CurrentIndex = 0;
    }

    public void MoveLast()
    {
        CurrentIndex = Changes.Count - 1;
    }

    public void ToggleCurrent()
    {
        _selected[CurrentIndex] = !_selected[CurrentIndex];
    }

    public IReadOnlyList<TemplateFileChange> GetSelectedChanges()
    {
        return Changes.Where((_, index) => _selected[index]).ToArray();
    }

    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= Changes.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}

internal sealed record TemplateUpdateSelectionResult(
    bool ShouldApply,
    IReadOnlyList<TemplateFileChange> Changes
);

internal static class TemplateUpdateSelector
{
    private const int FooterHeight = 4;
    private const int PanelOverhead = 6;
    private const int ScrollMarkerRows = 2;

    public static async Task<TemplateUpdateSelectionResult> SelectAsync(
        IAnsiConsole console,
        IReadOnlyList<TemplateFileChange> changes,
        Localizer localizer,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(changes);
        ArgumentNullException.ThrowIfNull(localizer);
        if (!console.Profile.Capabilities.Interactive)
        {
            throw new InvalidOperationException(localizer.Get(Localizer.UpdateInteractiveRequired));
        }

        var state = new TemplateUpdateSelectionState(changes);
        string? validationMessage = null;
        var diffScrollOffset = 0;
        var layout = CreateLayout(console, state, localizer, validationMessage, diffScrollOffset);

        return await console
            .Live(layout)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Crop)
            .Cropping(VerticalOverflowCropping.Top)
            .StartAsync(async context =>
            {
                // LiveDisplay does not render its initial target before invoking the callback.
                // Refresh here so the selector is visible while it waits for the first key.
                context.Refresh();

                while (true)
                {
                    var key = await console.Input.ReadKeyAsync(
                        intercept: true,
                        cancellationToken: cancellationToken
                    );
                    if (key is null)
                    {
                        return new TemplateUpdateSelectionResult(false, []);
                    }

                    if (key.Value.Modifiers.HasFlag(ConsoleModifiers.Control)
                        && key.Value.Key == ConsoleKey.C)
                    {
                        return new TemplateUpdateSelectionResult(false, []);
                    }

                    switch (key.Value.Key)
                    {
                        case ConsoleKey.UpArrow:
                            state.MovePrevious();
                            diffScrollOffset = 0;
                            validationMessage = null;
                            break;
                        case ConsoleKey.DownArrow:
                            state.MoveNext();
                            diffScrollOffset = 0;
                            validationMessage = null;
                            break;
                        case ConsoleKey.Home:
                            state.MoveFirst();
                            diffScrollOffset = 0;
                            validationMessage = null;
                            break;
                        case ConsoleKey.End:
                            state.MoveLast();
                            diffScrollOffset = 0;
                            validationMessage = null;
                            break;
                        case ConsoleKey.PageUp:
                            diffScrollOffset = ScrollDiff(
                                console,
                                state.Changes[state.CurrentIndex],
                                diffScrollOffset,
                                -1
                            );
                            validationMessage = null;
                            break;
                        case ConsoleKey.PageDown:
                            diffScrollOffset = ScrollDiff(
                                console,
                                state.Changes[state.CurrentIndex],
                                diffScrollOffset,
                                1
                            );
                            validationMessage = null;
                            break;
                        case ConsoleKey.Spacebar:
                        case ConsoleKey.Packet:
                            state.ToggleCurrent();
                            validationMessage = null;
                            break;
                        case ConsoleKey.Enter:
                            var selectedChanges = state.GetSelectedChanges();
                            if (selectedChanges.Count == 0)
                            {
                                validationMessage = localizer.Get(Localizer.UpdateSelectAtLeastOne);
                                break;
                            }

                            return new TemplateUpdateSelectionResult(true, selectedChanges);
                        case ConsoleKey.Escape:
                            return new TemplateUpdateSelectionResult(false, []);
                        }

                    context.UpdateTarget(
                        CreateLayout(
                            console,
                            state,
                            localizer,
                            validationMessage,
                            diffScrollOffset
                        )
                    );
                }
            });
    }

    private static Layout CreateLayout(
        IAnsiConsole console,
        TemplateUpdateSelectionState state,
        Localizer localizer,
        string? validationMessage,
        int diffScrollOffset
    )
    {
        var body = new Layout("body").SplitColumns(
            new Layout("files")
                .Ratio(1)
                .MinimumSize(30)
                .Update(CreateFilePanel(console, state, localizer)),
            new Layout("diff")
                .Ratio(3)
                .MinimumSize(50)
                .Update(
                    CreateDiffPanel(
                        console,
                        state.Changes[state.CurrentIndex],
                        localizer,
                        diffScrollOffset
                    )
                )
        );

        var footerText = validationMessage
            ?? localizer.Get(Localizer.UpdateSelectionSummary, state.SelectedCount, state.Changes.Count);
        var footer = new Panel(
                new Markup(
                    $"[yellow]{Markup.Escape(footerText)}[/]\n"
                    + $"[grey]{Markup.Escape(localizer.Get(Localizer.UpdateSelectionHelp))}[/]"
                )
            )
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey);

        return new Layout("root").SplitRows(
            body,
            new Layout("footer").Size(FooterHeight).Update(footer)
        );
    }

    private static Panel CreateFilePanel(
        IAnsiConsole console,
        TemplateUpdateSelectionState state,
        Localizer localizer
    )
    {
        var availableRows = GetPanelContentRows(console);
        var visibleRows = GetVisibleRows(
            state.Changes.Count,
            availableRows,
            out var showScrollMarkers
        );
        var start = Math.Clamp(
            state.CurrentIndex - visibleRows / 2,
            0,
            Math.Max(0, state.Changes.Count - visibleRows)
        );
        var end = Math.Min(state.Changes.Count, start + visibleRows);
        var rows = new List<IRenderable>();

        if (showScrollMarkers && start > 0)
        {
            rows.Add(new Markup("[grey]  ▲[/]"));
        }

        for (var index = start; index < end; index++)
        {
            rows.Add(new Markup(RenderFileRow(state, index, localizer)));
        }

        if (showScrollMarkers && end < state.Changes.Count)
        {
            rows.Add(new Markup("[grey]  ▼[/]"));
        }

        return new Panel(new Rows(rows))
            .Header(
                $"{localizer.Get(Localizer.UpdateSelectionTitle)} "
                + $"[grey]({start + 1}-{end}/{state.Changes.Count})[/]"
            )
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey);
    }

    private static string RenderFileRow(
        TemplateUpdateSelectionState state,
        int index,
        Localizer localizer
    )
    {
        var change = state.Changes[index];
        var isCurrent = state.CurrentIndex == index;
        var cursor = isCurrent ? "[yellow]>[/]" : " ";
        var checkbox = state.IsSelected(index) ? "[green][[x]][/]" : "[grey][[ ]][/]";
        var kind = change.IsNew ? "[green]+[/]" : "[yellow]~[/]";
        var path = Markup.Escape(change.RelativePath);
        var count = change.IsBinary
            ? $"[grey]{Markup.Escape(localizer.Get(Localizer.UpdateBinaryChange))}[/]"
            : $"[green]+{change.DiffStats.AddedLines}[/] [red]-{change.DiffStats.RemovedLines}[/]";
        var row = $"{cursor} {checkbox} {kind} {path} ({count})";

        return isCurrent ? $"[bold]{row}[/]" : row;
    }

    private static Panel CreateDiffPanel(
        IAnsiConsole console,
        TemplateFileChange change,
        Localizer localizer,
        int diffScrollOffset
    )
    {
        var lines = GetDiffLines(change);
        var availableRows = GetPanelContentRows(console);
        var visibleRows = GetVisibleRows(lines.Length, availableRows, out var showScrollMarkers);
        var maxOffset = Math.Max(0, lines.Length - visibleRows);
        var start = Math.Clamp(diffScrollOffset, 0, maxOffset);
        var end = Math.Min(lines.Length, start + visibleRows);
        var rows = new List<IRenderable>();

        if (showScrollMarkers && start > 0)
        {
            rows.Add(new Markup("[grey]  ▲[/]"));
        }

        for (var index = start; index < end; index++)
        {
            rows.Add(new Markup(RenderDiffLine(lines[index])));
        }

        if (showScrollMarkers && end < lines.Length)
        {
            rows.Add(new Markup("[grey]  ▼[/]"));
        }

        var range = lines.Length == 0 ? "0/0" : $"{start + 1}-{end}/{lines.Length}";
        var title =
            $"{localizer.Get(Localizer.UpdateDiffTitle)} "
            + $"{Markup.Escape(change.RelativePath)} [grey]({range})[/]";
        return new Panel(new Rows(rows))
            .Header(title)
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey);
    }

    private static string[] GetDiffLines(TemplateFileChange change) =>
        change.Diff.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);

    private static int ScrollDiff(
        IAnsiConsole console,
        TemplateFileChange change,
        int currentOffset,
        int pageDirection
    )
    {
        var lines = GetDiffLines(change);
        var visibleRows = GetVisibleRows(
            lines.Length,
            GetPanelContentRows(console),
            out _
        );
        var maxOffset = Math.Max(0, lines.Length - visibleRows);
        var pageSize = Math.Max(1, visibleRows);
        return Math.Clamp(currentOffset + (pageDirection * pageSize), 0, maxOffset);
    }

    private static int GetVisibleRows(
        int totalRows,
        int availableRows,
        out bool showScrollMarkers
    )
    {
        showScrollMarkers = totalRows > availableRows && availableRows > ScrollMarkerRows;
        return showScrollMarkers
            ? availableRows - ScrollMarkerRows
            : Math.Max(1, availableRows);
    }

    private static int GetPanelContentRows(IAnsiConsole console)
    {
        var height = console.Profile.Height;
        if (console.Profile.Out.IsTerminal && !Console.IsOutputRedirected)
        {
            try
            {
                height = Math.Min(height, Console.WindowHeight);
            }
            catch (IOException)
            {
                // Some terminals do not expose a window size. Keep the profile size.
            }
            catch (ArgumentOutOfRangeException)
            {
                // The terminal can be resized while the selector is rendering.
            }
        }

        return Math.Max(1, height - FooterHeight - PanelOverhead);
    }

    private static string RenderDiffLine(string line)
    {
        var escaped = Markup.Escape(line);
        if (line.StartsWith("---", StringComparison.Ordinal)
            || line.StartsWith("+++", StringComparison.Ordinal))
        {
            return $"[bold cyan]{escaped}[/]";
        }

        if (line.StartsWith("+ ", StringComparison.Ordinal))
        {
            return $"[green]{escaped}[/]";
        }

        if (line.StartsWith("- ", StringComparison.Ordinal))
        {
            return $"[red]{escaped}[/]";
        }

        if (line.StartsWith("! ", StringComparison.Ordinal))
        {
            return $"[yellow]{escaped}[/]";
        }

        if (line == "...")
        {
            return "[grey]...[/]";
        }

        return $"[grey]{escaped}[/]";
    }
}
