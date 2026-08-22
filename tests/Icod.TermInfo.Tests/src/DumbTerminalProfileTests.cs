using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class DumbTerminalProfileTests
{
    [Fact]
    public void BuiltInDatabaseLoadsDumbProfile()
    {
        TerminalDescription terminal =
            TerminalDatabase.BuiltIn.Load("dumb");

        Assert.Same(TerminalProfiles.Dumb, terminal);
        Assert.Equal("dumb", terminal.Name);
        Assert.Empty(terminal.Aliases);
    }

    [Fact]
    public void DumbProfileMatchesLowestCommonDenominatorCapabilities()
    {
        TerminalDescription terminal = TerminalProfiles.Dumb;

        Assert.True(terminal.GetBoolean(BooleanCapability.AutoRightMargin));
        Assert.Equal<int?>(
            80,
            terminal.GetNumber(NumericCapability.Columns));
        Assert.Equal("\a", terminal.GetString(StringCapability.Bell));
        Assert.Equal("\r", terminal.GetString(StringCapability.CarriageReturn));
        Assert.Equal("\n", terminal.GetString(StringCapability.CursorDownOne));
        Assert.Equal("\n", terminal.GetString(StringCapability.ScrollForward));
    }

    [Fact]
    public void DumbProfileDoesNotAdvertiseScreenControl()
    {
        TerminalDescription terminal = TerminalProfiles.Dumb;

        Assert.Null(terminal.GetNumber(NumericCapability.Lines));
        Assert.Null(terminal.GetNumber(NumericCapability.Colors));
        Assert.Null(terminal.GetString(StringCapability.ClearScreen));
        Assert.Null(terminal.GetString(StringCapability.CursorAddress));
        Assert.Null(terminal.GetString(StringCapability.EnterBoldMode));
    }

    [Fact]
    public void TraditionalNamesMatchTypedCapabilities()
    {
        TerminalDescription terminal = TerminalProfiles.Dumb;

        Assert.True(terminal.TryGetBoolean("am", out bool autoMargin));
        Assert.True(autoMargin);

        Assert.False(terminal.TryGetBoolean("gn", out bool genericType));
        Assert.False(genericType);

        Assert.True(terminal.TryGetNumber("cols", out int columns));
        Assert.Equal<int?>(
            columns,
            terminal.GetNumber(NumericCapability.Columns));

        Assert.True(terminal.TryGetString("bel", out string? bell));
        Assert.Equal(
            terminal.GetString(StringCapability.Bell),
            bell);
    }

    [Fact]
    public void KnownButAbsentCapabilityReturnsFalseFromTryGet()
    {
        TerminalDescription terminal = TerminalProfiles.Dumb;

        Assert.False(terminal.TryGetNumber("lines", out int lines));
        Assert.Equal(0, lines);

        Assert.False(terminal.TryGetString("clear", out string? clear));
        Assert.Null(clear);
    }

    [Fact]
    public void UnknownCapabilityNameIsRejected()
    {
        TerminalDescription terminal = TerminalProfiles.Dumb;

        Assert.Throws<ArgumentException>(
            () => terminal.TryGetString("not-a-capability", out _));
    }

    [Fact]
    public void RequiredAbsentCapabilityThrows()
    {
        TerminalDescription terminal = TerminalProfiles.Dumb;

        Assert.Throws<InvalidOperationException>(
            () => terminal.GetRequiredString(StringCapability.ClearScreen));
    }

    [Fact]
    public void UnsupportedTerminalDoesNotImplicitlyBecomeDumb()
    {
        Assert.False(
            TerminalDatabase.BuiltIn.TryLoad(
                "xterm-256color",
                out TerminalDescription? terminal));

        Assert.Null(terminal);
    }

    [Fact]
    public void ExplicitFallbackReturnsDumbForUnsupportedTerminal()
    {
        TerminalDescription terminal =
            TerminalDatabase.BuiltIn.Resolve(
                "xterm-256color",
                TerminalProfiles.Dumb);

        Assert.Same(TerminalProfiles.Dumb, terminal);
    }
}
