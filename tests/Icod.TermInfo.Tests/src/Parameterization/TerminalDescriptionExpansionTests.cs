using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class TerminalDescriptionExpansionTests
{
    [Fact]
    public void TerminalDescriptionExpandsParameterizedCapability()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("test")
                .SetString(
                    StringCapability.CursorAddress,
                    "\x1b[%i%p1%d;%p2%dH")
                .Build();

        Assert.Equal(
            "\x1b[3;8H",
            terminal.Expand(
                StringCapability.CursorAddress,
                2,
                7));
    }

    [Fact]
    public void TerminalDescriptionStillRejectsAbsentCapability()
    {
        Assert.Throws<InvalidOperationException>(
            () => TerminalProfiles.Dumb.Expand(
                StringCapability.CursorAddress,
                0,
                0));
    }
}
