using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class WindowsTerminalProfileTests
{
    [Fact]
    public void IdentitiesResolveExactlyWithoutAliases()
    {
        TerminalDescription indexed = TerminalProfiles.MsTerminal;
        TerminalDescription direct = TerminalProfiles.MsTerminalDirect;

        Assert.Equal("ms-terminal", indexed.Name);
        Assert.Equal("Windows terminal", indexed.Description);
        Assert.Empty(indexed.Aliases);
        Assert.Same(indexed, TerminalDatabase.BuiltIn.Load("ms-terminal"));

        Assert.Equal("ms-terminal-direct", direct.Name);
        Assert.Equal("Windows terminal with direct-colors", direct.Description);
        Assert.Empty(direct.Aliases);
        Assert.Same(
            direct,
            TerminalDatabase.BuiltIn.Load("ms-terminal-direct"));

        foreach (string name in new[]
        {
            "ms+terminal",
            "windows-terminal",
            "ms-terminal-256color",
            "ms-terminal-direct256",
            "ms_terminal",
        })
        {
            Assert.False(TerminalDatabase.BuiltIn.TryLoad(name, out _));
        }
    }

    [Fact]
    public void CommonSourceOverridesAndCancellationsMatchBaseline()
    {
        foreach (TerminalDescription terminal in new[]
        {
            TerminalProfiles.MsTerminal,
            TerminalProfiles.MsTerminalDirect,
        })
        {
            Assert.True(terminal.GetBoolean(BooleanCapability.BackspacesWithBs));
            Assert.True(terminal.GetBoolean(BooleanCapability.HasMetaKey));
            Assert.True(terminal.GetBoolean(BooleanCapability.NoPadCharacter));

            Assert.Equal(
                "\u001b[B",
                terminal.GetString(StringCapability.CursorDownOne));
            Assert.Equal(
                "\u007f",
                terminal.GetString(StringCapability.KeyBackspace));
            Assert.Equal(
                "\u001bOE",
                terminal.GetString(StringCapability.KeyBegin));
            Assert.Equal(
                "\u001b[Z",
                terminal.GetString(StringCapability.KeyBackTab));
            Assert.Equal(
                "\u001b[?1l",
                terminal.GetString(StringCapability.ExitKeypadMode));
            Assert.Equal(
                "\u001b[?1h",
                terminal.GetString(StringCapability.EnterKeypadMode));

            Assert.Null(terminal.GetString(StringCapability.ExitMetaMode));
            Assert.Null(terminal.GetString(StringCapability.EnterMetaMode));
            Assert.Null(terminal.GetString(StringCapability.OriginalColors));

            Assert.Equal(
                "\u001b[?69l",
                terminal.GetString(StringCapability.ClearMargins));
            Assert.Equal(
                "\u001b[?69h\u001b[%i%p1%d;%p2%ds",
                terminal.GetString(StringCapability.SetLrMargin));
        }
    }

    [Fact]
    public void PcFunctionKeyFragmentMatchesNcursesWithoutUnrelatedXtermKeys()
    {
        foreach (TerminalDescription terminal in new[]
        {
            TerminalProfiles.MsTerminal,
            TerminalProfiles.MsTerminalDirect,
        })
        {
            Dictionary<int, string> expected = CreateExpectedFunctionKeys();

            for (int number = 1; number <= 63; number++)
            {
                StringCapability capability =
                    Enum.Parse<StringCapability>($"KeyF{number}");

                Assert.Equal(
                    expected[number],
                    terminal.GetString(capability));
            }

            Assert.Null(terminal.GetString(StringCapability.KeyEnter));
            Assert.Null(terminal.GetString(StringCapability.KeyA1));
            Assert.Null(terminal.GetString(StringCapability.KeyA3));
            Assert.Null(terminal.GetString(StringCapability.KeyB2));
            Assert.Null(terminal.GetString(StringCapability.KeyC1));
            Assert.Null(terminal.GetString(StringCapability.KeyC3));

            foreach (string name in new[]
            {
                "ka2",
                "kb1",
                "kb3",
                "kc2",
                "kp5",
                "kpADD",
                "kpCMA",
                "kpDIV",
                "kpDOT",
                "kpMUL",
                "kpSUB",
                "kpZRO",
                "kPause",
                "kPrint",
                "kPrint2",
                "kPrint3",
                "kPrint4",
                "kPrint5",
                "kPrint6",
                "kPrint7",
                "kScroll",
            })
            {
                Assert.False(terminal.TryGetExtendedString(name, out _));
            }
        }
    }

    [Fact]
    public void ModernProtocolMetadataMatchesNcursesBaseline()
    {
        foreach (TerminalDescription terminal in new[]
        {
            TerminalProfiles.MsTerminal,
            TerminalProfiles.MsTerminalDirect,
        })
        {
            AssertExtendedString(terminal, "BD", "\u001b[?2004l");
            AssertExtendedString(terminal, "BE", "\u001b[?2004h");
            AssertExtendedString(terminal, "PE", "\u001b[201~");
            AssertExtendedString(terminal, "PS", "\u001b[200~");

            Assert.Equal(
                "\u001b[<",
                terminal.GetString(StringCapability.KeyMouse));
            AssertExtendedString(
                terminal,
                "XM",
                "\u001b[?1006;1004;1003%?%p1%{1}%=%th%el%;");
            AssertExtendedString(
                terminal,
                "xm",
                "\u001b[<%i%p3%d;%p1%d;%p2%d;%?%p4%tM%em%;");

            AssertExtendedString(terminal, "fd", "\u001b[?1004l");
            AssertExtendedString(terminal, "fe", "\u001b[?1004h");
            AssertExtendedString(terminal, "kxIN", "\u001b[I");
            AssertExtendedString(terminal, "kxOUT", "\u001b[O");

            AssertExtendedString(terminal, "RV", "\u001b[>c");
            AssertExtendedString(
                terminal,
                "rv",
                "\u001b\\[>0;10;1c");

            AssertExtendedString(terminal, "Cr", "\u001b]112\a");
            AssertExtendedString(terminal, "Cs", "\u001b]12;%p1%s\a");
            AssertExtendedString(
                terminal,
                "Ms",
                "\u001b]52;%p1%s;%p2%s\a");
            AssertExtendedString(terminal, "Se", "\u001b[2 q");
            AssertExtendedString(terminal, "Ss", "\u001b[%p1%d q");

            AssertExtendedString(terminal, "Rmol", "\u001b[55m");
            AssertExtendedString(terminal, "Smol", "\u001b[53m");
            AssertExtendedString(terminal, "rmxx", "\u001b[29m");
            AssertExtendedString(terminal, "smxx", "\u001b[9m");
        }
    }

    [Fact]
    public void IndexedColorProfileMatchesXterm256ColorSourceFragment()
    {
        TerminalDescription terminal = TerminalProfiles.MsTerminal;
        TerminalColorSupport support =
            TerminalColors.GetColorSupport(terminal);

        Assert.Equal(TerminalColorModel.Indexed, support.Model);
        Assert.Equal(TerminalColorTier.Color256, support.Tier);
        Assert.Equal<int?>(256, support.ColorCount);
        Assert.Equal(256, support.IndexedColorCount);
        Assert.Equal<int?>(65536, support.ColorPairCount);
        Assert.True(support.CanChangeColor);
        Assert.True(support.HasInitializeColor);
        Assert.True(support.HasOriginalColorPair);
        Assert.False(support.HasOriginalColors);

        Assert.Equal(
            "\u001b[38;5;196m",
            TerminalColors.ExpandForeground(terminal, 196));
        Assert.Equal(
            "\u001b[48;5;196m",
            TerminalColors.ExpandBackground(terminal, 196));
    }

    [Fact]
    public void DirectProfileUsesGenericPackedRgbSemantics()
    {
        TerminalDescription terminal = TerminalProfiles.MsTerminalDirect;
        TerminalColorSupport support =
            TerminalColors.GetColorSupport(terminal);

        Assert.Equal(TerminalColorModel.DirectRgb, support.Model);
        Assert.Equal(TerminalColorTier.TrueColor, support.Tier);
        Assert.Equal<int?>(0x1000000, support.ColorCount);
        Assert.Equal(8, support.IndexedColorCount);
        Assert.Equal<int?>(0x10000, support.ColorPairCount);
        Assert.False(support.CanChangeColor);
        Assert.False(support.HasInitializeColor);
        Assert.True(support.HasOriginalColorPair);
        Assert.False(support.HasOriginalColors);

        Assert.True(terminal.TryGetExtendedBoolean("RGB", out bool rgb));
        Assert.True(rgb);
        Assert.True(terminal.TryGetExtendedNumber("CO", out int indexed));
        Assert.Equal(8, indexed);

        Assert.Equal(
            "\u001b[38:2::128:64:192m",
            TerminalColors.ExpandForeground(
                terminal,
                new TerminalRgbColor(0x80, 0x40, 0xC0)));
        Assert.Equal(
            "\u001b[48:2::128:64:192m",
            TerminalColors.ExpandBackground(
                terminal,
                new TerminalRgbColor(0x80, 0x40, 0xC0)));
    }

    [Fact]
    public void WindowsTerminalProfilesRemainDistinctFromXterm()
    {
        Assert.NotSame(TerminalProfiles.Xterm256Color, TerminalProfiles.MsTerminal);
        Assert.NotSame(TerminalProfiles.XtermDirect, TerminalProfiles.MsTerminalDirect);

        Assert.Equal(
            "\u001b[?1l",
            TerminalProfiles.MsTerminal.GetString(
                StringCapability.ExitKeypadMode));
        Assert.Equal(
            "\u001b[?1l\u001b>",
            TerminalProfiles.Xterm.GetString(
                StringCapability.ExitKeypadMode));

        Assert.Null(
            TerminalProfiles.MsTerminal.GetString(
                StringCapability.OriginalColors));
    }

    [Fact]
    public void ProfileConstructionIsPlatformIndependentDataOnly()
    {
        TerminalDescription indexed = WindowsTerminalProfile.Create();
        TerminalDescription direct = WindowsTerminalProfile.CreateDirect();

        Assert.Equal(
            TerminalProfiles.MsTerminal.BooleanCapabilities,
            indexed.BooleanCapabilities);
        Assert.Equal(
            TerminalProfiles.MsTerminal.NumericCapabilities,
            indexed.NumericCapabilities);
        Assert.Equal(
            TerminalProfiles.MsTerminal.StringCapabilities,
            indexed.StringCapabilities);
        Assert.Equal(
            TerminalProfiles.MsTerminal.ExtendedCapabilities.OrderBy(pair => pair.Key),
            indexed.ExtendedCapabilities.OrderBy(pair => pair.Key));

        Assert.Equal(
            TerminalProfiles.MsTerminalDirect.BooleanCapabilities,
            direct.BooleanCapabilities);
        Assert.Equal(
            TerminalProfiles.MsTerminalDirect.NumericCapabilities,
            direct.NumericCapabilities);
        Assert.Equal(
            TerminalProfiles.MsTerminalDirect.StringCapabilities,
            direct.StringCapabilities);
        Assert.Equal(
            TerminalProfiles.MsTerminalDirect.ExtendedCapabilities.OrderBy(pair => pair.Key),
            direct.ExtendedCapabilities.OrderBy(pair => pair.Key));
    }

    private static Dictionary<int, string> CreateExpectedFunctionKeys()
    {
        Dictionary<int, string> expected = new();
        char[] finals = ['P', 'Q', 'R', 'S'];
        int[] codes = [15, 17, 18, 19, 20, 21, 23, 24];

        for (int i = 0; i < finals.Length; i++)
        {
            expected[i + 1] = $"\u001bO{finals[i]}";
        }

        for (int i = 0; i < codes.Length; i++)
        {
            expected[i + 5] = $"\u001b[{codes[i]}~";
        }

        AddModifiedFunctionKeyBank(expected, 13, 2, finals, codes);
        AddModifiedFunctionKeyBank(expected, 25, 5, finals, codes);
        AddModifiedFunctionKeyBank(expected, 37, 6, finals, codes);
        AddModifiedFunctionKeyBank(expected, 49, 3, finals, codes);

        for (int i = 0; i < 3; i++)
        {
            expected[i + 61] = $"\u001b[1;4{finals[i]}";
        }

        return expected;
    }

    private static void AddModifiedFunctionKeyBank(
        Dictionary<int, string> expected,
        int firstFunctionKey,
        int modifier,
        char[] finals,
        int[] codes)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(finals);
        ArgumentNullException.ThrowIfNull(codes);

        for (int i = 0; i < finals.Length; i++)
        {
            expected[firstFunctionKey + i] =
                $"\u001b[1;{modifier}{finals[i]}";
        }

        for (int i = 0; i < codes.Length; i++)
        {
            expected[firstFunctionKey + 4 + i] =
                $"\u001b[{codes[i]};{modifier}~";
        }
    }

    private static void AssertExtendedString(
        TerminalDescription terminal,
        string name,
        string expected)
    {
        Assert.True(terminal.TryGetExtendedString(name, out string? actual));
        Assert.Equal(expected, actual);
    }
}
