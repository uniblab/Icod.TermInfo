using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class XtermModernMetadataTests
{
    [Fact]
    public void SgrMouseFragmentOwnsStandardAndExtendedPrograms()
    {
        TerminalDescription common =
            new TerminalDescriptionBuilder("xterm-common")
                .ApplyXtermCommon()
                .Build();
        TerminalDescription mouse =
            new TerminalDescriptionBuilder("xterm-mouse")
                .ApplyXtermSgrMouseMetadata()
                .Build();

        Assert.Null(common.GetString(StringCapability.KeyMouse));
        Assert.Equal(
            "\u001b[<",
            mouse.GetRequiredString(StringCapability.KeyMouse));
        Assert.True(mouse.TryGetExtendedString("XM", out string? mode));
        Assert.True(mouse.TryGetExtendedString("xm", out string? eventProgram));

        Assert.Equal(
            "\u001b[?1006;1000h",
            TermInfoParameterExpander.Expand(mode!, 1));
        Assert.Equal(
            "\u001b[?1006;1000l",
            TermInfoParameterExpander.Expand(mode!, 0));
        Assert.Equal(
            "\u001b[<0;5;10;M",
            TermInfoParameterExpander.Expand(eventProgram!, 4, 9, 0, 1));
        Assert.Equal(
            "\u001b[<3;1;1;m",
            TermInfoParameterExpander.Expand(eventProgram!, 0, 0, 3, 0));
    }

    [Fact]
    public void FocusFragmentCarriesModeAndEventKeys()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("xterm-focus")
                .ApplyXtermFocusMetadata()
                .Build();

        Assert.True(terminal.TryGetExtendedBoolean("XF", out bool focus));
        Assert.True(focus);
        AssertExtendedString(terminal, "fd", "\u001b[?1004l");
        AssertExtendedString(terminal, "fe", "\u001b[?1004h");
        AssertExtendedString(terminal, "kxIN", "\u001b[I");
        AssertExtendedString(terminal, "kxOUT", "\u001b[O");
    }

    [Fact]
    public void BracketedPasteFragmentCarriesModeAndBoundaryStrings()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("xterm-paste")
                .ApplyXtermBracketedPasteMetadata()
                .Build();

        AssertExtendedString(terminal, "BD", "\u001b[?2004l");
        AssertExtendedString(terminal, "BE", "\u001b[?2004h");
        AssertExtendedString(terminal, "PS", "\u001b[200~");
        AssertExtendedString(terminal, "PE", "\u001b[201~");
    }

    [Fact]
    public void Tmux2FragmentCarriesCursorAndClipboardPrograms()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("xterm-tmux2")
                .ApplyXtermTmux2Metadata()
                .Build();

        AssertExtendedString(terminal, "Cr", "\u001b]112\u001b\\");
        AssertExtendedString(terminal, "Se", "\u001b[ q");
        Assert.True(terminal.TryGetExtendedString("Cs", out string? cursorColor));
        Assert.True(terminal.TryGetExtendedString("Ms", out string? clipboard));
        Assert.True(terminal.TryGetExtendedString("Ss", out string? cursorStyle));

        Assert.Equal(
            "\u001b]12;red\u001b\\",
            TermInfoParameterExpander.Expand(cursorColor!, "red"));
        Assert.Equal(
            "\u001b]52;c;YWJj\u001b\\",
            TermInfoParameterExpander.Expand(clipboard!, "c", "YWJj"));
        Assert.Equal(
            "\u001b[5 q",
            TermInfoParameterExpander.Expand(cursorStyle!, 5));
    }

    [Fact]
    public void ReportFragmentMatchesSelectedXtermP370Overrides()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("xterm-report")
                .ApplyXtermReportMetadata()
                .Build();

        AssertExtendedString(terminal, "RV", "\u001b[>c");
        AssertExtendedString(
            terminal,
            "rv",
            "\u001b\\[>41;[1-6][0-9][0-9];0c");
        AssertExtendedString(terminal, "XR", "\u001b[>0q");
        AssertExtendedString(
            terminal,
            "xr",
            "\u001bP>\\|XTerm\\(([1-9][0-9]+)\\)\u001b\\\\");
    }

    [Fact]
    public void ModifiedKeyMetadataIncludesSelectedPcAndThreeKeyFamilies()
    {
        TerminalDescription terminal = TerminalProfiles.Xterm;

        (string Name, string Value)[] expected =
        [
            ("kDN", "\u001b[1;2B"),
            ("kUP", "\u001b[1;2A"),
            ("kDC5", "\u001b[3;5~"),
            ("kEND6", "\u001b[1;6F"),
            ("kHOM4", "\u001b[1;4H"),
            ("kIC7", "\u001b[2;7~"),
            ("kLFT3", "\u001b[1;3D"),
            ("kNXT5", "\u001b[6;5~"),
            ("kPRV6", "\u001b[5;6~"),
            ("kRIT7", "\u001b[1;7C"),
            ("kDN3", "\u001b[1;3B"),
            ("kUP5", "\u001b[1;5A"),
            ("kPause", "\u001b[26;2~"),
            ("kPrint", "\u001b[25~"),
            ("kPrint7", "\u001b[25;7~"),
            ("kScroll", "\u001b[28;2~"),
        ];

        foreach ((string name, string value) in expected)
        {
            AssertExtendedString(terminal, name, value);
        }
    }

    [Fact]
    public void EveryBuiltInXtermFamilySharesModernMetadata()
    {
        TerminalDescription baseline = TerminalProfiles.Xterm;
        TerminalDescription[] variants =
        [
            TerminalProfiles.Xterm16Color,
            TerminalProfiles.Xterm88Color,
            TerminalProfiles.Xterm256Color,
            TerminalProfiles.XtermDirect,
            TerminalProfiles.XtermDirect16,
            TerminalProfiles.XtermDirect256,
        ];
        string[] names =
        [
            "AX", "XF", "XT", "E3",
            "BD", "BE", "PE", "PS",
            "Cr", "Cs", "Ms", "Se", "Ss",
            "XM", "xm", "fd", "fe", "kxIN", "kxOUT",
            "RV", "rv", "XR", "xr", "smxx", "rmxx",
            "kLFT7", "kPrint7", "kScroll",
        ];

        foreach (TerminalDescription variant in variants)
        {
            Assert.True(baseline.GetBoolean(BooleanCapability.BackspacesWithBs));
            Assert.Equal(
                baseline.GetBoolean(BooleanCapability.BackspacesWithBs),
                variant.GetBoolean(BooleanCapability.BackspacesWithBs));
            Assert.Equal(
                baseline.GetString(StringCapability.KeyMouse),
                variant.GetString(StringCapability.KeyMouse));

            foreach (string name in names)
            {
                Assert.True(
                    baseline.TryGetExtendedCapability(
                        name,
                        out TermInfoCapabilityValue expected));
                Assert.True(
                    variant.TryGetExtendedCapability(
                        name,
                        out TermInfoCapabilityValue actual));
                Assert.Equal(expected, actual);
            }
        }
    }

    [Fact]
    public void ModernMetadataFragmentsRejectNullBuilder()
    {
        Assert.Throws<ArgumentNullException>(
            () => XtermModernCapabilityFragments.ApplyXtermModernMetadata(null!));
        Assert.Throws<ArgumentNullException>(
            () => XtermModernCapabilityFragments.ApplyXtermBracketedPasteMetadata(null!));
        Assert.Throws<ArgumentNullException>(
            () => XtermModernCapabilityFragments.ApplyXtermTmux2Metadata(null!));
        Assert.Throws<ArgumentNullException>(
            () => XtermModernCapabilityFragments.ApplyXtermSgrMouseMetadata(null!));
        Assert.Throws<ArgumentNullException>(
            () => XtermModernCapabilityFragments.ApplyXtermFocusMetadata(null!));
        Assert.Throws<ArgumentNullException>(
            () => XtermModernCapabilityFragments.ApplyXtermReportMetadata(null!));
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
