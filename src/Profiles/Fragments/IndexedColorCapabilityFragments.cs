namespace Icod.TermInfo;

internal static class IndexedColorCapabilityFragments
{
    private const string AnsiForeground = "\x1b[3%p1%dm";
    private const string AnsiBackground = "\x1b[4%p1%dm";

    private const string AixtermForeground =
        "\x1b[%?%p1%{8}%<%t%p1%{30}%+%e%p1%{82}%+%;%dm";
    private const string AixtermBackground =
        "\x1b[%?%p1%{8}%<%t%p1%{40}%+%e%p1%{92}%+%;%dm";

    private const string XtermIndexedForeground =
        "\x1b[%?%p1%{8}%<%t3%p1%d%e%p1%{16}%<%t9%p1%{8}%-%d"
        + "%e38;5;%p1%d%;m";
    private const string XtermIndexedBackground =
        "\x1b[%?%p1%{8}%<%t4%p1%d%e%p1%{16}%<%t10%p1%{8}%-%d"
        + "%e48;5;%p1%d%;m";

    private const string XtermInitializeColor =
        "\x1b]4;%p1%d;rgb:%p2%{255}%*%{1000}%/%2.2X/"
        + "%p3%{255}%*%{1000}%/%2.2X/"
        + "%p4%{255}%*%{1000}%/%2.2X\x1b\\";
    private const string XtermOriginalColors = "\x1b]104\x1b\\";

    internal static TerminalDescriptionBuilder ApplyAnsiIndexed(
        this TerminalDescriptionBuilder builder,
        int colorCount,
        int colorPairCount)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ValidateColorPairCount(colorPairCount);

        if (colorCount is < 1 or > 8)
        {
            throw new ArgumentOutOfRangeException(
                nameof(colorCount),
                colorCount,
                "ANSI indexed-color fragments support between one and eight colors.");
        }

        return builder
            .SetNumber(NumericCapability.Colors, colorCount)
            .SetNumber(NumericCapability.ColorPairs, colorPairCount)
            .SetString(StringCapability.SetForegroundColor, AnsiForeground)
            .SetString(StringCapability.SetBackgroundColor, AnsiBackground);
    }

    internal static TerminalDescriptionBuilder ApplyAixterm16(
        this TerminalDescriptionBuilder builder,
        int colorPairCount)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ValidateColorPairCount(colorPairCount);

        return builder
            .SetNumber(NumericCapability.Colors, 16)
            .SetNumber(NumericCapability.ColorPairs, colorPairCount)
            .SetString(StringCapability.SetForegroundColor, AixtermForeground)
            .SetString(StringCapability.SetBackgroundColor, AixtermBackground);
    }

    internal static TerminalDescriptionBuilder ApplyXtermExtendedIndexed(
        this TerminalDescriptionBuilder builder,
        int colorCount,
        int colorPairCount)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ValidateColorPairCount(colorPairCount);

        if (colorCount is <= 16 or > 256)
        {
            throw new ArgumentOutOfRangeException(
                nameof(colorCount),
                colorCount,
                "The xterm extended indexed-color fragment supports 17 through 256 colors.");
        }

        return builder
            .SetNumber(NumericCapability.Colors, colorCount)
            .SetNumber(NumericCapability.ColorPairs, colorPairCount)
            .SetString(
                StringCapability.SetForegroundColor,
                XtermIndexedForeground)
            .SetString(
                StringCapability.SetBackgroundColor,
                XtermIndexedBackground);
    }

    internal static TerminalDescriptionBuilder ApplyXtermPaletteControls(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .SetBoolean(BooleanCapability.CanChangeColor)
            .SetString(StringCapability.InitializeColor, XtermInitializeColor)
            .SetString(StringCapability.OriginalColors, XtermOriginalColors);
    }

    private static void ValidateColorPairCount(int colorPairCount)
    {
        if (colorPairCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(colorPairCount),
                colorPairCount,
                "The color-pair count cannot be negative.");
        }
    }
}
