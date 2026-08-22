using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class PublicExceptionContractTests
{
    [Fact]
    public void FormatExceptionSupportsStandardPublicConstructors()
    {
        Exception inner = new InvalidOperationException("inner");

        TermInfoFormatException empty = new();
        TermInfoFormatException message = new("message");
        TermInfoFormatException nested = new("nested", inner);

        Assert.Equal(-1, empty.Position);
        Assert.Equal("message", message.Message);
        Assert.Equal(-1, message.Position);
        Assert.Same(inner, nested.InnerException);
        Assert.Equal(-1, nested.Position);
    }

    [Fact]
    public void EvaluationExceptionSupportsStandardPublicConstructors()
    {
        Exception inner = new InvalidOperationException("inner");

        TermInfoEvaluationException empty = new();
        TermInfoEvaluationException message = new("message");
        TermInfoEvaluationException nested = new("nested", inner);

        Assert.Equal(-1, empty.Position);
        Assert.Equal("message", message.Message);
        Assert.Equal(-1, message.Position);
        Assert.Same(inner, nested.InnerException);
        Assert.Equal(-1, nested.Position);
    }
}
