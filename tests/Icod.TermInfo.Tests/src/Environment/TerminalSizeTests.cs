using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class TerminalSizeTests
{
    [Fact]
    public void ConstructorStoresPositiveDimensions()
    {
        TerminalSize size = new(132, 43);

        Assert.Equal(132, size.Columns);
        Assert.Equal(43, size.Rows);
    }

    [Theory]
    [InlineData(0, 24)]
    [InlineData(-1, 24)]
    [InlineData(80, 0)]
    [InlineData(80, -1)]
    public void ConstructorRejectsNonPositiveDimensions(
        int columns,
        int rows)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new TerminalSize(columns, rows));
    }
}
