using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class TermInfoParameterErrorTests
{
    [Theory]
    [InlineData("%")]
    [InlineData("%q")]
    [InlineData("%p0")]
    [InlineData("%p")]
    [InlineData("%P1")]
    [InlineData("%g_")]
    [InlineData("%'a")]
    [InlineData("%{12")]
    [InlineData("%{}")]
    [InlineData("%{999999999999999999999999999}")]
    [InlineData("%t")]
    [InlineData("%e")]
    [InlineData("%;")]
    [InlineData("%:05q")]
    [InlineData("%.s")]
    [InlineData("%10001d")]
    [InlineData("%.10001d")]
    [InlineData("%?%p1%tmissing-end")]
    public void MalformedProgramsThrowFormatException(string source)
    {
        TermInfoFormatException exception =
            Assert.Throws<TermInfoFormatException>(
                () => TermInfoParameterProgram.Parse(source));

        Assert.InRange(exception.Position, 0, source.Length);
    }

    [Fact]
    public void MissingParameterThrowsEvaluationException()
    {
        Assert.Throws<TermInfoEvaluationException>(
            () => TermInfoParameterExpander.Expand("%p2%d", 1));
    }

    [Fact]
    public void StackUnderflowThrowsEvaluationException()
    {
        Assert.Throws<TermInfoEvaluationException>(
            () => TermInfoParameterExpander.Expand("%+"));
    }

    [Fact]
    public void NumericFormattingRejectsString()
    {
        Assert.Throws<TermInfoEvaluationException>(
            () => TermInfoParameterExpander.Expand("%p1%d", "text"));
    }

    [Fact]
    public void StringFormattingRejectsInteger()
    {
        Assert.Throws<TermInfoEvaluationException>(
            () => TermInfoParameterExpander.Expand("%p1%s", 12));
    }

    [Fact]
    public void StringLengthRejectsInteger()
    {
        Assert.Throws<TermInfoEvaluationException>(
            () => TermInfoParameterExpander.Expand("%p1%l%d", 12));
    }

    [Fact]
    public void DivisionByZeroThrowsEvaluationException()
    {
        Assert.Throws<TermInfoEvaluationException>(
            () => TermInfoParameterExpander.Expand(
                "%p1%p2%/%d",
                1,
                0));
    }

    [Fact]
    public void ModuloByZeroThrowsEvaluationException()
    {
        Assert.Throws<TermInfoEvaluationException>(
            () => TermInfoParameterExpander.Expand(
                "%p1%p2%m%d",
                1,
                0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(256)]
    public void CharacterOutputRejectsValuesOutsideByteRange(int value)
    {
        Assert.Throws<TermInfoEvaluationException>(
            () => TermInfoParameterExpander.Expand("%p1%c", value));
    }

    [Fact]
    public void IncrementRejectsStringParameter()
    {
        Assert.Throws<TermInfoEvaluationException>(
            () => TermInfoParameterExpander.Expand(
                "%i%p1%s",
                "text"));
    }

    [Fact]
    public void MoreThanNineParametersAreRejected()
    {
        TermInfoParameterProgram program =
            TermInfoParameterProgram.Parse(string.Empty);

        Assert.Throws<ArgumentException>(
            () => program.Expand(new TermInfoParameter[10]));
    }

    [Fact]
    public void NullProgramSourceIsRejected()
    {
        Assert.Throws<ArgumentNullException>(
            () => TermInfoParameterProgram.Parse(null!));
    }

    [Fact]
    public void NullExpansionContextIsRejected()
    {
        TermInfoParameterProgram program =
            TermInfoParameterProgram.Parse(string.Empty);

        Assert.Throws<ArgumentNullException>(
            () => program.Expand(null!, Array.Empty<TermInfoParameter>()));
    }
}
