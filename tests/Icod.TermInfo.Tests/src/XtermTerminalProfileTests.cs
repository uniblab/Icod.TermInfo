using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class XtermTerminalProfileTests
{
    [Fact]
    public void BuiltInDatabaseLoadsXtermAsItself()
    {
        TerminalDescription terminal = TerminalDatabase.BuiltIn.Load("xterm");

        Assert.Same(TerminalProfiles.Xterm, terminal);
        Assert.NotSame(TerminalProfiles.Ansi, terminal);
        Assert.Equal("xterm", terminal.Name);
        Assert.Empty(terminal.Aliases);
    }

    [Fact]
    public void BooleanCapabilitiesMatchSelectedXtermBaseline()
    {
        HashSet<BooleanCapability> expected =
        [
            BooleanCapability.AutoRightMargin,
            BooleanCapability.BackColorErase,
            BooleanCapability.BackspacesWithBs,
            BooleanCapability.EatNewlineGlitch,
            BooleanCapability.HasMetaKey,
            BooleanCapability.MoveInsertMode,
            BooleanCapability.MoveStandoutMode,
            BooleanCapability.NoPadCharacter,
        ];

        foreach (BooleanCapability capability in Enum.GetValues<BooleanCapability>())
        {
            Assert.Equal(
                expected.Contains(capability),
                TerminalProfiles.Xterm.GetBoolean(capability));
        }
    }

    [Fact]
    public void NumericCapabilitiesMatchSelectedXtermBaseline()
    {
        Dictionary<NumericCapability, int> expected = new()
        {
            [NumericCapability.Columns] = 80,
            [NumericCapability.Lines] = 24,
            [NumericCapability.Colors] = 8,
            [NumericCapability.ColorPairs] = 64,
            [NumericCapability.InitialTabWidth] = 8,
        };

        foreach (NumericCapability capability in Enum.GetValues<NumericCapability>())
        {
            int? actual = TerminalProfiles.Xterm.GetNumber(capability);

            if (expected.TryGetValue(capability, out int value))
            {
                Assert.Equal<int?>(value, actual);
            }
            else
            {
                Assert.Null(actual);
            }
        }
    }

    [Fact]
    public void EveryAdvertisedTypedStringHasGoldenCoverage()
    {
        Dictionary<StringCapability, string> expected = CreateExpectedStrings();

        foreach (StringCapability capability in Enum.GetValues<StringCapability>())
        {
            string? actual = TerminalProfiles.Xterm.GetString(capability);

            if (expected.TryGetValue(capability, out string? value))
            {
                Assert.Equal(value, actual);
            }
            else
            {
                Assert.Null(actual);
            }
        }
    }

    [Fact]
    public void EveryAdvertisedNamedCapabilityHasGoldenCoverage()
    {
        Dictionary<string, TermInfoCapabilityValue> expected =
            CreateExpectedExtendedCapabilities();
        IReadOnlyDictionary<string, TermInfoCapabilityValue> actual =
            TerminalProfiles.Xterm.ExtendedCapabilities;

        Assert.Equal(expected.Count, actual.Count);

        foreach ((string name, TermInfoCapabilityValue value) in expected)
        {
            Assert.True(actual.TryGetValue(name, out TermInfoCapabilityValue found));
            Assert.Equal(value, found);
        }
    }

    [Fact]
    public void XtermRetainsOrdinaryEightColorSemantics()
    {
        TerminalColorSupport support =
            TerminalColors.GetColorSupport(TerminalProfiles.Xterm);

        Assert.Equal(TerminalColorModel.Indexed, support.Model);
        Assert.Equal(TerminalColorTier.Color8, support.Tier);
        Assert.Equal<int?>(8, support.ColorCount);
        Assert.Equal(8, support.IndexedColorCount);
        Assert.Equal<int?>(64, support.ColorPairCount);
        Assert.True(support.BackColorErase);
        Assert.False(support.CanChangeColor);

        Assert.Equal(
            "\u001b[30m",
            TerminalColors.ExpandForeground(TerminalProfiles.Xterm, 0));
        Assert.Equal(
            "\u001b[37m",
            TerminalColors.ExpandForeground(TerminalProfiles.Xterm, 7));
        Assert.Equal(
            "\u001b[40m",
            TerminalColors.ExpandBackground(TerminalProfiles.Xterm, 0));
        Assert.Equal(
            "\u001b[47m",
            TerminalColors.ExpandBackground(TerminalProfiles.Xterm, 7));
    }

    [Fact]
    public void FullScreenAndCursorVisibilityProgramsMatchBaseline()
    {
        TerminalDescription terminal = TerminalProfiles.Xterm;

        Assert.Equal(
            "\u001b[?1049h\u001b[22;0;0t",
            terminal.GetRequiredString(
                StringCapability.EnterCursorAddressingMode));
        Assert.Equal(
            "\u001b[?1049l\u001b[23;0;0t",
            terminal.GetRequiredString(
                StringCapability.ExitCursorAddressingMode));
        Assert.Equal(
            "\u001b[?25l",
            terminal.GetRequiredString(StringCapability.CursorInvisible));
        Assert.Equal(
            "\u001b[?12l\u001b[?25h",
            terminal.GetRequiredString(StringCapability.CursorNormal));
        Assert.Equal(
            "\u001b[?12;25h",
            terminal.GetRequiredString(StringCapability.CursorVeryVisible));
    }

    [Fact]
    public void MouseFocusAndPasteMetadataRemainDescriptive()
    {
        TerminalDescription terminal = TerminalProfiles.Xterm;

        Assert.True(terminal.TryGetExtendedString("XM", out string? mouseMode));
        Assert.Equal(
            "\u001b[?1006;1000h",
            TermInfoParameterExpander.Expand(mouseMode!, 1));
        Assert.Equal(
            "\u001b[?1006;1000l",
            TermInfoParameterExpander.Expand(mouseMode!, 0));

        Assert.True(terminal.TryGetExtendedString("BE", out string? pasteOn));
        Assert.True(terminal.TryGetExtendedString("BD", out string? pasteOff));
        Assert.True(terminal.TryGetExtendedString("PS", out string? pasteStart));
        Assert.True(terminal.TryGetExtendedString("PE", out string? pasteEnd));
        Assert.Equal("\u001b[?2004h", pasteOn);
        Assert.Equal("\u001b[?2004l", pasteOff);
        Assert.Equal("\u001b[200~", pasteStart);
        Assert.Equal("\u001b[201~", pasteEnd);

        Assert.True(terminal.TryGetExtendedBoolean("XF", out bool focus));
        Assert.True(focus);
        Assert.True(terminal.TryGetExtendedString("fe", out string? focusOn));
        Assert.True(terminal.TryGetExtendedString("fd", out string? focusOff));
        Assert.Equal("\u001b[?1004h", focusOn);
        Assert.Equal("\u001b[?1004l", focusOff);
    }

    [Fact]
    public void CursorStyleAndClipboardMetadataUseCurrentTmux2Fragment()
    {
        TerminalDescription terminal = TerminalProfiles.Xterm;

        Assert.True(terminal.TryGetExtendedString("Se", out string? cursorReset));
        Assert.True(terminal.TryGetExtendedString("Ss", out string? cursorStyle));
        Assert.True(terminal.TryGetExtendedString("Cr", out string? colorReset));
        Assert.True(terminal.TryGetExtendedString("Cs", out string? cursorColor));
        Assert.True(terminal.TryGetExtendedString("Ms", out string? clipboard));

        Assert.Equal("\u001b[ q", cursorReset);
        Assert.Equal("\u001b[5 q", TermInfoParameterExpander.Expand(cursorStyle!, 5));
        Assert.Equal("\u001b]112\u001b\\", colorReset);
        Assert.Equal(
            "\u001b]12;red\u001b\\",
            TermInfoParameterExpander.Expand(cursorColor!, "red"));
        Assert.Equal(
            "\u001b]52;c;YWJj\u001b\\",
            TermInfoParameterExpander.Expand(clipboard!, "c", "YWJj"));
    }

    [Theory]
    [InlineData("xterm-mono")]
    [InlineData("xterm-16color")]
    [InlineData("xterm-88color")]
    [InlineData("xterm-256color")]
    [InlineData("xterm-direct")]
    [InlineData("xterm-direct16")]
    [InlineData("xterm-direct256")]
    public void SelectedXtermVariantsResolveOnlyWhenImplemented(string name)
    {
        bool expectedSupported =
            name is "xterm-16color"
            or "xterm-88color"
            or "xterm-256color"
            or "xterm-direct"
            or "xterm-direct16"
            or "xterm-direct256";

        bool resolved =
            TerminalDatabase.BuiltIn.TryLoad(
                name,
                out TerminalDescription? terminal);

        Assert.Equal(expectedSupported, resolved);
        Assert.Equal(expectedSupported, terminal is not null);
    }

    private static Dictionary<StringCapability, string> CreateExpectedStrings()
    {
        return new Dictionary<StringCapability, string>
        {
            [StringCapability.Bell] = "\a",
            [StringCapability.BackTab] = "\u001b[Z",
            [StringCapability.EnterBlinkMode] = "\u001b[5m",
            [StringCapability.EnterBoldMode] = "\u001b[1m",
            [StringCapability.EnterDimMode] = "\u001b[2m",
            [StringCapability.CarriageReturn] = "\r",
            [StringCapability.ChangeScrollRegion] = "\u001b[%i%p1%d;%p2%dr",
            [StringCapability.ClearScreen] = "\u001b[H\u001b[2J",
            [StringCapability.CursorLeft] = "\u001b[%p1%dD",
            [StringCapability.CursorLeftOne] = "\b",
            [StringCapability.CursorDown] = "\u001b[%p1%dB",
            [StringCapability.CursorDownOne] = "\n",
            [StringCapability.CursorRight] = "\u001b[%p1%dC",
            [StringCapability.CursorRightOne] = "\u001b[C",
            [StringCapability.CursorAddress] = "\u001b[%i%p1%d;%p2%dH",
            [StringCapability.CursorUp] = "\u001b[%p1%dA",
            [StringCapability.CursorUpOne] = "\u001b[A",
            [StringCapability.DeleteCharacters] = "\u001b[%p1%dP",
            [StringCapability.DeleteCharacter] = "\u001b[P",
            [StringCapability.DeleteLines] = "\u001b[%p1%dM",
            [StringCapability.DeleteLine] = "\u001b[M",
            [StringCapability.ClearToEndOfScreen] = "\u001b[J",
            [StringCapability.ClearToEndOfLine] = "\u001b[K",
            [StringCapability.ClearToBeginningOfLine] = "\u001b[1K",
            [StringCapability.CursorHome] = "\u001b[H",
            [StringCapability.ColumnAddress] = "\u001b[%i%p1%dG",
            [StringCapability.Tab] = "\t",
            [StringCapability.SetTab] = "\u001bH",
            [StringCapability.InsertCharacters] = "\u001b[%p1%d@",
            [StringCapability.InsertLines] = "\u001b[%p1%dL",
            [StringCapability.InsertLine] = "\u001b[L",
            [StringCapability.ScrollForward] = "\n",
            [StringCapability.EnterInvisibleMode] = "\u001b[8m",
            [StringCapability.OriginalColorPair] = "\u001b[39;49m",
            [StringCapability.RestoreCursor] = "\u001b8",
            [StringCapability.EnterReverseMode] = "\u001b[7m",
            [StringCapability.ScrollReverse] = "\u001bM",
            [StringCapability.ExitAlternateCharacterSetMode] = "\u001b(B",
            [StringCapability.ExitAutomaticMargins] = "\u001b[?7l",
            [StringCapability.ExitKeypadMode] = "\u001b[?1l\u001b>",
            [StringCapability.ExitStandoutMode] = "\u001b[27m",
            [StringCapability.ExitUnderlineMode] = "\u001b[24m",
            [StringCapability.SaveCursor] = "\u001b7",
            [StringCapability.SetAttributes] =
                "%?%p9%t\u001b(0%e\u001b(B%;"
                + "\u001b[0%?%p6%t;1%;%?%p5%t;2%;%?%p2%t;4%;"
                + "%?%p1%p3%|%t;7%;%?%p4%t;5%;%?%p7%t;8%;m",
            [StringCapability.ExitAttributeMode] = "\u001b(B\u001b[m",
            [StringCapability.EnterAlternateCharacterSetMode] = "\u001b(0",
            [StringCapability.EnterAutomaticMargins] = "\u001b[?7h",
            [StringCapability.EnterKeypadMode] = "\u001b[?1h\u001b=",
            [StringCapability.EnterStandoutMode] = "\u001b[7m",
            [StringCapability.EnterUnderlineMode] = "\u001b[4m",
            [StringCapability.RowAddress] = "\u001b[%i%p1%dd",
            [StringCapability.AlternateCharacterSet] =
                "``aaffggiijjkkllmmnnooppqqrrssttuuvvwwxxyyzz{{||}}~~",
            [StringCapability.ResetString2] = "\u001b[!p\u001b[?3;4l\u001b[4l\u001b>",
            [StringCapability.EraseCharacters] = "\u001b[%p1%dX",
            [StringCapability.ClearAllTabs] = "\u001b[3g",
            [StringCapability.EnterCursorAddressingMode] =
                "\u001b[?1049h\u001b[22;0;0t",
            [StringCapability.ExitCursorAddressingMode] =
                "\u001b[?1049l\u001b[23;0;0t",
            [StringCapability.CursorInvisible] = "\u001b[?25l",
            [StringCapability.CursorNormal] = "\u001b[?12l\u001b[?25h",
            [StringCapability.CursorVeryVisible] = "\u001b[?12;25h",
            [StringCapability.FlashScreen] = "\u001b[?5h$<100/>\u001b[?5l",
            [StringCapability.NewLine] = "\u001bE",
            [StringCapability.ScrollForwardLines] = "\u001b[%p1%dS",
            [StringCapability.ScrollReverseLines] = "\u001b[%p1%dT",
            [StringCapability.EnterInsertMode] = "\u001b[4h",
            [StringCapability.ExitInsertMode] = "\u001b[4l",
            [StringCapability.EnterMetaMode] = "\u001b[?1034h",
            [StringCapability.ExitMetaMode] = "\u001b[?1034l",
            [StringCapability.EnterItalicMode] = "\u001b[3m",
            [StringCapability.ExitItalicMode] = "\u001b[23m",
            [StringCapability.SetLegacyForegroundColor] =
                "\u001b[3%?%p1%{1}%=%t4%e%p1%{3}%=%t6%e"
                + "%p1%{4}%=%t1%e%p1%{6}%=%t3%e%p1%d%;m",
            [StringCapability.SetLegacyBackgroundColor] =
                "\u001b[4%?%p1%{1}%=%t4%e%p1%{3}%=%t6%e"
                + "%p1%{4}%=%t1%e%p1%{6}%=%t3%e%p1%d%;m",
            [StringCapability.InitString2] = "\u001b[!p\u001b[?3;4l\u001b[4l\u001b>",
            [StringCapability.ResetString1] = "\u001bc",
            [StringCapability.KeyMouse] = "\u001b[<",
            [StringCapability.MemoryLock] = "\u001bl",
            [StringCapability.MemoryUnlock] = "\u001bm",
            [StringCapability.RepeatCharacter] = "%p1%c\u001b[%p2%{1}%-%db",
            [StringCapability.PrintScreen] = "\u001b[i",
            [StringCapability.PrinterOff] = "\u001b[4i",
            [StringCapability.PrinterOn] = "\u001b[5i",
            [StringCapability.KeyBackspace] = "\b",
            [StringCapability.KeyBackTab] = "\u001b[Z",
            [StringCapability.KeyBegin] = "\u001bOE",
            [StringCapability.KeyDeleteCharacter] = "\u001b[3~",
            [StringCapability.KeyEnd] = "\u001bOF",
            [StringCapability.KeyEnter] = "\u001bOM",
            [StringCapability.KeyHome] = "\u001bOH",
            [StringCapability.KeyInsertCharacter] = "\u001b[2~",
            [StringCapability.KeyNextPage] = "\u001b[6~",
            [StringCapability.KeyPreviousPage] = "\u001b[5~",
            [StringCapability.KeyCursorDown] = "\u001bOB",
            [StringCapability.KeyCursorLeft] = "\u001bOD",
            [StringCapability.KeyCursorRight] = "\u001bOC",
            [StringCapability.KeyCursorUp] = "\u001bOA",
            [StringCapability.KeyF1] = "\u001bOP",
            [StringCapability.KeyF2] = "\u001bOQ",
            [StringCapability.KeyF3] = "\u001bOR",
            [StringCapability.KeyF4] = "\u001bOS",
            [StringCapability.KeyF5] = "\u001b[15~",
            [StringCapability.KeyF6] = "\u001b[17~",
            [StringCapability.KeyF7] = "\u001b[18~",
            [StringCapability.KeyF8] = "\u001b[19~",
            [StringCapability.KeyF9] = "\u001b[20~",
            [StringCapability.KeyF10] = "\u001b[21~",
            [StringCapability.KeyF11] = "\u001b[23~",
            [StringCapability.KeyF12] = "\u001b[24~",
            [StringCapability.KeyF13] = "\u001b[1;2P",
            [StringCapability.KeyF14] = "\u001b[1;2Q",
            [StringCapability.KeyF15] = "\u001b[1;2R",
            [StringCapability.KeyF16] = "\u001b[1;2S",
            [StringCapability.KeyF17] = "\u001b[15;2~",
            [StringCapability.KeyF18] = "\u001b[17;2~",
            [StringCapability.KeyF19] = "\u001b[18;2~",
            [StringCapability.KeyF20] = "\u001b[19;2~",
            [StringCapability.KeyF21] = "\u001b[20;2~",
            [StringCapability.KeyF22] = "\u001b[21;2~",
            [StringCapability.KeyF23] = "\u001b[23;2~",
            [StringCapability.KeyF24] = "\u001b[24;2~",
            [StringCapability.KeyA1] = "\u001bOw",
            [StringCapability.KeyA3] = "\u001bOy",
            [StringCapability.KeyB2] = "\u001bOu",
            [StringCapability.KeyC1] = "\u001bOq",
            [StringCapability.KeyC3] = "\u001bOs",
            [StringCapability.KeyScrollForward] = "\u001b[1;2B",
            [StringCapability.KeyScrollReverse] = "\u001b[1;2A",
            [StringCapability.KeyShiftDeleteCharacter] = "\u001b[3;2~",
            [StringCapability.KeyShiftEnd] = "\u001b[1;2F",
            [StringCapability.KeyShiftHome] = "\u001b[1;2H",
            [StringCapability.KeyShiftInsertCharacter] = "\u001b[2;2~",
            [StringCapability.KeyShiftLeft] = "\u001b[1;2D",
            [StringCapability.KeyShiftNextPage] = "\u001b[6;2~",
            [StringCapability.KeyShiftPreviousPage] = "\u001b[5;2~",
            [StringCapability.KeyShiftRight] = "\u001b[1;2C",
            [StringCapability.SetForegroundColor] = "\u001b[3%p1%dm",
            [StringCapability.SetBackgroundColor] = "\u001b[4%p1%dm",
        };
    }

    private static Dictionary<string, TermInfoCapabilityValue>
        CreateExpectedExtendedCapabilities()
    {
        Dictionary<string, TermInfoCapabilityValue> expected =
            new(StringComparer.Ordinal)
            {
                ["AX"] = new TermInfoCapabilityValue(true),
                ["XF"] = new TermInfoCapabilityValue(true),
                ["XT"] = new TermInfoCapabilityValue(true),
                ["E3"] = new TermInfoCapabilityValue("\u001b[3J"),
                ["BD"] = new TermInfoCapabilityValue("\u001b[?2004l"),
                ["BE"] = new TermInfoCapabilityValue("\u001b[?2004h"),
                ["PE"] = new TermInfoCapabilityValue("\u001b[201~"),
                ["PS"] = new TermInfoCapabilityValue("\u001b[200~"),
                ["Cr"] = new TermInfoCapabilityValue("\u001b]112\u001b\\"),
                ["Cs"] = new TermInfoCapabilityValue("\u001b]12;%p1%s\u001b\\"),
                ["Ms"] = new TermInfoCapabilityValue("\u001b]52;%p1%s;%p2%s\u001b\\"),
                ["Se"] = new TermInfoCapabilityValue("\u001b[ q"),
                ["Ss"] = new TermInfoCapabilityValue("\u001b[%p1%d q"),
                ["XM"] = new TermInfoCapabilityValue(
                    "\u001b[?1006;1000%?%p1%{1}%=%th%el%;"),
                ["xm"] = new TermInfoCapabilityValue(
                    "\u001b[<%i%p3%d;%p1%d;%p2%d;%?%p4%tM%em%;"),
                ["fd"] = new TermInfoCapabilityValue("\u001b[?1004l"),
                ["fe"] = new TermInfoCapabilityValue("\u001b[?1004h"),
                ["kxIN"] = new TermInfoCapabilityValue("\u001b[I"),
                ["kxOUT"] = new TermInfoCapabilityValue("\u001b[O"),
                ["RV"] = new TermInfoCapabilityValue("\u001b[>c"),
                ["rv"] = new TermInfoCapabilityValue(
                    "\u001b\\[>41;[1-6][0-9][0-9];0c"),
                ["XR"] = new TermInfoCapabilityValue("\u001b[>0q"),
                ["xr"] = new TermInfoCapabilityValue(
                    "\u001bP>\\|XTerm\\(([1-9][0-9]+)\\)\u001b\\\\"),
                ["smxx"] = new TermInfoCapabilityValue("\u001b[9m"),
                ["rmxx"] = new TermInfoCapabilityValue("\u001b[29m"),
            };

        foreach ((string name, string value) in CreateExpectedNamedKeys())
        {
            expected.Add(name, new TermInfoCapabilityValue(value));
        }

        return expected;
    }

    private static (string Name, string Value)[] CreateExpectedNamedKeys()
    {
        return
        [
            ("ka2", "\u001bOx"),
            ("kb1", "\u001bOt"), ("kb3", "\u001bOv"),
            ("kc2", "\u001bOr"),
            ("kp5", "\u001bOE"), ("kpADD", "\u001bOk"), ("kpCMA", "\u001bOl"),
            ("kpDIV", "\u001bOo"), ("kpDOT", "\u001bOn"), ("kpMUL", "\u001bOj"),
            ("kpSUB", "\u001bOm"), ("kpZRO", "\u001bOp"),
            ("kDN", "\u001b[1;2B"), ("kUP", "\u001b[1;2A"),
            ("kDC3", "\u001b[3;3~"), ("kDC4", "\u001b[3;4~"),
            ("kDC5", "\u001b[3;5~"), ("kDC6", "\u001b[3;6~"),
            ("kDC7", "\u001b[3;7~"),
            ("kEND3", "\u001b[1;3F"), ("kEND4", "\u001b[1;4F"),
            ("kEND5", "\u001b[1;5F"), ("kEND6", "\u001b[1;6F"),
            ("kEND7", "\u001b[1;7F"),
            ("kHOM3", "\u001b[1;3H"), ("kHOM4", "\u001b[1;4H"),
            ("kHOM5", "\u001b[1;5H"), ("kHOM6", "\u001b[1;6H"),
            ("kHOM7", "\u001b[1;7H"),
            ("kIC3", "\u001b[2;3~"), ("kIC4", "\u001b[2;4~"),
            ("kIC5", "\u001b[2;5~"), ("kIC6", "\u001b[2;6~"),
            ("kIC7", "\u001b[2;7~"),
            ("kLFT3", "\u001b[1;3D"), ("kLFT4", "\u001b[1;4D"),
            ("kLFT5", "\u001b[1;5D"), ("kLFT6", "\u001b[1;6D"),
            ("kLFT7", "\u001b[1;7D"),
            ("kNXT3", "\u001b[6;3~"), ("kNXT4", "\u001b[6;4~"),
            ("kNXT5", "\u001b[6;5~"), ("kNXT6", "\u001b[6;6~"),
            ("kNXT7", "\u001b[6;7~"),
            ("kPRV3", "\u001b[5;3~"), ("kPRV4", "\u001b[5;4~"),
            ("kPRV5", "\u001b[5;5~"), ("kPRV6", "\u001b[5;6~"),
            ("kPRV7", "\u001b[5;7~"),
            ("kRIT3", "\u001b[1;3C"), ("kRIT4", "\u001b[1;4C"),
            ("kRIT5", "\u001b[1;5C"), ("kRIT6", "\u001b[1;6C"),
            ("kRIT7", "\u001b[1;7C"),
            ("kDN3", "\u001b[1;3B"), ("kDN4", "\u001b[1;4B"),
            ("kDN5", "\u001b[1;5B"), ("kDN6", "\u001b[1;6B"),
            ("kDN7", "\u001b[1;7B"),
            ("kUP3", "\u001b[1;3A"), ("kUP4", "\u001b[1;4A"),
            ("kUP5", "\u001b[1;5A"), ("kUP6", "\u001b[1;6A"),
            ("kUP7", "\u001b[1;7A"),
            ("kPause", "\u001b[26;2~"), ("kPrint", "\u001b[25~"),
            ("kPrint2", "\u001b[25;2~"), ("kPrint3", "\u001b[25;3~"),
            ("kPrint4", "\u001b[25;4~"), ("kPrint5", "\u001b[25;5~"),
            ("kPrint6", "\u001b[25;6~"), ("kPrint7", "\u001b[25;7~"),
            ("kScroll", "\u001b[28;2~"),
        ];
    }
}
