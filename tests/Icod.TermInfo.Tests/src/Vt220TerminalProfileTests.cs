using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class Vt220TerminalProfileTests
{
    [Fact]
    public void BuiltInDatabaseLoadsVt220AndVt200Alias()
    {
        TerminalDescription canonical = TerminalDatabase.BuiltIn.Load("vt220");
        TerminalDescription alias = TerminalDatabase.BuiltIn.Load("vt200");

        Assert.Same(TerminalProfiles.Vt220, canonical);
        Assert.Same(canonical, alias);
        Assert.Equal("vt220", canonical.Name);
        Assert.Single(canonical.Aliases);
        Assert.Equal("vt200", canonical.Aliases[0]);
    }

    [Fact]
    public void BooleanAndNumericCapabilitiesMatchSelectedCanonicalSevenBitBaseline()
    {
        HashSet<BooleanCapability> expectedBooleans =
        [
            BooleanCapability.AutoRightMargin,
            BooleanCapability.BackspacesWithBs,
            BooleanCapability.MoveInsertMode,
            BooleanCapability.MoveStandoutMode,
            BooleanCapability.EatNewlineGlitch,
            BooleanCapability.XonXoff,
        ];
        Dictionary<NumericCapability, int> expectedNumbers = new()
        {
            [NumericCapability.Columns] = 80,
            [NumericCapability.Lines] = 24,
            [NumericCapability.InitialTabWidth] = 8,
            [NumericCapability.VirtualTerminal] = 3,
        };

        foreach (BooleanCapability capability in Enum.GetValues<BooleanCapability>())
        {
            Assert.Equal(
                expectedBooleans.Contains(capability),
                TerminalProfiles.Vt220.GetBoolean(capability));
        }

        foreach (NumericCapability capability in Enum.GetValues<NumericCapability>())
        {
            int? actual = TerminalProfiles.Vt220.GetNumber(capability);

            if (expectedNumbers.TryGetValue(capability, out int expected))
            {
                Assert.Equal<int?>(expected, actual);
            }
            else
            {
                Assert.Null(actual);
            }
        }

        Assert.Empty(TerminalProfiles.Vt220.ExtendedCapabilities);
    }

    [Fact]
    public void EveryAdvertisedTypedStringHasGoldenCoverage()
    {
        Dictionary<StringCapability, string> expected = CreateExpectedStrings();

        foreach (StringCapability capability in Enum.GetValues<StringCapability>())
        {
            string? actual = TerminalProfiles.Vt220.GetString(capability);

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
    public void DecEditingKeypadUsesFindSelectRatherThanPcHomeEnd()
    {
        TerminalDescription terminal = TerminalProfiles.Vt220;

        Assert.Equal(
            "\u001b[1~",
            terminal.GetRequiredString(StringCapability.KeyFind));
        Assert.Equal(
            "\u001b[2~",
            terminal.GetRequiredString(StringCapability.KeyInsertCharacter));
        Assert.Equal(
            "\u001b[3~",
            terminal.GetRequiredString(StringCapability.KeyDeleteCharacter));
        Assert.Equal(
            "\u001b[4~",
            terminal.GetRequiredString(StringCapability.KeySelect));
        Assert.Equal(
            "\u001b[5~",
            terminal.GetRequiredString(StringCapability.KeyPreviousPage));
        Assert.Equal(
            "\u001b[6~",
            terminal.GetRequiredString(StringCapability.KeyNextPage));

        Assert.Null(terminal.GetString(StringCapability.KeyHome));
        Assert.Null(terminal.GetString(StringCapability.KeyEnd));
    }

    [Fact]
    public void FunctionKeyLayoutRetainsMissingF5AndCanonicalVt220Values()
    {
        TerminalDescription terminal = TerminalProfiles.Vt220;

        Assert.Null(terminal.GetString(StringCapability.KeyF5));
        Assert.Equal("\u001b[17~", terminal.GetRequiredString(StringCapability.KeyF6));
        Assert.Equal("\u001b[21~", terminal.GetRequiredString(StringCapability.KeyF10));
        Assert.Equal("\u001b[23~", terminal.GetRequiredString(StringCapability.KeyF11));
        Assert.Equal("\u001b[24~", terminal.GetRequiredString(StringCapability.KeyF12));
        Assert.Equal("\u001b[25~", terminal.GetRequiredString(StringCapability.KeyF13));
        Assert.Equal("\u001b[26~", terminal.GetRequiredString(StringCapability.KeyF14));
        Assert.Null(terminal.GetString(StringCapability.KeyF15));
        Assert.Null(terminal.GetString(StringCapability.KeyF16));
        Assert.Equal("\u001b[31~", terminal.GetRequiredString(StringCapability.KeyF17));
        Assert.Equal("\u001b[34~", terminal.GetRequiredString(StringCapability.KeyF20));
        Assert.Equal("\u001b[28~", terminal.GetRequiredString(StringCapability.KeyHelp));
        Assert.Equal("\u001b[29~", terminal.GetRequiredString(StringCapability.KeyRedo));
    }

    [Fact]
    public void CursorVisibilityAndEditingProgramsMatchVt220Baseline()
    {
        TerminalDescription terminal = TerminalProfiles.Vt220;

        Assert.Equal(
            "\u001b[?25l",
            terminal.GetRequiredString(StringCapability.CursorInvisible));
        Assert.Equal(
            "\u001b[?25h",
            terminal.GetRequiredString(StringCapability.CursorNormal));
        Assert.Null(terminal.GetString(StringCapability.CursorVeryVisible));

        Assert.Equal(
            "\u001b[12P",
            terminal.Expand(StringCapability.DeleteCharacters, 12));
        Assert.Equal(
            "\u001b[9@",
            terminal.Expand(StringCapability.InsertCharacters, 9));
        Assert.Equal(
            "\u001b[6X",
            terminal.Expand(StringCapability.EraseCharacters, 6));
        Assert.Equal(
            "\u001b[1;24r",
            terminal.Expand(StringCapability.ChangeScrollRegion, 0, 23));
    }

    [Fact]
    public void Vt220RemainsMonochrome()
    {
        TerminalColorSupport support =
            TerminalColors.GetColorSupport(TerminalProfiles.Vt220);

        Assert.Equal(TerminalColorModel.None, support.Model);
        Assert.Equal(TerminalColorTier.Monochrome, support.Tier);
        Assert.Null(support.ColorCount);
        Assert.Null(support.ColorPairCount);
    }

    [Theory]
    [InlineData("vt220-w")]
    [InlineData("vt200-w")]
    [InlineData("vt220-8")]
    [InlineData("vt220-8bit")]
    [InlineData("vt220d")]
    [InlineData("vt320")]
    public void UnimplementedDecVariantsDoNotResolve(string name)
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
            [StringCapability.ChangeScrollRegion] = "\u001b[%i%p1%d;%p2%dr",
            [StringCapability.ClearScreen] = "\u001b[H\u001b[J",
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
            [StringCapability.Tab] = "\t",
            [StringCapability.SetTab] = "\u001bH",
            [StringCapability.InsertCharacters] = "\u001b[%p1%d@",
            [StringCapability.InsertCharacter] = "\u001b[@",
            [StringCapability.InsertLines] = "\u001b[%p1%dL",
            [StringCapability.InsertLine] = "\u001b[L",
            [StringCapability.ScrollForward] = "\u001bD",
            [StringCapability.RestoreCursor] = "\u001b8",
            [StringCapability.EnterBlinkMode] = "\u001b[5m",
            [StringCapability.EnterBoldMode] = "\u001b[1m",
            [StringCapability.EnterReverseMode] = "\u001b[7m",
            [StringCapability.ScrollReverse] = "\u001bM",
            [StringCapability.ExitAlternateCharacterSetMode] = "\u001b(B$<4>",
            [StringCapability.ExitAutomaticMargins] = "\u001b[?7l",
            [StringCapability.ExitStandoutMode] = "\u001b[27m",
            [StringCapability.ExitUnderlineMode] = "\u001b[24m",
            [StringCapability.SaveCursor] = "\u001b7",
            [StringCapability.SetAttributes] =
                "\u001b[0%?%p6%t;1%;%?%p2%t;4%;%?%p4%t;5%;"
                + "%?%p1%p3%|%t;7%;m"
                + "%?%p9%t\u001b(0%e\u001b(B%;$<2>",
            [StringCapability.ExitAttributeMode] = "\u001b[m\u001b(B",
            [StringCapability.EnterAlternateCharacterSetMode] = "\u001b(0$<2>",
            [StringCapability.EnterAutomaticMargins] = "\u001b[?7h",
            [StringCapability.EnterStandoutMode] = "\u001b[7m",
            [StringCapability.EnterUnderlineMode] = "\u001b[4m",
            [StringCapability.AlternateCharacterSet] =
                "``aaffggjjkkllmmnnooppqqrrssttuuvvwwxxyyzz{{||}}~~",
            [StringCapability.EnableAlternateCharacterSet] = "\u001b)0",
            [StringCapability.KeyBackspace] = "\b",
            [StringCapability.KeyCursorDown] = "\u001b[B",
            [StringCapability.KeyCursorLeft] = "\u001b[D",
            [StringCapability.KeyCursorRight] = "\u001b[C",
            [StringCapability.KeyCursorUp] = "\u001b[A",
            [StringCapability.KeyF1] = "\u001bOP",
            [StringCapability.KeyF2] = "\u001bOQ",
            [StringCapability.KeyF3] = "\u001bOR",
            [StringCapability.KeyF4] = "\u001bOS",
            [StringCapability.EraseCharacters] = "\u001b[%p1%dX",
            [StringCapability.ClearAllTabs] = "\u001b[3g",
            [StringCapability.CursorInvisible] = "\u001b[?25l",
            [StringCapability.CursorNormal] = "\u001b[?25h",
            [StringCapability.FlashScreen] = "\u001b[?5h$<200/>\u001b[?5l",
            [StringCapability.NewLine] = "\u001bE",
            [StringCapability.EnterInsertMode] = "\u001b[4h",
            [StringCapability.ExitInsertMode] = "\u001b[4l",
            [StringCapability.InitString2] =
                "\u001b[?7h\u001b>\u001b[?1l\u001b F\u001b[?4l",
            [StringCapability.ResetString1] = "\u001b[?3l",
            [StringCapability.PrintScreen] = "\u001b[i",
            [StringCapability.PrinterOff] = "\u001b[4i",
            [StringCapability.PrinterOn] = "\u001b[5i",
            [StringCapability.KeyDeleteCharacter] = "\u001b[3~",
            [StringCapability.KeyInsertCharacter] = "\u001b[2~",
            [StringCapability.KeyNextPage] = "\u001b[6~",
            [StringCapability.KeyPreviousPage] = "\u001b[5~",
            [StringCapability.KeyF6] = "\u001b[17~",
            [StringCapability.KeyF7] = "\u001b[18~",
            [StringCapability.KeyF8] = "\u001b[19~",
            [StringCapability.KeyF9] = "\u001b[20~",
            [StringCapability.KeyF10] = "\u001b[21~",
            [StringCapability.KeyF11] = "\u001b[23~",
            [StringCapability.KeyF12] = "\u001b[24~",
            [StringCapability.KeyF13] = "\u001b[25~",
            [StringCapability.KeyF14] = "\u001b[26~",
            [StringCapability.KeyF17] = "\u001b[31~",
            [StringCapability.KeyF18] = "\u001b[32~",
            [StringCapability.KeyF19] = "\u001b[33~",
            [StringCapability.KeyF20] = "\u001b[34~",
            [StringCapability.KeyFind] = "\u001b[1~",
            [StringCapability.KeyHelp] = "\u001b[28~",
            [StringCapability.KeyRedo] = "\u001b[29~",
            [StringCapability.KeySelect] = "\u001b[4~",
        };
    }
}
