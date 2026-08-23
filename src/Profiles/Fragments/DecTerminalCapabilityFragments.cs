namespace Icod.TermInfo;

internal static class DecTerminalCapabilityFragments
{
    // The VT220 core tracks the canonical seven-bit ncurses profile within the
    // current 0.7 typed vocabulary. Host tabset paths and function-key labels
    // are intentionally not modeled as process-local filesystem metadata.
    private const string DecSpecialGraphics =
        "``aaffggjjkkllmmnnooppqqrrssttuuvvwwxxyyzz{{||}}~~";

    internal static TerminalDescriptionBuilder ApplyVt100Core(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .SetBoolean(BooleanCapability.AutoRightMargin)
            .SetBoolean(BooleanCapability.MoveStandoutMode)
            .SetBoolean(BooleanCapability.EatNewlineGlitch)
            .SetBoolean(BooleanCapability.XonXoff)
            .SetNumber(NumericCapability.Columns, 80)
            .SetNumber(NumericCapability.Lines, 24)
            .SetNumber(NumericCapability.InitialTabWidth, 8)
            .SetNumber(NumericCapability.VirtualTerminal, 3)
            .SetString(StringCapability.Bell, "\a")
            .SetString(StringCapability.CarriageReturn, "\r")
            .SetString(
                StringCapability.ChangeScrollRegion,
                "\u001b[%i%p1%d;%p2%dr")
            .SetString(
                StringCapability.ClearScreen,
                "\u001b[H\u001b[J$<50>")
            .SetString(StringCapability.CursorLeft, "\u001b[%p1%dD")
            .SetString(StringCapability.CursorLeftOne, "\b")
            .SetString(StringCapability.CursorDown, "\u001b[%p1%dB")
            .SetString(StringCapability.CursorDownOne, "\n")
            .SetString(StringCapability.CursorRight, "\u001b[%p1%dC")
            .SetString(StringCapability.CursorRightOne, "\u001b[C$<2>")
            .SetString(StringCapability.CursorUp, "\u001b[%p1%dA")
            .SetString(StringCapability.CursorUpOne, "\u001b[A$<2>")
            .SetString(
                StringCapability.CursorAddress,
                "\u001b[%i%p1%d;%p2%dH$<5>")
            .SetString(StringCapability.CursorHome, "\u001b[H")
            .SetString(StringCapability.SaveCursor, "\u001b7")
            .SetString(StringCapability.RestoreCursor, "\u001b8")
            .SetString(
                StringCapability.ClearToEndOfScreen,
                "\u001b[J$<50>")
            .SetString(
                StringCapability.ClearToEndOfLine,
                "\u001b[K$<3>")
            .SetString(
                StringCapability.ClearToBeginningOfLine,
                "\u001b[1K$<3>")
            .SetString(StringCapability.Tab, "\t")
            .SetString(StringCapability.SetTab, "\u001bH")
            .SetString(StringCapability.ClearAllTabs, "\u001b[3g")
            .SetString(StringCapability.ScrollForward, "\n")
            .SetString(
                StringCapability.ScrollReverse,
                "\u001bM$<5>")
            .SetString(
                StringCapability.EnterBlinkMode,
                "\u001b[5m$<2>")
            .SetString(
                StringCapability.EnterBoldMode,
                "\u001b[1m$<2>")
            .SetString(
                StringCapability.EnterReverseMode,
                "\u001b[7m$<2>")
            .SetString(
                StringCapability.EnterStandoutMode,
                "\u001b[7m$<2>")
            .SetString(
                StringCapability.ExitStandoutMode,
                "\u001b[m$<2>")
            .SetString(
                StringCapability.EnterUnderlineMode,
                "\u001b[4m$<2>")
            .SetString(
                StringCapability.ExitUnderlineMode,
                "\u001b[m$<2>")
            .SetString(
                StringCapability.AlternateCharacterSet,
                DecSpecialGraphics)
            .SetString(
                StringCapability.EnableAlternateCharacterSet,
                "\u001b(B\u001b)0")
            .SetString(
                StringCapability.EnterAlternateCharacterSetMode,
                "\u000e")
            .SetString(
                StringCapability.ExitAlternateCharacterSetMode,
                "\u000f")
            .SetString(
                StringCapability.SetAttributes,
                "\u001b[0%?%p1%p6%|%t;1%;%?%p2%t;4%;"
                + "%?%p1%p3%|%t;7%;%?%p4%t;5%;m"
                + "%?%p9%t\u000e%e\u000f%;$<2>")
            .SetString(
                StringCapability.ExitAttributeMode,
                "\u001b[m\u000f$<2>")
            .SetString(
                StringCapability.ExitAutomaticMargins,
                "\u001b[?7l")
            .SetString(
                StringCapability.EnterAutomaticMargins,
                "\u001b[?7h")
            .SetString(
                StringCapability.ExitKeypadMode,
                "\u001b[?1l\u001b>")
            .SetString(
                StringCapability.EnterKeypadMode,
                "\u001b[?1h\u001b=")
            .SetString(
                StringCapability.ResetString2,
                "\u001b<\u001b>\u001b[?3;4;5l\u001b[?7;8h\u001b[r")
            .SetString(StringCapability.KeyBackspace, "\b")
            .SetString(StringCapability.KeyCursorDown, "\u001bOB")
            .SetString(StringCapability.KeyCursorLeft, "\u001bOD")
            .SetString(StringCapability.KeyCursorRight, "\u001bOC")
            .SetString(StringCapability.KeyCursorUp, "\u001bOA")
            .SetString(StringCapability.KeyF1, "\u001bOP")
            .SetString(StringCapability.KeyF2, "\u001bOQ")
            .SetString(StringCapability.KeyF3, "\u001bOR")
            .SetString(StringCapability.KeyF4, "\u001bOS");
    }

