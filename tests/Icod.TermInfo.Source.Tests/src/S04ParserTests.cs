using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Source.Tests;

public sealed class S04ParserTests
{
    [Fact]
    public void RepresentativeDocumentPreservesUnresolvedEntrySemantics()
    {
        const string source =
            "# leading comment\n"
            + "demo|demo-alt|Demonstration terminal,\n"
            + "\tam,\n"
            + "\tcols#0100,\n"
            + "\tclear=\\E[H,\n"
            + "\tsmkx@,\n"
            + "\tuse=xterm-256color,\n"
            + "\t.ind=\\E[%p1%dS,\n"
            + "second|Second terminal,\n"
            + "\txenl,\n";

        TermInfoSourceParseResult result =
            TermInfoSourceParser.Parse(
                source,
                "representative.ti");

        Assert.False(result.HasErrors);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(2, result.Document.Entries.Count);
        Assert.Contains(
            result.Document.Tokens,
            token =>
                token.Kind == TermInfoSourceTokenKind.Comment
                && token.Text == "# leading comment");

        TermInfoSourceEntry first =
            result.Document.Entries[0];
        Assert.Equal("demo", first.CanonicalName);
        Assert.Equal(
            new[] { "demo-alt" },
            first.Aliases);
        Assert.Equal(
            "Demonstration terminal",
            first.Description);
        Assert.Equal(
            "representative.ti",
            first.Span.SourceName);
        Assert.Equal(2, first.Span.Line);

        Assert.Equal(
            new[]
            {
                TermInfoSourceFieldKind.BooleanCapability,
                TermInfoSourceFieldKind.NumericCapability,
                TermInfoSourceFieldKind.StringCapability,
                TermInfoSourceFieldKind.CancelledCapability,
                TermInfoSourceFieldKind.UseReference,
                TermInfoSourceFieldKind.DisabledCapability,
            },
            first.Fields.Select(field => field.Kind));

        TermInfoSourceField boolean = first.Fields[0];
        Assert.Equal("am", boolean.CapabilityName);
        Assert.Null(boolean.ReferenceName);

        TermInfoSourceField numeric = first.Fields[1];
        Assert.Equal("cols", numeric.CapabilityName);
        Assert.Equal(64, numeric.NumericValue);

        TermInfoSourceField text = first.Fields[2];
        Assert.Equal("clear", text.CapabilityName);
        Assert.Equal("\x1b[H", text.StringValue);

        TermInfoSourceField cancelled = first.Fields[3];
        Assert.Equal("smkx", cancelled.CapabilityName);

        TermInfoSourceField inherited = first.Fields[4];
        Assert.Null(inherited.CapabilityName);
        Assert.Equal("xterm-256color", inherited.ReferenceName);

        TermInfoSourceField disabled = first.Fields[5];
        Assert.Equal("ind", disabled.CapabilityName);
        Assert.Equal(
            ".ind=\\E[%p1%dS",
            disabled.Text);

        TermInfoSourceEntry second =
            result.Document.Entries[1];
        Assert.Equal("second", second.CanonicalName);
        Assert.Single(second.Fields);
        Assert.Equal(
            "xenl",
            second.Fields[0].CapabilityName);
    }

    [Fact]
    public void FinalHeaderComponentRetainsAliasAndDescriptionMeaning()
    {
        TermInfoSourceParseResult result =
            TermInfoSourceParser.Parse(
                "demo|shortname,\n"
                + "\tam,\n");

        Assert.False(result.HasErrors);
        TermInfoSourceEntry entry =
            Assert.Single(result.Document.Entries);
        Assert.Equal(
            new[] { "shortname" },
            entry.Aliases);
        Assert.Equal("shortname", entry.Description);
    }

    [Fact]
    public void DuplicateAndUseFieldsRemainInSourceOrder()
    {
        TermInfoSourceParseResult result =
            TermInfoSourceParser.Parse(
                "demo|Description,\n"
                + "\tam,\n"
                + "\tam,\n"
                + "\tuse=first,\n"
                + "\tuse=second,\n"
                + "\tam@,\n");

        Assert.False(result.HasErrors);
        TermInfoSourceEntry entry =
            Assert.Single(result.Document.Entries);
        Assert.Equal(5, entry.Fields.Count);
        Assert.Equal("am", entry.Fields[0].CapabilityName);
        Assert.Equal("am", entry.Fields[1].CapabilityName);
        Assert.Equal("first", entry.Fields[2].ReferenceName);
        Assert.Equal("second", entry.Fields[3].ReferenceName);
        Assert.Equal(
            TermInfoSourceFieldKind.CancelledCapability,
            entry.Fields[4].Kind);
    }

