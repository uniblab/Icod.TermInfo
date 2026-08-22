using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class CapabilityCatalogTests
{
    [Fact]
    public void BooleanCatalogCoversEveryTypedCapability()
    {
        (string Name, BooleanCapability Capability)[] cases =
        [
            ("am", BooleanCapability.AutoRightMargin),
            ("gn", BooleanCapability.GenericType),
            ("msgr", BooleanCapability.MoveStandoutMode),
            ("xenl", BooleanCapability.EatNewlineGlitch),
            ("xon", BooleanCapability.XonXoff),
        ];

        Assert.Equal(Enum.GetValues<BooleanCapability>().Length, cases.Length);
        Assert.Equal(cases.Length, cases.Select(item => item.Name).Distinct().Count());
        Assert.Equal(cases.Length, cases.Select(item => item.Capability).Distinct().Count());

        foreach ((string name, BooleanCapability capability) in cases)
        {
            TerminalDescription terminal =
                new TerminalDescriptionBuilder("mapping-test")
                    .SetBoolean(capability)
                    .Build();

            Assert.True(terminal.TryGetBoolean(name, out bool value));
            Assert.True(value);
        }
    }

    [Fact]
    public void NumericCatalogCoversEveryTypedCapability()
    {
        (string Name, NumericCapability Capability)[] cases =
        [
            ("cols", NumericCapability.Columns),
            ("lines", NumericCapability.Lines),
            ("colors", NumericCapability.Colors),
            ("pairs", NumericCapability.ColorPairs),
            ("it", NumericCapability.InitialTabWidth),
            ("vt", NumericCapability.VirtualTerminal),
        ];

        Assert.Equal(Enum.GetValues<NumericCapability>().Length, cases.Length);
        Assert.Equal(cases.Length, cases.Select(item => item.Name).Distinct().Count());
        Assert.Equal(cases.Length, cases.Select(item => item.Capability).Distinct().Count());

        for (int i = 0; i < cases.Length; i++)
        {
            (string name, NumericCapability capability) = cases[i];
            int expected = 1000 + i;
            TerminalDescription terminal =
                new TerminalDescriptionBuilder("mapping-test")
                    .SetNumber(capability, expected)
                    .Build();

            Assert.True(terminal.TryGetNumber(name, out int value));
            Assert.Equal(expected, value);
        }
    }

    [Fact]
    public void StringCatalogCoversEveryTypedCapability()
    {
        (string Name, StringCapability Capability)[] cases =
        [
            ("bel", StringCapability.Bell),
            ("cbt", StringCapability.BackTab),
            ("blink", StringCapability.EnterBlinkMode),
            ("bold", StringCapability.EnterBoldMode),
            ("dim", StringCapability.EnterDimMode),
            ("cr", StringCapability.CarriageReturn),
            ("csr", StringCapability.ChangeScrollRegion),
            ("clear", StringCapability.ClearScreen),
            ("cub", StringCapability.CursorLeft),
            ("cub1", StringCapability.CursorLeftOne),
            ("cud", StringCapability.CursorDown),
            ("cud1", StringCapability.CursorDownOne),
            ("cuf", StringCapability.CursorRight),
            ("cuf1", StringCapability.CursorRightOne),
            ("cup", StringCapability.CursorAddress),
            ("cuu", StringCapability.CursorUp),
            ("cuu1", StringCapability.CursorUpOne),
            ("dch", StringCapability.DeleteCharacters),
            ("dch1", StringCapability.DeleteCharacter),
            ("dl", StringCapability.DeleteLines),
            ("dl1", StringCapability.DeleteLine),
            ("ed", StringCapability.ClearToEndOfScreen),
            ("el", StringCapability.ClearToEndOfLine),
            ("el1", StringCapability.ClearToBeginningOfLine),
            ("home", StringCapability.CursorHome),
            ("hpa", StringCapability.ColumnAddress),
            ("ht", StringCapability.Tab),
            ("hts", StringCapability.SetTab),
            ("ich", StringCapability.InsertCharacters),
            ("ich1", StringCapability.InsertCharacter),
            ("il", StringCapability.InsertLines),
            ("il1", StringCapability.InsertLine),
            ("ind", StringCapability.ScrollForward),
            ("invis", StringCapability.EnterInvisibleMode),
            ("op", StringCapability.OriginalColorPair),
            ("rc", StringCapability.RestoreCursor),
            ("rev", StringCapability.EnterReverseMode),
            ("ri", StringCapability.ScrollReverse),
            ("rmacs", StringCapability.ExitAlternateCharacterSetMode),
            ("rmam", StringCapability.ExitAutomaticMargins),
            ("rmkx", StringCapability.ExitKeypadMode),
            ("rmso", StringCapability.ExitStandoutMode),
            ("rmul", StringCapability.ExitUnderlineMode),
            ("sc", StringCapability.SaveCursor),
            ("setab", StringCapability.SetBackgroundColor),
            ("setaf", StringCapability.SetForegroundColor),
            ("sgr", StringCapability.SetAttributes),
            ("sgr0", StringCapability.ExitAttributeMode),
            ("smacs", StringCapability.EnterAlternateCharacterSetMode),
            ("smam", StringCapability.EnterAutomaticMargins),
            ("smkx", StringCapability.EnterKeypadMode),
            ("smso", StringCapability.EnterStandoutMode),
            ("smul", StringCapability.EnterUnderlineMode),
            ("vpa", StringCapability.RowAddress),
            ("acsc", StringCapability.AlternateCharacterSet),
            ("enacs", StringCapability.EnableAlternateCharacterSet),
            ("kbs", StringCapability.KeyBackspace),
            ("kcud1", StringCapability.KeyCursorDown),
            ("kcub1", StringCapability.KeyCursorLeft),
            ("kcuf1", StringCapability.KeyCursorRight),
            ("kcuu1", StringCapability.KeyCursorUp),
            ("khome", StringCapability.KeyHome),
            ("kf1", StringCapability.KeyF1),
            ("kf2", StringCapability.KeyF2),
            ("kf3", StringCapability.KeyF3),
            ("kf4", StringCapability.KeyF4),
            ("rs2", StringCapability.ResetString2),
        ];

        Assert.Equal(Enum.GetValues<StringCapability>().Length, cases.Length);
        Assert.Equal(cases.Length, cases.Select(item => item.Name).Distinct().Count());
        Assert.Equal(cases.Length, cases.Select(item => item.Capability).Distinct().Count());

        foreach ((string name, StringCapability capability) in cases)
        {
            TerminalDescription terminal =
                new TerminalDescriptionBuilder("mapping-test")
                    .SetString(capability, name)
                    .Build();

            Assert.True(terminal.TryGetString(name, out string? value));
            Assert.Equal(name, value);
        }
    }
}