    internal static TerminalDescriptionBuilder ApplyVt102Editing(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .SetString(StringCapability.DeleteCharacter, "\u001b[P")
            .SetString(StringCapability.DeleteLine, "\u001b[M")
            .SetString(StringCapability.InsertLine, "\u001b[L")
            .SetString(StringCapability.ExitInsertMode, "\u001b[4l")
            .SetString(StringCapability.EnterInsertMode, "\u001b[4h");
    }

    internal static TerminalDescriptionBuilder ApplyVt220Core(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .SetBoolean(BooleanCapability.BackspacesWithBs)
            .SetBoolean(BooleanCapability.AutoRightMargin)
            .SetBoolean(BooleanCapability.MoveInsertMode)
            .SetBoolean(BooleanCapability.MoveStandoutMode)
            .SetBoolean(BooleanCapability.EatNewlineGlitch)
            .SetBoolean(BooleanCapability.XonXoff)
            .SetNumber(NumericCapability.Columns, 80)
            .SetNumber(NumericCapability.Lines, 24)
            .SetNumber(NumericCapability.InitialTabWidth, 8)
            .SetNumber(NumericCapability.VirtualTerminal, 3)
            .SetString(StringCapability.Bell, "\a")
            .SetString(StringCapability.CarriageReturn, "\r")
            .SetString(
                StringCapability.ChangeScrollRegion,
                "\u001b[%i%p1%d;%p2%dr")
            .SetString(
                StringCapability.ClearScreen,
                "\u001b[H\u001b[J")
            .SetString(StringCapability.CursorLeft, "\u001b[%p1%dD")
            .SetString(StringCapability.CursorLeftOne, "\b")
            .SetString(StringCapability.CursorDown, "\u001b[%p1%dB")
            .SetString(StringCapability.CursorDownOne, "\n")
            .SetString(StringCapability.CursorRight, "\u001b[%p1%dC")
            .SetString(StringCapability.CursorRightOne, "\u001b[C")
            .SetString(StringCapability.CursorUp, "\u001b[%p1%dA")
            .SetString(StringCapability.CursorUpOne, "\u001b[A")
            .SetString(
                StringCapability.CursorAddress,
                "\u001b[%i%p1%d;%p2%dH")
            .SetString(StringCapability.CursorHome, "\u001b[H")
            .SetString(StringCapability.SaveCursor, "\u001b7")
            .SetString(StringCapability.RestoreCursor, "\u001b8")
            .SetString(StringCapability.DeleteCharacters, "\u001b[%p1%dP")
            .SetString(StringCapability.DeleteCharacter, "\u001b[P")
            .SetString(StringCapability.InsertCharacters, "\u001b[%p1%d@")
            .SetString(StringCapability.InsertCharacter, "\u001b[@")
            .SetString(StringCapability.DeleteLines, "\u001b[%p1%dM")
            .SetString(StringCapability.DeleteLine, "\u001b[M")
            .SetString(StringCapability.InsertLines, "\u001b[%p1%dL")
            .SetString(StringCapability.InsertLine, "\u001b[L")
            .SetString(StringCapability.ExitInsertMode, "\u001b[4l")
            .SetString(StringCapability.EnterInsertMode, "\u001b[4h")
            .SetString(StringCapability.EraseCharacters, "\u001b[%p1%dX")
            .SetString(StringCapability.ClearToEndOfScreen, "\u001b[J")
            .SetString(StringCapability.ClearToEndOfLine, "\u001b[K")
            .SetString(
                StringCapability.ClearToBeginningOfLine,
                "\u001b[1K")
            .SetString(StringCapability.Tab, "\t")
            .SetString(StringCapability.SetTab, "\u001bH")
            .SetString(StringCapability.ClearAllTabs, "\u001b[3g")
            .SetString(StringCapability.ScrollForward, "\u001bD")
            .SetString(StringCapability.ScrollReverse, "\u001bM")
            .SetString(StringCapability.NewLine, "\u001bE")
            .SetString(StringCapability.EnterBlinkMode, "\u001b[5m")
            .SetString(StringCapability.EnterBoldMode, "\u001b[1m")
            .SetString(StringCapability.EnterReverseMode, "\u001b[7m")
            .SetString(StringCapability.EnterStandoutMode, "\u001b[7m")
            .SetString(StringCapability.ExitStandoutMode, "\u001b[27m")
            .SetString(StringCapability.EnterUnderlineMode, "\u001b[4m")
            .SetString(StringCapability.ExitUnderlineMode, "\u001b[24m")
            .SetString(
                StringCapability.AlternateCharacterSet,
                DecSpecialGraphics)
            .SetString(
                StringCapability.EnableAlternateCharacterSet,
                "\u001b)0")
            .SetString(
                StringCapability.EnterAlternateCharacterSetMode,
                "\u001b(0$<2>")
            .SetString(
                StringCapability.ExitAlternateCharacterSetMode,
                "\u001b(B$<4>")
            .SetString(
                StringCapability.SetAttributes,
                "\u001b[0%?%p6%t;1%;%?%p2%t;4%;%?%p4%t;5%;"
                + "%?%p1%p3%|%t;7%;m"
                + "%?%p9%t\u001b(0%e\u001b(B%;$<2>")
            .SetString(
                StringCapability.ExitAttributeMode,
                "\u001b[m\u001b(B")
            .SetString(
                StringCapability.ExitAutomaticMargins,
                "\u001b[?7l")
            .SetString(
                StringCapability.EnterAutomaticMargins,
                "\u001b[?7h")
            .SetString(
                StringCapability.InitString2,
                "\u001b[?7h\u001b>\u001b[?1l\u001b F\u001b[?4l")
            .SetString(StringCapability.ResetString1, "\u001b[?3l")
            .SetString(
                StringCapability.FlashScreen,
                "\u001b[?5h$<200/>\u001b[?5l")
            .SetString(StringCapability.PrintScreen, "\u001b[i")
            .SetString(StringCapability.PrinterOff, "\u001b[4i")
            .SetString(StringCapability.PrinterOn, "\u001b[5i")
            .SetString(StringCapability.KeyBackspace, "\b")
            .SetString(StringCapability.KeyCursorDown, "\u001b[B")
            .SetString(StringCapability.KeyCursorLeft, "\u001b[D")
            .SetString(StringCapability.KeyCursorRight, "\u001b[C")
            .SetString(StringCapability.KeyCursorUp, "\u001b[A")
            .SetString(StringCapability.KeyF1, "\u001bOP")
            .SetString(StringCapability.KeyF2, "\u001bOQ")
            .SetString(StringCapability.KeyF3, "\u001bOR")
            .SetString(StringCapability.KeyF4, "\u001bOS")
            .SetString(StringCapability.KeyF13, "\u001b[25~")
            .SetString(StringCapability.KeyF14, "\u001b[26~")
            .SetString(StringCapability.KeyF17, "\u001b[31~")
            .SetString(StringCapability.KeyF18, "\u001b[32~")
            .SetString(StringCapability.KeyF19, "\u001b[33~")
            .SetString(StringCapability.KeyF20, "\u001b[34~")
            .SetString(StringCapability.KeyHelp, "\u001b[28~")
            .SetString(StringCapability.KeyRedo, "\u001b[29~")
            .ApplyVt220DecEditingKeys()
            .ApplyVt220UnshiftedFunctionKeys();
    }

