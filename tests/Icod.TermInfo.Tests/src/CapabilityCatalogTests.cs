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
            ("mir", BooleanCapability.MoveInsertMode),
            ("bce", BooleanCapability.BackColorErase),
            ("ccc", BooleanCapability.CanChangeColor),
            ("hls", BooleanCapability.HueLightnessSaturation),
            ("km", BooleanCapability.HasMetaKey),
            ("npc", BooleanCapability.NoPadCharacter),
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
            ("ncv", NumericCapability.NoColorVideo),
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
            ("ech", StringCapability.EraseCharacters),
            ("tbc", StringCapability.ClearAllTabs),
            ("smcup", StringCapability.EnterCursorAddressingMode),
            ("rmcup", StringCapability.ExitCursorAddressingMode),
            ("civis", StringCapability.CursorInvisible),
            ("cnorm", StringCapability.CursorNormal),
            ("cvvis", StringCapability.CursorVeryVisible),
            ("flash", StringCapability.FlashScreen),
            ("nel", StringCapability.NewLine),
            ("indn", StringCapability.ScrollForwardLines),
            ("rin", StringCapability.ScrollReverseLines),
            ("smir", StringCapability.EnterInsertMode),
            ("rmir", StringCapability.ExitInsertMode),
            ("smm", StringCapability.EnterMetaMode),
            ("rmm", StringCapability.ExitMetaMode),
            ("sitm", StringCapability.EnterItalicMode),
            ("ritm", StringCapability.ExitItalicMode),
            ("initc", StringCapability.InitializeColor),
            ("oc", StringCapability.OriginalColors),
            ("setf", StringCapability.SetLegacyForegroundColor),
            ("setb", StringCapability.SetLegacyBackgroundColor),
            ("is1", StringCapability.InitString1),
            ("is2", StringCapability.InitString2),
            ("is3", StringCapability.InitString3),
            ("rs1", StringCapability.ResetString1),
            ("rs3", StringCapability.ResetString3),
            ("kmous", StringCapability.KeyMouse),
            ("meml", StringCapability.MemoryLock),
            ("memu", StringCapability.MemoryUnlock),
            ("rep", StringCapability.RepeatCharacter),
            ("mc0", StringCapability.PrintScreen),
            ("mc4", StringCapability.PrinterOff),
            ("mc5", StringCapability.PrinterOn),
            ("kcbt", StringCapability.KeyBackTab),
            ("kbeg", StringCapability.KeyBegin),
            ("kdch1", StringCapability.KeyDeleteCharacter),
            ("kend", StringCapability.KeyEnd),
            ("kent", StringCapability.KeyEnter),
            ("kich1", StringCapability.KeyInsertCharacter),
            ("knp", StringCapability.KeyNextPage),
            ("kpp", StringCapability.KeyPreviousPage),
            ("kf5", StringCapability.KeyF5),
            ("kf6", StringCapability.KeyF6),
            ("kf7", StringCapability.KeyF7),
            ("kf8", StringCapability.KeyF8),
            ("kf9", StringCapability.KeyF9),
            ("kf10", StringCapability.KeyF10),
            ("kf11", StringCapability.KeyF11),
            ("kf12", StringCapability.KeyF12),
            ("kf13", StringCapability.KeyF13),
            ("kf14", StringCapability.KeyF14),
            ("kf15", StringCapability.KeyF15),
            ("kf16", StringCapability.KeyF16),
            ("kf17", StringCapability.KeyF17),
            ("kf18", StringCapability.KeyF18),
            ("kf19", StringCapability.KeyF19),
            ("kf20", StringCapability.KeyF20),
            ("kf21", StringCapability.KeyF21),
            ("kf22", StringCapability.KeyF22),
            ("kf23", StringCapability.KeyF23),
            ("kf24", StringCapability.KeyF24),
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
