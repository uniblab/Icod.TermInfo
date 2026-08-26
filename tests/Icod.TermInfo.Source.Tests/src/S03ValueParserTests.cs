using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Source.Tests;

public sealed class S03ValueParserTests
{
    [Theory]
    [InlineData("cols#0", 0)]
    [InlineData("cols#80", 80)]
    [InlineData("cols#0377", 255)]
    [InlineData("cols#0xff", 255)]
    [InlineData("cols#0X7fffffff", int.MaxValue)]
    [InlineData("cols#2147483640", 2_147_483_640)]
    public void NumericValuesSupportDecimalOctalAndHexadecimal(
        string field,
        int expected)
    {
        TermInfoSourceToken token =
            LexCapability(
                field,
                TermInfoSourceTokenKind.NumericCapability);

        TermInfoSourceNumericValueResult result =
            TermInfoSourceValueParser.ParseNumeric(token);

        Assert.False(result.HasErrors);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("cols#", TermInfoSourceDiagnosticCodes.MissingNumericValue)]
    [InlineData("cols#0x", TermInfoSourceDiagnosticCodes.InvalidNumericValue)]
    [InlineData("cols#08", TermInfoSourceDiagnosticCodes.InvalidNumericValue)]
    [InlineData("cols#12z", TermInfoSourceDiagnosticCodes.InvalidNumericValue)]
    [InlineData("cols#+1", TermInfoSourceDiagnosticCodes.InvalidNumericValue)]
    [InlineData("cols#-1", TermInfoSourceDiagnosticCodes.InvalidNumericValue)]
    [InlineData("cols#2147483648", TermInfoSourceDiagnosticCodes.NumericValueOutOfRange)]
    [InlineData("cols#0x80000000", TermInfoSourceDiagnosticCodes.NumericValueOutOfRange)]
    public void InvalidNumericValuesProduceDeterministicDiagnostics(
        string field,
        string expectedCode)
    {
        TermInfoSourceToken token =
            LexCapability(
                field,
                TermInfoSourceTokenKind.NumericCapability);

        TermInfoSourceNumericValueResult result =
            TermInfoSourceValueParser.ParseNumeric(token);

        Assert.True(result.HasErrors);
        Assert.Null(result.Value);
        TermInfoSourceDiagnostic diagnostic =
            Assert.Single(result.Diagnostics);
        Assert.Equal(expectedCode, diagnostic.Code);
        Assert.Equal(
            TermInfoSourceDiagnosticSeverity.Error,
            diagnostic.Severity);
        Assert.Equal("values.ti", diagnostic.Span!.SourceName);
        Assert.Equal(2, diagnostic.Span.Line);
    }

    [Fact]
    public void StandardStringEscapesDecodeToByteEquivalentCharacters()
    {
        TermInfoSourceToken token =
            LexCapability(
                "value=\\E\\e\\a\\n\\l\\r\\t\\b\\f\\s\\^\\\\\\,\\:\\|",
                TermInfoSourceTokenKind.StringCapability);

        TermInfoSourceStringValueResult result =
            TermInfoSourceValueParser.ParseString(token);

        Assert.False(result.HasErrors);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(
            "\x1b\x1b\a\n\n\r\t\b\f ^\\,:|",
            result.Value);
    }

    [Fact]
    public void ControlNotationPreservesTerminfoNullCompatibility()
    {
        TermInfoSourceToken token =
            LexCapability(
                "value=^G^?^@",
                TermInfoSourceTokenKind.StringCapability);

        TermInfoSourceStringValueResult result =
            TermInfoSourceValueParser.ParseString(token);

        Assert.False(result.HasErrors);
        Assert.Equal(
            new string(
                new[]
                {
                    '\x07',
                    '\x7f',
                    '\x80',
                }),
            result.Value);
    }

    [Fact]
    public void OctalEscapesPreserveByteSemanticsAndZeroMapsToEightBitValue()
    {
        TermInfoSourceToken token =
            LexCapability(
                "value=\\007\\200\\000\\0\\777",
                TermInfoSourceTokenKind.StringCapability);

        TermInfoSourceStringValueResult result =
            TermInfoSourceValueParser.ParseString(token);

        Assert.False(result.HasErrors);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(
            new string(
                new[]
                {
                    '\x07',
                    '\x80',
                    '\x80',
                    '\x80',
                    '\xff',
                }),
            result.Value);
    }

    [Fact]
    public void NonOctalDigitsInOctalEscapeWarnButRetainNcursesCompatibleValue()
    {
        TermInfoSourceToken token =
            LexCapability(
                "value=\\089",
                TermInfoSourceTokenKind.StringCapability);

        TermInfoSourceStringValueResult result =
            TermInfoSourceValueParser.ParseString(token);

        Assert.False(result.HasErrors);
        Assert.Equal("I", result.Value);
        Assert.Equal(2, result.Diagnostics.Count);
        Assert.All(
            result.Diagnostics,
            diagnostic =>
            {
                Assert.Equal(
                    TermInfoSourceDiagnosticCodes.NonOctalDigitInStringEscape,
                    diagnostic.Code);
                Assert.Equal(
                    TermInfoSourceDiagnosticSeverity.Warning,
                    diagnostic.Severity);
            });
    }

