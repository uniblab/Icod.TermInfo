namespace Icod.TermInfo;

internal static class XtermKeyCapabilityFragments
{
    internal static TerminalDescriptionBuilder ApplyXtermKeys(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .SetString(StringCapability.KeyBackspace, "\b")
            .SetString(StringCapability.KeyBackTab, "\u001b[Z")
            .SetString(StringCapability.KeyBegin, "\u001bOE")
            .SetString(StringCapability.KeyDeleteCharacter, "\u001b[3~")
            .SetString(StringCapability.KeyEnd, "\u001bOF")
            .SetString(StringCapability.KeyEnter, "\u001bOM")
            .SetString(StringCapability.KeyHome, "\u001bOH")
            .SetString(StringCapability.KeyInsertCharacter, "\u001b[2~")
            .SetString(StringCapability.KeyNextPage, "\u001b[6~")
            .SetString(StringCapability.KeyPreviousPage, "\u001b[5~")
            .SetString(StringCapability.KeyCursorDown, "\u001bOB")
            .SetString(StringCapability.KeyCursorLeft, "\u001bOD")
            .SetString(StringCapability.KeyCursorRight, "\u001bOC")
            .SetString(StringCapability.KeyCursorUp, "\u001bOA")
            .SetString(StringCapability.KeyF1, "\u001bOP")
            .SetString(StringCapability.KeyF2, "\u001bOQ")
            .SetString(StringCapability.KeyF3, "\u001bOR")
            .SetString(StringCapability.KeyF4, "\u001bOS")
            .SetString(StringCapability.KeyF5, "\u001b[15~")
            .SetString(StringCapability.KeyF6, "\u001b[17~")
            .SetString(StringCapability.KeyF7, "\u001b[18~")
            .SetString(StringCapability.KeyF8, "\u001b[19~")
            .SetString(StringCapability.KeyF9, "\u001b[20~")
            .SetString(StringCapability.KeyF10, "\u001b[21~")
            .SetString(StringCapability.KeyF11, "\u001b[23~")
            .SetString(StringCapability.KeyF12, "\u001b[24~")
            .SetString(StringCapability.KeyF13, "\u001b[1;2P")
            .SetString(StringCapability.KeyF14, "\u001b[1;2Q")
            .SetString(StringCapability.KeyF15, "\u001b[1;2R")
            .SetString(StringCapability.KeyF16, "\u001b[1;2S")
            .SetString(StringCapability.KeyF17, "\u001b[15;2~")
            .SetString(StringCapability.KeyF18, "\u001b[17;2~")
            .SetString(StringCapability.KeyF19, "\u001b[18;2~")
            .SetString(StringCapability.KeyF20, "\u001b[19;2~")
            .SetString(StringCapability.KeyF21, "\u001b[20;2~")
            .SetString(StringCapability.KeyF22, "\u001b[21;2~")
            .SetString(StringCapability.KeyF23, "\u001b[23;2~")
            .SetString(StringCapability.KeyF24, "\u001b[24;2~")
            .SetString(StringCapability.KeyA1, "\u001bOw")
            .SetString(StringCapability.KeyA3, "\u001bOy")
            .SetString(StringCapability.KeyB2, "\u001bOu")
            .SetString(StringCapability.KeyC1, "\u001bOq")
            .SetString(StringCapability.KeyC3, "\u001bOs")
            .SetString(StringCapability.KeyScrollForward, "\u001b[1;2B")
            .SetString(StringCapability.KeyScrollReverse, "\u001b[1;2A")
            .SetString(StringCapability.KeyShiftDeleteCharacter, "\u001b[3;2~")
            .SetString(StringCapability.KeyShiftEnd, "\u001b[1;2F")
            .SetString(StringCapability.KeyShiftHome, "\u001b[1;2H")
            .SetString(StringCapability.KeyShiftInsertCharacter, "\u001b[2;2~")
            .SetString(StringCapability.KeyShiftLeft, "\u001b[1;2D")
            .SetString(StringCapability.KeyShiftNextPage, "\u001b[6;2~")
            .SetString(StringCapability.KeyShiftPreviousPage, "\u001b[5;2~")
            .SetString(StringCapability.KeyShiftRight, "\u001b[1;2C");

        foreach ((string name, string value) in CreateNamedKeys())
        {
            builder.SetExtendedString(name, value);
        }

        return builder;
    }

    private static (string Name, string Value)[] CreateNamedKeys()
    {
        return
        [
            ("ka2", "\u001bOx"),
            ("kb1", "\u001bOt"),
            ("kb3", "\u001bOv"),
            ("kc2", "\u001bOr"),
            ("kp5", "\u001bOE"),
            ("kpADD", "\u001bOk"),
            ("kpCMA", "\u001bOl"),
            ("kpDIV", "\u001bOo"),
            ("kpDOT", "\u001bOn"),
            ("kpMUL", "\u001bOj"),
            ("kpSUB", "\u001bOm"),
            ("kpZRO", "\u001bOp"),
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
            ("kPause", "\u001b[26;2~"),
            ("kPrint", "\u001b[25~"),
            ("kPrint2", "\u001b[25;2~"),
            ("kPrint3", "\u001b[25;3~"),
            ("kPrint4", "\u001b[25;4~"),
            ("kPrint5", "\u001b[25;5~"),
            ("kPrint6", "\u001b[25;6~"),
            ("kPrint7", "\u001b[25;7~"),
            ("kScroll", "\u001b[28;2~"),
        ];
    }
}
