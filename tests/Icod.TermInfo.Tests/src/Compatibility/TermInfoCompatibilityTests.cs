using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class TermInfoCompatibilityTests
{
    [Fact]
    public void TiGetOperationsUseManagedCapabilitySemantics()
    {
        TerminalDescription ansi = TerminalProfiles.Ansi;
        TerminalDescription dumb = TerminalProfiles.Dumb;

        Assert.True(TermInfoCompatibility.TiGetFlag(ansi, "am"));
        Assert.Equal<int?>(
            8,
            TermInfoCompatibility.TiGetNum(ansi, "colors"));
        Assert.Equal(
            "\x1b[H\x1b[J",
            TermInfoCompatibility.TiGetStr(ansi, "clear"));

        Assert.False(TermInfoCompatibility.TiGetFlag(dumb, "msgr"));
        Assert.Null(TermInfoCompatibility.TiGetNum(dumb, "colors"));
        Assert.Null(TermInfoCompatibility.TiGetStr(dumb, "cup"));
    }

    [Theory]
    [InlineData("not-a-boolean", 0)]
    [InlineData("not-a-number", 1)]
    [InlineData("not-a-string", 2)]
    public void TiGetOperationsRejectUnknownCapabilityNames(
        string name,
        int operation)
    {
        TerminalDescription terminal = TerminalProfiles.Ansi;

        switch (operation)
        {
            case 0:
                Assert.Throws<ArgumentException>(
                    () => TermInfoCompatibility.TiGetFlag(terminal, name));
                break;
            case 1:
                Assert.Throws<ArgumentException>(
                    () => TermInfoCompatibility.TiGetNum(terminal, name));
                break;
            case 2:
                Assert.Throws<ArgumentException>(
                    () => TermInfoCompatibility.TiGetStr(terminal, name));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(operation));
        }
    }

    [Fact]
    public void TParmAndTiParmUseSharedParameterEngine()
    {
        const string source = "\x1b[%i%p1%d;%p2%dH";

        Assert.Equal(
            "\x1b[5;13H",
            TermInfoCompatibility.TParm(source, 4, 12));
        Assert.Equal(
            "\x1b[5;13H",
            TermInfoCompatibility.TiParm(source, 4, 12));
    }

    [Fact]
    public void TParmAndTiParmHonorExplicitPersistentContext()
    {
        TermInfoExpansionContext context = new();

        Assert.Equal(
            string.Empty,
            TermInfoCompatibility.TParm(
                "%p1%PA",
                context,
                23));
        Assert.Equal(
            "23",
            TermInfoCompatibility.TiParm(
                "%gA%d",
                context));

        Assert.Equal(
            "0",
            TermInfoCompatibility.TiParm("%gA%d"));
    }

    [Fact]
    public void TPutsAndPutPStripPaddingThroughCallbackSurface()
    {
        List<char> tputsOutput = [];
        List<char> putpOutput = [];

        TermInfoCompatibility.TPuts(
            "A$<5>B",
            3,
            tputsOutput.Add);
        TermInfoCompatibility.PutP(
            "C$<2>D",
            putpOutput.Add);

        Assert.Equal("AB", new string(tputsOutput.ToArray()));
        Assert.Equal("CD", new string(putpOutput.ToArray()));
    }

    [Fact]
    public void CompatibilitySurfaceValidatesReferenceParameters()
    {
        Assert.Throws<ArgumentNullException>(
            () => TermInfoCompatibility.TiGetFlag(null!, "am"));
        Assert.Throws<ArgumentNullException>(
            () => TermInfoCompatibility.TiGetNum(TerminalProfiles.Ansi, null!));
        Assert.Throws<ArgumentNullException>(
            () => TermInfoCompatibility.TiGetStr(TerminalProfiles.Ansi, null!));
        Assert.Throws<ArgumentNullException>(
            () => TermInfoCompatibility.TParm(null!, 1));
        Assert.Throws<ArgumentNullException>(
            () => TermInfoCompatibility.TParm("%p1%d", (TermInfoParameter[])null!));
        Assert.Throws<ArgumentNullException>(
            () => TermInfoCompatibility.TiParm(
                "%p1%d",
                (TermInfoExpansionContext)null!,
                1));
        Assert.Throws<ArgumentNullException>(
            () => TermInfoCompatibility.TPuts(
                null!,
                1,
                _ => { }));
        Assert.Throws<ArgumentNullException>(
            () => TermInfoCompatibility.PutP(
                "value",
                null!));
    }
}
