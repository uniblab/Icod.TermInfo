using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Source.Tests;

public sealed class S05ClassificationTests
{
    [Fact]
    public void EveryStandardShortAndLongNameMapsToRuntimeSemanticIdentity()
    {
        StringBuilder source =
            new(
                "catalog|S05 catalog mapping,\n");
        List<Action<TermInfoSourceField>> assertions = [];

        foreach (
            StandardCapabilityMetadata<BooleanCapability> metadata
            in StandardCapabilityCatalog.BooleanCapabilities)
        {
            AddBooleanExpectation(
                source,
                assertions,
                metadata.ShortName,
                metadata);
            AddBooleanExpectation(
                source,
                assertions,
                metadata.LongName,
                metadata);
        }

        foreach (
            StandardCapabilityMetadata<NumericCapability> metadata
            in StandardCapabilityCatalog.NumericCapabilities)
        {
            AddNumericExpectation(
                source,
                assertions,
                metadata.ShortName,
                metadata);
            AddNumericExpectation(
                source,
                assertions,
                metadata.LongName,
                metadata);
        }

        foreach (
            StandardCapabilityMetadata<StringCapability> metadata
            in StandardCapabilityCatalog.StringCapabilities)
        {
            AddStringExpectation(
                source,
                assertions,
                metadata.ShortName,
                metadata);
            AddStringExpectation(
                source,
                assertions,
                metadata.LongName,
                metadata);
        }

        TermInfoSourceParseResult result =
            TermInfoSourceParser.Parse(
                source.ToString(),
                "catalog.ti");

        Assert.False(
            result.HasErrors,
            FormatDiagnostics(result.Diagnostics));
        TermInfoSourceEntry entry =
            Assert.Single(result.Document.Entries);
        Assert.Equal(
            assertions.Count,
            entry.Fields.Count);

        for (int index = 0; index < assertions.Count; index++)
        {
            assertions[index](entry.Fields[index]);
        }
    }

    [Fact]
    public void ExtendedClassificationSeparatesKnownUnknownAndTermcapSpellings()
    {
        const string source =
            "extensions|S05 extended classification,\n"
            + "\tAX,\n"
            + "\tXT,\n"
            + "\tsmxx=\\E[9m,\n"
            + "\tVendorFeature#7,\n"
            + "\tco#81,\n";

        TermInfoSourceParseResult result =
            TermInfoSourceParser.Parse(source);

        Assert.False(
            result.HasErrors,
            FormatDiagnostics(result.Diagnostics));
        TermInfoSourceEntry entry =
            Assert.Single(result.Document.Entries);

        AssertClassification(
            entry,
            "AX",
            TermInfoSourceCapabilityClassification.KnownExtended);
        AssertClassification(
            entry,
            "XT",
            TermInfoSourceCapabilityClassification.KnownExtended);
        AssertClassification(
            entry,
            "smxx",
            TermInfoSourceCapabilityClassification.KnownExtended);
        AssertClassification(
            entry,
            "VendorFeature",
            TermInfoSourceCapabilityClassification.UnknownExtended);

        TermInfoSourceField termcap =
            entry.Fields.Single(
                field => field.CapabilityName == "co");
        Assert.Equal(
            TermInfoSourceCapabilityClassification.UnknownExtended,
            termcap.CapabilityClassification);
        Assert.Null(termcap.StandardNumericCapability);
        Assert.Equal("co", termcap.CanonicalCapabilityName);
    }

    [Fact]
    public void InvalidNamesAndStandardTypeMismatchesProduceDeterministicDiagnostics()
    {
        const string source =
            "bad|S05 diagnostics,\n"
            + "\tuse#1,\n"
            + "\tbad name,\n"
            + "\tam#1,\n"
            + "\tcols=12,\n"
            + "\tclear#80,\n";

        TermInfoSourceParseResult result =
            TermInfoSourceParser.Parse(
                source,
                "bad.ti");

        Assert.True(result.HasErrors);
        Assert.Equal(
            new[]
            {
                TermInfoSourceDiagnosticCodes.InvalidCapabilityName,
                TermInfoSourceDiagnosticCodes.InvalidCapabilityName,
                TermInfoSourceDiagnosticCodes.StandardCapabilityTypeMismatch,
                TermInfoSourceDiagnosticCodes.StandardCapabilityTypeMismatch,
                TermInfoSourceDiagnosticCodes.StandardCapabilityTypeMismatch,
            },
            result.Diagnostics.Select(
                diagnostic => diagnostic.Code));

        TermInfoSourceEntry entry =
            Assert.Single(result.Document.Entries);
        Assert.Equal(
            TermInfoSourceCapabilityClassification.Invalid,
            entry.Fields[0].CapabilityClassification);
        Assert.Equal(
            TermInfoSourceCapabilityClassification.Invalid,
            entry.Fields[1].CapabilityClassification);

        Assert.Equal(
            BooleanCapability.AutoRightMargin,
            entry.Fields[2].StandardBooleanCapability);
        Assert.Equal(
            NumericCapability.Columns,
            entry.Fields[3].StandardNumericCapability);
        Assert.Equal(
            StringCapability.ClearScreen,
            entry.Fields[4].StandardStringCapability);
    }

