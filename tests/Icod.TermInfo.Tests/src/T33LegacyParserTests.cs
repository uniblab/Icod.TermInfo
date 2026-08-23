using System.Reflection;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T33LegacyParserTests
{
    [Fact]
    public void ParserPublicSurfaceMatchesT32Freeze()
    {
        Assert.Equal(
            1_048_576,
            CompiledTermInfoParserOptions.DefaultMaximumEntrySize);
        Assert.Equal(
            16_777_216,
            CompiledTermInfoParserOptions.MaximumSupportedEntrySize);

        ConstructorInfo optionsConstructor =
            Assert.Single(
                typeof(CompiledTermInfoParserOptions)
                    .GetConstructors(
                        BindingFlags.Public
                        | BindingFlags.Instance));
        ParameterInfo optionsParameter =
            Assert.Single(optionsConstructor.GetParameters());

        Assert.Equal(typeof(int), optionsParameter.ParameterType);
        Assert.True(optionsParameter.HasDefaultValue);
        Assert.Equal(
            CompiledTermInfoParserOptions.DefaultMaximumEntrySize,
            optionsParameter.DefaultValue);

        PropertyInfo maximumEntrySize =
            typeof(CompiledTermInfoParserOptions).GetProperty(
                nameof(CompiledTermInfoParserOptions.MaximumEntrySize))!;
        Assert.True(maximumEntrySize.CanRead);
        Assert.False(maximumEntrySize.CanWrite);

        MethodInfo parse =
            Assert.Single(
                typeof(CompiledTermInfoParser)
                    .GetMethods(
                        BindingFlags.Public
                        | BindingFlags.Static
                        | BindingFlags.DeclaredOnly));
        Assert.Equal(nameof(CompiledTermInfoParser.Parse), parse.Name);
        Assert.Equal(typeof(TerminalDescription), parse.ReturnType);

        ParameterInfo[] parseParameters = parse.GetParameters();
        Assert.Equal(2, parseParameters.Length);
        Assert.Equal(
            typeof(ReadOnlySpan<byte>),
            parseParameters[0].ParameterType);
        Assert.Equal(
            typeof(CompiledTermInfoParserOptions),
            parseParameters[1].ParameterType);
        Assert.True(parseParameters[1].HasDefaultValue);
        Assert.Null(parseParameters[1].DefaultValue);

        Assert.True(
            typeof(FormatException).IsAssignableFrom(
                typeof(CompiledTermInfoFormatException)));

        ConstructorInfo[] exceptionConstructors =
            typeof(CompiledTermInfoFormatException)
                .GetConstructors(
                    BindingFlags.Public
                    | BindingFlags.Instance);
        Assert.Equal(3, exceptionConstructors.Length);
        Assert.Contains(
            exceptionConstructors,
            constructor => constructor.GetParameters().Length == 0);
        Assert.Contains(
            exceptionConstructors,
            constructor =>
            {
                ParameterInfo[] parameters = constructor.GetParameters();
                return parameters.Length == 1
                    && parameters[0].ParameterType == typeof(string);
            });
        Assert.Contains(
            exceptionConstructors,
            constructor =>
            {
                ParameterInfo[] parameters = constructor.GetParameters();
                return parameters.Length == 2
                    && parameters[0].ParameterType == typeof(string)
                    && parameters[1].ParameterType == typeof(Exception);
            });

        CompiledTermInfoFormatException defaultException = new();
        Assert.Equal(-1, defaultException.Offset);
        Assert.Null(defaultException.Section);
    }

    [Fact]
    public void ParserOptionsEnforceFrozenResourceBounds()
    {
        CompiledTermInfoParserOptions defaults = new();
        Assert.Equal(
            CompiledTermInfoParserOptions.DefaultMaximumEntrySize,
            defaults.MaximumEntrySize);

        CompiledTermInfoParserOptions maximum =
            new(
                CompiledTermInfoParserOptions.MaximumSupportedEntrySize);
        Assert.Equal(
            CompiledTermInfoParserOptions.MaximumSupportedEntrySize,
            maximum.MaximumEntrySize);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CompiledTermInfoParserOptions(0));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CompiledTermInfoParserOptions(
                CompiledTermInfoParserOptions.MaximumSupportedEntrySize + 1));

        byte[] entry =
            ReadFixture(
                "compiled/t29-legacy-minimal.bin");
        CompiledTermInfoParserOptions tooSmall =
            new(entry.Length - 1);

        CompiledTermInfoFormatException exception =
            Assert.Throws<CompiledTermInfoFormatException>(
                () => CompiledTermInfoParser.Parse(
                    entry,
                    tooSmall));

        Assert.Equal("entry", exception.Section);
        Assert.Equal(-1, exception.Offset);
    }

    [Fact]
    public void LegacyMinimalFixtureParsesIntoFrozenSemanticModel()
    {
        TerminalDescription terminal =
            ParseFixture(
                "compiled/t29-legacy-minimal.bin");

        Assert.Equal("t29-legacy-minimal", terminal.Name);
        Assert.Equal(
            "T29 minimal legacy fixture",
            terminal.Description);
        Assert.Equal(
            new[] { "t29lm" },
            terminal.Aliases);

        Assert.True(
            terminal.GetBoolean(
                BooleanCapability.AutoRightMargin));
        Assert.False(
            terminal.GetBoolean(
                BooleanCapability.AutoLeftMargin));

        Assert.Equal<int?>(
            80,
            terminal.GetNumber(
                NumericCapability.Columns));
        Assert.Equal<int?>(
            24,
            terminal.GetNumber(
                NumericCapability.Lines));
        Assert.Null(
            terminal.GetNumber(
                NumericCapability.InitialTabWidth));

        Assert.Equal(
            "\u001b[H\u001b[2J",
            terminal.GetString(
                StringCapability.ClearScreen));
        Assert.Equal(
            "\u001b[%i%p1%d;%p2%dH",
            terminal.GetString(
                StringCapability.CursorAddress));
        Assert.Null(
            terminal.GetString(
                StringCapability.Bell));

        Assert.Equal(
            "\u001b[2;3H",
            terminal.Expand(
                StringCapability.CursorAddress,
                1,
                2));
    }

    [Fact]
    public void LegacyAlignmentFixtureHonorsNumericPaddingByte()
    {
        TerminalDescription terminal =
            ParseFixture(
                "compiled/t29-legacy-alignment.bin");

        Assert.Equal("t29-legacy-alignment", terminal.Name);
        Assert.Equal(
            new[] { "t29la" },
            terminal.Aliases);
        Assert.True(
            terminal.GetBoolean(
                BooleanCapability.AutoRightMargin));
        Assert.Equal<int?>(
            81,
            terminal.GetNumber(
                NumericCapability.Columns));
        Assert.Equal(
            "\u0007",
            terminal.GetString(
                StringCapability.Bell));
    }

    [Fact]
    public void LegacyEdgeFixturePreservesCancellationPaddingAndHighByteData()
    {
        TerminalDescription terminal =
            ParseFixture(
                "compiled/t29-legacy-edge.bin");

        Assert.Equal("t29-legacy-edge", terminal.Name);
        Assert.Equal(
            "T29 cancellation padding and high-byte fixture",
            terminal.Description);
        Assert.Equal(
            new[] { "t29le" },
            terminal.Aliases);

        Assert.True(
            terminal.GetBoolean(
                BooleanCapability.EatNewlineGlitch));
        Assert.False(
            terminal.GetBoolean(
                BooleanCapability.AutoLeftMargin));
        Assert.Null(
            terminal.GetNumber(
                NumericCapability.Lines));
        Assert.Equal<int?>(
            82,
            terminal.GetNumber(
                NumericCapability.Columns));
        Assert.Equal<int?>(
            8,
            terminal.GetNumber(
                NumericCapability.Colors));
        Assert.Null(
            terminal.GetString(
                StringCapability.Bell));

        Assert.Equal(
            "\u001b[H\u001b[2J$<5>",
            terminal.GetString(
                StringCapability.ClearScreen));
        Assert.Equal(
            "\u001b[%i%p1%d;%p2%dH$<2*>",
            terminal.GetString(
                StringCapability.CursorAddress));
        Assert.Equal(
            "\u0080",
            terminal.GetString(
                StringCapability.KeyBackspace));
        Assert.Equal(
            "\u001b[2;3H$<2*>",
            terminal.Expand(
                StringCapability.CursorAddress,
                1,
                2));
    }

    [Theory]
    [InlineData("malformed/truncated-header.bin")]
    [InlineData("malformed/impossible-count.bin")]
    [InlineData("malformed/bad-names-terminator.bin")]
    [InlineData("malformed/illegal-string-offset.bin")]
    [InlineData("malformed/unsupported-magic.bin")]
    public void LegacyMalformedFixturesFailWithCompiledFormatException(
        string relativePath)
    {
        byte[] entry = ReadFixture(relativePath);

        CompiledTermInfoFormatException exception =
            Assert.Throws<CompiledTermInfoFormatException>(
                () => CompiledTermInfoParser.Parse(entry));

        Assert.NotNull(exception.Section);
        Assert.True(exception.Offset >= 0);
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
