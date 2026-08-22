using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class TermInfoParameterExpansionTests
{
    [Fact]
    public void PercentEscapeProducesLiteralPercent()
    {
        Assert.Equal(
            "load=100%",
            TermInfoParameterExpander.Expand("load=100%%"));
    }

    [Fact]
    public void ParametersCanBePrintedInOrder()
    {
        Assert.Equal(
            "12,34",
            TermInfoParameterExpander.Expand(
                "%p1%d,%p2%d",
                12,
                34));
    }

    [Fact]
    public void NinthParameterCanBeAddressed()
    {
        Assert.Equal(
            "9",
            TermInfoParameterExpander.Expand(
                "%p9%d",
                1, 2, 3, 4, 5, 6, 7, 8, 9));
    }

    [Fact]
    public void AnsiCursorAddressingIncrementsFirstTwoParameters()
    {
        Assert.Equal(
            "\x1b[5;13H",
            TermInfoParameterExpander.Expand(
                "\x1b[%i%p1%d;%p2%dH",
                4,
                12));
    }

    [Fact]
    public void IncrementOperatorAllowsOneParameterCapabilities()
    {
        Assert.Equal(
            "7",
            TermInfoParameterExpander.Expand(
                "%i%p1%d",
                6));
    }

    [Theory]
    [InlineData("%p1%p2%+%d", 7, 5, "12")]
    [InlineData("%p1%p2%-%d", 7, 5, "2")]
    [InlineData("%p1%p2%*%d", 7, 5, "35")]
    [InlineData("%p1%p2%/%d", 7, 5, "1")]
    [InlineData("%p1%p2%m%d", 7, 5, "2")]
    public void ArithmeticOperatorsUsePostfixOperandOrder(
        string source,
        int left,
        int right,
        string expected)
    {
        Assert.Equal(
            expected,
            TermInfoParameterExpander.Expand(source, left, right));
    }

    [Theory]
    [InlineData("%p1%p2%&%d", 6, 3, "2")]
    [InlineData("%p1%p2%|%d", 6, 3, "7")]
    [InlineData("%p1%p2%^%d", 6, 3, "5")]
    public void BitwiseOperatorsAreSupported(
        string source,
        int left,
        int right,
        string expected)
    {
        Assert.Equal(
            expected,
            TermInfoParameterExpander.Expand(source, left, right));
    }

    [Theory]
    [InlineData("%p1%p2%=%d", 4, 4, "1")]
    [InlineData("%p1%p2%=%d", 4, 5, "0")]
    [InlineData("%p1%p2%>%d", 5, 4, "1")]
    [InlineData("%p1%p2%<%d", 5, 4, "0")]
    [InlineData("%p1%p2%A%d", 1, 2, "1")]
    [InlineData("%p1%p2%A%d", 1, 0, "0")]
    [InlineData("%p1%p2%O%d", 0, 2, "1")]
    [InlineData("%p1%p2%O%d", 0, 0, "0")]
    public void ComparisonAndLogicalOperatorsReturnZeroOrOne(
        string source,
        int left,
        int right,
        string expected)
    {
        Assert.Equal(
            expected,
            TermInfoParameterExpander.Expand(source, left, right));
    }

    [Fact]
    public void UnaryOperatorsAreSupported()
    {
        Assert.Equal("1", TermInfoParameterExpander.Expand("%{0}%!%d"));
        Assert.Equal("0", TermInfoParameterExpander.Expand("%{9}%!%d"));
        Assert.Equal("-2", TermInfoParameterExpander.Expand("%{1}%~%d"));
    }

    [Fact]
    public void IntegerAndCharacterConstantsAreSupported()
    {
        Assert.Equal(
            "-5",
            TermInfoParameterExpander.Expand("%{-5}%d"));
        Assert.Equal(
            "!",
            TermInfoParameterExpander.Expand("%{1}%' '%+%c"));
    }

    [Fact]
    public void StringOutputAndLengthAreSupported()
    {
        Assert.Equal(
            "hello:5",
            TermInfoParameterExpander.Expand(
                "%p1%s:%p1%l%d",
                "hello"));
    }

    [Fact]
    public void CharacterOutputUsesByteRange()
    {
        Assert.Equal(
            "A",
            TermInfoParameterExpander.Expand("%p1%c", 65));
    }

    [Theory]
    [InlineData("%p1%05d", 42, "00042")]
    [InlineData("%p1%:05d", 42, "00042")]
    [InlineData("%p1%:+d", 42, "+42")]
    [InlineData("%p1% d", 42, " 42")]
    [InlineData("%p1%:-5d", 42, "42   ")]
    [InlineData("%p1%5.4d", 42, " 0042")]
    public void DecimalFormattingSupportsFlagsWidthAndPrecision(
        string source,
        int value,
        string expected)
    {
        Assert.Equal(
            expected,
            TermInfoParameterExpander.Expand(source, value));
    }

    [Fact]
    public void AlternateRadixFormattingIsSupported()
    {
        Assert.Equal(
            "052",
            TermInfoParameterExpander.Expand("%p1%#o", 42));
        Assert.Equal(
            "0x2a",
            TermInfoParameterExpander.Expand("%p1%#x", 42));
        Assert.Equal(
            "0X2A",
            TermInfoParameterExpander.Expand("%p1%#X", 42));
    }

    [Fact]
    public void StringFormattingSupportsWidthAndPrecision()
    {
        Assert.Equal(
            "  abc",
            TermInfoParameterExpander.Expand("%p1%5.3s", "abcdef"));
        Assert.Equal(
            "abc  ",
            TermInfoParameterExpander.Expand("%p1%:-5.3s", "abcdef"));
    }

    [Fact]
    public void DecimalFormattingHandlesLongBoundaries()
    {
        Assert.Equal(
            long.MinValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
            TermInfoParameterExpander.Expand("%p1%d", long.MinValue));
        Assert.Equal(
            long.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
            TermInfoParameterExpander.Expand("%p1%d", long.MaxValue));
    }

    [Fact]
    public void DynamicVariablesAreScopedToOneExpansion()
    {
        TermInfoParameterProgram setAndGet =
            TermInfoParameterProgram.Parse("%p1%Pa%ga%d");
        TermInfoParameterProgram getOnly =
            TermInfoParameterProgram.Parse("%ga%d");
        TermInfoExpansionContext context = new();

        Assert.Equal("19", setAndGet.Expand(context, 19));
        Assert.Equal("0", getOnly.Expand(context));
    }

    [Fact]
    public void VariablesPreserveStringValues()
    {
        Assert.Equal(
            "saved",
            TermInfoParameterExpander.Expand(
                "%p1%Pa%ga%s",
                "saved"));
    }

    [Fact]
    public void UppercaseVariablesPersistOnlyInExplicitContext()
    {
        TermInfoParameterProgram set =
            TermInfoParameterProgram.Parse("%p1%PA");
        TermInfoParameterProgram get =
            TermInfoParameterProgram.Parse("%gA%d");
        TermInfoExpansionContext context = new();

        Assert.Equal(string.Empty, set.Expand(context, 23));
        Assert.Equal("23", get.Expand(context));
        Assert.Equal("0", get.Expand());

        context.Reset();
        Assert.Equal("0", get.Expand(context));
    }

    [Theory]
    [InlineData(1, "yes")]
    [InlineData(0, "no")]
    public void ConditionalSupportsThenAndElse(
        int condition,
        string expected)
    {
        Assert.Equal(
            expected,
            TermInfoParameterExpander.Expand(
                "%?%p1%tyes%eno%;",
                condition));
    }

    [Theory]
    [InlineData(1, 1, "A")]
    [InlineData(1, 0, "B")]
    [InlineData(0, 1, "C")]
    public void NestedConditionalsAreSupported(
        int first,
        int second,
        string expected)
    {
        Assert.Equal(
            expected,
            TermInfoParameterExpander.Expand(
                "%?%p1%t%?%p2%tA%eB%;%eC%;",
                first,
                second));
    }

    [Theory]
    [InlineData(1, "one")]
    [InlineData(2, "two")]
    [InlineData(3, "other")]
    public void ElseIfConditionalFormIsSupported(
        int value,
        string expected)
    {
        Assert.Equal(
            expected,
            TermInfoParameterExpander.Expand(
                "%?%p1%{1}%=%tone%e%p1%{2}%=%ttwo%eother%;",
                value));
    }

    [Fact]
    public void SgrStyleNestedConditionsCanBeEvaluated()
    {
        const string Sgr =
            "\x1b[0%?%p1%t;7%;%?%p2%t;4%;m";

        Assert.Equal(
            "\x1b[0;7;4m",
            TermInfoParameterExpander.Expand(Sgr, 1, 1));
        Assert.Equal(
            "\x1b[0;4m",
            TermInfoParameterExpander.Expand(Sgr, 0, 1));
    }

    [Fact]
    public void SkippedConditionalBranchIsNotEvaluated()
    {
        Assert.Equal(
            "safe",
            TermInfoParameterExpander.Expand(
                "%?%p1%t%{1}%{0}%/%d%esafe%;",
                0));
    }

    [Fact]
    public void ParsedProgramCanBeReused()
    {
        TermInfoParameterProgram program =
            TermInfoParameterProgram.Parse("%p1%{1}%+%d");

        Assert.Equal("2", program.Expand(1));
        Assert.Equal("42", program.Expand(41));
        Assert.Equal("%p1%{1}%+%d", program.Source);
    }

    [Fact]
    public void PaddingAnnotationsArePreservedForTputsLayer()
    {
        Assert.Equal(
            "A7$<10>B",
            TermInfoParameterExpander.Expand(
                "A%p1%d$<10>B",
                7));
    }
}
