using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class DecTerminalCapabilityFragmentTests
{
    [Fact]
    public void Vt220PcEditingFragmentMatchesNcursesMapping()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("vt220-pcedit-test")
                .ApplyVt220PcEditingKeys()
                .Build();

        Assert.Equal(
            "\u001b[3~",
            terminal.GetRequiredString(StringCapability.KeyDeleteCharacter));
        Assert.Equal(
            "\u001b[4~",
            terminal.GetRequiredString(StringCapability.KeyEnd));
        Assert.Equal(
            "\u001b[1~",
            terminal.GetRequiredString(StringCapability.KeyHome));
        Assert.Equal(
            "\u001b[2~",
            terminal.GetRequiredString(StringCapability.KeyInsertCharacter));
        Assert.Equal(
            "\u001b[6~",
            terminal.GetRequiredString(StringCapability.KeyNextPage));
        Assert.Equal(
            "\u001b[5~",
            terminal.GetRequiredString(StringCapability.KeyPreviousPage));
    }

    [Fact]
    public void DecFragmentsValidateBuilderArguments()
    {
        Assert.Throws<ArgumentNullException>(
            () => DecTerminalCapabilityFragments.ApplyVt100Core(null!));
        Assert.Throws<ArgumentNullException>(
            () => DecTerminalCapabilityFragments.ApplyVt102Editing(null!));
        Assert.Throws<ArgumentNullException>(
            () => DecTerminalCapabilityFragments.ApplyVt220Core(null!));
        Assert.Throws<ArgumentNullException>(
            () => DecTerminalCapabilityFragments.ApplyVt220PcEditingKeys(null!));
        Assert.Throws<ArgumentNullException>(
            () => DecTerminalCapabilityFragments.ApplyVt220DecEditingKeys(null!));
        Assert.Throws<ArgumentNullException>(
            () => DecTerminalCapabilityFragments.ApplyVt220UnshiftedFunctionKeys(null!));
        Assert.Throws<ArgumentNullException>(
            () => DecTerminalCapabilityFragments.ApplyVt220CursorVisibility(null!));
    }
}
