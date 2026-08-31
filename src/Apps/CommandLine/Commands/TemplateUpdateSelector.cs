using CoreMod.Services;
using Share;
using Spectre.Console.Rendering;

namespace CommandLine.Commands;

/// <summary>
/// A selectable entry in the template update list.
/// </summary>
public sealed class TemplateUpdateSelectionItem
{
    private readonly List<TemplateFileChange> _changes;

    public TemplateUpdateSelectionItem(
        string displayPath,
        IReadOnlyList<TemplateFileChange> changes,
        bool isSkill
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayPath);
        ArgumentNullException.ThrowIfNull(changes);
        if (changes.Count == 0)
        {
            throw new ArgumentException("At least one template change is required.", nameof(changes));
        }

        DisplayPath = displayPath;
        _changes = changes.ToList();
        IsSkill = isSkill;
    }

    public string DisplayPath { get; }

    public IReadOnlyList<TemplateFileChange> Changes => _changes;

    public bool IsSkill { get; }

    public int ChangeCount => _changes.Count;

    public int AddedLines => _changes.Sum(change => change.DiffStats.AddedLines);

    public int RemovedLines => _changes.Sum(change => change.DiffStats.RemovedLines);

    internal void AddChange(TemplateFileChange change)
    {
        ArgumentNullException.ThrowIfNull(change);
        _changes.Add(change);
    }
}

/// <summary>
/// Tracks the focused update item and the files selected for a template update.
/// </summary>
public sealed class TemplateUpdateSelectionState
{
    private readonly bool[] _selected;

    public TemplateUpdateSelectionState(IReadOnlyList<TemplateFileChange> changes)
        : this(CreateFileItems(changes))
    {
    }

    private TemplateUpdateSelectionState(IReadOnlyList<TemplateUpdateSelectionItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Count == 0)
        {
            throw new ArgumentException("At least one template change is required.", nameof(items));
        }

