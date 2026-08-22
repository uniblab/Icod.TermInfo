namespace Icod.TermInfo;

internal static class Vt100TerminalProfile
{
    internal static TerminalDescription Create()
    {
        return new TerminalDescriptionBuilder("vt100")
            .AddAlias("vt100-am")
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
                "\x1b[%i%p1%d;%p2%dr")
            .SetString(
                StringCapability.ClearScreen,
                "\x1b[H\x1b[J$<50>")
            .SetString(StringCapability.CursorLeft, "\x1b[%p1%dD")
            .SetString(StringCapability.CursorLeftOne, "\b")
            .SetString(StringCapability.CursorDown, "\x1b[%p1%dB")
            .SetString(StringCapability.CursorDownOne, "\n")
            .SetString(StringCapability.CursorRight, "\x1b[%p1%dC")
            .SetString(StringCapability.CursorRightOne, "\x1b[C$<2>")
            .SetString(StringCapability.CursorUp, "\x1b[%p1%dA")
            .SetString(StringCapability.CursorUpOne, "\x1b[A$<2>")
            .SetString(
                StringCapability.CursorAddress,
                "\x1b[%i%p1%d;%p2%dH$<5>")
            .SetString(StringCapability.CursorHome, "\x1b[H")
            .SetString(StringCapability.SaveCursor, "\u001b7")
            .SetString(StringCapability.RestoreCursor, "\u001b8")
            .SetString(
                StringCapability.ClearToEndOfScreen,
                "\x1b[J$<50>")
            .SetString(
                StringCapability.ClearToEndOfLine,
                "\x1b[K$<3>")
            .SetString(
                StringCapability.ClearToBeginningOfLine,
                "\x1b[1K$<3>")
            .SetString(StringCapability.Tab, "\t")
            .SetString(StringCapability.SetTab, "\x1bH")
            .SetString(StringCapability.ClearAllTabs, "\x1b[3g")
            .SetString(StringCapability.ScrollForward, "\n")
            .SetString(StringCapability.ScrollReverse, "\x1bM$<5>")
            .SetString(StringCapability.EnterBlinkMode, "\x1b[5m$<2>")
            .SetString(StringCapability.EnterBoldMode, "\x1b[1m$<2>")
            .SetString(StringCapability.EnterReverseMode, "\x1b[7m$<2>")
            .SetString(StringCapability.EnterStandoutMode, "\x1b[7m$<2>")
            .SetString(StringCapability.ExitStandoutMode, "\x1b[m$<2>")
            .SetString(StringCapability.EnterUnderlineMode, "\x1b[4m$<2>")
            .SetString(StringCapability.ExitUnderlineMode, "\x1b[m$<2>")
            .SetString(
                StringCapability.AlternateCharacterSet,
                "``aaffggjjkkllmmnnooppqqrrssttuuvvwwxxyyzz{{||}}~~")
            .SetString(
                StringCapability.EnableAlternateCharacterSet,
                "\x1b(B\x1b)0")
            .SetString(
                StringCapability.EnterAlternateCharacterSetMode,
                "\x0e")
            .SetString(
                StringCapability.ExitAlternateCharacterSetMode,
                "\x0f")
            .SetString(
                StringCapability.SetAttributes,
                "\x1b[0%?%p1%p6%|%t;1%;%?%p2%t;4%;"
                + "%?%p1%p3%|%t;7%;%?%p4%t;5%;m"
                + "%?%p9%t\x0e%e\x0f%;$<2>")
            .SetString(
                StringCapability.ExitAttributeMode,
                "\x1b[m\x0f$<2>")
            .SetString(StringCapability.ExitAutomaticMargins, "\x1b[?7l")
            .SetString(StringCapability.EnterAutomaticMargins, "\x1b[?7h")
            .SetString(StringCapability.ExitKeypadMode, "\x1b[?1l\x1b>")
            .SetString(StringCapability.EnterKeypadMode, "\x1b[?1h\x1b=")
            .SetString(
                StringCapability.ResetString2,
                "\x1b<\x1b>\x1b[?3;4;5l\x1b[?7;8h\x1b[r")
            .SetString(StringCapability.KeyBackspace, "\b")
            .SetString(StringCapability.KeyCursorDown, "\x1bOB")
            .SetString(StringCapability.KeyCursorLeft, "\x1bOD")
            .SetString(StringCapability.KeyCursorRight, "\x1bOC")
            .SetString(StringCapability.KeyCursorUp, "\x1bOA")
            .SetString(StringCapability.KeyF1, "\x1bOP")
            .SetString(StringCapability.KeyF2, "\x1bOQ")
            .SetString(StringCapability.KeyF3, "\x1bOR")
            .SetString(StringCapability.KeyF4, "\x1bOS")
            .Build();
    }
}
