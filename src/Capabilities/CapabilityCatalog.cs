namespace Icod.TermInfo;

internal static class CapabilityCatalog
{
    private static readonly IReadOnlyDictionary<string, BooleanCapability> BooleanCapabilities =
        new Dictionary<string, BooleanCapability>(StringComparer.Ordinal)
        {
            ["am"] = BooleanCapability.AutoRightMargin,
            ["gn"] = BooleanCapability.GenericType,
        };

    private static readonly IReadOnlyDictionary<string, NumericCapability> NumericCapabilities =
        new Dictionary<string, NumericCapability>(StringComparer.Ordinal)
        {
            ["cols"] = NumericCapability.Columns,
            ["lines"] = NumericCapability.Lines,
            ["colors"] = NumericCapability.Colors,
            ["pairs"] = NumericCapability.ColorPairs,
        };

    private static readonly IReadOnlyDictionary<string, StringCapability> StringCapabilities =
        new Dictionary<string, StringCapability>(StringComparer.Ordinal)
        {
            ["bel"] = StringCapability.Bell,
            ["cr"] = StringCapability.CarriageReturn,
            ["cud1"] = StringCapability.CursorDownOne,
            ["ind"] = StringCapability.ScrollForward,
            ["clear"] = StringCapability.ClearScreen,
            ["cup"] = StringCapability.CursorAddress,
            ["bold"] = StringCapability.EnterBoldMode,
            ["sgr0"] = StringCapability.ExitAttributeMode,
            ["setaf"] = StringCapability.SetForegroundColor,
            ["setab"] = StringCapability.SetBackgroundColor,
        };

    internal static bool TryGetBoolean(
        string name,
        out BooleanCapability capability)
    {
        ArgumentNullException.ThrowIfNull(name);

        return BooleanCapabilities.TryGetValue(name, out capability);
    }

    internal static bool TryGetNumeric(
        string name,
        out NumericCapability capability)
    {
        ArgumentNullException.ThrowIfNull(name);

        return NumericCapabilities.TryGetValue(name, out capability);
    }

    internal static bool TryGetString(
        string name,
        out StringCapability capability)
    {
        ArgumentNullException.ThrowIfNull(name);

        return StringCapabilities.TryGetValue(name, out capability);
    }
}
