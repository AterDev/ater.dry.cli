using CommandLine.Commands;
using CoreMod.Services;
using Microsoft.Extensions.Localization;
using Moq;
using Share;
using Spectre.Console.Testing;
using Xunit;

namespace CoreMod.Tests.Commands;

public sealed class TemplateUpdateSelectorTests
{
    [Fact]
    public void SelectionState_ShouldToggleAndReturnSelectedChangesInFileOrder()
    {
        var first = new TemplateFileChange("first.cs", "old", "new");
        var second = new TemplateFileChange("second.cs", null, "added");
        var third = new TemplateFileChange("third.cs", "old", "new");
        var state = new TemplateUpdateSelectionState([first, second, third]);

        state.ToggleCurrent();
        state.MoveNext();
        state.ToggleCurrent();
        state.MoveNext();
        state.MoveNext();

        Assert.Equal(2, state.SelectedCount);
        Assert.Equal(2, state.CurrentIndex);
        Assert.Equal([first, second], state.GetSelectedChanges());
        Assert.True(state.IsSelected(0));
        Assert.True(state.IsSelected(1));
        Assert.False(state.IsSelected(2));
    }

    [Fact]
    public void SelectionState_ShouldKeepFocusWithinTheFileList()
    {
        var state = new TemplateUpdateSelectionState(
            [new TemplateFileChange("file.cs", "old", "new")]
        );

        state.MovePrevious();
        state.MoveNext();
        state.MoveNext();
        state.MoveLast();
        state.MoveFirst();

        Assert.Equal(0, state.CurrentIndex);
    }

    [Fact]
    public async Task Selector_ShouldApplyOnlyFilesToggledWithSpace()
    {
        var first = new TemplateFileChange("first.cs", "old", "new");
        var second = new TemplateFileChange("second.cs", null, "added");
        using var console = new TestConsole()
            .Interactive()
            .EmitAnsiSequences()
            .Width(120)
            .Height(30);
        console.Input.PushKey(ConsoleKey.Spacebar);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Spacebar);
        console.Input.PushKey(ConsoleKey.Enter);

        var result = await TemplateUpdateSelector.SelectAsync(
            console,
            [first, second],
            CreateLocalizer(),
            TestContext.Current.CancellationToken
        );

        Assert.True(result.ShouldApply);
        Assert.Equal([first, second], result.Changes);
        Assert.Contains("UpdateDiffTitle", console.Output);
        Assert.Contains("\u001b[", console.Output);
    }

    [Fact]
    public async Task Selector_ShouldCancelOnEscapeWithoutSelectingFiles()
    {
        using var console = new TestConsole().Interactive().Width(120).Height(30);
        console.Input.PushKey(ConsoleKey.Escape);

        var result = await TemplateUpdateSelector.SelectAsync(
            console,
            [new TemplateFileChange("file.cs", "old", "new")],
            CreateLocalizer(),
            TestContext.Current.CancellationToken
        );

        Assert.False(result.ShouldApply);
        Assert.Empty(result.Changes);
    }

    [Fact]
    public async Task Selector_ShouldKeepFocusedFileVisibleAndScrollLongDiff()
    {
        var changes = Enumerable
            .Range(0, 8)
            .Select(
                index =>
                    new TemplateFileChange(
                        $"file-{index}.cs",
                        string.Join(Environment.NewLine, Enumerable.Range(0, 12).Select(line => $"old-{index}-{line}")),
                        string.Join(Environment.NewLine, Enumerable.Range(0, 12).Select(line => $"new-{index}-{line}"))
                    )
            )
            .ToArray();
        using var console = new TestConsole()
            .Interactive()
            .EmitAnsiSequences()
            .Width(120)
            .Height(16);
        for (var index = 0; index < 5; index++)
        {
            console.Input.PushKey(ConsoleKey.DownArrow);
        }

        console.Input.PushKey(ConsoleKey.PageDown);
        console.Input.PushKey(ConsoleKey.Escape);

        var result = await TemplateUpdateSelector.SelectAsync(
            console,
            changes,
            CreateLocalizer(),
            TestContext.Current.CancellationToken
        );

        Assert.False(result.ShouldApply);
        Assert.Contains("file-5.cs", console.Output);
        Assert.Contains("▲", console.Output);
        Assert.Contains("▼", console.Output);
    }

    private static Localizer CreateLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<Localizer>>();
        localizer
            .Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] _) => new LocalizedString(name, name));
        return new Localizer(localizer.Object);
    }
}