    [Fact]
    public void CancellationAndDisabledFieldsAreClassifiedWithoutApplyingSemantics()
    {
        const string source =
            "pending|S05 unresolved semantics,\n"
            + "\tcols@,\n"
            + "\t.clear#80,\n"
            + "\tuse=dumb,\n";

        TermInfoSourceParseResult result =
            TermInfoSourceParser.Parse(source);

        Assert.False(
            result.HasErrors,
            FormatDiagnostics(result.Diagnostics));
        TermInfoSourceEntry entry =
            Assert.Single(result.Document.Entries);

        TermInfoSourceField cancelled =
            entry.Fields[0];
        Assert.Equal(
            TermInfoSourceFieldKind.CancelledCapability,
            cancelled.Kind);
        Assert.Equal(
            TermInfoSourceCapabilityClassification.Standard,
            cancelled.CapabilityClassification);
        Assert.Equal(
            NumericCapability.Columns,
            cancelled.StandardNumericCapability);

        TermInfoSourceField disabled =
            entry.Fields[1];
        Assert.Equal(
            TermInfoSourceFieldKind.DisabledCapability,
            disabled.Kind);
        Assert.Equal(
            TermInfoSourceCapabilityClassification.Standard,
            disabled.CapabilityClassification);
        Assert.Equal(
            StringCapability.ClearScreen,
            disabled.StandardStringCapability);

        TermInfoSourceField use =
            entry.Fields[2];
        Assert.Equal(
            TermInfoSourceFieldKind.UseReference,
            use.Kind);
        Assert.Null(use.CapabilityClassification);
        Assert.Null(use.CanonicalCapabilityName);
    }

    [Fact]
    public void ExistingCompiledFixtureCorpusClassifiesWithoutS05Errors()
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

        foreach (
            string path
            in Directory
                .EnumerateFiles(
                    sourceRoot,
                    "*.ti",
                    SearchOption.TopDirectoryOnly)
                .OrderBy(
                    value => value,
                    StringComparer.Ordinal))
        {
            TermInfoSourceParseResult result =
                TermInfoSourceParser.Parse(
                    File.ReadAllText(path),
                    Path.GetFileName(path));

            Assert.DoesNotContain(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Code
                        == TermInfoSourceDiagnosticCodes.InvalidCapabilityName
                    || diagnostic.Code
                        == TermInfoSourceDiagnosticCodes.StandardCapabilityTypeMismatch);

            foreach (
                TermInfoSourceField field
                in result.Document.Entries.SelectMany(
                    entry => entry.Fields))
            {
                if (field.Kind == TermInfoSourceFieldKind.UseReference)
                {
                    Assert.Null(field.CapabilityClassification);
                    continue;
                }

                Assert.NotNull(field.CapabilityClassification);
            }
        }
    }

    private static void AddBooleanExpectation(
        StringBuilder source,
        ICollection<Action<TermInfoSourceField>> assertions,
        string name,
        StandardCapabilityMetadata<BooleanCapability> metadata)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(assertions);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(metadata);

        source.Append('\t').Append(name).Append(",\n");
        assertions.Add(
            field =>
            {
                Assert.Equal(
                    TermInfoSourceCapabilityClassification.Standard,
                    field.CapabilityClassification);
                Assert.Equal(metadata.ShortName, field.CanonicalCapabilityName);
                Assert.Equal(
                    TermInfoCapabilityValueKind.Boolean,
                    field.StandardValueKind);
                Assert.Equal(
                    metadata.Capability,
                    field.StandardBooleanCapability);
                Assert.Null(field.StandardNumericCapability);
                Assert.Null(field.StandardStringCapability);
            });
    }

    private static void AddNumericExpectation(
        StringBuilder source,
        ICollection<Action<TermInfoSourceField>> assertions,
        string name,
        StandardCapabilityMetadata<NumericCapability> metadata)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(assertions);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(metadata);

        source.Append('\t').Append(name).Append("#1,\n");
        assertions.Add(
            field =>
            {
                Assert.Equal(
                    TermInfoSourceCapabilityClassification.Standard,
                    field.CapabilityClassification);
                Assert.Equal(metadata.ShortName, field.CanonicalCapabilityName);
                Assert.Equal(
                    TermInfoCapabilityValueKind.Number,
                    field.StandardValueKind);
                Assert.Equal(
                    metadata.Capability,
                    field.StandardNumericCapability);
                Assert.Null(field.StandardBooleanCapability);
                Assert.Null(field.StandardStringCapability);
            });
    }

    private static void AddStringExpectation(
        StringBuilder source,
        ICollection<Action<TermInfoSourceField>> assertions,
        string name,
        StandardCapabilityMetadata<StringCapability> metadata)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(assertions);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(metadata);

        source.Append('\t').Append(name).Append("=x,\n");
        assertions.Add(
            field =>
            {
                Assert.Equal(
                    TermInfoSourceCapabilityClassification.Standard,
                    field.CapabilityClassification);
                Assert.Equal(metadata.ShortName, field.CanonicalCapabilityName);
                Assert.Equal(
                    TermInfoCapabilityValueKind.String,
                    field.StandardValueKind);
                Assert.Equal(
                    metadata.Capability,
                    field.StandardStringCapability);
                Assert.Null(field.StandardBooleanCapability);
                Assert.Null(field.StandardNumericCapability);
            });
    }

    private static void AssertClassification(
        TermInfoSourceEntry entry,
        string capabilityName,
        TermInfoSourceCapabilityClassification expected)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentException.ThrowIfNullOrWhiteSpace(capabilityName);

        TermInfoSourceField field =
            entry.Fields.Single(
                item => item.CapabilityName == capabilityName);
        Assert.Equal(expected, field.CapabilityClassification);
        Assert.Equal(capabilityName, field.CanonicalCapabilityName);
        Assert.Null(field.StandardBooleanCapability);
        Assert.Null(field.StandardNumericCapability);
        Assert.Null(field.StandardStringCapability);
        Assert.Null(field.StandardValueKind);
    }

    private static string FormatDiagnostics(
        IEnumerable<TermInfoSourceDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        return string.Join(
            "; ",
            diagnostics.Select(
                diagnostic =>
                    diagnostic.Code
                    + " "
                    + diagnostic.Message));
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
