using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class AnsiTerminalProfileTests
{
    [Fact]
    public void BuiltInDatabaseLoadsAnsiProfile()
    {
        TerminalDescription terminal =
            TerminalDatabase.BuiltIn.Load("ansi");

        Assert.Same(TerminalProfiles.Ansi, terminal);
        Assert.Equal("ansi", terminal.Name);
        Assert.Empty(terminal.Aliases);
    }

    [Fact]
    public void BooleanCapabilitiesMatchAnsiContract()
    {
        HashSet<BooleanCapability> expected =
        [
            BooleanCapability.AutoRightMargin,
            BooleanCapability.MoveStandoutMode,
            BooleanCapability.MoveInsertMode,
        ];

        foreach (BooleanCapability capability in Enum.GetValues<BooleanCapability>())
        {
            Assert.Equal(
                expected.Contains(capability),
                TerminalProfiles.Ansi.GetBoolean(capability));
        }
    }

    [Fact]
    public void NumericCapabilitiesMatchAnsiContract()
    {
        Dictionary<NumericCapability, int> expected = new()
        {
            [NumericCapability.Columns] = 80,
            [NumericCapability.Lines] = 24,
            [NumericCapability.Colors] = 8,
            [NumericCapability.ColorPairs] = 64,
            [NumericCapability.InitialTabWidth] = 8,
            [NumericCapability.NoColorVideo] = 3,
        };

        foreach (NumericCapability capability in Enum.GetValues<NumericCapability>())
        {
            int? actual = TerminalProfiles.Ansi.GetNumber(capability);

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
    public void StringCapabilitiesMatchAnsiGoldenTable()
    {
        Dictionary<StringCapability, string> expected = CreateExpectedStrings();

        foreach (StringCapability capability in Enum.GetValues<StringCapability>())
        {
            string? actual = TerminalProfiles.Ansi.GetString(capability);

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
    public void TraditionalNamesExposeAnsiColorMetadata()
    {
        TerminalDescription terminal = TerminalProfiles.Ansi;

        Assert.True(terminal.TryGetNumber("colors", out int colors));
        Assert.Equal(8, colors);

        Assert.True(terminal.TryGetNumber("pairs", out int pairs));
        Assert.Equal(64, pairs);

        Assert.True(terminal.TryGetNumber("ncv", out int noColorVideo));
        Assert.Equal(3, noColorVideo);
    }

    [Fact]
    public void CursorAddressUsesParameterExpansionEngine()
    {
        string sequence =
            TerminalProfiles.Ansi.Expand(
                StringCapability.CursorAddress,
                0,
                0);

        Assert.Equal("\x1b[1;1H", sequence);

        sequence =
            TerminalProfiles.Ansi.Expand(
                StringCapability.CursorAddress,
                10,
                20);

        Assert.Equal("\x1b[11;21H", sequence);
    }

    [Fact]
    public void RelativeMovementAndErasureUseParameterExpansionEngine()
    {
        TerminalDescription terminal = TerminalProfiles.Ansi;

        Assert.Equal(
            "\x1b[7D",
            terminal.Expand(StringCapability.CursorLeft, 7));
        Assert.Equal(
            "\x1b[6B",
            terminal.Expand(StringCapability.CursorDown, 6));
        Assert.Equal(
            "\x1b[5C",
            terminal.Expand(StringCapability.CursorRight, 5));
        Assert.Equal(
            "\x1b[4A",
            terminal.Expand(StringCapability.CursorUp, 4));
        Assert.Equal(
            "\x1b[12X",
            terminal.Expand(StringCapability.EraseCharacters, 12));
    }

    [Fact]
    public void EightColorCapabilitiesExpandToClassicAnsiSequences()
    {
        TerminalDescription terminal = TerminalProfiles.Ansi;

        Assert.Equal(
            "\x1b[30m",
            terminal.Expand(StringCapability.SetForegroundColor, 0));
        Assert.Equal(
            "\x1b[37m",
            terminal.Expand(StringCapability.SetForegroundColor, 7));
        Assert.Equal(
            "\x1b[40m",
            terminal.Expand(StringCapability.SetBackgroundColor, 0));
        Assert.Equal(
            "\x1b[47m",
            terminal.Expand(StringCapability.SetBackgroundColor, 7));
        Assert.Equal(
            "\x1b[39;49m",
            terminal.GetRequiredString(StringCapability.OriginalColorPair));
    }

    [Fact]
    public void SetAttributesUsesTerminfoConditionalProgram()
    {
        string sequence =
            TerminalProfiles.Ansi.Expand(
                StringCapability.SetAttributes,
                0,
                1,
                0,
                0,
                0,
                1,
                1,
                0,
                0);

        Assert.Equal("\x1b[0;10;4;1;8m", sequence);
    }

    [Fact]
    public void AnsiProfileDoesNotAdvertiseExtendedColorModes()
    {
        Assert.Equal<int?>(
            8,
            TerminalProfiles.Ansi.GetNumber(NumericCapability.Colors));

        foreach (string value in CreateExpectedStrings().Values)
        {
            Assert.DoesNotContain("38;5", value, StringComparison.Ordinal);
            Assert.DoesNotContain("48;5", value, StringComparison.Ordinal);
            Assert.DoesNotContain("38;2", value, StringComparison.Ordinal);
            Assert.DoesNotContain("48;2", value, StringComparison.Ordinal);
        }
    }

    [Theory]
    [InlineData("xterm-256color")]
    [InlineData("screen")]
    [InlineData("tmux")]
    [InlineData("linux")]
    public void ModernTerminalNamesDoNotImplicitlyResolveAsAnsi(string name)
    {
        Assert.False(
            TerminalDatabase.BuiltIn.TryLoad(
                name,
                out TerminalDescription? terminal));
        Assert.Null(terminal);
    }

    private static Dictionary<StringCapability, string> CreateExpectedStrings()
    {
        return new Dictionary<StringCapability, string>
        {
            [StringCapability.Bell] = "\a",
            [StringCapability.CarriageReturn] = "\r",
            [StringCapability.ClearScreen] = "\x1b[H\x1b[J",
            [StringCapability.CursorLeft] = "\x1b[%p1%dD",
            [StringCapability.CursorLeftOne] = "\x1b[D",
            [StringCapability.CursorDown] = "\x1b[%p1%dB",
            [StringCapability.CursorDownOne] = "\x1b[B",
            [StringCapability.CursorRight] = "\x1b[%p1%dC",
            [StringCapability.CursorRightOne] = "\x1b[C",
            [StringCapability.CursorUp] = "\x1b[%p1%dA",
            [StringCapability.CursorUpOne] = "\x1b[A",
            [StringCapability.CursorAddress] = "\x1b[%i%p1%d;%p2%dH",
            [StringCapability.CursorHome] = "\x1b[H",
            [StringCapability.ColumnAddress] = "\x1b[%i%p1%dG",
            [StringCapability.RowAddress] = "\x1b[%i%p1%dd",
            [StringCapability.SaveCursor] = "\u001b7",
            [StringCapability.RestoreCursor] = "\u001b8",
            [StringCapability.ClearToEndOfScreen] = "\x1b[J",
            [StringCapability.ClearToEndOfLine] = "\x1b[K",
            [StringCapability.ClearToBeginningOfLine] = "\x1b[1K",
            [StringCapability.EraseCharacters] = "\x1b[%p1%dX",
            [StringCapability.DeleteCharacters] = "\x1b[%p1%dP",
            [StringCapability.DeleteCharacter] = "\x1b[P",
            [StringCapability.DeleteLines] = "\x1b[%p1%dM",
            [StringCapability.DeleteLine] = "\x1b[M",
            [StringCapability.InsertCharacters] = "\x1b[%p1%d@",
            [StringCapability.InsertLines] = "\x1b[%p1%dL",
            [StringCapability.InsertLine] = "\x1b[L",
            [StringCapability.BackTab] = "\x1b[Z",
            [StringCapability.Tab] = "\x1b[I",
            [StringCapability.SetTab] = "\x1bH",
            [StringCapability.ClearAllTabs] = "\x1b[3g",
            [StringCapability.ScrollForward] = "\n",
            [StringCapability.EnterBlinkMode] = "\x1b[5m",
            [StringCapability.EnterBoldMode] = "\x1b[1m",
            [StringCapability.EnterReverseMode] = "\x1b[7m",
            [StringCapability.EnterInvisibleMode] = "\x1b[8m",
            [StringCapability.EnterStandoutMode] = "\x1b[7m",
            [StringCapability.ExitStandoutMode] = "\x1b[m",
            [StringCapability.EnterUnderlineMode] = "\x1b[4m",
            [StringCapability.ExitUnderlineMode] = "\x1b[m",
            [StringCapability.EnterAlternateCharacterSetMode] = "\x1b[11m",
            [StringCapability.ExitAlternateCharacterSetMode] = "\x1b[10m",
            [StringCapability.SetAttributes] =
                "\x1b[0;10%?%p1%t;7%;%?%p2%t;4%;%?%p3%t;7%;"
                + "%?%p4%t;5%;%?%p6%t;1%;%?%p7%t;8%;"
                + "%?%p9%t;11%;m",
            [StringCapability.ExitAttributeMode] = "\x1b[0;10m",
            [StringCapability.SetForegroundColor] = "\x1b[3%p1%dm",
            [StringCapability.SetBackgroundColor] = "\x1b[4%p1%dm",
            [StringCapability.OriginalColorPair] = "\x1b[39;49m",
            [StringCapability.KeyBackspace] = "\b",
            [StringCapability.KeyCursorDown] = "\x1b[B",
            [StringCapability.KeyCursorLeft] = "\x1b[D",
            [StringCapability.KeyCursorRight] = "\x1b[C",
            [StringCapability.KeyCursorUp] = "\x1b[A",
            [StringCapability.KeyHome] = "\x1b[H",
        };
    }
}
