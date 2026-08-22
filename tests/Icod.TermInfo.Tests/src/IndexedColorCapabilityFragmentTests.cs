using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class IndexedColorCapabilityFragmentTests
{
    [Theory]
    [InlineData(4, 3, TerminalColorTier.Color4, "\x1b[30m", "\x1b[33m", "\x1b[40m", "\x1b[43m")]
    [InlineData(8, 64, TerminalColorTier.Color8, "\x1b[30m", "\x1b[37m", "\x1b[40m", "\x1b[47m")]
    [InlineData(16, 256, TerminalColorTier.Color16, "\x1b[30m", "\x1b[97m", "\x1b[40m", "\x1b[107m")]
    [InlineData(88, 7744, TerminalColorTier.OtherIndexed, "\x1b[30m", "\x1b[38;5;87m", "\x1b[40m", "\x1b[48;5;87m")]
    [InlineData(256, 65536, TerminalColorTier.Color256, "\x1b[30m", "\x1b[38;5;255m", "\x1b[40m", "\x1b[48;5;255m")]
    public void RequestedIndexedDepthsHaveGoldenBehavior(
        int colors,
        int pairs,
        TerminalColorTier expectedTier,
        string expectedFirstForeground,
        string expectedLastForeground,
        string expectedFirstBackground,
        string expectedLastBackground)
    {
        TerminalDescription terminal =
            CreateIndexedTerminal(colors, pairs);
        TerminalColorSupport support =
            TerminalColors.GetColorSupport(terminal);

        Assert.Equal(TerminalColorModel.Indexed, support.Model);
        Assert.Equal(expectedTier, support.Tier);
        Assert.Equal<int?>(colors, support.ColorCount);
        Assert.Equal(colors, support.IndexedColorCount);
        Assert.Equal<int?>(pairs, support.ColorPairCount);
        Assert.Equal(
            expectedFirstForeground,
            TerminalColors.ExpandForeground(terminal, 0));
        Assert.Equal(
            expectedLastForeground,
            TerminalColors.ExpandForeground(terminal, colors - 1));
        Assert.Equal(
            expectedFirstBackground,
            TerminalColors.ExpandBackground(terminal, 0));
        Assert.Equal(
            expectedLastBackground,
            TerminalColors.ExpandBackground(terminal, colors - 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalColors.ExpandForeground(terminal, -1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalColors.ExpandBackground(terminal, colors));
    }

    [Fact]
    public void Aixterm16SelectorUsesBrightColorBranches()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("aixterm-16-test")
                .ApplyAixterm16(256)
                .Build();

        Assert.Equal("\x1b[30m", TerminalColors.ExpandForeground(terminal, 0));
        Assert.Equal("\x1b[37m", TerminalColors.ExpandForeground(terminal, 7));
        Assert.Equal("\x1b[90m", TerminalColors.ExpandForeground(terminal, 8));
        Assert.Equal("\x1b[97m", TerminalColors.ExpandForeground(terminal, 15));
        Assert.Equal("\x1b[40m", TerminalColors.ExpandBackground(terminal, 0));
        Assert.Equal("\x1b[47m", TerminalColors.ExpandBackground(terminal, 7));
        Assert.Equal("\x1b[100m", TerminalColors.ExpandBackground(terminal, 8));
        Assert.Equal("\x1b[107m", TerminalColors.ExpandBackground(terminal, 15));
    }

    [Fact]
    public void XtermExtendedSelectorUsesAnsiBrightAndPaletteBranches()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("xterm-indexed-test")
                .ApplyXtermExtendedIndexed(256, 65536)
                .Build();

        Assert.Equal("\x1b[30m", TerminalColors.ExpandForeground(terminal, 0));
        Assert.Equal("\x1b[37m", TerminalColors.ExpandForeground(terminal, 7));
        Assert.Equal("\x1b[90m", TerminalColors.ExpandForeground(terminal, 8));
        Assert.Equal("\x1b[97m", TerminalColors.ExpandForeground(terminal, 15));
        Assert.Equal("\x1b[38;5;16m", TerminalColors.ExpandForeground(terminal, 16));
        Assert.Equal("\x1b[38;5;255m", TerminalColors.ExpandForeground(terminal, 255));

        Assert.Equal("\x1b[40m", TerminalColors.ExpandBackground(terminal, 0));
        Assert.Equal("\x1b[47m", TerminalColors.ExpandBackground(terminal, 7));
        Assert.Equal("\x1b[100m", TerminalColors.ExpandBackground(terminal, 8));
        Assert.Equal("\x1b[107m", TerminalColors.ExpandBackground(terminal, 15));
        Assert.Equal("\x1b[48;5;16m", TerminalColors.ExpandBackground(terminal, 16));
        Assert.Equal("\x1b[48;5;255m", TerminalColors.ExpandBackground(terminal, 255));
    }

    [Theory]
    [InlineData(17)]
    [InlineData(42)]
    [InlineData(88)]
    [InlineData(173)]
    [InlineData(256)]
    public void XtermExtendedFragmentSupportsGenericIndexedRanges(int colors)
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder($"xterm-indexed-{colors}")
                .ApplyXtermExtendedIndexed(colors, 19)
                .Build();
        TerminalColorSupport support =
            TerminalColors.GetColorSupport(terminal);

        Assert.Equal(TerminalColorModel.Indexed, support.Model);
        Assert.Equal(colors, support.IndexedColorCount);
        Assert.Equal<int?>(19, support.ColorPairCount);
        Assert.Equal(
            $"\x1b[38;5;{colors - 1}m",
            TerminalColors.ExpandForeground(terminal, colors - 1));
        Assert.Equal(
            $"\x1b[48;5;{colors - 1}m",
            TerminalColors.ExpandBackground(terminal, colors - 1));
    }

    [Fact]
    public void EightColorFragmentMatchesBuiltInAnsiColorContract()
    {
        TerminalDescription fragment =
            new TerminalDescriptionBuilder("ansi-fragment-test")
                .ApplyAnsiIndexed(8, 64)
                .Build();

        Assert.Equal(
            TerminalProfiles.Ansi.GetNumber(NumericCapability.Colors),
            fragment.GetNumber(NumericCapability.Colors));
        Assert.Equal(
            TerminalProfiles.Ansi.GetNumber(NumericCapability.ColorPairs),
            fragment.GetNumber(NumericCapability.ColorPairs));
        Assert.Equal(
            TerminalProfiles.Ansi.GetString(StringCapability.SetForegroundColor),
            fragment.GetString(StringCapability.SetForegroundColor));
        Assert.Equal(
            TerminalProfiles.Ansi.GetString(StringCapability.SetBackgroundColor),
            fragment.GetString(StringCapability.SetBackgroundColor));
    }

    [Fact]
    public void PairCountRemainsIndependentOfColorCount()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("independent-pairs")
                .ApplyXtermExtendedIndexed(88, 123)
                .Build();
        TerminalColorSupport support =
            TerminalColors.GetColorSupport(terminal);

        Assert.Equal<int?>(88, support.ColorCount);
        Assert.Equal<int?>(123, support.ColorPairCount);
        Assert.Equal(88, support.IndexedColorCount);
        Assert.Equal(
            "\x1b[38;5;87m",
            TerminalColors.ExpandForeground(terminal, 87));
    }

    [Fact]
    public void XtermPaletteControlsPreserveAndExpandRawCapabilities()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("xterm-palette-test")
                .ApplyXtermExtendedIndexed(256, 65536)
                .ApplyXtermPaletteControls()
                .Build();
        TerminalColorSupport support =
            TerminalColors.GetColorSupport(terminal);

        Assert.True(support.CanChangeColor);
        Assert.True(support.HasInitializeColor);
        Assert.True(support.HasOriginalColors);
        Assert.Equal(
            "\x1b]4;12;rgb:FF/7F/00\x1b\\",
            terminal.Expand(
                StringCapability.InitializeColor,
                12,
                1000,
                500,
                0));
        Assert.Equal(
            "\x1b]104\x1b\\",
            terminal.GetRequiredString(StringCapability.OriginalColors));
    }

    [Fact]
    public void PaletteControlsComposeWithAixterm16Selection()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("xterm-16-composed-test")
                .ApplyAixterm16(256)
                .ApplyXtermPaletteControls()
                .Build();
        TerminalColorSupport support =
            TerminalColors.GetColorSupport(terminal);

        Assert.Equal(TerminalColorTier.Color16, support.Tier);
        Assert.True(support.CanChangeColor);
        Assert.True(support.HasInitializeColor);
        Assert.True(support.HasOriginalColors);
        Assert.Equal("\x1b[97m", TerminalColors.ExpandForeground(terminal, 15));
    }

    [Fact]
    public void IndexedFragmentsValidateArguments()
    {
        TerminalDescriptionBuilder builder =
            new("validation-test");

        Assert.Throws<ArgumentNullException>(
            () => IndexedColorCapabilityFragments.ApplyAnsiIndexed(
                null!,
                8,
                64));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => builder.ApplyAnsiIndexed(0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => builder.ApplyAnsiIndexed(9, 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => builder.ApplyAnsiIndexed(8, -1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => builder.ApplyAixterm16(-1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => builder.ApplyXtermExtendedIndexed(16, 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => builder.ApplyXtermExtendedIndexed(257, 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => builder.ApplyXtermExtendedIndexed(88, -1));
        Assert.Throws<ArgumentNullException>(
            () => IndexedColorCapabilityFragments.ApplyXtermPaletteControls(null!));
    }

    private static TerminalDescription CreateIndexedTerminal(
        int colors,
        int pairs)
    {
        TerminalDescriptionBuilder builder =
            new($"indexed-golden-{colors}");

        if (colors <= 8)
        {
            builder.ApplyAnsiIndexed(colors, pairs);
        }
        else if (colors == 16)
        {
            builder.ApplyAixterm16(pairs);
        }
        else
        {
            builder.ApplyXtermExtendedIndexed(colors, pairs);
        }

        return builder.Build();
    }
}
