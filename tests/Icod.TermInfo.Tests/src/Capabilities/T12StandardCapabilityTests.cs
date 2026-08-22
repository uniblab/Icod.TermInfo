using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T12StandardCapabilityTests
{
    [Fact]
    public void FullScreenAndCursorVisibilityPrimitivesRoundTripByTypedAndShortName()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("sample")
                .SetString(
                    StringCapability.EnterCursorAddressingMode,
                    "enter-screen")
                .SetString(
                    StringCapability.ExitCursorAddressingMode,
                    "exit-screen")
                .SetString(StringCapability.CursorInvisible, "hide-cursor")
                .SetString(StringCapability.CursorNormal, "normal-cursor")
                .SetString(StringCapability.CursorVeryVisible, "show-cursor")
                .Build();

        Assert.Equal(
            "enter-screen",
            terminal.GetString(StringCapability.EnterCursorAddressingMode));
        Assert.Equal(
            "exit-screen",
            terminal.GetString(StringCapability.ExitCursorAddressingMode));
        Assert.Equal(
            "hide-cursor",
            terminal.GetString(StringCapability.CursorInvisible));
        Assert.Equal(
            "normal-cursor",
            terminal.GetString(StringCapability.CursorNormal));
        Assert.Equal(
            "show-cursor",
            terminal.GetString(StringCapability.CursorVeryVisible));

        Assert.True(terminal.TryGetString("smcup", out string? smcup));
        Assert.Equal("enter-screen", smcup);
        Assert.True(terminal.TryGetString("rmcup", out string? rmcup));
        Assert.Equal("exit-screen", rmcup);
        Assert.True(terminal.TryGetString("civis", out string? civis));
        Assert.Equal("hide-cursor", civis);
        Assert.True(terminal.TryGetString("cnorm", out string? cnorm));
        Assert.Equal("normal-cursor", cnorm);
        Assert.True(terminal.TryGetString("cvvis", out string? cvvis));
        Assert.Equal("show-cursor", cvvis);
    }

    [Fact]
    public void ColorPrimitivesRemainRawTerminalDescriptionData()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("sample")
                .SetBoolean(BooleanCapability.BackColorErase)
                .SetBoolean(BooleanCapability.CanChangeColor)
                .SetBoolean(BooleanCapability.HueLightnessSaturation)
                .SetString(
                    StringCapability.InitializeColor,
                    "init:%p1%d:%p2%d:%p3%d:%p4%d")
                .SetString(StringCapability.OriginalColors, "original-colors")
                .SetString(
                    StringCapability.SetLegacyForegroundColor,
                    "foreground:%p1%d")
                .SetString(
                    StringCapability.SetLegacyBackgroundColor,
                    "background:%p1%d")
                .Build();

        Assert.True(terminal.GetBoolean(BooleanCapability.BackColorErase));
        Assert.True(terminal.GetBoolean(BooleanCapability.CanChangeColor));
        Assert.True(
            terminal.GetBoolean(BooleanCapability.HueLightnessSaturation));

        Assert.True(terminal.TryGetBoolean("bce", out bool bce));
        Assert.True(bce);
        Assert.True(terminal.TryGetBoolean("ccc", out bool ccc));
        Assert.True(ccc);
        Assert.True(terminal.TryGetBoolean("hls", out bool hls));
        Assert.True(hls);

        Assert.Equal(
            "init:1:2:3:4",
            terminal.Expand(StringCapability.InitializeColor, 1, 2, 3, 4));
        Assert.Equal(
            "foreground:7",
            terminal.Expand(StringCapability.SetLegacyForegroundColor, 7));
        Assert.Equal(
            "background:6",
            terminal.Expand(StringCapability.SetLegacyBackgroundColor, 6));
        Assert.Equal(
            "original-colors",
            terminal.GetString(StringCapability.OriginalColors));
    }

    [Fact]
    public void ParameterizedScrollingUsesSharedExpansionEngine()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("sample")
                .SetString(StringCapability.ScrollForwardLines, "F%p1%d")
                .SetString(StringCapability.ScrollReverseLines, "R%p1%d")
                .Build();

        Assert.Equal("F12", terminal.Expand(StringCapability.ScrollForwardLines, 12));
        Assert.Equal("R7", terminal.Expand(StringCapability.ScrollReverseLines, 7));
    }

    [Fact]
    public void StandardNamesPromotedByT12CannotBeStoredAsExtensions()
    {
        TerminalDescriptionBuilder builder =
            new TerminalDescriptionBuilder("sample");

        string[] booleanNames =
        [
            "bce",
            "ccc",
            "hls",
            "km",
            "npc",
        ];
        string[] stringNames =
        [
            "smcup",
            "rmcup",
            "civis",
            "cnorm",
            "cvvis",
            "initc",
            "oc",
            "kmous",
            "indn",
            "rin",
            "kf24",
        ];

        foreach (string name in booleanNames)
        {
            Assert.Throws<ArgumentException>(
                () => builder.SetExtendedBoolean(name));
        }

        foreach (string name in stringNames)
        {
            Assert.Throws<ArgumentException>(
                () => builder.SetExtendedString(name, "value"));
        }
    }

    [Fact]
    public void ExistingBuiltInProfilesDoNotAcquireNewCapabilitiesImplicitly()
    {
        TerminalDescription[] terminals =
        [
            TerminalProfiles.Ansi,
            TerminalProfiles.Vt100,
            TerminalProfiles.Dumb,
        ];

        foreach (TerminalDescription terminal in terminals)
        {
            Assert.Null(
                terminal.GetString(
                    StringCapability.EnterCursorAddressingMode));
            Assert.Null(terminal.GetString(StringCapability.CursorInvisible));
            Assert.Null(terminal.GetString(StringCapability.InitializeColor));
            Assert.Null(terminal.GetString(StringCapability.KeyMouse));
            Assert.False(terminal.GetBoolean(BooleanCapability.BackColorErase));
            Assert.False(terminal.GetBoolean(BooleanCapability.CanChangeColor));
        }
    }

    [Fact]
    public void LaterXtermProfileResolutionDoesNotAliasEarlierBuiltIns()
    {
        Assert.True(
            TerminalDatabase.BuiltIn.TryLoad(
                "xterm",
                out TerminalDescription? terminal));
        Assert.Same(TerminalProfiles.Xterm, terminal);
        Assert.NotSame(TerminalProfiles.Ansi, terminal);
        Assert.NotSame(TerminalProfiles.Vt100, terminal);
        Assert.NotSame(TerminalProfiles.Dumb, terminal);
    }

    [Fact]
    public void AddedFunctionKeyRangeIsRepresentableWithoutSpecialCases()
    {
        TerminalDescriptionBuilder builder =
            new TerminalDescriptionBuilder("sample");

        StringCapability[] capabilities =
        [
            StringCapability.KeyF5,
            StringCapability.KeyF6,
            StringCapability.KeyF7,
            StringCapability.KeyF8,
            StringCapability.KeyF9,
            StringCapability.KeyF10,
            StringCapability.KeyF11,
            StringCapability.KeyF12,
            StringCapability.KeyF13,
            StringCapability.KeyF14,
            StringCapability.KeyF15,
            StringCapability.KeyF16,
            StringCapability.KeyF17,
            StringCapability.KeyF18,
            StringCapability.KeyF19,
            StringCapability.KeyF20,
            StringCapability.KeyF21,
            StringCapability.KeyF22,
            StringCapability.KeyF23,
            StringCapability.KeyF24,
        ];

        for (int i = 0; i < capabilities.Length; i++)
        {
            builder.SetString(capabilities[i], $"F{i + 5}");
        }

        TerminalDescription terminal = builder.Build();

        for (int i = 0; i < capabilities.Length; i++)
        {
            Assert.Equal($"F{i + 5}", terminal.GetString(capabilities[i]));
        }
    }
}
