namespace Icod.TermInfo;

internal static class XtermDirectColorCapabilityFragments
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

    internal static TerminalDescriptionBuilder ApplyXtermDirectEightColor(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return ApplyXtermDirectColor(
            builder,
            8,
            DirectForeground,
            DirectBackground);
    }

    internal static TerminalDescriptionBuilder ApplyXtermDirectSixteenColor(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return ApplyXtermDirectColor(
            builder,
            16,
            Direct16Foreground,
            Direct16Background);
    }

    internal static TerminalDescriptionBuilder ApplyXtermDirect256Color(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return ApplyXtermDirectColor(
            builder,
            256,
            Direct256Foreground,
            Direct256Background);
    }

    private static TerminalDescriptionBuilder ApplyXtermDirectColor(
        TerminalDescriptionBuilder builder,
        int indexedPrefixCount,
        string foreground,
        string background)
    {
        return builder
            .SetBoolean(BooleanCapability.CanChangeColor, false)
            .SetNumber(NumericCapability.Colors, DirectColorCount)
            .SetNumber(NumericCapability.ColorPairs, DirectColorPairCount)
            .SetString(StringCapability.OriginalColorPair, "\u001b[39;49m")
            .SetString(StringCapability.SetForegroundColor, foreground)
            .SetString(StringCapability.SetBackgroundColor, background)
            .RemoveString(StringCapability.InitializeColor)
            .RemoveString(StringCapability.OriginalColors)
            .RemoveString(StringCapability.SetLegacyForegroundColor)
            .RemoveString(StringCapability.SetLegacyBackgroundColor)
            .SetExtendedBoolean("RGB")
            .SetExtendedNumber("CO", indexedPrefixCount);
    }
}