    internal static TerminalDescriptionBuilder ApplyVt220PcEditingKeys(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .SetString(StringCapability.KeyDeleteCharacter, "\u001b[3~")
            .SetString(StringCapability.KeyEnd, "\u001b[4~")
            .SetString(StringCapability.KeyHome, "\u001b[1~")
            .SetString(StringCapability.KeyInsertCharacter, "\u001b[2~")
            .SetString(StringCapability.KeyNextPage, "\u001b[6~")
            .SetString(StringCapability.KeyPreviousPage, "\u001b[5~");
    }

    internal static TerminalDescriptionBuilder ApplyVt220DecEditingKeys(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .SetString(StringCapability.KeyDeleteCharacter, "\u001b[3~")
            .SetString(StringCapability.KeyFind, "\u001b[1~")
            .SetString(StringCapability.KeyInsertCharacter, "\u001b[2~")
            .SetString(StringCapability.KeyNextPage, "\u001b[6~")
            .SetString(StringCapability.KeyPreviousPage, "\u001b[5~")
            .SetString(StringCapability.KeySelect, "\u001b[4~");
    }

    internal static TerminalDescriptionBuilder ApplyVt220UnshiftedFunctionKeys(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .SetString(StringCapability.KeyF6, "\u001b[17~")
            .SetString(StringCapability.KeyF7, "\u001b[18~")
            .SetString(StringCapability.KeyF8, "\u001b[19~")
            .SetString(StringCapability.KeyF9, "\u001b[20~")
            .SetString(StringCapability.KeyF10, "\u001b[21~")
            .SetString(StringCapability.KeyF11, "\u001b[23~")
            .SetString(StringCapability.KeyF12, "\u001b[24~");
    }

    internal static TerminalDescriptionBuilder ApplyVt220CursorVisibility(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .SetString(StringCapability.CursorInvisible, "\u001b[?25l")
            .SetString(StringCapability.CursorNormal, "\u001b[?25h");
    }
}
