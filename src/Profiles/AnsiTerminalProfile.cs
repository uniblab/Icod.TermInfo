namespace Icod.TermInfo;

internal static class AnsiTerminalProfile
{
    internal static TerminalDescription Create()
    {
        return new TerminalDescriptionBuilder("ansi")
            .SetBoolean(BooleanCapability.AutoRightMargin)
            .SetBoolean(BooleanCapability.MoveStandoutMode)
            .SetBoolean(BooleanCapability.MoveInsertMode)
            .SetNumber(NumericCapability.Columns, 80)
            .SetNumber(NumericCapability.Lines, 24)
            .SetNumber(NumericCapability.Colors, 8)
            .SetNumber(NumericCapability.ColorPairs, 64)
            .SetNumber(NumericCapability.InitialTabWidth, 8)
            .SetNumber(NumericCapability.NoColorVideo, 3)
            .SetString(StringCapability.Bell, "\a")
            .SetString(StringCapability.CarriageReturn, "\r")
            .SetString(StringCapability.ClearScreen, "\x1b[H\x1b[J")
            .SetString(StringCapability.CursorLeft, "\x1b[%p1%dD")
            .SetString(StringCapability.CursorLeftOne, "\x1b[D")
            .SetString(StringCapability.CursorDown, "\x1b[%p1%dB")
            .SetString(StringCapability.CursorDownOne, "\x1b[B")
            .SetString(StringCapability.CursorRight, "\x1b[%p1%dC")
            .SetString(StringCapability.CursorRightOne, "\x1b[C")
            .SetString(StringCapability.CursorUp, "\x1b[%p1%dA")
            .SetString(StringCapability.CursorUpOne, "\x1b[A")
            .SetString(
                StringCapability.CursorAddress,
                "\x1b[%i%p1%d;%p2%dH")
            .SetString(StringCapability.CursorHome, "\x1b[H")
            .SetString(
                StringCapability.ColumnAddress,
                "\x1b[%i%p1%dG")
            .SetString(StringCapability.RowAddress, "\x1b[%i%p1%dd")
            .SetString(StringCapability.SaveCursor, "\u001b7")
            .SetString(StringCapability.RestoreCursor, "\u001b8")
            .SetString(StringCapability.ClearToEndOfScreen, "\x1b[J")
            .SetString(StringCapability.ClearToEndOfLine, "\x1b[K")
            .SetString(
                StringCapability.ClearToBeginningOfLine,
                "\x1b[1K")
            .SetString(
                StringCapability.EraseCharacters,
                "\x1b[%p1%dX")
            .SetString(
                StringCapability.DeleteCharacters,
                "\x1b[%p1%dP")
            .SetString(StringCapability.DeleteCharacter, "\x1b[P")
            .SetString(StringCapability.DeleteLines, "\x1b[%p1%dM")
            .SetString(StringCapability.DeleteLine, "\x1b[M")
            .SetString(
                StringCapability.InsertCharacters,
                "\x1b[%p1%d@")
            .SetString(StringCapability.InsertLines, "\x1b[%p1%dL")
            .SetString(StringCapability.InsertLine, "\x1b[L")
            .SetString(StringCapability.BackTab, "\x1b[Z")
            .SetString(StringCapability.Tab, "\x1b[I")
            .SetString(StringCapability.SetTab, "\x1bH")
            .SetString(StringCapability.ClearAllTabs, "\x1b[3g")
            .SetString(StringCapability.ScrollForward, "\n")
            .SetString(StringCapability.EnterBlinkMode, "\x1b[5m")
            .SetString(StringCapability.EnterBoldMode, "\x1b[1m")
            .SetString(StringCapability.EnterReverseMode, "\x1b[7m")
            .SetString(StringCapability.EnterInvisibleMode, "\x1b[8m")
            .SetString(StringCapability.EnterStandoutMode, "\x1b[7m")
            .SetString(StringCapability.ExitStandoutMode, "\x1b[m")
            .SetString(StringCapability.EnterUnderlineMode, "\x1b[4m")
            .SetString(StringCapability.ExitUnderlineMode, "\x1b[m")
            .SetString(
                StringCapability.EnterAlternateCharacterSetMode,
                "\x1b[11m")
            .SetString(
                StringCapability.ExitAlternateCharacterSetMode,
                "\x1b[10m")
            .SetString(
                StringCapability.SetAttributes,
                "\x1b[0;10%?%p1%t;7%;%?%p2%t;4%;%?%p3%t;7%;"
                + "%?%p4%t;5%;%?%p6%t;1%;%?%p7%t;8%;"
                + "%?%p9%t;11%;m")
            .SetString(StringCapability.ExitAttributeMode, "\x1b[0;10m")
            .SetString(
                StringCapability.SetForegroundColor,
                "\x1b[3%p1%dm")
            .SetString(
                StringCapability.SetBackgroundColor,
                "\x1b[4%p1%dm")
            .SetString(
                StringCapability.OriginalColorPair,
                "\x1b[39;49m")
            .SetString(StringCapability.KeyBackspace, "\b")
            .SetString(StringCapability.KeyCursorDown, "\x1b[B")
            .SetString(StringCapability.KeyCursorLeft, "\x1b[D")
            .SetString(StringCapability.KeyCursorRight, "\x1b[C")
            .SetString(StringCapability.KeyCursorUp, "\x1b[A")
            .SetString(StringCapability.KeyHome, "\x1b[H")
            .Build();
    }
}
