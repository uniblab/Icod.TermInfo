namespace Icod.TermInfo;

internal static class WindowsTerminalCapabilityFragments
{
    private static readonly int[] FunctionKeyTildeCodes =
        [15, 17, 18, 19, 20, 21, 23, 24];

    private static readonly char[] FunctionKeySs3Finals =
        ['P', 'Q', 'R', 'S'];

    private const string XtermInitializeColor =
        "\u001b]4;%p1%d;rgb:%p2%{255}%*%{1000}%/%2.2X/"
        + "%p3%{255}%*%{1000}%/%2.2X/"
        + "%p4%{255}%*%{1000}%/%2.2X\u001b\\";

    internal static TerminalDescriptionBuilder ApplyWindowsTerminalInheritedCapabilities(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            // xterm-basic. The 0.7 common fragment already represents the
            // standard xterm-basic inheritance used here.
            .ApplyXtermCommon()
            .SetBoolean(BooleanCapability.BackspacesWithBs)
            .SetExtendedBoolean("AX")
            .SetExtendedBoolean("XT")
            .SetExtendedString("E3", "\u001b[3J")
            // bracketed+paste
            .ApplyXtermBracketedPasteMetadata()
            // xterm+pcfkeys only. ApplyXtermKeys also carries xterm+keypad and
            // xterm+3keys data used by the xterm built-in, which ms+terminal
            // does not inherit.
            .ApplyWindowsTerminalPcFunctionKeys()
            // linux+kbs
            .SetString(StringCapability.KeyBackspace, "\u007f")
            // ansi+rep and ecma+index are already represented by xterm common;
            // keep the parameterized index operations explicit for provenance.
            .SetString(
                StringCapability.ScrollForwardLines,
                "\u001b[%p1%dS")
            .SetString(
                StringCapability.ScrollReverseLines,
                "\u001b[%p1%dT")
            // xterm+sm+1006, later refined by xterm+sm+1003.
            .ApplyXtermSgrMouseMetadata()
            .SetExtendedString(
                "XM",
                "\u001b[?1006;1004;1003%?%p1%{1}%=%th%el%;")
            // ECMA-48 overline and strikeout.
            .SetExtendedString("Rmol", "\u001b[55m")
            .SetExtendedString("Smol", "\u001b[53m")
            .SetExtendedString("rmxx", "\u001b[29m")
            .SetExtendedString("smxx", "\u001b[9m")
            // report+da2.
            .SetExtendedString("RV", "\u001b[>c")
            .SetExtendedString(
                "rv",
                "\u001b\\[>[0-9]+;[0-9]+;[0-9]+c")
            // vt420+lrmm.
            .SetString(StringCapability.ClearMargins, "\u001b[?69l")
            .SetString(
                StringCapability.SetLeftMarginParm,
                "\u001b[?69h\u001b[%i%p1%ds")
            .SetString(
                StringCapability.SetLrMargin,
                "\u001b[?69h\u001b[%i%p1%d;%p2%ds")
            .SetString(
                StringCapability.SetRightMarginParm,
                "\u001b[?69h\u001b[%i;%p1%ds")
            // xterm+focus.
            .ApplyXtermFocusMetadata()
            // xterm+tmux (BEL-terminated form, not xterm+tmux2).
            .SetExtendedString("Cr", "\u001b]112\a")
            .SetExtendedString("Cs", "\u001b]12;%p1%s\a")
            .SetExtendedString("Ms", "\u001b]52;%p1%s;%p2%s\a")
            .SetExtendedString("Se", "\u001b[2 q")
            .SetExtendedString("Ss", "\u001b[%p1%d q");
    }

    internal static TerminalDescriptionBuilder ApplyWindowsTerminalIndexedColor(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            // xterm+256color.
            .ApplyXtermExtendedIndexed(256, 65536)
            .SetBoolean(BooleanCapability.CanChangeColor)
            .SetString(StringCapability.InitializeColor, XtermInitializeColor)
            .SetString(StringCapability.OriginalColors, "\u001b]104\a")
            .RemoveString(StringCapability.SetLegacyForegroundColor)
            .RemoveString(StringCapability.SetLegacyBackgroundColor);
    }

