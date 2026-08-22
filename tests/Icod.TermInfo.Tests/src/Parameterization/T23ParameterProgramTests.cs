using System.Reflection;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T23ParameterProgramTests
{
    [Fact]
    public void AssemblyIdentifiesT23DevelopmentVersion()
    {
        Assembly assembly = typeof(TermInfoParameterProgram).Assembly;
        string? informationalVersion =
            assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

        Assert.NotNull(informationalVersion);
        Assert.True(
            informationalVersion!.StartsWith(
                "0.8.0-alpha.3",
                StringComparison.Ordinal),
            $"Unexpected informational version '{informationalVersion}'.");
    }

    [Fact]
    public void AnalysisClassifiesParameterAndVariableUse()
    {
        TermInfoParameterProgram program =
            TermInfoParameterProgram.Parse(
                "%p1%{1}%+%d:"
                + "%p2%l%d:"
                + "%p3%Pa%ga%s:"
                + "%p4%PB%gB%d");
        TermInfoParameterProgramAnalysis analysis =
            program.Analysis;

        Assert.False(analysis.UsesImplicitFormatParameters);
        Assert.Equal(
            new[] { 0, 1, 2, 3 },
            analysis.ReferencedParameterIndices);
        Assert.Equal(3, analysis.HighestParameterIndex);
        Assert.Equal(
            TermInfoParameterUsage.Integer,
            analysis.ParameterUsages[0]);
        Assert.Equal(
            TermInfoParameterUsage.String,
            analysis.ParameterUsages[1]);
        Assert.Equal(
            TermInfoParameterUsage.String,
            analysis.ParameterUsages[2]);
        Assert.Equal(
            TermInfoParameterUsage.Integer,
            analysis.ParameterUsages[3]);
        Assert.Equal(
            new[] { 'a' },
            analysis.DynamicVariables);
        Assert.Equal(
            new[] { 'B' },
            analysis.StaticVariables);
        Assert.True(analysis.InstructionCount > 0);
        Assert.True(analysis.MaximumStackDepth > 0);
        Assert.False(analysis.MayUnderflow);

        Assert.True(
            ((IList<int>)analysis.ReferencedParameterIndices)
                .IsReadOnly);
        Assert.True(
            ((IList<TermInfoParameterUsage>)analysis.ParameterUsages)
                .IsReadOnly);
    }

    [Fact]
    public void AnalysisRetainsPathDependentParameterTypes()
    {
        TermInfoParameterProgram program =
            TermInfoParameterProgram.Parse(
                "%?%p1%t%p2%d%e%p2%s%;");
        TermInfoParameterProgramAnalysis analysis =
            program.Analysis;

        Assert.Equal(
            TermInfoParameterUsage.Integer,
            analysis.ParameterUsages[0]);
        Assert.Equal(
            TermInfoParameterUsage.Integer
            | TermInfoParameterUsage.String,
            analysis.ParameterUsages[1]);

        Assert.Equal(
            "42",
            program.Expand(1, 42));
        Assert.Equal(
            "text",
            program.Expand(0, "text"));

        Assert.Throws<TermInfoEvaluationException>(
            () => program.Expand(1, "text"));
        Assert.Throws<TermInfoEvaluationException>(
            () => program.Expand(0, 42));
    }

    [Fact]
    public void ImplicitLegacyFormattingConsumesSuccessiveParameters()
    {
        TermInfoParameterProgram program =
            TermInfoParameterProgram.Parse(
                "\u001b[%d;%dH");
        TermInfoParameterProgramAnalysis analysis =
            program.Analysis;

        Assert.True(analysis.UsesImplicitFormatParameters);
        Assert.Equal(
            new[] { 0, 1 },
            analysis.ReferencedParameterIndices);
        Assert.Equal(
            TermInfoParameterUsage.Integer,
            analysis.ParameterUsages[0]);
        Assert.Equal(
            TermInfoParameterUsage.Integer,
            analysis.ParameterUsages[1]);
        Assert.Equal(
            "\u001b[4;12H",
            program.Expand(4, 12));
    }

    [Fact]
    public void ImplicitLegacyFormattingPreservesStringParameters()
    {
        TermInfoParameterProgram program =
            TermInfoParameterProgram.Parse(
                "%s:%d");

        Assert.Equal(
            "sample:7",
            program.Expand("sample", 7));
        Assert.Equal(
            TermInfoParameterUsage.String,
            program.Analysis.ParameterUsages[0]);
        Assert.Equal(
            TermInfoParameterUsage.Integer,
            program.Analysis.ParameterUsages[1]);
    }

    [Fact]
    public void ImplicitLegacyFormattingObservesIncrementedParameters()
    {
        Assert.Equal(
            "5;13",
            TermInfoParameterExpander.Expand(
                "%i%d;%d",
                4,
                12));
    }

    [Fact]
    public void ExplicitParameterProgramsDoNotAcquireImplicitStackValues()
    {
        TermInfoParameterProgram program =
            TermInfoParameterProgram.Parse(
                "%p1%d:%d");

        Assert.False(
            program.Analysis.UsesImplicitFormatParameters);
        Assert.True(program.Analysis.MayUnderflow);
        Assert.Throws<TermInfoEvaluationException>(
            () => program.Expand(1, 2));
    }

    [Fact]
    public void SourceLengthIsBounded()
    {
        string accepted =
            new(
                'x',
                TermInfoParameterLimits.MaximumSourceLength);
        string rejected =
            accepted + "x";

        TermInfoParameterProgram program =
            TermInfoParameterProgram.Parse(accepted);

        Assert.Equal(accepted, program.Source);
        Assert.Throws<TermInfoFormatException>(
            () => TermInfoParameterProgram.Parse(rejected));
    }

    [Fact]
    public void ParsedInstructionCountIsBounded()
    {
        string accepted =
            string.Concat(
                Enumerable.Repeat(
                    "%{1}%d",
                    TermInfoParameterLimits.MaximumInstructionCount / 2));
        string rejected =
            accepted + "%{1}%d";

        TermInfoParameterProgram program =
            TermInfoParameterProgram.Parse(accepted);

        Assert.Equal(
            TermInfoParameterLimits.MaximumInstructionCount,
            program.Analysis.InstructionCount);
        Assert.Throws<TermInfoFormatException>(
            () => TermInfoParameterProgram.Parse(rejected));
    }

    [Fact]
    public void ConditionalNestingIsBounded()
    {
        string accepted =
            CreateNestedConditional(
                TermInfoParameterLimits.MaximumConditionalNesting);
        string rejected =
            CreateNestedConditional(
                TermInfoParameterLimits.MaximumConditionalNesting + 1);

        TermInfoParameterProgram program =
            TermInfoParameterProgram.Parse(accepted);

        Assert.Equal(
            TermInfoParameterLimits.MaximumConditionalNesting,
            program.Analysis.MaximumConditionalNesting);
        Assert.Equal(
            "ok",
            program.Expand());
        Assert.Throws<TermInfoFormatException>(
            () => TermInfoParameterProgram.Parse(rejected));
    }

    [Fact]
    public void AnalysisBoundsMaximumStackGrowth()
    {
        string accepted =
            "%p1"
            + string.Concat(
                Enumerable.Repeat(
                    "%{1}",
                    TermInfoParameterLimits.MaximumStackDepth - 1));
        string rejected =
            accepted + "%{1}";

        TermInfoParameterProgram program =
            TermInfoParameterProgram.Parse(accepted);

        Assert.Equal(
            TermInfoParameterLimits.MaximumStackDepth,
            program.Analysis.MaximumStackDepth);
        Assert.Throws<TermInfoFormatException>(
            () => TermInfoParameterProgram.Parse(rejected));
    }

    [Fact]
    public void ExpandedOutputLengthIsBounded()
    {
        const string WideField = "%{1}%10000d";
        string accepted =
            string.Concat(
                Enumerable.Repeat(
                    WideField,
                    104));
        string rejected =
            accepted + WideField;

        Assert.Equal(
            1_040_000,
            TermInfoParameterProgram.Parse(accepted)
                .Expand()
                .Length);
        Assert.Throws<TermInfoEvaluationException>(
            () => TermInfoParameterProgram.Parse(rejected)
                .Expand());
    }

    [Theory]
    [InlineData("%p1%{1}%+%d", long.MaxValue)]
    [InlineData("%p1%{1}%-%d", long.MinValue)]
    [InlineData("%p1%{2}%*%d", long.MaxValue)]
    [InlineData("%p1%{-1}%/%d", long.MinValue)]
    public void ArithmeticOverflowFailsDeterministically(
        string source,
        long value)
    {
        Assert.Throws<TermInfoEvaluationException>(
            () => TermInfoParameterExpander.Expand(
                source,
                value));
    }

    [Fact]
    public void IncrementOverflowFailsDeterministically()
    {
        Assert.Throws<TermInfoEvaluationException>(
            () => TermInfoParameterExpander.Expand(
                "%i%p1%d",
                long.MaxValue));
    }

    [Fact]
    public void WidthAndPrecisionLimitsRemainUsableAtBoundary()
    {
        Assert.Equal(
            10_000,
            TermInfoParameterExpander.Expand(
                "%p1%10000d",
                1)
                .Length);
        Assert.Equal(
            10_000,
            TermInfoParameterExpander.Expand(
                "%p1%.10000d",
                1)
                .Length);
    }

    private static string CreateNestedConditional(int depth)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(depth);

        return string.Concat(
                Enumerable.Repeat(
                    "%?%{1}%t",
                    depth))
            + "ok"
            + string.Concat(
                Enumerable.Repeat(
                    "%;",
                    depth));
    }
}
