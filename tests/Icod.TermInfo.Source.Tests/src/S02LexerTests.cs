using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Source.Tests;

public sealed class S02LexerTests
{
    [Fact]
    public void RepresentativeSourceIsClassifiedWithoutValueDecoding()
    {
        const string source =
            "# leading comment\n"
            + "demo|demo-alt|Demonstration terminal,\n"
            + "\tam,\n"
            + "\tcols#80,\n"
            + "\tclear=\\E[H\\,literal,\n"
            + "\tsmkx@,\n"
            + "\tuse=xterm-256color,\n";

        TermInfoSourceLexResult result =
            TermInfoSourceLexer.Tokenize(
                source,
                "demo.ti");

        Assert.False(result.HasErrors);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(
            new[]
            {
                TermInfoSourceTokenKind.Comment,
                TermInfoSourceTokenKind.TerminalName,
                TermInfoSourceTokenKind.Alias,
                TermInfoSourceTokenKind.Description,
                TermInfoSourceTokenKind.BooleanCapability,
                TermInfoSourceTokenKind.NumericCapability,
                TermInfoSourceTokenKind.StringCapability,
                TermInfoSourceTokenKind.CancelledCapability,
                TermInfoSourceTokenKind.UseReference,
            },
            result.Tokens.Select(token => token.Kind));

        Assert.Equal(
            "demo",
            result.Tokens[1].Text);
        Assert.Equal(
            "demo-alt",
            result.Tokens[2].Text);
        Assert.Equal(
            "Demonstration terminal",
            result.Tokens[3].Text);
        Assert.Equal(
            "clear=\\E[H\\,literal",
            result.Tokens[6].Text);
        Assert.Equal(
            "demo.ti",
            result.Tokens[6].Span.SourceName);
        Assert.Equal(
            5,
            result.Tokens[6].Span.Line);
        Assert.Equal(
            2,
            result.Tokens[6].Span.Column);
    }

    [Fact]
    public void FinalHeaderComponentWithoutWhitespaceRetainsAliasAndDescriptionRoles()
    {
        const string source =
            "demo|shortname,\n"
            + "\tam,\n";

        TermInfoSourceLexResult result =
            TermInfoSourceLexer.Tokenize(source);

        Assert.False(result.HasErrors);
        TermInfoSourceToken alias =
            Assert.Single(
                result.Tokens,
                token =>
                    token.Kind == TermInfoSourceTokenKind.Alias);
        TermInfoSourceToken description =
            Assert.Single(
                result.Tokens,
                token =>
                    token.Kind == TermInfoSourceTokenKind.Description);

        Assert.Equal("shortname", alias.Text);
        Assert.Equal("shortname", description.Text);
        Assert.Equal(alias.Span.Offset, description.Span.Offset);
        Assert.Equal(alias.Span.Length, description.Span.Length);
    }

    [Fact]
    public void OddBackslashRunEscapesCommaAndEvenRunDoesNot()
    {
        const string source =
            "demo|Description,\n"
            + "\tone=left\\,right,\n"
            + "\ttwo=ends\\\\,\n"
            + "\tam,\n";

        TermInfoSourceLexResult result =
            TermInfoSourceLexer.Tokenize(source);

        Assert.False(result.HasErrors);
        Assert.Equal(
            "one=left\\,right",
            result.Tokens
                .Single(token => token.Text.StartsWith("one=", StringComparison.Ordinal))
                .Text);
        Assert.Equal(
            "two=ends\\\\",
            result.Tokens
                .Single(token => token.Text.StartsWith("two=", StringComparison.Ordinal))
                .Text);
        Assert.Contains(
            result.Tokens,
            token =>
                token.Kind == TermInfoSourceTokenKind.BooleanCapability
                && token.Text == "am");
    }

