using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class TermInfoParameterTests
{
    [Fact]
    public void IntegerParameterExposesIntegerValue()
    {
        TermInfoParameter parameter = new(42L);

        Assert.True(parameter.IsInteger);
        Assert.False(parameter.IsString);
        Assert.Equal(42L, parameter.IntegerValue);
        Assert.Throws<InvalidOperationException>(() => parameter.StringValue);
    }

    [Fact]
    public void StringParameterExposesStringValue()
    {
        TermInfoParameter parameter = new("hello");

        Assert.True(parameter.IsString);
        Assert.False(parameter.IsInteger);
        Assert.Equal("hello", parameter.StringValue);
        Assert.Throws<InvalidOperationException>(() => parameter.IntegerValue);
    }

    [Fact]
    public void NullStringParameterIsRejected()
    {
        Assert.Throws<ArgumentNullException>(
            () => new TermInfoParameter((string)null!));
    }

    [Fact]
    public void DefaultParameterIsIntegerZero()
    {
        TermInfoParameter parameter = default;

        Assert.True(parameter.IsInteger);
        Assert.Equal(0L, parameter.IntegerValue);
    }

    [Fact]
    public void ImplicitConversionsPreserveValues()
    {
        TermInfoParameter integer = 17;
        TermInfoParameter largeInteger = 4_000_000_000L;
        TermInfoParameter text = "value";

        Assert.Equal(17L, integer.IntegerValue);
        Assert.Equal(4_000_000_000L, largeInteger.IntegerValue);
        Assert.Equal("value", text.StringValue);
    }
}
