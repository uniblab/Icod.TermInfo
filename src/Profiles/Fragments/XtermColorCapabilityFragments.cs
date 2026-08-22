namespace Icod.TermInfo;

internal static class XtermColorCapabilityFragments
{
    private const string LegacyForeground =
        "\u001b[3%?%p1%{1}%=%t4%e%p1%{3}%=%t6%e"
        + "%p1%{4}%=%t1%e%p1%{6}%=%t3%e%p1%d%;m";

    private const string LegacyBackground =
        "\u001b[4%?%p1%{1}%=%t4%e%p1%{3}%=%t6%e"
        + "%p1%{4}%=%t1%e%p1%{6}%=%t3%e%p1%d%;m";

    private const string AixtermLegacyForeground =
        "%p1%{8}%/%{6}%*%{3}%+\u001b[%d%p1%{8}%m%Pa%?%ga%{1}%=%t4%e"
        + "%ga%{3}%=%t6%e%ga%{4}%=%t1%e%ga%{6}%=%t3%e%ga%d%;m";

    private const string AixtermLegacyBackground =
        "%p1%{8}%/%{6}%*%{4}%+\u001b[%d%p1%{8}%m%Pa%?%ga%{1}%=%t4%e"
        + "%ga%{3}%=%t6%e%ga%{4}%=%t1%e%ga%{6}%=%t3%e%ga%d%;m";

    internal static TerminalDescriptionBuilder ApplyXtermBasicEightColor(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .ApplyAnsiIndexed(8, 64)
            .SetString(
                StringCapability.SetLegacyForegroundColor,
                LegacyForeground)
            .SetString(
                StringCapability.SetLegacyBackgroundColor,
                LegacyBackground);
    }

    internal static TerminalDescriptionBuilder ApplyXtermSixteenColor(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .ApplyAixterm16(256)
            .ApplyXtermPaletteControls()
            .SetString(
                StringCapability.SetLegacyForegroundColor,
                AixtermLegacyForeground)
            .SetString(
                StringCapability.SetLegacyBackgroundColor,
                AixtermLegacyBackground);
    }
}