    [Fact]
    public void UnknownBackslashEscapeWarnsAndRetainsEscapedCharacter()
    {
        TermInfoSourceToken token =
            LexCapability(
                "value=left\\qright",
                TermInfoSourceTokenKind.StringCapability);

        TermInfoSourceStringValueResult result =
            TermInfoSourceValueParser.ParseString(token);

        Assert.False(result.HasErrors);
        Assert.Equal("leftqright", result.Value);
        TermInfoSourceDiagnostic diagnostic =
            Assert.Single(result.Diagnostics);
        Assert.Equal(
            TermInfoSourceDiagnosticCodes.UnknownStringEscape,
            diagnostic.Code);
        Assert.Equal(
            TermInfoSourceDiagnosticSeverity.Warning,
            diagnostic.Severity);
    }

    [Fact]
    public void PercentPreventsCaretFromBeingConsumedAsControlNotation()
    {
        TermInfoSourceToken token =
            LexCapability(
                "value=%^A",
                TermInfoSourceTokenKind.StringCapability);

        TermInfoSourceStringValueResult result =
            TermInfoSourceValueParser.ParseString(token);

        Assert.False(result.HasErrors);
        Assert.Equal("%^A", result.Value);
    }

    [Fact]
    public void RawAndEscapedIndentedLineContinuationsAreRemoved()
    {
        const string source =
            "demo|Description,\r\n"
            + "\tvalue=one\r\n"
            + "\t\ttwo\\\r\n"
            + "\t\tthree,\r\n";

        TermInfoSourceLexResult lex =
            TermInfoSourceLexer.Tokenize(
                source,
                "continuation.ti");
        Assert.False(lex.HasErrors);
        TermInfoSourceToken token =
            Assert.Single(
                lex.Tokens,
                item =>
                    item.Kind == TermInfoSourceTokenKind.StringCapability);

        TermInfoSourceStringValueResult result =
            TermInfoSourceValueParser.ParseString(token);

        Assert.False(result.HasErrors);
        Assert.Empty(result.Diagnostics);
        Assert.Equal("onetwothree", result.Value);
    }

    [Fact]
    public void UnindentedMultilineStringProducesErrorAtLineBoundary()
    {
        const string source =
            "demo|Description,\n"
            + "\tvalue=one\n"
            + "two,\n";

        TermInfoSourceLexResult lex =
            TermInfoSourceLexer.Tokenize(
                source,
                "continuation.ti");
        TermInfoSourceToken token =
            Assert.Single(
                lex.Tokens,
                item =>
                    item.Kind == TermInfoSourceTokenKind.StringCapability);

        TermInfoSourceStringValueResult result =
            TermInfoSourceValueParser.ParseString(token);

        Assert.True(result.HasErrors);
        Assert.Null(result.Value);
        TermInfoSourceDiagnostic diagnostic =
            Assert.Single(result.Diagnostics);
        Assert.Equal(
            TermInfoSourceDiagnosticCodes.UnindentedStringContinuation,
            diagnostic.Code);
        Assert.Equal(2, diagnostic.Span!.Line);
    }

    [Fact]
    public void LiteralNullCharacterIsRejected()
    {
        TermInfoSourceToken token =
            LexCapability(
                "value=left\0right",
                TermInfoSourceTokenKind.StringCapability);

        TermInfoSourceStringValueResult result =
            TermInfoSourceValueParser.ParseString(token);

        Assert.True(result.HasErrors);
        Assert.Null(result.Value);
        TermInfoSourceDiagnostic diagnostic =
            Assert.Single(result.Diagnostics);
        Assert.Equal(
            TermInfoSourceDiagnosticCodes.EmbeddedNullCharacter,
            diagnostic.Code);
    }

    [Fact]
    public void IncompleteControlEscapeProducesError()
    {
        TermInfoSourceToken token =
            LexCapability(
                "value=abc^",
                TermInfoSourceTokenKind.StringCapability);

        TermInfoSourceStringValueResult result =
            TermInfoSourceValueParser.ParseString(token);

        Assert.True(result.HasErrors);
        Assert.Null(result.Value);
        Assert.Equal(
            TermInfoSourceDiagnosticCodes.IncompleteControlEscape,
            Assert.Single(result.Diagnostics).Code);
    }

    [Fact]
    public void IncompleteBackslashEscapeProducesError()
    {
        const string source =
            "demo|Description,\n"
            + "\tvalue=abc\\";

        TermInfoSourceLexResult lex =
            TermInfoSourceLexer.Tokenize(
                source,
                "incomplete.ti");
        TermInfoSourceToken token =
            Assert.Single(
                lex.Tokens,
                item =>
                    item.Kind == TermInfoSourceTokenKind.StringCapability);

        TermInfoSourceStringValueResult result =
            TermInfoSourceValueParser.ParseString(token);

        Assert.True(result.HasErrors);
        Assert.Null(result.Value);
        Assert.Equal(
            TermInfoSourceDiagnosticCodes.IncompleteBackslashEscape,
            Assert.Single(result.Diagnostics).Code);
    }

