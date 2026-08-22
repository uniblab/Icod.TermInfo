using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class Vt100TerminalProfileTests
{
    [Fact]
    public void BuiltInDatabaseLoadsVt100ProfileAndAlias()
    {
        TerminalDescription canonical =
            TerminalDatabase.BuiltIn.Load("vt100");
        TerminalDescription alias =
            TerminalDatabase.BuiltIn.Load("vt100-am");

        Assert.Same(TerminalProfiles.Vt100, canonical);
        Assert.Same(canonical, alias);
        Assert.Equal("vt100", canonical.Name);
        Assert.Single(canonical.Aliases);
        Assert.Equal("vt100-am", canonical.Aliases[0]);
    }

    [Fact]
    public void BooleanCapabilitiesMatchVt100Contract()
    {
        HashSet<BooleanCapability> expected =
        [
            BooleanCapability.AutoRightMargin,
            BooleanCapability.MoveStandoutMode,
            BooleanCapability.EatNewlineGlitch,
            BooleanCapability.XonXoff,
        ];

        foreach (BooleanCapability capability in Enum.GetValues<BooleanCapability>())
        {
            Assert.Equal(
                expected.Contains(capability),
                TerminalProfiles.Vt100.GetBoolean(capability));
        }
    }

    [Fact]
    public void NumericCapabilitiesMatchVt100Contract()
    {
        Dictionary<NumericCapability, int> expected = new()
        {
            [NumericCapability.Columns] = 80,
            [NumericCapability.Lines] = 24,
            [NumericCapability.InitialTabWidth] = 8,
            [NumericCapability.VirtualTerminal] = 3,
        };

        foreach (NumericCapability capability in Enum.GetValues<NumericCapability>())
        {
            int? actual = TerminalProfiles.Vt100.GetNumber(capability);

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
    public void StringCapabilitiesMatchVt100GoldenTable()
    {
        Dictionary<StringCapability, string> expected = CreateExpectedStrings();

        foreach (StringCapability capability in Enum.GetValues<StringCapability>())
        {
            string? actual = TerminalProfiles.Vt100.GetString(capability);

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
    public void TraditionalNamesExposeVt100GeometryAndIdentity()
    {
        TerminalDescription terminal = TerminalProfiles.Vt100;

        Assert.True(terminal.TryGetNumber("cols", out int columns));
        Assert.Equal(80, columns);

        Assert.True(terminal.TryGetNumber("lines", out int lines));
        Assert.Equal(24, lines);

        Assert.True(terminal.TryGetNumber("it", out int tabWidth));
        Assert.Equal(8, tabWidth);

        Assert.True(terminal.TryGetNumber("vt", out int virtualTerminal));
        Assert.Equal(3, virtualTerminal);
    }

    [Fact]
    public void CursorAddressAndScrollRegionUseParameterExpansionEngine()
    {
        TerminalDescription terminal = TerminalProfiles.Vt100;

        Assert.Equal(
            "\x1b[1;1H$<5>",
            terminal.Expand(StringCapability.CursorAddress, 0, 0));
        Assert.Equal(
            "\x1b[11;21H$<5>",
            terminal.Expand(StringCapability.CursorAddress, 10, 20));
        Assert.Equal(
            "\x1b[1;24r",
            terminal.Expand(StringCapability.ChangeScrollRegion, 0, 23));
    }

    [Fact]
    public void RelativeMovementUsesSharedParameterExpansionEngine()
    {
        TerminalDescription terminal = TerminalProfiles.Vt100;

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
    }

    [Fact]
    public void SetAttributesUsesTerminfoConditionalProgram()
    {
        TerminalDescription terminal = TerminalProfiles.Vt100;

        string alternateCharacterSet =
            terminal.Expand(
                StringCapability.SetAttributes,
                0,
                1,
                0,
                1,
                0,
                1,
                0,
                0,
                1);

        Assert.Equal(
            "\x1b[0;1;4;5m\x0e$<2>",
            alternateCharacterSet);

        string normalCharacterSet =
            terminal.Expand(
                StringCapability.SetAttributes,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0);

        Assert.Equal(
            "\x1b[0m\x0f$<2>",
            normalCharacterSet);
    }

    [Fact]
    public void ApplicationCursorAndPfKeysMatchVt100Sequences()
    {
        TerminalDescription terminal = TerminalProfiles.Vt100;

        Assert.Equal(
            "\x1bOA",
            terminal.GetRequiredString(StringCapability.KeyCursorUp));
        Assert.Equal(
            "\x1bOB",
            terminal.GetRequiredString(StringCapability.KeyCursorDown));
        Assert.Equal(
            "\x1bOC",
            terminal.GetRequiredString(StringCapability.KeyCursorRight));
        Assert.Equal(
            "\x1bOD",
            terminal.GetRequiredString(StringCapability.KeyCursorLeft));

        Assert.Equal(
            "\x1bOP",
            terminal.GetRequiredString(StringCapability.KeyF1));
        Assert.Equal(
            "\x1bOQ",
            terminal.GetRequiredString(StringCapability.KeyF2));
        Assert.Equal(
            "\x1bOR",
            terminal.GetRequiredString(StringCapability.KeyF3));
        Assert.Equal(
            "\x1bOS",
            terminal.GetRequiredString(StringCapability.KeyF4));
    }

    [Fact]
    public void Vt100PaddingAnnotationsArePreservedForT5()
    {
        TerminalDescription terminal = TerminalProfiles.Vt100;

        Assert.Equal(
            "\x1b[H\x1b[J$<50>",
            terminal.GetRequiredString(StringCapability.ClearScreen));
        Assert.Equal(
            "\x1b[1m$<2>",
            terminal.GetRequiredString(StringCapability.EnterBoldMode));
        Assert.Equal(
            "\x1b[C$<2>",
            terminal.GetRequiredString(StringCapability.CursorRightOne));
        Assert.Equal(
            "\x1b[K$<3>",
            terminal.GetRequiredString(StringCapability.ClearToEndOfLine));
        Assert.Equal(
            "\x1bM$<5>",
            terminal.GetRequiredString(StringCapability.ScrollReverse));

        string expanded =
            terminal.Expand(StringCapability.CursorAddress, 4, 12);

        Assert.Equal("\x1b[5;13H$<5>", expanded);
    }

    [Fact]
    public void Vt100ProfileIsMonochrome()
    {
        TerminalDescription terminal = TerminalProfiles.Vt100;

        Assert.Null(terminal.GetNumber(NumericCapability.Colors));
        Assert.Null(terminal.GetNumber(NumericCapability.ColorPairs));
        Assert.Null(terminal.GetNumber(NumericCapability.NoColorVideo));
        Assert.Null(terminal.GetString(StringCapability.SetForegroundColor));
        Assert.Null(terminal.GetString(StringCapability.SetBackgroundColor));
        Assert.Null(terminal.GetString(StringCapability.OriginalColorPair));
    }

    [Fact]
    public void Vt100DoesNotAdvertiseVt102EditingExtensions()
    {
        TerminalDescription terminal = TerminalProfiles.Vt100;

        Assert.Null(terminal.GetString(StringCapability.DeleteCharacter));
        Assert.Null(terminal.GetString(StringCapability.DeleteCharacters));
        Assert.Null(terminal.GetString(StringCapability.DeleteLine));
        Assert.Null(terminal.GetString(StringCapability.DeleteLines));
        Assert.Null(terminal.GetString(StringCapability.InsertCharacter));
        Assert.Null(terminal.GetString(StringCapability.InsertCharacters));
        Assert.Null(terminal.GetString(StringCapability.InsertLine));
        Assert.Null(terminal.GetString(StringCapability.InsertLines));
        Assert.Null(terminal.GetString(StringCapability.EraseCharacters));
    }

    private static Dictionary<StringCapability, string> CreateExpectedStrings()
    {
        return new Dictionary<StringCapability, string>
        {
            [StringCapability.Bell] = "\a",
            [StringCapability.CarriageReturn] = "\r",
            [StringCapability.ChangeScrollRegion] =
                "\x1b[%i%p1%d;%p2%dr",
            [StringCapability.ClearScreen] =
                "\x1b[H\x1b[J$<50>",
            [StringCapability.CursorLeft] = "\x1b[%p1%dD",
            [StringCapability.CursorLeftOne] = "\b",
            [StringCapability.CursorDown] = "\x1b[%p1%dB",
            [StringCapability.CursorDownOne] = "\n",
            [StringCapability.CursorRight] = "\x1b[%p1%dC",
            [StringCapability.CursorRightOne] = "\x1b[C$<2>",
            [StringCapability.CursorUp] = "\x1b[%p1%dA",
            [StringCapability.CursorUpOne] = "\x1b[A$<2>",
            [StringCapability.CursorAddress] =
                "\x1b[%i%p1%d;%p2%dH$<5>",
            [StringCapability.CursorHome] = "\x1b[H",
            [StringCapability.SaveCursor] = "\u001b7",
            [StringCapability.RestoreCursor] = "\u001b8",
            [StringCapability.ClearToEndOfScreen] =
                "\x1b[J$<50>",
            [StringCapability.ClearToEndOfLine] =
                "\x1b[K$<3>",
            [StringCapability.ClearToBeginningOfLine] =
                "\x1b[1K$<3>",
            [StringCapability.Tab] = "\t",
            [StringCapability.SetTab] = "\x1bH",
            [StringCapability.ClearAllTabs] = "\x1b[3g",
            [StringCapability.ScrollForward] = "\n",
            [StringCapability.ScrollReverse] = "\x1bM$<5>",
            [StringCapability.EnterBlinkMode] = "\x1b[5m$<2>",
            [StringCapability.EnterBoldMode] = "\x1b[1m$<2>",
            [StringCapability.EnterReverseMode] = "\x1b[7m$<2>",
            [StringCapability.EnterStandoutMode] = "\x1b[7m$<2>",
            [StringCapability.ExitStandoutMode] = "\x1b[m$<2>",
            [StringCapability.EnterUnderlineMode] = "\x1b[4m$<2>",
            [StringCapability.ExitUnderlineMode] = "\x1b[m$<2>",
            [StringCapability.AlternateCharacterSet] =
                "``aaffggjjkkllmmnnooppqqrrssttuuvvwwxxyyzz{{||}}~~",
            [StringCapability.EnableAlternateCharacterSet] =
                "\x1b(B\x1b)0",
            [StringCapability.EnterAlternateCharacterSetMode] = "\x0e",
            [StringCapability.ExitAlternateCharacterSetMode] = "\x0f",
            [StringCapability.SetAttributes] =
                "\x1b[0%?%p1%p6%|%t;1%;%?%p2%t;4%;"
                + "%?%p1%p3%|%t;7%;%?%p4%t;5%;m"
                + "%?%p9%t\x0e%e\x0f%;$<2>",
            [StringCapability.ExitAttributeMode] =
                "\x1b[m\x0f$<2>",
            [StringCapability.ExitAutomaticMargins] = "\x1b[?7l",
            [StringCapability.EnterAutomaticMargins] = "\x1b[?7h",
            [StringCapability.ExitKeypadMode] = "\x1b[?1l\x1b>",
            [StringCapability.EnterKeypadMode] = "\x1b[?1h\x1b=",
            [StringCapability.ResetString2] =
                "\x1b<\x1b>\x1b[?3;4;5l\x1b[?7;8h\x1b[r",
            [StringCapability.KeyBackspace] = "\b",
            [StringCapability.KeyCursorDown] = "\x1bOB",
            [StringCapability.KeyCursorLeft] = "\x1bOD",
            [StringCapability.KeyCursorRight] = "\x1bOC",
            [StringCapability.KeyCursorUp] = "\x1bOA",
            [StringCapability.KeyF1] = "\x1bOP",
            [StringCapability.KeyF2] = "\x1bOQ",
            [StringCapability.KeyF3] = "\x1bOR",
            [StringCapability.KeyF4] = "\x1bOS",
        };
    }
}
