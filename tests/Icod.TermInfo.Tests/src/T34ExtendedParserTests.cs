using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T34ExtendedParserTests
{
    [Fact]
    public void NcursesExtendedFixtureParsesAllValueKindsAndExactNames()
    {
        TerminalDescription terminal =
            ParseFixture(
                "compiled/t29-extended.bin");

        Assert.Equal("t29-extended", terminal.Name);
        Assert.Equal(
            "T29 ncurses extended fixture",
            terminal.Description);
        Assert.Equal(
            new[] { "t29x" },
            terminal.Aliases);

        Assert.True(
            terminal.GetBoolean(
                BooleanCapability.AutoRightMargin));
        Assert.Equal<int?>(
            90,
            terminal.GetNumber(
                NumericCapability.Columns));
        Assert.Equal(
            "\u001b[H\u001b[2J",
            terminal.GetString(
                StringCapability.ClearScreen));

        Assert.True(
            terminal.TryGetExtendedBoolean(
                "XBool",
                out bool xBool));
        Assert.True(xBool);

        Assert.True(
            terminal.TryGetExtendedBoolean(
                "xyz",
                out bool xyz));
        Assert.True(xyz);

        Assert.True(
            terminal.TryGetExtendedNumber(
                "XNum",
                out int xNum));
        Assert.Equal(12345, xNum);

        Assert.True(
            terminal.TryGetExtendedString(
                "XStr",
                out string? xStr));
        Assert.Equal(
            "alpha\u001bbeta",
            xStr);

        Assert.False(
            terminal.TryGetExtendedBoolean(
                "xbool",
                out _));
    }

    [Fact]
    public void ExtendedNumberFixturePreservesThirtyTwoBitStandardAndExtendedValues()
    {
        TerminalDescription terminal =
            ParseFixture(
                "compiled/t29-extended32.bin");

        Assert.Equal("t29-extended32", terminal.Name);
        Assert.Equal(
            new[] { "t29x32" },
            terminal.Aliases);

        Assert.Equal<int?>(
            16_777_216,
            terminal.GetNumber(
                NumericCapability.Colors));
        Assert.Equal<int?>(
            65_536,
            terminal.GetNumber(
                NumericCapability.ColorPairs));
        Assert.Equal(
            "\u001b[H\u001b[2J",
            terminal.GetString(
                StringCapability.ClearScreen));

        Assert.True(
            terminal.TryGetExtendedBoolean(
                "XBool",
                out bool xBool));
        Assert.True(xBool);

        Assert.True(
            terminal.TryGetExtendedNumber(
                "XNum",
                out int xNum));
        Assert.Equal(
            2_147_483_640,
            xNum);

        Assert.True(
            terminal.TryGetExtendedString(
                "XStr",
                out string? xStr));
        Assert.Equal("omega", xStr);
    }

    [Theory]
    [InlineData("malformed/malformed-extended-header.bin")]
    [InlineData("malformed/impossible-extended-count.bin")]
    [InlineData("malformed/illegal-extended-string-offset.bin")]
    [InlineData("malformed/extended-standard-name-collision.bin")]
    public void MalformedExtendedFixturesFailWithCompiledFormatException(
        string relativePath)
    {
        byte[] entry =
            ReadFixture(relativePath);

        CompiledTermInfoFormatException exception =
            Assert.Throws<CompiledTermInfoFormatException>(
                () => CompiledTermInfoParser.Parse(entry));

        Assert.NotNull(exception.Section);
    }

    [Fact]
    public void StandardNameCollisionIsReportedAsExtendedNameFailure()
    {
        byte[] entry =
            ReadFixture(
                "malformed/extended-standard-name-collision.bin");

        CompiledTermInfoFormatException exception =
            Assert.Throws<CompiledTermInfoFormatException>(
                () => CompiledTermInfoParser.Parse(entry));

        Assert.Equal(
            "extended-names",
            exception.Section);
    }

    private static TerminalDescription ParseFixture(
        string relativePath)
    {
        return CompiledTermInfoParser.Parse(
            ReadFixture(relativePath));
    }

    private static byte[] ReadFixture(string relativePath)
    {
        return File.ReadAllBytes(
            Path.Combine(
                AppContext.BaseDirectory,
                "fixtures",
                "compiled-terminfo",
                relativePath.Replace(
                    '/',
                    Path.DirectorySeparatorChar)));
    }
}
