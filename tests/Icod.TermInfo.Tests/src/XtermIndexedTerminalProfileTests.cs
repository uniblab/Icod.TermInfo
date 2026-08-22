using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class XtermIndexedTerminalProfileTests
{
    private const string XtermIndexedForeground =
        "\u001b[%?%p1%{8}%<%t3%p1%d%e%p1%{16}%<%t9%p1%{8}%-%d"
        + "%e38;5;%p1%d%;m";

    private const string XtermIndexedBackground =
        "\u001b[%?%p1%{8}%<%t4%p1%d%e%p1%{16}%<%t10%p1%{8}%-%d"
        + "%e48;5;%p1%d%;m";

    private const string AixtermForeground =
        "\u001b[%?%p1%{8}%<%t%p1%{30}%+%e%p1%{82}%+%;%dm";

    private const string AixtermBackground =
        "\u001b[%?%p1%{8}%<%t%p1%{40}%+%e%p1%{92}%+%;%dm";

    private const string AixtermLegacyForeground =
        "%p1%{8}%/%{6}%*%{3}%+\u001b[%d%p1%{8}%m%Pa%?%ga%{1}%=%t4%e"
        + "%ga%{3}%=%t6%e%ga%{4}%=%t1%e%ga%{6}%=%t3%e%ga%d%;m";

    private const string AixtermLegacyBackground =
        "%p1%{8}%/%{6}%*%{4}%+\u001b[%d%p1%{8}%m%Pa%?%ga%{1}%=%t4%e"
        + "%ga%{3}%=%t6%e%ga%{4}%=%t1%e%ga%{6}%=%t3%e%ga%d%;m";

    private const string InitializeColor =
        "\u001b]4;%p1%d;rgb:%p2%{255}%*%{1000}%/%2.2X/"
        + "%p3%{255}%*%{1000}%/%2.2X/"
        + "%p4%{255}%*%{1000}%/%2.2X\u001b\\";

    private const string OriginalColors = "\u001b]104\u001b\\";
    private const string ResetWithOriginalColors =
        "\u001bc\u001b]104\u001b\\";

    [Fact]
    public void BuiltInDatabaseLoadsSelectedIndexedProfilesExactly()
    {
        (string Name, TerminalDescription Profile, int Colors, int Pairs)[] cases =
        [
            ("xterm-16color", TerminalProfiles.Xterm16Color, 16, 256),
            ("xterm-88color", TerminalProfiles.Xterm88Color, 88, 7744),
            ("xterm-256color", TerminalProfiles.Xterm256Color, 256, 65536),
        ];

        foreach ((string name, TerminalDescription profile, int colors, int pairs)
            in cases)
        {
            TerminalDescription loaded = TerminalDatabase.BuiltIn.Load(name);

            Assert.Same(profile, loaded);
            Assert.Equal(name, profile.Name);
            Assert.Empty(profile.Aliases);
            Assert.Equal<int?>(
                colors,
                profile.GetNumber(NumericCapability.Colors));
            Assert.Equal<int?>(
                pairs,
                profile.GetNumber(NumericCapability.ColorPairs));
        }
    }

    [Fact]
    public void IndexedProfilesReuseModernXtermCommonKeysAndMetadata()
    {
        TerminalDescription[] variants =
        [
            TerminalProfiles.Xterm16Color,
            TerminalProfiles.Xterm88Color,
            TerminalProfiles.Xterm256Color,
        ];

        foreach (TerminalDescription variant in variants)
        {
            foreach (BooleanCapability capability in
                Enum.GetValues<BooleanCapability>())
            {
                if (capability == BooleanCapability.CanChangeColor)
                {
                    Assert.True(variant.GetBoolean(capability));
                    continue;
                }

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
                if (IsColorSpecificString(capability))
                {
                    continue;
                }

                Assert.Equal(
                    TerminalProfiles.Xterm.GetString(capability),
                    variant.GetString(capability));
            }

            Assert.Equal(
                TerminalProfiles.Xterm.ExtendedCapabilities.Count,
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
    public void SixteenColorProfileMatchesAuthoritativeColorLayer()
    {
        TerminalDescription terminal = TerminalProfiles.Xterm16Color;
        TerminalColorSupport support = TerminalColors.GetColorSupport(terminal);

        Assert.Equal(TerminalColorModel.Indexed, support.Model);
        Assert.Equal(TerminalColorTier.Color16, support.Tier);
        Assert.Equal(16, support.IndexedColorCount);
        Assert.Equal<int?>(256, support.ColorPairCount);
        Assert.True(support.BackColorErase);
        Assert.True(support.CanChangeColor);
        Assert.True(support.HasInitializeColor);
        Assert.True(support.HasOriginalColors);

        Assert.Equal(
            AixtermForeground,
            terminal.GetRequiredString(StringCapability.SetForegroundColor));
        Assert.Equal(
            AixtermBackground,
            terminal.GetRequiredString(StringCapability.SetBackgroundColor));
        Assert.Equal(
            AixtermLegacyForeground,
            terminal.GetRequiredString(
                StringCapability.SetLegacyForegroundColor));
        Assert.Equal(
            AixtermLegacyBackground,
            terminal.GetRequiredString(
                StringCapability.SetLegacyBackgroundColor));

        Assert.Equal(
            "\u001b[30m",
            TerminalColors.ExpandForeground(terminal, 0));
        Assert.Equal(
            "\u001b[37m",
            TerminalColors.ExpandForeground(terminal, 7));
        Assert.Equal(
            "\u001b[90m",
            TerminalColors.ExpandForeground(terminal, 8));
        Assert.Equal(
            "\u001b[97m",
            TerminalColors.ExpandForeground(terminal, 15));
        Assert.Equal(
            "\u001b[40m",
            TerminalColors.ExpandBackground(terminal, 0));
        Assert.Equal(
            "\u001b[47m",
            TerminalColors.ExpandBackground(terminal, 7));
        Assert.Equal(
            "\u001b[100m",
            TerminalColors.ExpandBackground(terminal, 8));
        Assert.Equal(
            "\u001b[107m",
            TerminalColors.ExpandBackground(terminal, 15));

        Assert.Equal(
            "\u001b[34m",
            terminal.Expand(
                StringCapability.SetLegacyForegroundColor,
                1));
        Assert.Equal(
            "\u001b[31m",
            terminal.Expand(
                StringCapability.SetLegacyForegroundColor,
                4));
        Assert.Equal(
            "\u001b[94m",
            terminal.Expand(
                StringCapability.SetLegacyForegroundColor,
                9));
        Assert.Equal(
            "\u001b[44m",
            terminal.Expand(
                StringCapability.SetLegacyBackgroundColor,
                1));
        Assert.Equal(
            "\u001b[41m",
            terminal.Expand(
                StringCapability.SetLegacyBackgroundColor,
                4));
        Assert.Equal(
            "\u001b[104m",
            terminal.Expand(
                StringCapability.SetLegacyBackgroundColor,
                9));

        AssertPaletteControls(terminal);
    }

    [Fact]
    public void EightyEightColorProfileMatchesAuthoritativeColorLayer()
    {
        AssertExtendedIndexedProfile(
            TerminalProfiles.Xterm88Color,
            88,
            7744,
            TerminalColorTier.OtherIndexed,
            87);
    }

    [Fact]
    public void TwoHundredFiftySixColorProfileMatchesAuthoritativeColorLayer()
    {
        AssertExtendedIndexedProfile(
            TerminalProfiles.Xterm256Color,
            256,
            65536,
            TerminalColorTier.Color256,
            255);
    }

    [Fact]
    public void HistoricalXtermMonoIsNotSynthesizedFromModernXterm()
    {
        Assert.False(
            TerminalDatabase.BuiltIn.TryLoad(
                "xterm-mono",
                out TerminalDescription? terminal));
        Assert.Null(terminal);
    }

    private static void AssertExtendedIndexedProfile(
        TerminalDescription terminal,
        int colors,
        int pairs,
        TerminalColorTier tier,
        int lastIndex)
    {
        TerminalColorSupport support = TerminalColors.GetColorSupport(terminal);

        Assert.Equal(TerminalColorModel.Indexed, support.Model);
        Assert.Equal(tier, support.Tier);
        Assert.Equal(colors, support.IndexedColorCount);
        Assert.Equal<int?>(pairs, support.ColorPairCount);
        Assert.True(support.BackColorErase);
        Assert.True(support.CanChangeColor);
        Assert.True(support.HasInitializeColor);
        Assert.True(support.HasOriginalColors);

        Assert.Equal(
            XtermIndexedForeground,
            terminal.GetRequiredString(StringCapability.SetForegroundColor));
        Assert.Equal(
            XtermIndexedBackground,
            terminal.GetRequiredString(StringCapability.SetBackgroundColor));
        Assert.Null(
            terminal.GetString(StringCapability.SetLegacyForegroundColor));
        Assert.Null(
            terminal.GetString(StringCapability.SetLegacyBackgroundColor));

        Assert.Equal(
            "\u001b[30m",
            TerminalColors.ExpandForeground(terminal, 0));
        Assert.Equal(
            "\u001b[37m",
            TerminalColors.ExpandForeground(terminal, 7));
        Assert.Equal(
            "\u001b[90m",
            TerminalColors.ExpandForeground(terminal, 8));
        Assert.Equal(
            "\u001b[97m",
            TerminalColors.ExpandForeground(terminal, 15));
        Assert.Equal(
            "\u001b[38;5;16m",
            TerminalColors.ExpandForeground(terminal, 16));
        Assert.Equal(
            $"\u001b[38;5;{lastIndex}m",
            TerminalColors.ExpandForeground(terminal, lastIndex));

        Assert.Equal(
            "\u001b[40m",
            TerminalColors.ExpandBackground(terminal, 0));
        Assert.Equal(
            "\u001b[47m",
            TerminalColors.ExpandBackground(terminal, 7));
        Assert.Equal(
            "\u001b[100m",
            TerminalColors.ExpandBackground(terminal, 8));
        Assert.Equal(
            "\u001b[107m",
            TerminalColors.ExpandBackground(terminal, 15));
        Assert.Equal(
            "\u001b[48;5;16m",
            TerminalColors.ExpandBackground(terminal, 16));
        Assert.Equal(
            $"\u001b[48;5;{lastIndex}m",
            TerminalColors.ExpandBackground(terminal, lastIndex));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalColors.ExpandForeground(terminal, colors));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalColors.ExpandBackground(terminal, -1));

        AssertPaletteControls(terminal);
    }

    private static void AssertPaletteControls(TerminalDescription terminal)
    {
        Assert.Equal(
            "\u001b[39;49m",
            terminal.GetRequiredString(StringCapability.OriginalColorPair));
        Assert.Equal(
            InitializeColor,
            terminal.GetRequiredString(StringCapability.InitializeColor));
        Assert.Equal(
            OriginalColors,
            terminal.GetRequiredString(StringCapability.OriginalColors));
        Assert.Equal(
            ResetWithOriginalColors,
            terminal.GetRequiredString(StringCapability.ResetString1));
        Assert.Equal(
            "\u001b]4;12;rgb:FF/7F/00\u001b\\",
            terminal.Expand(
                StringCapability.InitializeColor,
                12,
                1000,
                500,
                0));
    }

    private static bool IsColorSpecificString(StringCapability capability)
    {
        return capability is
            StringCapability.SetForegroundColor
            or StringCapability.SetBackgroundColor
            or StringCapability.SetLegacyForegroundColor
            or StringCapability.SetLegacyBackgroundColor
            or StringCapability.InitializeColor
            or StringCapability.OriginalColors
            or StringCapability.ResetString1;
    }
}
