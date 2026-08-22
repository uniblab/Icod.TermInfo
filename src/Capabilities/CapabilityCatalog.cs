namespace Icod.TermInfo;

internal static class CapabilityCatalog
{
    private static readonly IReadOnlyDictionary<string, BooleanCapability> BooleanCapabilities =
        new Dictionary<string, BooleanCapability>(StringComparer.Ordinal)
        {
            ["am"] = BooleanCapability.AutoRightMargin,
            ["gn"] = BooleanCapability.GenericType,
            ["msgr"] = BooleanCapability.MoveStandoutMode,
            ["xenl"] = BooleanCapability.EatNewlineGlitch,
            ["xon"] = BooleanCapability.XonXoff,
            ["mir"] = BooleanCapability.MoveInsertMode,
        };

    private static readonly IReadOnlyDictionary<string, NumericCapability> NumericCapabilities =
        new Dictionary<string, NumericCapability>(StringComparer.Ordinal)
        {
            ["cols"] = NumericCapability.Columns,
            ["lines"] = NumericCapability.Lines,
            ["colors"] = NumericCapability.Colors,
            ["pairs"] = NumericCapability.ColorPairs,
            ["it"] = NumericCapability.InitialTabWidth,
            ["vt"] = NumericCapability.VirtualTerminal,
            ["ncv"] = NumericCapability.NoColorVideo,
        };

    private static readonly IReadOnlyDictionary<string, StringCapability> StringCapabilities =
        new Dictionary<string, StringCapability>(StringComparer.Ordinal)
        {
            ["bel"] = StringCapability.Bell,
            ["cbt"] = StringCapability.BackTab,
            ["blink"] = StringCapability.EnterBlinkMode,
            ["bold"] = StringCapability.EnterBoldMode,
            ["dim"] = StringCapability.EnterDimMode,
            ["cr"] = StringCapability.CarriageReturn,
            ["csr"] = StringCapability.ChangeScrollRegion,
            ["clear"] = StringCapability.ClearScreen,
            ["cub"] = StringCapability.CursorLeft,
            ["cub1"] = StringCapability.CursorLeftOne,
            ["cud"] = StringCapability.CursorDown,
            ["cud1"] = StringCapability.CursorDownOne,
            ["cuf"] = StringCapability.CursorRight,
            ["cuf1"] = StringCapability.CursorRightOne,
            ["cup"] = StringCapability.CursorAddress,
            ["cuu"] = StringCapability.CursorUp,
            ["cuu1"] = StringCapability.CursorUpOne,
            ["dch"] = StringCapability.DeleteCharacters,
            ["dch1"] = StringCapability.DeleteCharacter,
            ["dl"] = StringCapability.DeleteLines,
            ["dl1"] = StringCapability.DeleteLine,
            ["ed"] = StringCapability.ClearToEndOfScreen,
            ["el"] = StringCapability.ClearToEndOfLine,
            ["el1"] = StringCapability.ClearToBeginningOfLine,
            ["home"] = StringCapability.CursorHome,
            ["hpa"] = StringCapability.ColumnAddress,
            ["ht"] = StringCapability.Tab,
            ["hts"] = StringCapability.SetTab,
            ["ich"] = StringCapability.InsertCharacters,
            ["ich1"] = StringCapability.InsertCharacter,
            ["il"] = StringCapability.InsertLines,
            ["il1"] = StringCapability.InsertLine,
            ["ind"] = StringCapability.ScrollForward,
            ["invis"] = StringCapability.EnterInvisibleMode,
            ["op"] = StringCapability.OriginalColorPair,
            ["rc"] = StringCapability.RestoreCursor,
            ["rev"] = StringCapability.EnterReverseMode,
            ["ri"] = StringCapability.ScrollReverse,
            ["rmacs"] = StringCapability.ExitAlternateCharacterSetMode,
            ["rmam"] = StringCapability.ExitAutomaticMargins,
            ["rmkx"] = StringCapability.ExitKeypadMode,
            ["rmso"] = StringCapability.ExitStandoutMode,
            ["rmul"] = StringCapability.ExitUnderlineMode,
            ["sc"] = StringCapability.SaveCursor,
            ["setab"] = StringCapability.SetBackgroundColor,
            ["setaf"] = StringCapability.SetForegroundColor,
            ["sgr"] = StringCapability.SetAttributes,
            ["sgr0"] = StringCapability.ExitAttributeMode,
            ["smacs"] = StringCapability.EnterAlternateCharacterSetMode,
            ["smam"] = StringCapability.EnterAutomaticMargins,
            ["smkx"] = StringCapability.EnterKeypadMode,
            ["smso"] = StringCapability.EnterStandoutMode,
            ["smul"] = StringCapability.EnterUnderlineMode,
            ["vpa"] = StringCapability.RowAddress,
            ["acsc"] = StringCapability.AlternateCharacterSet,
            ["enacs"] = StringCapability.EnableAlternateCharacterSet,
            ["kbs"] = StringCapability.KeyBackspace,
            ["kcud1"] = StringCapability.KeyCursorDown,
            ["kcub1"] = StringCapability.KeyCursorLeft,
            ["kcuf1"] = StringCapability.KeyCursorRight,
            ["kcuu1"] = StringCapability.KeyCursorUp,
            ["khome"] = StringCapability.KeyHome,
            ["kf1"] = StringCapability.KeyF1,
            ["kf2"] = StringCapability.KeyF2,
            ["kf3"] = StringCapability.KeyF3,
            ["kf4"] = StringCapability.KeyF4,
            ["rs2"] = StringCapability.ResetString2,
            ["ech"] = StringCapability.EraseCharacters,
            ["tbc"] = StringCapability.ClearAllTabs,
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
