namespace Icod.TermInfo;

internal static class XtermColorCapabilityFragments
{
    private const string LegacyForeground =
        "\u001b[3%?%p1%{1}%=%t4%e%p1%{3}%=%t6%e"
        + "%p1%{4}%=%t1%e%p1%{6}%=%t3%e%p1%d%;m";

    private const string LegacyBackground =
        "\u001b[4%?%p1%{1}%=%t4%e%p1%{3}%=%t6%e"
        + "%p1%{4}%=%t1%e%p1%{6}%=%t3%e%p1%d%;m";

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
}
