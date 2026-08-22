namespace Icod.TermInfo;

internal static class XtermCoreCapabilityFragments
{
    private const string AlternateCharacterSet =
        "``aaffggiijjkkllmmnnooppqqrrssttuuvvwwxxyyzz{{||}}~~";

    internal static TerminalDescriptionBuilder ApplyXtermCommon(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .SetBoolean(BooleanCapability.AutoRightMargin)
            .SetBoolean(BooleanCapability.BackColorErase)
            .SetBoolean(BooleanCapability.EatNewlineGlitch)
            .SetBoolean(BooleanCapability.HasMetaKey)
            .SetBoolean(BooleanCapability.MoveInsertMode)
            .SetBoolean(BooleanCapability.MoveStandoutMode)
            .SetBoolean(BooleanCapability.NoPadCharacter)
            .SetNumber(NumericCapability.Columns, 80)
            .SetNumber(NumericCapability.Lines, 24)
            .SetNumber(NumericCapability.InitialTabWidth, 8)
            .SetString(StringCapability.Bell, "\a")
            .SetString(StringCapability.BackTab, "\u001b[Z")
            .SetString(StringCapability.EnterBlinkMode, "\u001b[5m")
            .SetString(StringCapability.EnterBoldMode, "\u001b[1m")
            .SetString(StringCapability.EnterDimMode, "\u001b[2m")
            .SetString(StringCapability.CarriageReturn, "\r")
            .SetString(
                StringCapability.ChangeScrollRegion,
                "\u001b[%i%p1%d;%p2%dr")
            .SetString(StringCapability.ClearScreen, "\u001b[H\u001b[2J")
            .SetString(StringCapability.CursorLeft, "\u001b[%p1%dD")
            .SetString(StringCapability.CursorLeftOne, "\b")
            .SetString(StringCapability.CursorDown, "\u001b[%p1%dB")
            .SetString(StringCapability.CursorDownOne, "\n")
            .SetString(StringCapability.CursorRight, "\u001b[%p1%dC")
            .SetString(StringCapability.CursorRightOne, "\u001b[C")
            .SetString(
                StringCapability.CursorAddress,
                "\u001b[%i%p1%d;%p2%dH")
            .SetString(StringCapability.CursorUp, "\u001b[%p1%dA")
            .SetString(StringCapability.CursorUpOne, "\u001b[A")
            .SetString(
                StringCapability.DeleteCharacters,
                "\u001b[%p1%dP")
            .SetString(StringCapability.DeleteCharacter, "\u001b[P")
            .SetString(StringCapability.DeleteLines, "\u001b[%p1%dM")
            .SetString(StringCapability.DeleteLine, "\u001b[M")
            .SetString(StringCapability.ClearToEndOfScreen, "\u001b[J")
            .SetString(StringCapability.ClearToEndOfLine, "\u001b[K")
            .SetString(
                StringCapability.ClearToBeginningOfLine,
                "\u001b[1K")
            .SetString(StringCapability.CursorHome, "\u001b[H")
            .SetString(
                StringCapability.ColumnAddress,
                "\u001b[%i%p1%dG")
            .SetString(StringCapability.Tab, "\t")
            .SetString(StringCapability.SetTab, "\u001bH")
            .SetString(
                StringCapability.InsertCharacters,
                "\u001b[%p1%d@")
            .SetString(StringCapability.InsertLines, "\u001b[%p1%dL")
            .SetString(StringCapability.InsertLine, "\u001b[L")
            .SetString(StringCapability.ScrollForward, "\n")
            .SetString(StringCapability.EnterInvisibleMode, "\u001b[8m")
            .SetString(
                StringCapability.OriginalColorPair,
                "\u001b[39;49m")
            .SetString(StringCapability.RestoreCursor, "\u001b8")
            .SetString(StringCapability.EnterReverseMode, "\u001b[7m")
            .SetString(StringCapability.ScrollReverse, "\u001bM")
            .SetString(
                StringCapability.ExitAlternateCharacterSetMode,
                "\u001b(B")
            .SetString(StringCapability.ExitAutomaticMargins, "\u001b[?7l")
            .SetString(
                StringCapability.ExitKeypadMode,
                "\u001b[?1l\u001b>")
            .SetString(StringCapability.ExitStandoutMode, "\u001b[27m")
            .SetString(StringCapability.ExitUnderlineMode, "\u001b[24m")
            .SetString(StringCapability.SaveCursor, "\u001b7")
            .SetString(
                StringCapability.SetAttributes,
                "%?%p9%t\u001b(0%e\u001b(B%;"
                + "\u001b[0%?%p6%t;1%;%?%p5%t;2%;%?%p2%t;4%;"
                + "%?%p1%p3%|%t;7%;%?%p4%t;5%;%?%p7%t;8%;m")
            .SetString(
                StringCapability.ExitAttributeMode,
                "\u001b(B\u001b[m")
            .SetString(
                StringCapability.EnterAlternateCharacterSetMode,
                "\u001b(0")
            .SetString(StringCapability.EnterAutomaticMargins, "\u001b[?7h")
            .SetString(
                StringCapability.EnterKeypadMode,
                "\u001b[?1h\u001b=")
            .SetString(StringCapability.EnterStandoutMode, "\u001b[7m")
            .SetString(StringCapability.EnterUnderlineMode, "\u001b[4m")
            .SetString(
                StringCapability.RowAddress,
                "\u001b[%i%p1%dd")
            .SetString(
                StringCapability.AlternateCharacterSet,
                AlternateCharacterSet)
            .SetString(StringCapability.ResetString2, "\u001b[!p\u001b[?3;4l\u001b[4l\u001b>")
            .SetString(
                StringCapability.EraseCharacters,
                "\u001b[%p1%dX")
            .SetString(StringCapability.ClearAllTabs, "\u001b[3g")
            .SetString(
                StringCapability.EnterCursorAddressingMode,
                "\u001b[?1049h\u001b[22;0;0t")
            .SetString(
                StringCapability.ExitCursorAddressingMode,
                "\u001b[?1049l\u001b[23;0;0t")
            .SetString(StringCapability.CursorInvisible, "\u001b[?25l")
            .SetString(
                StringCapability.CursorNormal,
                "\u001b[?12l\u001b[?25h")
            .SetString(
                StringCapability.CursorVeryVisible,
                "\u001b[?12;25h")
            .SetString(
                StringCapability.FlashScreen,
                "\u001b[?5h$<100/>\u001b[?5l")
            .SetString(StringCapability.NewLine, "\u001bE")
            .SetString(
                StringCapability.ScrollForwardLines,
                "\u001b[%p1%dS")
            .SetString(
                StringCapability.ScrollReverseLines,
                "\u001b[%p1%dT")
            .SetString(StringCapability.EnterInsertMode, "\u001b[4h")
            .SetString(StringCapability.ExitInsertMode, "\u001b[4l")
            .SetString(StringCapability.EnterMetaMode, "\u001b[?1034h")
            .SetString(StringCapability.ExitMetaMode, "\u001b[?1034l")
            .SetString(StringCapability.EnterItalicMode, "\u001b[3m")
            .SetString(StringCapability.ExitItalicMode, "\u001b[23m")
            .SetString(
                StringCapability.InitString2,
                "\u001b[!p\u001b[?3;4l\u001b[4l\u001b>")
            .SetString(StringCapability.ResetString1, "\u001bc")
            .SetString(StringCapability.MemoryLock, "\u001bl")
            .SetString(StringCapability.MemoryUnlock, "\u001bm")
            .SetString(
                StringCapability.RepeatCharacter,
                "%p1%c\u001b[%p2%{1}%-%db")
            .SetString(StringCapability.PrintScreen, "\u001b[i")
            .SetString(StringCapability.PrinterOff, "\u001b[4i")
            .SetString(StringCapability.PrinterOn, "\u001b[5i");
    }
}