    internal static TerminalDescriptionBuilder ApplyWindowsTerminalSourceEntry(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            // Explicit ms+terminal capabilities and cancellations.
            .SetBoolean(BooleanCapability.NoPadCharacter)
            .SetString(StringCapability.CursorDownOne, "\u001b[B")
            .SetString(StringCapability.KeyBegin, "\u001bOE")
            .SetString(StringCapability.KeyBackTab, "\u001b[Z")
            .CancelString(StringCapability.OriginalColors)
            .SetString(StringCapability.ExitKeypadMode, "\u001b[?1l")
            .CancelString(StringCapability.ExitMetaMode)
            .SetString(StringCapability.EnterKeypadMode, "\u001b[?1h")
            .CancelString(StringCapability.EnterMetaMode)
            .SetExtendedString("rv", "\u001b\\[>0;10;1c");
    }

    private static TerminalDescriptionBuilder ApplyWindowsTerminalPcFunctionKeys(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ApplyVt220PcEditingKeys();

        builder
            // xterm+app
            .SetString(StringCapability.KeyEnd, "\u001bOF")
            .SetString(StringCapability.KeyHome, "\u001bOH")
            .SetString(StringCapability.KeyCursorDown, "\u001bOB")
            .SetString(StringCapability.KeyCursorLeft, "\u001bOD")
            .SetString(StringCapability.KeyCursorRight, "\u001bOC")
            .SetString(StringCapability.KeyCursorUp, "\u001bOA")
            // xterm+pcc2
            .SetString(StringCapability.KeyScrollForward, "\u001b[1;2B")
            .SetString(StringCapability.KeyScrollReverse, "\u001b[1;2A")
            .SetString(StringCapability.KeyShiftLeft, "\u001b[1;2D")
            .SetString(StringCapability.KeyShiftRight, "\u001b[1;2C")
            // xterm+pce2
            .SetString(StringCapability.KeyShiftDeleteCharacter, "\u001b[3;2~")
            .SetString(StringCapability.KeyShiftEnd, "\u001b[1;2F")
            .SetString(StringCapability.KeyShiftHome, "\u001b[1;2H")
            .SetString(StringCapability.KeyShiftInsertCharacter, "\u001b[2;2~")
            .SetString(StringCapability.KeyShiftNextPage, "\u001b[6;2~")
            .SetString(StringCapability.KeyShiftPreviousPage, "\u001b[5;2~");

        ApplyPcFunctionKeys(builder);
        ApplyPcNamedKeys(builder);
        return builder;
    }

    private static void ApplyPcFunctionKeys(TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        for (int i = 0; i < FunctionKeySs3Finals.Length; i++)
        {
            SetFunctionKey(
                builder,
                i + 1,
                $"\u001bO{FunctionKeySs3Finals[i]}");
        }

        for (int i = 0; i < FunctionKeyTildeCodes.Length; i++)
        {
            SetFunctionKey(
                builder,
                i + 5,
                $"\u001b[{FunctionKeyTildeCodes[i]}~");
        }

        ApplyModifiedFunctionKeyBank(builder, 13, 2);
        ApplyModifiedFunctionKeyBank(builder, 25, 5);
        ApplyModifiedFunctionKeyBank(builder, 37, 6);
        ApplyModifiedFunctionKeyBank(builder, 49, 3);

        for (int i = 0; i < 3; i++)
        {
            SetFunctionKey(
                builder,
                i + 61,
                $"\u001b[1;4{FunctionKeySs3Finals[i]}");
        }
    }