    [Fact]
    public void ValueErrorsRetainRawFieldsAndFlowIntoParseDiagnostics()
    {
        const string source =
            "demo|Description,\n"
            + "\tcols#09,\n"
            + "\tclear=^,\n";

        TermInfoSourceParseResult result =
            TermInfoSourceParser.Parse(
                source,
                "bad-values.ti");

        Assert.True(result.HasErrors);
        Assert.Equal(
            new[]
            {
                TermInfoSourceDiagnosticCodes.InvalidNumericValue,
                TermInfoSourceDiagnosticCodes.IncompleteControlEscape,
            },
            result.Diagnostics.Select(diagnostic => diagnostic.Code));

        TermInfoSourceEntry entry =
            Assert.Single(result.Document.Entries);
        Assert.Equal(2, entry.Fields.Count);
        Assert.Equal("cols", entry.Fields[0].CapabilityName);
        Assert.Null(entry.Fields[0].NumericValue);
        Assert.Equal("cols#09", entry.Fields[0].Text);
        Assert.Equal("clear", entry.Fields[1].CapabilityName);
        Assert.Null(entry.Fields[1].StringValue);
        Assert.Equal("clear=^", entry.Fields[1].Text);
    }

    [Fact]
    public void LexicalDiagnosticsAndMalformedHeadersRemainRecoverable()
    {
        TermInfoSourceParseResult result =
            TermInfoSourceParser.Parse(
                "|alias||,\n"
                + "\tuse=,\n");

        Assert.True(result.HasErrors);
        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code
                    == TermInfoSourceDiagnosticCodes.EmptyTerminalName);
        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code
                    == TermInfoSourceDiagnosticCodes.MissingUseReference);

        TermInfoSourceEntry entry =
            Assert.Single(result.Document.Entries);
        Assert.Equal(string.Empty, entry.CanonicalName);
        Assert.Contains("alias", entry.Aliases);
        TermInfoSourceField inherited =
            Assert.Single(entry.Fields);
        Assert.Equal(
            TermInfoSourceFieldKind.UseReference,
            inherited.Kind);
        Assert.Equal(string.Empty, inherited.ReferenceName);
    }

    [Fact]
    public void CombinedDiagnosticsAreReturnedInSourceOrder()
    {
        TermInfoSourceParseResult result =
            TermInfoSourceParser.Parse(
                "demo|Description,,\n"
                + "\tcols#09,\n");

        Assert.True(result.HasErrors);
        Assert.Equal(
            new[]
            {
                TermInfoSourceDiagnosticCodes.EmptyField,
                TermInfoSourceDiagnosticCodes.InvalidNumericValue,
            },
            result.Diagnostics.Select(diagnostic => diagnostic.Code));
        Assert.True(
            result.Diagnostics[0].Span!.Offset
                < result.Diagnostics[1].Span!.Offset);
    }

    [Fact]
    public void TextReaderUsesTheSameUnresolvedModel()
    {
        using StringReader reader =
            new(
                "demo|Description,\n"
                + "\tcols#0x50,\n");

        TermInfoSourceParseResult result =
            TermInfoSourceParser.Parse(
                reader,
                "reader.ti");

        Assert.False(result.HasErrors);
        TermInfoSourceEntry entry =
            Assert.Single(result.Document.Entries);
        TermInfoSourceField field =
            Assert.Single(entry.Fields);
        Assert.Equal(80, field.NumericValue);
        Assert.Equal("reader.ti", field.Span.SourceName);
    }

    [Fact]
    public void ExistingCompiledFixtureSourceCorpusParsesIntoEntries()
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
            TermInfoSourceParseResult result =
                TermInfoSourceParser.Parse(
                    File.ReadAllText(path),
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
            Assert.NotEmpty(result.Document.Entries);
            Assert.All(
                result.Document.Entries,
                entry =>
                    Assert.False(
                        string.IsNullOrWhiteSpace(
                            entry.CanonicalName)));
        }
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