        Items = items.ToArray();
        Changes = Items.SelectMany(item => item.Changes).ToArray();
        _selected = new bool[Items.Count];
    }

    public IReadOnlyList<TemplateFileChange> Changes { get; }

    public IReadOnlyList<TemplateUpdateSelectionItem> Items { get; }

    public int CurrentIndex { get; private set; }

    public int SelectedCount => Items
        .Select((item, index) => _selected[index] ? item.ChangeCount : 0)
        .Sum();

    public int TotalChangeCount => Changes.Count;

    public TemplateUpdateSelectionItem CurrentItem => Items[CurrentIndex];

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
        CurrentIndex = Math.Min(Items.Count - 1, CurrentIndex + 1);
    }

    public void MoveFirst()
    {
        CurrentIndex = 0;
    }

    public void MoveLast()
    {
        CurrentIndex = Items.Count - 1;
    }

    public void ToggleCurrent()
    {
        ValidateIndex(CurrentIndex);
        _selected[CurrentIndex] = !_selected[CurrentIndex];
    }

    public IReadOnlyList<TemplateFileChange> GetSelectedChanges()
    {
        return Items
            .SelectMany(
                (item, index) => _selected[index]
                    ? item.Changes
                    : Array.Empty<TemplateFileChange>()
            )
            .ToArray();
    }

    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= Items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    internal static TemplateUpdateSelectionState CreateGrouped(
        IReadOnlyList<TemplateUpdateSelectionItem> items
    ) => new(items);

    private static IReadOnlyList<TemplateUpdateSelectionItem> CreateFileItems(
        IReadOnlyList<TemplateFileChange> changes
    )
    {
        ArgumentNullException.ThrowIfNull(changes);
        return changes
            .Select(change => new TemplateUpdateSelectionItem(change.RelativePath, [change], isSkill: false))
            .ToArray();
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
    private const string SkillRootName = "skills";
    private const string SkillFilesLocalizationKey = "UpdateSkillFiles";
    private const string SkillDiffTitleLocalizationKey = "UpdateSkillDiffTitle";

    private enum ActivePane
    {
        Files,
        Diff,
    }

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

        var state = TemplateUpdateSelectionState.CreateGrouped(CreateSelectionItems(changes));
        string? validationMessage = null;
        var diffScrollOffset = 0;
        var activePane = ActivePane.Files;
        var layout = CreateLayout(
            console,
            state,
            localizer,
            validationMessage,
            diffScrollOffset,
            activePane
        );

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
                            if (activePane == ActivePane.Files)
                            {
                                state.MovePrevious();
                                diffScrollOffset = 0;
                            }
                            else
                            {
                                diffScrollOffset = ScrollDiffByRows(
                                    console,
                                    state.CurrentItem,
                                    diffScrollOffset,
                                    -1
                                );
                            }
                            validationMessage = null;
                            break;
                        case ConsoleKey.DownArrow:
                            if (activePane == ActivePane.Files)
                            {
                                state.MoveNext();
                                diffScrollOffset = 0;
                            }
                            else
                            {
                                diffScrollOffset = ScrollDiffByRows(
                                    console,
                                    state.CurrentItem,
                                    diffScrollOffset,
                                    1
                                );
                            }
                            validationMessage = null;
                            break;
                        case ConsoleKey.Home:
                            if (activePane == ActivePane.Files)
                            {
                                state.MoveFirst();
                                diffScrollOffset = 0;
                            }
                            else
                            {
                                diffScrollOffset = 0;
                            }
                            validationMessage = null;
                            break;
                        case ConsoleKey.End:
                            if (activePane == ActivePane.Files)
                            {
                                state.MoveLast();
                                diffScrollOffset = 0;
                            }
                            else
                            {
                                diffScrollOffset = GetMaximumDiffOffset(
                                    console,
                                    state.CurrentItem
                                );
                            }
                            validationMessage = null;
                            break;
                        case ConsoleKey.Tab:
                            activePane = activePane == ActivePane.Files
                                ? ActivePane.Diff
                                : ActivePane.Files;
                            validationMessage = null;
                            break;
                        case ConsoleKey.LeftArrow:
                            activePane = ActivePane.Files;
                            validationMessage = null;
                            break;
                        case ConsoleKey.RightArrow:
                            activePane = ActivePane.Diff;
                            validationMessage = null;
                            break;
                        case ConsoleKey.PageUp:
                            activePane = ActivePane.Diff;
                            diffScrollOffset = ScrollDiff(
                                console,
                                state.CurrentItem,
                                diffScrollOffset,
                                -1
                            );
                            validationMessage = null;
                            break;
                        case ConsoleKey.PageDown:
                            activePane = ActivePane.Diff;
                            diffScrollOffset = ScrollDiff(
                                console,
                                state.CurrentItem,
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
                            diffScrollOffset,
                            activePane
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
        int diffScrollOffset,
        ActivePane activePane
    )
    {
        var body = new Layout("body").SplitColumns(
            new Layout("files")
                .Ratio(1)
                .MinimumSize(30)
                .Update(
                    CreateFilePanel(
                        console,
                        state,
                        localizer,
                        activePane == ActivePane.Files
                    )
                ),
            new Layout("diff")
                .Ratio(3)
                .MinimumSize(50)
                .Update(
                    CreateDiffPanel(
                        console,
                        state.CurrentItem,
                        localizer,
                        diffScrollOffset,
                        activePane == ActivePane.Diff
                    )
                )
        );

        var footerText = validationMessage
            ?? localizer.Get(Localizer.UpdateSelectionSummary, state.SelectedCount, state.TotalChangeCount);
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
        Localizer localizer,
        bool isActive
    )
    {
        var availableRows = GetPanelContentRows(console);
        var visibleRows = GetVisibleRows(state.Items.Count, availableRows);
        var start = Math.Clamp(
            state.CurrentIndex - visibleRows / 2,
            0,
            Math.Max(0, state.Items.Count - visibleRows)
        );
        var end = Math.Min(state.Items.Count, start + visibleRows);
        var rows = new List<IRenderable>();

        for (var index = start; index < end; index++)
        {
            var scrollBar = RenderScrollBar(start, visibleRows, state.Items.Count, index - start);
            rows.Add(
                CreateSingleLineMarkup(
                    $"{scrollBar} {RenderSelectionItemRow(console, state, index, localizer)}"
                )
            );
        }

        return new Panel(new Rows(rows))
            .Header(
                $"{localizer.Get(Localizer.UpdateSelectionTitle)} "
                + $"[grey]({start + 1}-{end}/{state.Items.Count})[/]"
            )
            .Border(BoxBorder.Rounded)
            .BorderColor(isActive ? Color.Yellow : Color.Grey);
    }

    private static string RenderSelectionItemRow(
        IAnsiConsole console,
        TemplateUpdateSelectionState state,
        int index,
        Localizer localizer
    )
    {
        var item = state.Items[index];
        var isCurrent = state.CurrentIndex == index;
        var cursor = isCurrent ? "[yellow]>[/]" : " ";
        var checkbox = state.IsSelected(index) ? "[green][[x]][/]" : "[grey][[ ]][/]";
        var kind = item.IsSkill
            ? "[cyan]◆[/]"
            : item.Changes[0].IsNew ? "[green]+[/]" : "[yellow]~[/]";
        var displayPath = item.IsSkill ? GetSkillName(item.DisplayPath) : item.DisplayPath;
        var pathBudget = GetFilePathWidth(console, item);
        var path = Markup.Escape(TruncateText(displayPath, pathBudget));
        var count = item.IsSkill
            ? $"[grey]{Markup.Escape(localizer.Get(SkillFilesLocalizationKey, item.ChangeCount))}[/]"
            : item.Changes[0].IsBinary
                ? $"[grey]{Markup.Escape(localizer.Get(Localizer.UpdateBinaryChange))}[/]"
                : $"[green]+{item.AddedLines}[/] [red]-{item.RemovedLines}[/]";
        var row = $"{cursor} {checkbox} {kind} {path} ({count})";

        return isCurrent ? $"[bold]{row}[/]" : row;
    }

    private static Panel CreateDiffPanel(
        IAnsiConsole console,
        TemplateUpdateSelectionItem item,
        Localizer localizer,
        int diffScrollOffset,
        bool isActive
    )
    {
        var lines = GetDiffLines(item);
        var availableRows = GetPanelContentRows(console);
        var visibleRows = GetVisibleRows(lines.Length, availableRows);
        var maxOffset = Math.Max(0, lines.Length - visibleRows);
        var start = Math.Clamp(diffScrollOffset, 0, maxOffset);
        var end = Math.Min(lines.Length, start + visibleRows);
        var rows = new List<IRenderable>();

        for (var index = start; index < end; index++)
        {
            var scrollBar = RenderScrollBar(start, visibleRows, lines.Length, index - start);
            rows.Add(
                CreateSingleLineMarkup(
                    $"{scrollBar} {RenderDiffLine(lines[index], GetDiffContentWidth(console))}"
                )
            );
        }

        var range = lines.Length == 0 ? "0/0" : $"{start + 1}-{end}/{lines.Length}";
        var titleKey = item.IsSkill ? SkillDiffTitleLocalizationKey : Localizer.UpdateDiffTitle;
        var itemInfo = item.IsSkill
            ? $", {localizer.Get(SkillFilesLocalizationKey, item.ChangeCount)}"
            : string.Empty;
        var title =
            $"{localizer.Get(titleKey)} "
            + $"{Markup.Escape(item.DisplayPath)} "
            + $"[grey]({range}{itemInfo})[/]";
        return new Panel(new Rows(rows))
            .Header(title)
            .Border(BoxBorder.Rounded)
            .BorderColor(isActive ? Color.Yellow : Color.Grey);
    }

    private static string[] GetDiffLines(TemplateUpdateSelectionItem item)
    {
        var lines = new List<string>();
        foreach (var change in item.Changes)
        {
            if (lines.Count > 0)
            {
                lines.Add(string.Empty);
            }

            lines.AddRange(GetDiffLines(change));
        }

        return lines.Count == 0 ? [string.Empty] : lines.ToArray();
    }

    private static string[] GetDiffLines(TemplateFileChange change) =>
        change.Diff.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);

    private static IReadOnlyList<TemplateUpdateSelectionItem> CreateSelectionItems(
        IReadOnlyList<TemplateFileChange> changes
    )
    {
        var items = new List<TemplateUpdateSelectionItem>();
        var skillItems = new Dictionary<string, TemplateUpdateSelectionItem>(
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var change in changes)
        {
            if (!TryGetSkillRoot(change.RelativePath, out var skillRoot))
            {
                items.Add(new TemplateUpdateSelectionItem(change.RelativePath, [change], isSkill: false));
                continue;
            }

            if (!skillItems.TryGetValue(skillRoot, out var skillItem))
            {
                skillItem = new TemplateUpdateSelectionItem(skillRoot, [change], isSkill: true);
                skillItems.Add(skillRoot, skillItem);
                items.Add(skillItem);
            }
            else
            {
                skillItem.AddChange(change);
            }
        }

        return items;
    }

    private static bool TryGetSkillRoot(string relativePath, out string skillRoot)
    {
        var parts = relativePath
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 4
            || (!parts[0].Equals(".agent", StringComparison.OrdinalIgnoreCase)
                && !parts[0].Equals(".agents", StringComparison.OrdinalIgnoreCase))
            || !parts[1].Equals(SkillRootName, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(parts[2]))
        {
            skillRoot = string.Empty;
            return false;
        }

        skillRoot = $"{parts[0]}/{parts[1]}/{parts[2]}";
        return true;
    }

    private static int ScrollDiff(
        IAnsiConsole console,
        TemplateUpdateSelectionItem item,
        int currentOffset,
        int pageDirection
    )
    {
        var lines = GetDiffLines(item);
        var visibleRows = GetVisibleRows(
            lines.Length,
            GetPanelContentRows(console)
        );
        var maxOffset = Math.Max(0, lines.Length - visibleRows);
        var pageSize = Math.Max(1, visibleRows);
        return Math.Clamp(currentOffset + (pageDirection * pageSize), 0, maxOffset);
    }

    private static int ScrollDiffByRows(
        IAnsiConsole console,
        TemplateUpdateSelectionItem item,
        int currentOffset,
        int rowDelta
    )
    {
        return Math.Clamp(
            currentOffset + rowDelta,
            0,
            GetMaximumDiffOffset(console, item)
        );
    }

    private static int GetMaximumDiffOffset(IAnsiConsole console, TemplateUpdateSelectionItem item)
    {
        var lineCount = GetDiffLines(item).Length;
        return Math.Max(
            0,
            lineCount - GetVisibleRows(lineCount, GetPanelContentRows(console))
        );
    }

    private static int GetVisibleRows(int totalRows, int availableRows) =>
        Math.Max(1, Math.Min(totalRows, availableRows));

    private static string RenderScrollBar(
        int offset,
        int visibleRows,
        int totalRows,
        int viewportRow
    )
    {
        if (totalRows <= visibleRows)
        {
            return "[grey]│[/]";
        }

        var thumbSize = Math.Max(1, (int)Math.Round((double)visibleRows * visibleRows / totalRows));
        var trackTravel = visibleRows - thumbSize;
        var maximumOffset = totalRows - visibleRows;
        var thumbStart = maximumOffset == 0
            ? 0
            : (int)Math.Round((double)offset * trackTravel / maximumOffset);
        return viewportRow >= thumbStart && viewportRow < thumbStart + thumbSize
            ? "[cyan]█[/]"
            : "[grey]│[/]";
    }

    private static Markup CreateSingleLineMarkup(string content) => new(content);

    private static int GetFilePathWidth(
        IAnsiConsole console,
        TemplateUpdateSelectionItem item
    )
    {
        var fixedWidth = item.IsSkill ? 14 : 18;
        return Math.Max(8, GetFilePanelContentWidth(console) - fixedWidth);
    }

    private static string GetSkillName(string skillRoot)
    {
        var parts = skillRoot.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.LastOrDefault() ?? skillRoot;
    }

    private static int GetFilePanelContentWidth(IAnsiConsole console)
    {
        var totalWidth = GetConsoleWidth(console);
        var filePanelWidth = Math.Max(30, totalWidth / 4);
        return Math.Max(8, filePanelWidth - 4);
    }

    private static int GetDiffContentWidth(IAnsiConsole console)
    {
        var totalWidth = GetConsoleWidth(console);
        var filePanelWidth = Math.Max(30, totalWidth / 4);
        var diffPanelWidth = Math.Max(50, totalWidth - filePanelWidth);
        return Math.Max(8, diffPanelWidth - 4);
    }

    private static int GetConsoleWidth(IAnsiConsole console)
    {
        var width = console.Profile.Width;
        if (console.Profile.Out.IsTerminal && !Console.IsOutputRedirected)
        {
            try
            {
                width = Math.Min(width, Console.WindowWidth);
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

        return Math.Max(1, width);
    }

    private static string TruncateText(string text, int maxWidth)
    {
        if (maxWidth <= 0)
        {
            return string.Empty;
        }

        if (text.Length <= maxWidth)
        {
            return text;
        }

        return maxWidth == 1 ? "…" : text[..(maxWidth - 1)] + "…";
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

    private static string RenderDiffLine(string line, int maxWidth)
    {
        var escaped = Markup.Escape(TruncateText(line, maxWidth));
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
