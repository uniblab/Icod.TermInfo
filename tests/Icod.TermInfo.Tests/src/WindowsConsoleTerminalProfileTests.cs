using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class WindowsConsoleTerminalProfileTests
{
    [Fact]
    public void IdentityAndExtensionsMatchGoldenBaseline()
    {
        TerminalDescription terminal = TerminalProfiles.WinConsole;

        Assert.Equal("winconsole", terminal.Name);
        Assert.Equal("Windows 10 new console", terminal.Description);
        Assert.Empty(terminal.Aliases);
        Assert.Same(terminal, TerminalDatabase.BuiltIn.Load("winconsole"));
        Assert.True(terminal.TryGetExtendedBoolean("AX", out bool ax));
        Assert.True(ax);
        Assert.True(terminal.TryGetExtendedNumber("U8", out int u8));
        Assert.Equal(1, u8);
        Assert.Equal(2, terminal.ExtendedCapabilities.Count);

        foreach (string name in new[] { "conhost", "windows-console", "winconsole-direct" })
        {
            Assert.False(TerminalDatabase.BuiltIn.TryLoad(name, out _));
        }
    }

    [Fact]
    public void StandardBooleanAndNumericCapabilitiesMatchGoldenBaseline()
    {
        TerminalDescription terminal = TerminalProfiles.WinConsole;
        BooleanCapability[] expectedBooleans =
        [
            BooleanCapability.AutoRightMargin,
            BooleanCapability.HasMetaKey,
            BooleanCapability.MoveInsertMode,
            BooleanCapability.MoveStandoutMode,
            BooleanCapability.EatNewlineGlitch,
        ];

        Assert.Equal(
            expectedBooleans.OrderBy(
                capability => StandardCapabilityCatalog.GetMetadata(capability).BinaryIndex),
            terminal.BooleanCapabilities);
        Assert.Equal<int?>(8, terminal.GetNumber(NumericCapability.InitialTabWidth));
        Assert.Equal<int?>(8, terminal.GetNumber(NumericCapability.Colors));
        Assert.Equal<int?>(64, terminal.GetNumber(NumericCapability.ColorPairs));
        Assert.Null(terminal.GetNumber(NumericCapability.NoColorVideo));
        Assert.Equal(3, terminal.NumericCapabilities.Count);
    }

    [Fact]
    public void SourceCancellationsRemoveInheritedValues()
    {
        TerminalDescription terminal = TerminalProfiles.WinConsole;
        StringCapability[] canceled =
        [
            StringCapability.EnterBlinkMode,
            StringCapability.EnterInvisibleMode,
            StringCapability.InsertCharacter,
            StringCapability.ExitInsertMode,
            StringCapability.ExitPcCharsetMode,
            StringCapability.EnterInsertMode,
            StringCapability.EnterPcCharsetMode,
            StringCapability.KeyF5,
        ];

        foreach (StringCapability capability in canceled)
        {
            Assert.Null(terminal.GetString(capability));
        }
    }

    [Fact]
    public void StandardStringCapabilitySetMatchesGoldenBaseline()
    {
        TerminalDescription terminal = TerminalProfiles.WinConsole;
        HashSet<StringCapability> expected =
        [
            StringCapability.AlternateCharacterSet,
            StringCapability.BackTab,
            StringCapability.Bell,
            StringCapability.CarriageReturn,
            StringCapability.ChangeScrollRegion,
            StringCapability.ClearAllTabs,
            StringCapability.ClearScreen,
            StringCapability.ClearToBeginningOfLine,
            StringCapability.ClearToEndOfLine,
            StringCapability.ClearToEndOfScreen,
            StringCapability.CursorAddress,
            StringCapability.CursorDown,
            StringCapability.CursorDownOne,
            StringCapability.CursorHome,
            StringCapability.CursorInvisible,
            StringCapability.CursorLeft,
            StringCapability.CursorLeftOne,
            StringCapability.CursorNormal,
            StringCapability.CursorRight,
            StringCapability.CursorRightOne,
            StringCapability.CursorUp,
            StringCapability.CursorUpOne,
            StringCapability.DeleteCharacter,
            StringCapability.DeleteCharacters,
            StringCapability.DeleteLine,
            StringCapability.DeleteLines,
            StringCapability.EnterAlternateCharacterSetMode,
            StringCapability.EnterBoldMode,
            StringCapability.EnterReverseMode,
            StringCapability.EnterStandoutMode,
            StringCapability.EnterUnderlineMode,
            StringCapability.EraseCharacters,
            StringCapability.ExitAlternateCharacterSetMode,
            StringCapability.ExitAttributeMode,
            StringCapability.ExitStandoutMode,
            StringCapability.ExitUnderlineMode,
            StringCapability.InitString1,
            StringCapability.InsertCharacters,
            StringCapability.InsertLine,
            StringCapability.InsertLines,
            StringCapability.KeyBackspace,
            StringCapability.KeyCursorDown,
            StringCapability.KeyCursorLeft,
            StringCapability.KeyCursorRight,
            StringCapability.KeyCursorUp,
            StringCapability.KeyDeleteCharacter,
            StringCapability.KeyEnd,
            StringCapability.KeyHome,
            StringCapability.KeyInsertCharacter,
            StringCapability.KeyNextPage,
            StringCapability.KeyPreviousPage,
            StringCapability.NewLine,
            StringCapability.OriginalColorPair,
            StringCapability.ResetString1,
            StringCapability.RestoreCursor,
            StringCapability.SaveCursor,
            StringCapability.ScrollForward,
            StringCapability.ScrollForwardLines,
            StringCapability.ScrollReverse,
            StringCapability.ScrollReverseLines,
            StringCapability.SetAttributes,
            StringCapability.SetBackgroundColor,
            StringCapability.SetForegroundColor,
            StringCapability.SetTab,
            StringCapability.Tab,
        ];

        for (int number = 1; number <= 60; number++)
        {
            if (number != 5)
            {
                expected.Add(Enum.Parse<StringCapability>($"KeyF{number}"));
            }
        }

        Assert.Equal(
            expected.OrderBy(
                capability => StandardCapabilityCatalog.GetMetadata(capability).BinaryIndex),
            terminal.StringCapabilities.Select(pair => pair.Key));
    }

    [Fact]
    public void DirectOverridesAndFunctionKeysMatchGoldenBaseline()
    {
        TerminalDescription terminal = TerminalProfiles.WinConsole;

        Assert.Equal(
            "++,,--..00``aaffgghhiijjkkllmmnnooppqqrrssttuuvvwwxxyyzz~~",
            terminal.GetString(StringCapability.AlternateCharacterSet));
        Assert.Equal("\u001b[0K", terminal.GetString(StringCapability.ClearToBeginningOfLine));
        Assert.Equal("\r\n", terminal.GetString(StringCapability.NewLine));
        Assert.Equal("\u001b[T", terminal.GetString(StringCapability.ScrollReverse));
        Assert.Equal("\u001b[!p", terminal.GetString(StringCapability.InitString1));
        Assert.Equal("\u001b[!p", terminal.GetString(StringCapability.ResetString1));
        Assert.Equal("\u001b[0m\u001b(B", terminal.GetString(StringCapability.ExitAttributeMode));
        Assert.Equal("\u001b(0", terminal.GetString(StringCapability.EnterAlternateCharacterSetMode));
        Assert.Equal("\u001b(B", terminal.GetString(StringCapability.ExitAlternateCharacterSetMode));
        Assert.Equal(
            "\u001b[0%?%p1%p6%|%t;1%;%?%p2%t;4%;"
            + "%?%p1%p3%|%t;7%;m%?%p9%t\u001b(0%e\u001b(B%;",
            terminal.GetString(StringCapability.SetAttributes));

        int[] codes = [11, 12, 13, 14, 15, 17, 18, 19, 20, 21, 24, 25];
        int[] starts = [13, 25, 37, 49];
        int[] modifiers = [2, 3, 4, 7];
        for (int bank = 0; bank < starts.Length; bank++)
        {
            for (int i = 0; i < codes.Length; i++)
            {
                StringCapability capability =
                    Enum.Parse<StringCapability>($"KeyF{starts[bank] + i}");
                Assert.Equal(
                    $"\u001b[{codes[i]};{modifiers[bank]}~",
                    terminal.GetString(capability));
            }
        }

        Assert.Equal("\u001b[11~", terminal.GetString(StringCapability.KeyF1));
        Assert.Equal("\u001b[14~", terminal.GetString(StringCapability.KeyF4));
        Assert.Equal("\u001b[17~", terminal.GetString(StringCapability.KeyF6));
        Assert.Equal("\u001b[24~", terminal.GetString(StringCapability.KeyF12));
    }

    [Fact]
    public void ProfileConstructionIsPlatformIndependentDataOnly()
    {
        TerminalDescription first = TerminalProfiles.WinConsole;
        TerminalDescription second = WindowsConsoleTerminalProfile.Create();

        Assert.Equal(first.BooleanCapabilities, second.BooleanCapabilities);
        Assert.Equal(first.NumericCapabilities, second.NumericCapabilities);
        Assert.Equal(first.StringCapabilities, second.StringCapabilities);
        Assert.Equal(
            first.ExtendedCapabilities.OrderBy(pair => pair.Key),
            second.ExtendedCapabilities.OrderBy(pair => pair.Key));
    }
}