    [Fact]
    public void ValueParserRejectsWrongTokenKindsAsProgrammerErrors()
    {
        TermInfoSourceToken boolean =
            LexCapability(
                "am",
                TermInfoSourceTokenKind.BooleanCapability);

        Assert.Throws<ArgumentException>(
            () =>
                TermInfoSourceValueParser.ParseNumeric(boolean));
        Assert.Throws<ArgumentException>(
            () =>
                TermInfoSourceValueParser.ParseString(boolean));
    }

    [Fact]
    public void ExistingCompiledFixtureSourcesDecodeWithoutSemanticErrors()
    {
        string root =
            FindRepositoryRoot();
        string fixtureRoot =
            Path.Combine(
                root,
                "tests",
                "Icod.TermInfo.Tests",
                "fixtures",
                "compiled-terminfo",
                "source");

        foreach (
            string path
            in Directory
                .EnumerateFiles(
                    fixtureRoot,
                    "*.ti",
                    SearchOption.TopDirectoryOnly)
                .OrderBy(
                    item => item,
                    StringComparer.Ordinal))
        {
            string source =
                File.ReadAllText(path);
            TermInfoSourceLexResult lex =
                TermInfoSourceLexer.Tokenize(
                    source,
                    Path.GetFileName(path));

            Assert.False(lex.HasErrors);

            foreach (TermInfoSourceToken token in lex.Tokens)
            {
                if (token.Kind == TermInfoSourceTokenKind.NumericCapability)
                {
                    TermInfoSourceNumericValueResult numeric =
                        TermInfoSourceValueParser.ParseNumeric(token);
                    Assert.False(numeric.HasErrors);
                }
                else if (token.Kind == TermInfoSourceTokenKind.StringCapability)
                {
                    TermInfoSourceStringValueResult text =
                        TermInfoSourceValueParser.ParseString(token);
                    Assert.False(text.HasErrors);
                }
            }
        }
    }

    [Fact]
    public void ExistingFixtureValuesMatchKnownCompiledSemantics()
    {
        string root =
            FindRepositoryRoot();
        string fixtureRoot =
            Path.Combine(
                root,
                "tests",
                "Icod.TermInfo.Tests",
                "fixtures",
                "compiled-terminfo",
                "source");

        TermInfoSourceLexResult edge =
            TermInfoSourceLexer.Tokenize(
                File.ReadAllText(
                    Path.Combine(
                        fixtureRoot,
                        "t29-legacy-edge.ti")));
        TermInfoSourceStringValueResult kbs =
            TermInfoSourceValueParser.ParseString(
                edge.Tokens.Single(
                    token =>
                        token.Text.StartsWith(
                            "kbs=",
                            StringComparison.Ordinal)));
        Assert.Equal("\x80", kbs.Value);

        TermInfoSourceLexResult extended =
            TermInfoSourceLexer.Tokenize(
                File.ReadAllText(
                    Path.Combine(
                        fixtureRoot,
                        "t29-extended.ti")));
        TermInfoSourceStringValueResult xstr =
            TermInfoSourceValueParser.ParseString(
                extended.Tokens.Single(
                    token =>
                        token.Text.StartsWith(
                            "XStr=",
                            StringComparison.Ordinal)));
        Assert.Equal("alpha\u001bbeta", xstr.Value);

        TermInfoSourceLexResult extended32 =
            TermInfoSourceLexer.Tokenize(
                File.ReadAllText(
                    Path.Combine(
                        fixtureRoot,
                        "t29-extended32.ti")));
        TermInfoSourceNumericValueResult xnum =
            TermInfoSourceValueParser.ParseNumeric(
                extended32.Tokens.Single(
                    token =>
                        token.Text.StartsWith(
                            "XNum#",
                            StringComparison.Ordinal)));
        Assert.Equal(2_147_483_640, xnum.Value);
    }

    private static TermInfoSourceToken LexCapability(
        string field,
        TermInfoSourceTokenKind kind)
    {
        ArgumentNullException.ThrowIfNull(field);

        string source =
            "demo|Description,\n"
            + "\t"
            + field
            + ",\n";
        TermInfoSourceLexResult result =
            TermInfoSourceLexer.Tokenize(
                source,
                "values.ti");

        Assert.False(result.HasErrors);
        return Assert.Single(
            result.Tokens,
            token => token.Kind == kind);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? current =
            new(
                AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(
                    Path.Combine(
                        current.FullName,
                        "Icod.TermInfo.sln")))
            {
                return current.FullName;
            }

            current =
                current.Parent;
        }

        throw new InvalidOperationException(
            "Unable to locate the Icod.TermInfo repository root.");
    }
}
