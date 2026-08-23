namespace Icod.TermInfo;

internal static class WindowsConsoleCapabilityFragments
{
    private static readonly int[] FunctionKeyCodes =
    [
        11, 12, 13, 14, 15, 17, 18, 19, 20, 21, 24, 25,
    ];

    private static readonly StringCapability[][] FunctionKeyBanks =
    [
        [
            StringCapability.KeyF13, StringCapability.KeyF14,
            StringCapability.KeyF15, StringCapability.KeyF16,
            StringCapability.KeyF17, StringCapability.KeyF18,
            StringCapability.KeyF19, StringCapability.KeyF20,
            StringCapability.KeyF21, StringCapability.KeyF22,
            StringCapability.KeyF23, StringCapability.KeyF24,
        ],
        [
            StringCapability.KeyF25, StringCapability.KeyF26,
            StringCapability.KeyF27, StringCapability.KeyF28,
            StringCapability.KeyF29, StringCapability.KeyF30,
            StringCapability.KeyF31, StringCapability.KeyF32,
            StringCapability.KeyF33, StringCapability.KeyF34,
            StringCapability.KeyF35, StringCapability.KeyF36,
        ],
        [
            StringCapability.KeyF37, StringCapability.KeyF38,
            StringCapability.KeyF39, StringCapability.KeyF40,
            StringCapability.KeyF41, StringCapability.KeyF42,
            StringCapability.KeyF43, StringCapability.KeyF44,
            StringCapability.KeyF45, StringCapability.KeyF46,
            StringCapability.KeyF47, StringCapability.KeyF48,
        ],
        [
            StringCapability.KeyF49, StringCapability.KeyF50,
            StringCapability.KeyF51, StringCapability.KeyF52,
            StringCapability.KeyF53, StringCapability.KeyF54,
            StringCapability.KeyF55, StringCapability.KeyF56,
            StringCapability.KeyF57, StringCapability.KeyF58,
            StringCapability.KeyF59, StringCapability.KeyF60,
        ],
    ];

    private static readonly int[] FunctionKeyModifiers = [2, 3, 4, 7];

    internal static TerminalDescriptionBuilder ApplyWindowsConsoleInheritedCapabilities(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .SetNumber(NumericCapability.InitialTabWidth, 8)
            .SetNumber(NumericCapability.Colors, 8)
            .SetNumber(NumericCapability.ColorPairs, 64)
            .SetNumber(NumericCapability.NoColorVideo, 3)
            .SetString(StringCapability.BackTab, "\u001b[Z")
            .SetString(StringCapability.ChangeScrollRegion, "\u001b[%i%p1%d;%p2%dr")
            .SetString(StringCapability.ClearAllTabs, "\u001b[3g")
            .SetString(StringCapability.ClearScreen, "\u001b[H\u001b[J")
            .SetString(StringCapability.ClearToEndOfLine, "\u001b[K")
            .SetString(StringCapability.ClearToEndOfScreen, "\u001b[J")
            .SetString(StringCapability.CursorAddress, "\u001b[%i%p1%d;%p2%dH")
            .SetString(StringCapability.CursorDownOne, "\u001b[B")
            .SetString(StringCapability.CursorHome, "\u001b[H")
            .SetString(StringCapability.CursorLeftOne, "\u001b[D")
            .SetString(StringCapability.CursorRightOne, "\u001b[C")
            .SetString(StringCapability.CursorUpOne, "\u001b[A")
            .SetString(StringCapability.DeleteCharacter, "\u001b[P")
            .SetString(StringCapability.DeleteLine, "\u001b[M")
            .SetString(StringCapability.InsertLine, "\u001b[L")
            .SetString(StringCapability.EnterBlinkMode, "\u001b[5m")
            .SetString(StringCapability.EnterBoldMode, "\u001b[1m")
            .SetString(StringCapability.EnterInvisibleMode, "\u001b[8m")
            .SetString(StringCapability.EnterReverseMode, "\u001b[7m")
            .SetString(StringCapability.EnterStandoutMode, "\u001b[7m")
            .SetString(StringCapability.EnterUnderlineMode, "\u001b[4m")
            .SetString(StringCapability.ExitStandoutMode, "\u001b[27m")
            .SetString(StringCapability.ExitUnderlineMode, "\u001b[24m")
            .SetString(StringCapability.InsertCharacter, "\u001b[@")
            .SetString(StringCapability.ExitInsertMode, "\u001b[4l")
            .SetString(StringCapability.EnterInsertMode, "\u001b[4h")
            .SetString(StringCapability.DeleteCharacters, "\u001b[%p1%dP")
            .SetString(StringCapability.DeleteLines, "\u001b[%p1%dM")
            .SetString(StringCapability.CursorDown, "\u001b[%p1%dB")
            .SetString(StringCapability.InsertCharacters, "\u001b[%p1%d@")
            .SetString(StringCapability.ScrollForwardLines, "\u001b[%p1%dS")
            .SetString(StringCapability.InsertLines, "\u001b[%p1%dL")
            .SetString(StringCapability.CursorLeft, "\u001b[%p1%dD")
            .SetString(StringCapability.CursorRight, "\u001b[%p1%dC")
            .SetString(StringCapability.ScrollReverseLines, "\u001b[%p1%dT")
            .SetString(StringCapability.CursorUp, "\u001b[%p1%dA")
            .SetString(StringCapability.RestoreCursor, "\u001b8")
            .SetString(StringCapability.SaveCursor, "\u001b7")
            .SetString(StringCapability.SetTab, "\u001bH")
            .SetString(StringCapability.Tab, "\t")
            .SetString(StringCapability.ExitPcCharsetMode, "\u001b[10m")
            .SetString(StringCapability.EnterPcCharsetMode, "\u001b[11m")
            .SetString(StringCapability.OriginalColorPair, "\u001b[39;49m")
            .SetString(StringCapability.SetBackgroundColor, "\u001b[4%p1%dm")
            .SetString(StringCapability.SetForegroundColor, "\u001b[3%p1%dm")
            .SetExtendedBoolean("AX")
            .ApplyVt220CursorVisibility()
            .ApplyVt220PcEditingKeys()
            .SetString(StringCapability.KeyBackspace, "\b")
            .SetString(StringCapability.KeyCursorDown, "\u001b[B")
            .SetString(StringCapability.KeyCursorLeft, "\u001b[D")
            .SetString(StringCapability.KeyCursorRight, "\u001b[C")
            .SetString(StringCapability.KeyCursorUp, "\u001b[A")
            .SetString(StringCapability.KeyF1, "\u001b[11~")
            .SetString(StringCapability.KeyF2, "\u001b[12~")
            .SetString(StringCapability.KeyF3, "\u001b[13~")
            .SetString(StringCapability.KeyF4, "\u001b[14~")
            .SetString(StringCapability.KeyF5, "\u001b[15~")
            .ApplyVt220UnshiftedFunctionKeys();

        builder.CancelString(StringCapability.KeyF5);
        return builder;
    }

