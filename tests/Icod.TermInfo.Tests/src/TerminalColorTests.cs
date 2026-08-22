using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class TerminalColorTests
{
    [Fact]
    public void ProviderCreatedFourColorDescriptionUsesSemanticColorCore()
    {
        TerminalDescription expected =
            new TerminalDescriptionBuilder("four-color")
                .SetBoolean(BooleanCapability.BackColorErase)
                .SetBoolean(BooleanCapability.CanChangeColor)
                .SetBoolean(BooleanCapability.HueLightnessSaturation)
                .SetNumber(NumericCapability.Colors, 4)
                .SetNumber(NumericCapability.ColorPairs, 7)
                .SetNumber(NumericCapability.NoColorVideo, 5)
                .SetString(StringCapability.SetForegroundColor, "F%p1%d")
                .SetString(StringCapability.SetBackgroundColor, "B%p1%d")
                .SetString(
                    StringCapability.InitializeColor,
                    "I%p1%d;%p2%d;%p3%d;%p4%d")
                .SetString(StringCapability.OriginalColorPair, "P")
                .SetString(StringCapability.OriginalColors, "O")
                .Build();
        InMemoryTerminalDescriptionProvider provider =
            new(new[] { expected });

        Assert.True(
            provider.TryLoad(
                "four-color",
                out TerminalDescription? terminal));
        Assert.Same(expected, terminal);

        TerminalColorSupport support =
            TerminalColors.GetColorSupport(terminal!);

        Assert.Equal(TerminalColorModel.Indexed, support.Model);
        Assert.Equal(TerminalColorTier.Color4, support.Tier);
        Assert.Equal<int?>(4, support.ColorCount);
        Assert.Equal(4, support.IndexedColorCount);
        Assert.Equal<int?>(7, support.ColorPairCount);
        Assert.Equal<int?>(5, support.NoColorVideoMask);
        Assert.Null(support.RgbLayout);
        Assert.True(support.HasForegroundSelector);
        Assert.True(support.HasBackgroundSelector);
        Assert.True(support.BackColorErase);
        Assert.True(support.CanChangeColor);
        Assert.True(support.UsesHlsColorInitialization);
        Assert.True(support.HasInitializeColor);
        Assert.True(support.HasOriginalColorPair);
        Assert.True(support.HasOriginalColors);

        Assert.Equal("F0", TerminalColors.ExpandForeground(terminal!, 0));
        Assert.Equal("F3", TerminalColors.ExpandForeground(terminal!, 3));
        Assert.Equal("B0", TerminalColors.ExpandBackground(terminal!, 0));
        Assert.Equal("B3", TerminalColors.ExpandBackground(terminal!, 3));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalColors.ExpandForeground(terminal!, -1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalColors.ExpandBackground(terminal!, 4));
    }

    [Fact]
    public void AnsiRemainsExactlyEightColorIndexed()
    {
        TerminalColorSupport support =
            TerminalColors.GetColorSupport(TerminalProfiles.Ansi);

        Assert.Equal(TerminalColorModel.Indexed, support.Model);
        Assert.Equal(TerminalColorTier.Color8, support.Tier);
        Assert.Equal<int?>(8, support.ColorCount);
        Assert.Equal(8, support.IndexedColorCount);
        Assert.Equal<int?>(64, support.ColorPairCount);
        Assert.Equal<int?>(3, support.NoColorVideoMask);
        Assert.Null(support.RgbLayout);

        Assert.Equal(
            "\x1b[30m",
            TerminalColors.ExpandForeground(TerminalProfiles.Ansi, 0));
        Assert.Equal(
            "\x1b[37m",
            TerminalColors.ExpandForeground(TerminalProfiles.Ansi, 7));
        Assert.Equal(
            "\x1b[40m",
            TerminalColors.ExpandBackground(TerminalProfiles.Ansi, 0));
        Assert.Equal(
            "\x1b[47m",
            TerminalColors.ExpandBackground(TerminalProfiles.Ansi, 7));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalColors.ExpandForeground(TerminalProfiles.Ansi, 8));
        Assert.Throws<InvalidOperationException>(
            () => TerminalColors.ExpandForeground(
                TerminalProfiles.Ansi,
                new TerminalRgbColor(1, 2, 3)));
    }

    [Fact]
    public void Vt100RemainsMonochrome()
    {
        TerminalColorSupport support =
            TerminalColors.GetColorSupport(TerminalProfiles.Vt100);

        Assert.Equal(TerminalColorModel.None, support.Model);
        Assert.Equal(TerminalColorTier.Monochrome, support.Tier);
        Assert.Equal(0, support.IndexedColorCount);
        Assert.Null(support.RgbLayout);

        Assert.Throws<InvalidOperationException>(
            () => TerminalColors.ExpandForeground(TerminalProfiles.Vt100, 0));
    }

    [Theory]
    [InlineData(16, TerminalColorTier.Color16)]
    [InlineData(17, TerminalColorTier.OtherIndexed)]
    [InlineData(42, TerminalColorTier.OtherIndexed)]
    [InlineData(88, TerminalColorTier.OtherIndexed)]
    [InlineData(256, TerminalColorTier.Color256)]
    public void ArbitraryIndexedCountsAreSupported(
        int colors,
        TerminalColorTier expectedTier)
    {
        TerminalDescription terminal = CreateIndexedTerminal(colors);

        TerminalColorSupport support =
            TerminalColors.GetColorSupport(terminal);

        Assert.Equal(TerminalColorModel.Indexed, support.Model);
        Assert.Equal(expectedTier, support.Tier);
        Assert.Equal<int?>(colors, support.ColorCount);
        Assert.Equal(colors, support.IndexedColorCount);
        Assert.Equal(
            $"F{colors - 1}",
            TerminalColors.ExpandForeground(terminal, colors - 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalColors.ExpandForeground(terminal, colors));
    }

    [Fact]
    public void BooleanRgbDerivesEightEightEightTrueColorAndRetainsCoPrefix()
    {
        TerminalDescription terminal =
            CreateDirectTerminal("direct-boolean", 1 << 24)
                .SetNumber(NumericCapability.ColorPairs, 65536)
                .SetExtendedBoolean("RGB")
                .SetExtendedNumber("CO", 8)
                .Build();

        TerminalColorSupport support =
            TerminalColors.GetColorSupport(terminal);

        Assert.Equal(TerminalColorModel.DirectRgb, support.Model);
        Assert.Equal(TerminalColorTier.TrueColor, support.Tier);
        Assert.Equal<int?>(1 << 24, support.ColorCount);
        Assert.Equal<int?>(65536, support.ColorPairCount);
        Assert.Equal(8, support.IndexedColorCount);
        Assert.Equal<TerminalRgbLayout?>(
            new TerminalRgbLayout(8, 8, 8),
            support.RgbLayout);

        Assert.Equal(
            "F1193046",
            TerminalColors.ExpandForeground(
                terminal,
                new TerminalRgbColor(0x12, 0x34, 0x56)));
        Assert.Equal(
            "B16777215",
            TerminalColors.ExpandBackground(
                terminal,
                new TerminalRgbColor(0xff, 0xff, 0xff)));
        Assert.Equal(
            "F0",
            TerminalColors.ExpandForeground(
                terminal,
                new TerminalRgbColor(0, 0, 0)));
        Assert.Equal(
            "F16711680",
            TerminalColors.ExpandForeground(
                terminal,
                new TerminalRgbColor(255, 0, 0)));
        Assert.Equal(
            "F65280",
            TerminalColors.ExpandForeground(
                terminal,
                new TerminalRgbColor(0, 255, 0)));
        Assert.Equal(
            "F255",
            TerminalColors.ExpandForeground(
                terminal,
                new TerminalRgbColor(0, 0, 255)));

        Assert.Equal("F7", TerminalColors.ExpandForeground(terminal, 7));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalColors.ExpandForeground(terminal, 8));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalColors.ExpandForeground(
                terminal,
                new TerminalRgbColor(0, 0, 1)));
    }

    [Fact]
    public void NumericRgbDescribesEqualChannelWidthsWithoutAssumingEightBits()
    {
        TerminalDescription terminal =
            CreateDirectTerminal("direct-444", 4096)
                .SetExtendedNumber("RGB", 4)
                .Build();

        TerminalColorSupport support =
            TerminalColors.GetColorSupport(terminal);

        Assert.Equal(TerminalColorModel.DirectRgb, support.Model);
        Assert.Equal(TerminalColorTier.OtherDirectRgb, support.Tier);
        Assert.Equal<TerminalRgbLayout?>(
            new TerminalRgbLayout(4, 4, 4),
            support.RgbLayout);
        Assert.Equal(
            "F3840",
            TerminalColors.ExpandForeground(
                terminal,
                new TerminalRgbColor(255, 0, 0)));
        Assert.Throws<InvalidOperationException>(
            () => TerminalColors.ExpandForeground(terminal, 0));
    }

    [Fact]
    public void StringRgbDescribesExplicitChannelWidths()
    {
        TerminalDescription terminal =
            CreateDirectTerminal("direct-565", 65536)
                .SetExtendedString("RGB", "5/6/5")
                .Build();

        TerminalColorSupport support =
            TerminalColors.GetColorSupport(terminal);
        TerminalRgbLayout expected = new(5, 6, 5);

        Assert.Equal(TerminalColorModel.DirectRgb, support.Model);
        Assert.Equal(TerminalColorTier.OtherDirectRgb, support.Tier);
        Assert.Equal<TerminalRgbLayout?>(expected, support.RgbLayout);
        Assert.Equal(0xf800, expected.Pack(new TerminalRgbColor(255, 0, 0)));
        Assert.Equal(0x07e0, expected.Pack(new TerminalRgbColor(0, 255, 0)));
        Assert.Equal(0x001f, expected.Pack(new TerminalRgbColor(0, 0, 255)));
        Assert.Equal(0xffff, expected.Pack(new TerminalRgbColor(255, 255, 255)));
    }

    [Theory]
    [InlineData("8/8")]
    [InlineData("8/8/8/8")]
    [InlineData("8/x/8")]
    [InlineData("8/-1/8")]
    [InlineData("0/0/0")]
    [InlineData(" 8/8/8")]
    public void MalformedStringRgbMetadataIsRejected(string rgb)
    {
        TerminalDescription terminal =
            CreateDirectTerminal("malformed-rgb", 1 << 24)
                .SetExtendedString("RGB", rgb)
                .Build();

        Assert.Throws<InvalidOperationException>(
            () => TerminalColors.GetColorSupport(terminal));
    }

    [Fact]
    public void DirectRgbRequiresStandardSetafAndSetabSelectors()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("direct-without-setab")
                .SetNumber(NumericCapability.Colors, 1 << 24)
                .SetString(StringCapability.SetForegroundColor, "F%p1%d")
                .SetExtendedNumber("RGB", 8)
                .Build();

        Assert.Throws<InvalidOperationException>(
            () => TerminalColors.GetColorSupport(terminal));
    }

    [Fact]
    public void DirectCoMustBeNumericAndWithinAdvertisedColorRange()
    {
        TerminalDescription wrongType =
            CreateDirectTerminal("direct-co-string", 4096)
                .SetExtendedNumber("RGB", 4)
                .SetExtendedString("CO", "8")
                .Build();
        TerminalDescription tooLarge =
            CreateDirectTerminal("direct-co-large", 4096)
                .SetExtendedNumber("RGB", 4)
                .SetExtendedNumber("CO", 4096)
                .Build();

        Assert.Throws<InvalidOperationException>(
            () => TerminalColors.GetColorSupport(wrongType));
        Assert.Throws<InvalidOperationException>(
            () => TerminalColors.GetColorSupport(tooLarge));
    }

    [Fact]
    public void NegativeRawColorMetadataIsRejected()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("negative-colors")
                .SetNumber(NumericCapability.Colors, -1)
                .SetString(StringCapability.SetForegroundColor, "F%p1%d")
                .Build();

        Assert.Throws<InvalidOperationException>(
            () => TerminalColors.GetColorSupport(terminal));
    }

    [Fact]
    public void MissingOneIndexedSelectorPreservesDirectionSpecificBehavior()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("foreground-only")
                .SetNumber(NumericCapability.Colors, 4)
                .SetString(StringCapability.SetForegroundColor, "F%p1%d")
                .Build();
        TerminalColorSupport support =
            TerminalColors.GetColorSupport(terminal);

        Assert.Equal(TerminalColorModel.Indexed, support.Model);
        Assert.True(support.HasForegroundSelector);
        Assert.False(support.HasBackgroundSelector);
        Assert.Equal("F2", TerminalColors.ExpandForeground(terminal, 2));
        Assert.Throws<InvalidOperationException>(
            () => TerminalColors.ExpandBackground(terminal, 2));
    }

    [Fact]
    public void LegacyIndexedSelectorsRemainUsable()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("legacy-color")
                .SetNumber(NumericCapability.Colors, 8)
                .SetString(StringCapability.SetLegacyForegroundColor, "f%p1%d")
                .SetString(StringCapability.SetLegacyBackgroundColor, "b%p1%d")
                .Build();

        TerminalColorSupport support =
            TerminalColors.GetColorSupport(terminal);

        Assert.Equal(TerminalColorModel.Indexed, support.Model);
        Assert.Equal(TerminalColorTier.Color8, support.Tier);
        Assert.Equal("f4", TerminalColors.ExpandForeground(terminal, 4));
        Assert.Equal("b4", TerminalColors.ExpandBackground(terminal, 4));
    }

    [Fact]
    public void PublicHelpersValidateTerminalArguments()
    {
        Assert.Throws<ArgumentNullException>(
            () => TerminalColors.GetColorSupport(null!));
        Assert.Throws<ArgumentNullException>(
            () => TerminalColors.ExpandForeground(null!, 0));
        Assert.Throws<ArgumentNullException>(
            () => TerminalColors.ExpandBackground(null!, 0));
        Assert.Throws<ArgumentNullException>(
            () => TerminalColors.ExpandForeground(
                null!,
                new TerminalRgbColor(0, 0, 0)));
        Assert.Throws<ArgumentNullException>(
            () => TerminalColors.ExpandBackground(
                null!,
                new TerminalRgbColor(0, 0, 0)));
    }

    private static TerminalDescription CreateIndexedTerminal(int colors)
    {
        return new TerminalDescriptionBuilder($"indexed-{colors}")
            .SetNumber(NumericCapability.Colors, colors)
            .SetString(StringCapability.SetForegroundColor, "F%p1%d")
            .SetString(StringCapability.SetBackgroundColor, "B%p1%d")
            .Build();
    }

    private static TerminalDescriptionBuilder CreateDirectTerminal(
        string name,
        int colors)
    {
        return new TerminalDescriptionBuilder(name)
            .SetNumber(NumericCapability.Colors, colors)
            .SetString(StringCapability.SetForegroundColor, "F%p1%d")
            .SetString(StringCapability.SetBackgroundColor, "B%p1%d");
    }
}