    [Fact]
    public void MultilineStringRemainsOneRawToken()
    {
        const string source =
            "demo|Description,\r\n"
            + "\tsgr=\\E[0;%?%p1%t\r\n"
            + "\t\t7%;m,\r\n"
            + "\tam,\r\n";

        TermInfoSourceLexResult result =
            TermInfoSourceLexer.Tokenize(source);

        Assert.False(result.HasErrors);
        TermInfoSourceToken sgr =
            Assert.Single(
                result.Tokens,
                token =>
                    token.Kind == TermInfoSourceTokenKind.StringCapability);
        Assert.True(
            sgr.Text.Contains(
                "\r\n\t\t7%;m",
                StringComparison.Ordinal));
        Assert.Equal(2, sgr.Span.Line);
        Assert.Equal(2, sgr.Span.Column);
    }

    [Fact]
    public void CommentsMayAppearBetweenEntryFieldsAndEntries()
    {
        const string source =
            "first|First terminal,\n"
            + "\tam,\n"
            + "# retained between entries\n"
            + "second|Second terminal,\n"
            + "\txenl,\n";

        TermInfoSourceLexResult result =
            TermInfoSourceLexer.Tokenize(source);

        Assert.False(result.HasErrors);
        Assert.Equal(
            2,
            result.Tokens.Count(
                token =>
                    token.Kind == TermInfoSourceTokenKind.TerminalName));
        TermInfoSourceToken comment =
            Assert.Single(
                result.Tokens,
                token =>
                    token.Kind == TermInfoSourceTokenKind.Comment);
        Assert.Equal(
            "# retained between entries",
            comment.Text);
        Assert.Equal(3, comment.Span.Line);
        Assert.Equal(1, comment.Span.Column);
    }

    [Fact]
    public void NcursesDisabledCapabilityIsRetainedAsLexicalUnit()
    {
        const string source =
            "demo|Description,\n"
            + "\t.ind=\\E[%p1%dS,\n";

        TermInfoSourceLexResult result =
            TermInfoSourceLexer.Tokenize(source);

        Assert.False(result.HasErrors);
        TermInfoSourceToken disabled =
            Assert.Single(
                result.Tokens,
                token =>
                    token.Kind == TermInfoSourceTokenKind.DisabledCapability);
        Assert.Equal(
            ".ind=\\E[%p1%dS",
            disabled.Text);
    }

    [Fact]
    public void SourceSpanUsesUtf16OffsetsAndOneBasedLineAndColumn()
    {
        const string source =
            "demo|Description,\r\n"
            + "\tcols#80,\r\n";

        TermInfoSourceLexResult result =
            TermInfoSourceLexer.Tokenize(
                source,
                "locations.ti");
        TermInfoSourceToken numeric =
            Assert.Single(
                result.Tokens,
                token =>
                    token.Kind == TermInfoSourceTokenKind.NumericCapability);

        Assert.Equal(
            source.IndexOf(
                "cols#80",
                StringComparison.Ordinal),
            numeric.Span.Offset);
        Assert.Equal(2, numeric.Span.Line);
        Assert.Equal(2, numeric.Span.Column);
        Assert.Equal("cols#80".Length, numeric.Span.Length);
        Assert.Equal(
            numeric.Span.Offset + numeric.Span.Length,
            numeric.Span.EndOffset);
        Assert.Equal(
            "locations.ti",
            numeric.Span.SourceName);
    }

    [Fact]
    public void MissingFinalCommaProducesPreciseDiagnosticAndRetainsToken()
    {
        const string source =
            "demo|Description,\n"
            + "\tam";

        TermInfoSourceLexResult result =
            TermInfoSourceLexer.Tokenize(source);

        Assert.True(result.HasErrors);
        TermInfoSourceDiagnostic diagnostic =
            Assert.Single(result.Diagnostics);
        Assert.Equal(
            TermInfoSourceDiagnosticCodes.MissingFieldTerminator,
            diagnostic.Code);
        Assert.Equal(
            TermInfoSourceDiagnosticSeverity.Error,
            diagnostic.Severity);
        Assert.NotNull(diagnostic.Span);
        Assert.Equal(2, diagnostic.Span!.Line);
        Assert.Equal(2, diagnostic.Span.Column);
        Assert.Contains(
            result.Tokens,
            token =>
                token.Kind == TermInfoSourceTokenKind.BooleanCapability
                && token.Text == "am");
    }

