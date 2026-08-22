using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class XtermDirectTerminalProfileTests
{
    private const int DirectColorCount = 1 << 24;
    private const int DirectColorPairCount = 1 << 16;

    private const string DirectForeground =
        "\u001b[%?%p1%{8}%<%t3%p1%d%e38:2::%p1%{65536}%/%d:"
        + "%p1%{256}%/%{255}%&%d:%p1%{255}%&%d%;m";

    private const string DirectBackground =
        "\u001b[%?%p1%{8}%<%t4%p1%d%e48:2::%p1%{65536}%/%d:"
        + "%p1%{256}%/%{255}%&%d:%p1%{255}%&%d%;m";

    private const string Direct16Foreground =
        "\u001b[%?%p1%{8}%<%t3%p1%d%e%?%p1%{16}%<%t%p1%'R'%+%d"
        + "%e38:2::%p1%{65536}%/%d:%p1%{256}%/%{255}%&%d:"
        + "%p1%{255}%&%d%;%;m";

    private const string Direct16Background =
        "\u001b[%?%p1%{8}%<%t4%p1%d%e%?%p1%{16}%<%t%p1%{92}%+%d"
        + "%e48:2::%p1%{65536}%/%d:%p1%{256}%/%{255}%&%d:"
        + "%p1%{255}%&%d%;%;m";

    private const string Direct256Foreground =
        "\u001b[%?%p1%{8}%<%t3%p1%d%e%p1%{16}%<%t9%p1%{8}%-%d%e"
        + "%?%p1%{256}%<%t38;5;%p1%d%e38:2::%p1%{65536}%/%d:"
        + "%p1%{256}%/%{255}%&%d:%p1%{255}%&%d%;%;m";

    private const string Direct256Background =
        "\u001b[%?%p1%{8}%<%t4%p1%d%e%p1%{16}%<%t10%p1%{8}%-%d%e"
        + "%?%p1%{256}%<%t48;5;%p1%d%e48:2::%p1%{65536}%/%d:"
        + "%p1%{256}%/%{255}%&%d:%p1%{255}%&%d%;%;m";

    [Fact]
    public void BuiltInDatabaseLoadsSelectedDirectProfilesExactly()
    {
        (string Name, TerminalDescription Profile, int IndexedPrefix)[] cases =
        [
            ("xterm-direct", TerminalProfiles.XtermDirect, 8),
            ("xterm-direct16", TerminalProfiles.XtermDirect16, 16),
            ("xterm-direct256", TerminalProfiles.XtermDirect256, 256),
        ];

        foreach ((string name, TerminalDescription profile, int indexedPrefix)
            in cases)
        {
            TerminalDescription loaded = TerminalDatabase.BuiltIn.Load(name);
            TerminalColorSupport support = TerminalColors.GetColorSupport(profile);

            Assert.Same(profile, loaded);
            Assert.Equal(name, profile.Name);
            Assert.Empty(profile.Aliases);
            Assert.Equal<int?>(
                DirectColorCount,
                profile.GetNumber(NumericCapability.Colors));
            Assert.Equal<int?>(
                DirectColorPairCount,
                profile.GetNumber(NumericCapability.ColorPairs));
            Assert.True(profile.TryGetExtendedBoolean("RGB", out bool rgb));
            Assert.True(rgb);
            Assert.True(profile.TryGetExtendedNumber("CO", out int co));
            Assert.Equal(indexedPrefix, co);

            Assert.Equal(TerminalColorModel.DirectRgb, support.Model);
            Assert.Equal(TerminalColorTier.TrueColor, support.Tier);
            Assert.Equal(indexedPrefix, support.IndexedColorCount);
            Assert.Equal<TerminalRgbLayout?>(
                new TerminalRgbLayout(8, 8, 8),
                support.RgbLayout);
        }
    }

    [Fact]
    public void DirectProfilesReuseModernXtermOutsideColorLayer()
    {
        TerminalDescription[] variants =
        [
            TerminalProfiles.XtermDirect,
            TerminalProfiles.XtermDirect16,
            TerminalProfiles.XtermDirect256,
        ];

        foreach (TerminalDescription variant in variants)
        {
            foreach (BooleanCapability capability in
                Enum.GetValues<BooleanCapability>())
            {
                Assert.Equal(
                    TerminalProfiles.Xterm.GetBoolean(capability),
                    variant.GetBoolean(capability));
            }

            foreach (NumericCapability capability in
                Enum.GetValues<NumericCapability>())
            {
                if (capability is NumericCapability.Colors
                    or NumericCapability.ColorPairs)
                {
                    continue;
                }

                Assert.Equal(
                    TerminalProfiles.Xterm.GetNumber(capability),
                    variant.GetNumber(capability));
            }

            foreach (StringCapability capability in
                Enum.GetValues<StringCapability>())
            {
                if (IsSelectorString(capability))
                {
                    continue;
                }

                Assert.Equal(
                    TerminalProfiles.Xterm.GetString(capability),
                    variant.GetString(capability));
            }

            Assert.Equal(
                TerminalProfiles.Xterm.ExtendedCapabilities.Count + 2,
                variant.ExtendedCapabilities.Count);

            foreach ((string name, TermInfoCapabilityValue value) in
                TerminalProfiles.Xterm.ExtendedCapabilities)
            {
                Assert.True(
                    variant.ExtendedCapabilities.TryGetValue(
                        name,
                        out TermInfoCapabilityValue found));
                Assert.Equal(value, found);
            }
        }
    }

    [Fact]
    public void DirectEightColorProfileMatchesAuthoritativeSelectors()
    {
        TerminalDescription terminal = TerminalProfiles.XtermDirect;

        Assert.Equal(
            DirectForeground,
            terminal.GetRequiredString(StringCapability.SetForegroundColor));
        Assert.Equal(
            DirectBackground,
            terminal.GetRequiredString(StringCapability.SetBackgroundColor));

        Assert.Equal("\u001b[30m", TerminalColors.ExpandForeground(terminal, 0));
        Assert.Equal("\u001b[37m", TerminalColors.ExpandForeground(terminal, 7));
        Assert.Equal("\u001b[40m", TerminalColors.ExpandBackground(terminal, 0));
        Assert.Equal("\u001b[47m", TerminalColors.ExpandBackground(terminal, 7));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalColors.ExpandForeground(terminal, 8));

        AssertDirectRgb(terminal);
        AssertDirectResetSemantics(terminal);
    }

    [Fact]
    public void DirectSixteenColorProfileMatchesAuthoritativeSelectors()
    {
        TerminalDescription terminal = TerminalProfiles.XtermDirect16;

        Assert.Equal(
            Direct16Foreground,
            terminal.GetRequiredString(StringCapability.SetForegroundColor));
        Assert.Equal(
            Direct16Background,
            terminal.GetRequiredString(StringCapability.SetBackgroundColor));

        Assert.Equal("\u001b[30m", TerminalColors.ExpandForeground(terminal, 0));
        Assert.Equal("\u001b[37m", TerminalColors.ExpandForeground(terminal, 7));
        Assert.Equal("\u001b[90m", TerminalColors.ExpandForeground(terminal, 8));
        Assert.Equal("\u001b[97m", TerminalColors.ExpandForeground(terminal, 15));
        Assert.Equal("\u001b[40m", TerminalColors.ExpandBackground(terminal, 0));
        Assert.Equal("\u001b[47m", TerminalColors.ExpandBackground(terminal, 7));
        Assert.Equal("\u001b[100m", TerminalColors.ExpandBackground(terminal, 8));
        Assert.Equal("\u001b[107m", TerminalColors.ExpandBackground(terminal, 15));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalColors.ExpandForeground(terminal, 16));

        AssertDirectRgb(terminal);
        AssertDirectResetSemantics(terminal);
    }

    [Fact]
    public void Direct256ColorProfileMatchesAuthoritativeSelectors()
    {
        TerminalDescription terminal = TerminalProfiles.XtermDirect256;

        Assert.Equal(
            Direct256Foreground,
            terminal.GetRequiredString(StringCapability.SetForegroundColor));
        Assert.Equal(
            Direct256Background,
            terminal.GetRequiredString(StringCapability.SetBackgroundColor));

        Assert.Equal("\u001b[30m", TerminalColors.ExpandForeground(terminal, 0));
        Assert.Equal("\u001b[37m", TerminalColors.ExpandForeground(terminal, 7));
        Assert.Equal("\u001b[90m", TerminalColors.ExpandForeground(terminal, 8));
        Assert.Equal("\u001b[97m", TerminalColors.ExpandForeground(terminal, 15));
        Assert.Equal(
            "\u001b[38;5;16m",
            TerminalColors.ExpandForeground(terminal, 16));
        Assert.Equal(
            "\u001b[38;5;255m",
            TerminalColors.ExpandForeground(terminal, 255));
        Assert.Equal(
            "\u001b[48;5;16m",
            TerminalColors.ExpandBackground(terminal, 16));
        Assert.Equal(
            "\u001b[48;5;255m",
            TerminalColors.ExpandBackground(terminal, 255));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalColors.ExpandForeground(terminal, 256));

        AssertDirectRgb(terminal);
        AssertDirectResetSemantics(terminal);
    }

    [Fact]
    public void DirectRgbHelperRespectsRetainedIndexedPrefixCollision()
    {
        TerminalDescription terminal = TerminalProfiles.XtermDirect256;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalColors.ExpandForeground(
                terminal,
                new TerminalRgbColor(0, 0, 255)));

        Assert.Equal(
            "\u001b[38:2::0:1:0m",
            TerminalColors.ExpandForeground(
                terminal,
                new TerminalRgbColor(0, 1, 0)));
        Assert.Equal(
            "\u001b[30m",
            TerminalColors.ExpandForeground(
                terminal,
                new TerminalRgbColor(0, 0, 0)));
    }

    private static void AssertDirectRgb(TerminalDescription terminal)
    {
        Assert.Equal(
            "\u001b[38:2::18:52:86m",
            TerminalColors.ExpandForeground(
                terminal,
                new TerminalRgbColor(0x12, 0x34, 0x56)));
        Assert.Equal(
            "\u001b[48:2::18:52:86m",
            TerminalColors.ExpandBackground(
                terminal,
                new TerminalRgbColor(0x12, 0x34, 0x56)));
    }

    private static void AssertDirectResetSemantics(TerminalDescription terminal)
    {
        TerminalColorSupport support = TerminalColors.GetColorSupport(terminal);

        Assert.True(support.BackColorErase);
        Assert.False(support.CanChangeColor);
        Assert.False(support.HasInitializeColor);
        Assert.True(support.HasOriginalColorPair);
        Assert.False(support.HasOriginalColors);
        Assert.Equal(
            "\u001b[39;49m",
            terminal.GetRequiredString(StringCapability.OriginalColorPair));
        Assert.Equal(
            "\u001bc",
            terminal.GetRequiredString(StringCapability.ResetString1));
        Assert.Null(terminal.GetString(StringCapability.InitializeColor));
        Assert.Null(terminal.GetString(StringCapability.OriginalColors));
        Assert.Null(
            terminal.GetString(StringCapability.SetLegacyForegroundColor));
        Assert.Null(
            terminal.GetString(StringCapability.SetLegacyBackgroundColor));
    }

    private static bool IsSelectorString(StringCapability capability)
    {
        return capability is
            StringCapability.SetForegroundColor
            or StringCapability.SetBackgroundColor
            or StringCapability.SetLegacyForegroundColor
            or StringCapability.SetLegacyBackgroundColor;
    }
}