    internal static TerminalDescriptionBuilder ApplyWindowsConsoleSourceEntry(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .SetBoolean(BooleanCapability.AutoRightMargin)
            .SetBoolean(BooleanCapability.HasMetaKey)
            .SetBoolean(BooleanCapability.MoveInsertMode)
            .SetBoolean(BooleanCapability.MoveStandoutMode)
            .SetBoolean(BooleanCapability.EatNewlineGlitch)
            .CancelNumber(NumericCapability.NoColorVideo)
            .SetExtendedNumber("U8", 1)
            .SetString(StringCapability.AlternateCharacterSet,
                "++,,--..00``aaffgghhiijjkkllmmnnooppqqrrssttuuvvwwxxyyzz~~")
            .SetString(StringCapability.Bell, "\a")
            .CancelString(StringCapability.EnterBlinkMode)
            .SetString(StringCapability.CarriageReturn, "\r")
            .SetString(StringCapability.EraseCharacters, "\u001b[%p1%dX")
            .SetString(StringCapability.ClearToBeginningOfLine, "\u001b[0K")
            .CancelString(StringCapability.InsertCharacter)
            .SetString(StringCapability.ScrollForward, "\n")
            .CancelString(StringCapability.EnterInvisibleMode)
            .SetString(StringCapability.InitString1, "\u001b[!p")
            .SetString(StringCapability.KeyHome, "\u001b[1~")
            .SetString(StringCapability.NewLine, "\r\n")
            .SetString(StringCapability.ScrollReverse, "\u001b[T")
            .SetString(StringCapability.ExitAlternateCharacterSetMode, "\u001b(B")
            .CancelString(StringCapability.ExitInsertMode)
            .CancelString(StringCapability.ExitPcCharsetMode)
            .SetString(StringCapability.ResetString1, "\u001b[!p")
            .SetString(StringCapability.SetAttributes,
                "\u001b[0%?%p1%p6%|%t;1%;%?%p2%t;4%;"
                + "%?%p1%p3%|%t;7%;m%?%p9%t\u001b(0%e\u001b(B%;")
            .SetString(StringCapability.ExitAttributeMode, "\u001b[0m\u001b(B")
            .SetString(StringCapability.EnterAlternateCharacterSetMode, "\u001b(0")
            .CancelString(StringCapability.EnterInsertMode)
            .CancelString(StringCapability.EnterPcCharsetMode);

        for (int bank = 0; bank < FunctionKeyBanks.Length; bank++)
        {
            ApplyFunctionKeyBank(
                builder,
                FunctionKeyBanks[bank],
                FunctionKeyModifiers[bank]);
        }

        return builder;
    }

    private static void ApplyFunctionKeyBank(
        TerminalDescriptionBuilder builder,
        IReadOnlyList<StringCapability> capabilities,
        int modifier)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(capabilities);

        if (capabilities.Count != FunctionKeyCodes.Length)
        {
            throw new ArgumentException(
                "A Windows Console function-key bank must contain twelve keys.",
                nameof(capabilities));
        }

        for (int i = 0; i < capabilities.Count; i++)
        {
            builder.SetString(
                capabilities[i],
                $"\u001b[{FunctionKeyCodes[i]};{modifier}~");
        }
    }
}
