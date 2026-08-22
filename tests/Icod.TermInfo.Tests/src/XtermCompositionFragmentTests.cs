using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class XtermCompositionFragmentTests
{
    [Fact]
    public void CommonFragmentDoesNotChooseIndexedPaletteOrSelectors()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("xterm-common")
                .ApplyXtermCommon()
                .Build();

        Assert.Null(terminal.GetNumber(NumericCapability.Colors));
        Assert.Null(terminal.GetNumber(NumericCapability.ColorPairs));
        Assert.Null(terminal.GetString(StringCapability.SetForegroundColor));
        Assert.Null(terminal.GetString(StringCapability.SetBackgroundColor));
        Assert.Null(
            terminal.GetString(
                StringCapability.SetLegacyForegroundColor));
        Assert.Null(
            terminal.GetString(
                StringCapability.SetLegacyBackgroundColor));

        Assert.True(terminal.GetBoolean(BooleanCapability.BackColorErase));
        Assert.Equal(
            "\u001b[39;49m",
            terminal.GetRequiredString(StringCapability.OriginalColorPair));
    }

    [Fact]
    public void BasicEightColorFragmentReconstructsCurrentXtermColorData()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("xterm-basic-color")
                .ApplyXtermCommon()
                .ApplyXtermBasicEightColor()
                .Build();

        Assert.Equal<int?>(8, terminal.GetNumber(NumericCapability.Colors));
        Assert.Equal<int?>(64, terminal.GetNumber(NumericCapability.ColorPairs));

        StringCapability[] selectors =
        [
            StringCapability.SetForegroundColor,
            StringCapability.SetBackgroundColor,
            StringCapability.SetLegacyForegroundColor,
            StringCapability.SetLegacyBackgroundColor,
            StringCapability.OriginalColorPair,
        ];

        foreach (StringCapability capability in selectors)
        {
            Assert.Equal(
                TerminalProfiles.Xterm.GetString(capability),
                terminal.GetString(capability));
        }

        TerminalColorSupport support = TerminalColors.GetColorSupport(terminal);
        Assert.Equal(TerminalColorModel.Indexed, support.Model);
        Assert.Equal(TerminalColorTier.Color8, support.Tier);
        Assert.Equal(8, support.IndexedColorCount);
    }

    [Fact]
    public void BasicEightColorFragmentRejectsNullBuilder()
    {
        Assert.Throws<ArgumentNullException>(
            () => XtermColorCapabilityFragments.ApplyXtermBasicEightColor(null!));
    }
}