    private static void ApplyModifiedFunctionKeyBank(
        TerminalDescriptionBuilder builder,
        int firstFunctionKey,
        int modifier)
    {
        ArgumentNullException.ThrowIfNull(builder);

        for (int i = 0; i < FunctionKeySs3Finals.Length; i++)
        {
            SetFunctionKey(
                builder,
                firstFunctionKey + i,
                $"\u001b[1;{modifier}{FunctionKeySs3Finals[i]}");
        }

        for (int i = 0; i < FunctionKeyTildeCodes.Length; i++)
        {
            SetFunctionKey(
                builder,
                firstFunctionKey + 4 + i,
                $"\u001b[{FunctionKeyTildeCodes[i]};{modifier}~");
        }
    }

    private static void SetFunctionKey(
        TerminalDescriptionBuilder builder,
        int number,
        string value)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(value);

        builder.SetString(
            Enum.Parse<StringCapability>($"KeyF{number}"),
            value);
    }

    private static void ApplyPcNamedKeys(TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        foreach ((string name, string value) in CreatePcNamedKeys())
        {
            builder.SetExtendedString(name, value);
        }
    }

    private static (string Name, string Value)[] CreatePcNamedKeys()
    {
        return
        [
            ("kDN", "\u001b[1;2B"),
            ("kUP", "\u001b[1;2A"),
            ("kDC3", "\u001b[3;3~"),
            ("kDC4", "\u001b[3;4~"),
            ("kDC5", "\u001b[3;5~"),
            ("kDC6", "\u001b[3;6~"),
            ("kDC7", "\u001b[3;7~"),
            ("kEND3", "\u001b[1;3F"),
            ("kEND4", "\u001b[1;4F"),
            ("kEND5", "\u001b[1;5F"),
            ("kEND6", "\u001b[1;6F"),
            ("kEND7", "\u001b[1;7F"),
            ("kHOM3", "\u001b[1;3H"),
            ("kHOM4", "\u001b[1;4H"),
            ("kHOM5", "\u001b[1;5H"),
            ("kHOM6", "\u001b[1;6H"),
            ("kHOM7", "\u001b[1;7H"),
            ("kIC3", "\u001b[2;3~"),
            ("kIC4", "\u001b[2;4~"),
            ("kIC5", "\u001b[2;5~"),
            ("kIC6", "\u001b[2;6~"),
            ("kIC7", "\u001b[2;7~"),
            ("kLFT3", "\u001b[1;3D"),
            ("kLFT4", "\u001b[1;4D"),
            ("kLFT5", "\u001b[1;5D"),
            ("kLFT6", "\u001b[1;6D"),
            ("kLFT7", "\u001b[1;7D"),
            ("kNXT3", "\u001b[6;3~"),
            ("kNXT4", "\u001b[6;4~"),
            ("kNXT5", "\u001b[6;5~"),
            ("kNXT6", "\u001b[6;6~"),
            ("kNXT7", "\u001b[6;7~"),
            ("kPRV3", "\u001b[5;3~"),
            ("kPRV4", "\u001b[5;4~"),
            ("kPRV5", "\u001b[5;5~"),
            ("kPRV6", "\u001b[5;6~"),
            ("kPRV7", "\u001b[5;7~"),
            ("kRIT3", "\u001b[1;3C"),
            ("kRIT4", "\u001b[1;4C"),
            ("kRIT5", "\u001b[1;5C"),
            ("kRIT6", "\u001b[1;6C"),
            ("kRIT7", "\u001b[1;7C"),
            ("kDN3", "\u001b[1;3B"),
            ("kDN4", "\u001b[1;4B"),
            ("kDN5", "\u001b[1;5B"),
            ("kDN6", "\u001b[1;6B"),
            ("kDN7", "\u001b[1;7B"),
            ("kUP3", "\u001b[1;3A"),
            ("kUP4", "\u001b[1;4A"),
            ("kUP5", "\u001b[1;5A"),
            ("kUP6", "\u001b[1;6A"),
            ("kUP7", "\u001b[1;7A"),
        ];
    }
}