    [Fact]
    public void OrphanedIndentedFieldProducesDiagnostic()
    {
        TermInfoSourceLexResult result =
            TermInfoSourceLexer.Tokenize(
                "\tam,\n");

        TermInfoSourceDiagnostic diagnostic =
            Assert.Single(result.Diagnostics);
        Assert.Equal(
            TermInfoSourceDiagnosticCodes.OrphanedCapabilityField,
            diagnostic.Code);
        Assert.Equal(1, diagnostic.Span!.Line);
        Assert.Equal(2, diagnostic.Span.Column);
    }

    [Theory]
    [InlineData("demo|Description,\n\t=bad,\n")]
    [InlineData("demo|Description,\n\t#12,\n")]
    [InlineData("demo|Description,\n\t@,\n")]
    public void CapabilityOperatorWithoutNameProducesDiagnostic(
        string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        TermInfoSourceLexResult result =
            TermInfoSourceLexer.Tokenize(source);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code
                    == TermInfoSourceDiagnosticCodes.MissingCapabilityName);
        Assert.Contains(
            result.Tokens,
            token =>
                token.Kind == TermInfoSourceTokenKind.Invalid);
    }

    [Fact]
    public void EmptyHeaderComponentsProduceStableDiagnostics()
    {
        TermInfoSourceLexResult result =
            TermInfoSourceLexer.Tokenize(
                "|alias||,\n");

        Assert.True(result.HasErrors);
        Assert.Equal(
            new[]
            {
                TermInfoSourceDiagnosticCodes.EmptyTerminalName,
                TermInfoSourceDiagnosticCodes.EmptyHeaderComponent,
                TermInfoSourceDiagnosticCodes.EmptyHeaderComponent,
            },
            result.Diagnostics.Select(diagnostic => diagnostic.Code));
    }

    [Fact]
    public void MissingUseReferenceProducesStableDiagnostic()
    {
        TermInfoSourceLexResult result =
            TermInfoSourceLexer.Tokenize(
                "demo|Description,\n"
                + "\tuse=,\n");

        TermInfoSourceDiagnostic diagnostic =
            Assert.Single(result.Diagnostics);
        Assert.Equal(
            TermInfoSourceDiagnosticCodes.MissingUseReference,
            diagnostic.Code);
        Assert.Contains(
            result.Tokens,
            token =>
                token.Kind == TermInfoSourceTokenKind.UseReference);
    }

    [Fact]
    public void EmptyFieldAndCancellationTailProduceStableDiagnostics()
    {
        TermInfoSourceLexResult result =
            TermInfoSourceLexer.Tokenize(
                "demo|Description,,\n"
                + "\tsmkx@unexpected,\n");

        Assert.Equal(
            new[]
            {
                TermInfoSourceDiagnosticCodes.EmptyField,
                TermInfoSourceDiagnosticCodes.UnexpectedTextAfterCancellation,
            },
            result.Diagnostics.Select(diagnostic => diagnostic.Code));
    }

    [Fact]
    public void ConfiguredLengthLimitReturnsDiagnosticWithoutThrowing()
    {
        TermInfoSourceLexerOptions options =
            new(
                maximumSourceLength: 8);

        TermInfoSourceLexResult result =
            TermInfoSourceLexer.Tokenize(
                "123456789",
                "too-large.ti",
                options);

        Assert.True(result.HasErrors);
        Assert.Empty(result.Tokens);
        TermInfoSourceDiagnostic diagnostic =
            Assert.Single(result.Diagnostics);
        Assert.Equal(
            TermInfoSourceDiagnosticCodes.MaximumSourceLengthExceeded,
            diagnostic.Code);
        Assert.Null(diagnostic.Span);
    }

    [Fact]
    public void TextReaderPathUsesSameTokenizationAndLengthPolicy()
    {
        const string source =
            "demo|Description,\n"
            + "\tam,\n";

        using StringReader reader =
            new(source);
        TermInfoSourceLexResult result =
            TermInfoSourceLexer.Tokenize(
                reader,
                "reader.ti");

        Assert.False(result.HasErrors);
        Assert.Contains(
            result.Tokens,
            token =>
                token.Kind == TermInfoSourceTokenKind.BooleanCapability);
        Assert.All(
            result.Tokens,
            token =>
                Assert.Equal(
                    "reader.ti",
                    token.Span.SourceName));

        using StringReader tooLarge =
            new("123456789");
        TermInfoSourceLexResult limited =
            TermInfoSourceLexer.Tokenize(
                tooLarge,
                options: new TermInfoSourceLexerOptions(8));
        Assert.Equal(
            TermInfoSourceDiagnosticCodes.MaximumSourceLengthExceeded,
            Assert.Single(limited.Diagnostics).Code);
    }

    [Fact]
    public void ExistingCompiledFixtureSourceCorpusTokenizesWithoutDiagnostics()
    {
        string root =
            FindRepositoryRoot();
        string sourceRoot =
            Path.Combine(
                root,
                "tests",
                "Icod.TermInfo.Tests",
                "fixtures",
                "compiled-terminfo",
                "source");
        string[] files =
            Directory.GetFiles(
                sourceRoot,
                "*.ti",
                SearchOption.TopDirectoryOnly);

        Assert.NotEmpty(files);
        foreach (
            string path
            in files.OrderBy(
                value => value,
                StringComparer.Ordinal))
        {
            string source =
                File.ReadAllText(path);
            TermInfoSourceLexResult result =
                TermInfoSourceLexer.Tokenize(
                    source,
                    Path.GetFileName(path));

            string diagnosticText =
                string.Join(
                    "; ",
                    result.Diagnostics.Select(
                        diagnostic =>
                            diagnostic.Code
                            + " "
                            + diagnostic.Message));
            Assert.False(
                result.HasErrors,
                $"{path}: {diagnosticText}");
            Assert.Contains(
                result.Tokens,
                token =>
                    token.Kind == TermInfoSourceTokenKind.TerminalName);
        }
    }

    [Fact]
    public void TokenizationIsDeterministic()
    {
        const string source =
            "demo|alt|Description,\n"
            + "\tam, cols#80, clear=\\E[H, use=xterm,\n";

        TermInfoSourceLexResult first =
            TermInfoSourceLexer.Tokenize(
                source,
                "deterministic.ti");
        TermInfoSourceLexResult second =
            TermInfoSourceLexer.Tokenize(
                source,
                "deterministic.ti");

        Assert.Equal(
            first.Tokens.Select(TokenSignature),
            second.Tokens.Select(TokenSignature));
        Assert.Equal(
            first.Diagnostics.Select(DiagnosticSignature),
            second.Diagnostics.Select(DiagnosticSignature));
    }

    [Fact]
    public void PublicContractsValidateProgrammerArguments()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
                TermInfoSourceLexer.Tokenize(
                    (string)null!));
        Assert.Throws<ArgumentNullException>(
            () =>
                TermInfoSourceLexer.Tokenize(
                    (TextReader)null!));
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new TermInfoSourceLexerOptions(0));
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new TermInfoSourceLexerOptions(
                    TermInfoSourceLexerOptions.MaximumSupportedSourceLength + 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new TermInfoSourceSpan(
                    null,
                    -1,
                    1,
                    1,
                    0));
    }

    private static string TokenSignature(
        TermInfoSourceToken token)
    {
        ArgumentNullException.ThrowIfNull(token);

        return
            $"{token.Kind}|{token.Text}|{token.Span.SourceName}|"
            + $"{token.Span.Offset}|{token.Span.Line}|"
            + $"{token.Span.Column}|{token.Span.Length}";
    }

    private static string DiagnosticSignature(
        TermInfoSourceDiagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);

        return
            $"{diagnostic.Code}|{diagnostic.Severity}|"
            + $"{diagnostic.Message}|{diagnostic.Span?.Offset}";
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
