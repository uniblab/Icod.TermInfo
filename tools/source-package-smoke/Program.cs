using System.Reflection;
using Icod.TermInfo;
using Icod.TermInfo.Source;

static void Require(
    bool condition,
    string message)
{
    ArgumentNullException.ThrowIfNull(message);

    if (!condition)
    {
        throw new InvalidOperationException(
            message);
    }
}

Assembly sourceAssembly =
    typeof(TermInfoSourceLexer).Assembly;
AssemblyName sourceName =
    sourceAssembly.GetName();

Require(
    sourceName.Name
        == "Icod.TermInfo.Source",
    "The source package assembly could not be loaded.");
Require(
    sourceName.Version
        == new Version(1, 0, 0, 0),
    "The source package must retain the stable 1.x assembly identity.");

const string source =
    "source-parent|Source package smoke parent,\n"
    + "\tlines#24,\n"
    + "package-smoke|Package smoke source,\n"
    + "\tam,\n"
    + "\tcols#0100,\n"
    + "\tclear=\\E[H,\n"
    + "\tAX,\n"
    + "\tuse=source-parent,\n";

TermInfoSourceLexResult lexed =
    TermInfoSourceLexer.Tokenize(
        source,
        "package-smoke.ti");
Require(
    !lexed.HasErrors,
    "The source package could not tokenize representative source.");
Require(
    lexed.Tokens.Any(
        token =>
            token.Kind == TermInfoSourceTokenKind.BooleanCapability
            && token.Text == "am"),
    "The source package did not expose Boolean capability lexing.");
Require(
    lexed.Tokens.Any(
        token =>
            token.Kind == TermInfoSourceTokenKind.UseReference
            && token.Span.SourceName == "package-smoke.ti"),
    "The source package did not expose use= lexing with source locations.");

TermInfoSourceNumericValueResult numeric =
    TermInfoSourceValueParser.ParseNumeric(
        lexed.Tokens.Single(
            token =>
                token.Kind == TermInfoSourceTokenKind.NumericCapability
                && token.Text == "cols#0100"));
Require(
    !numeric.HasErrors
        && numeric.Value == 64,
    "The source package did not decode octal numeric source values.");

TermInfoSourceStringValueResult text =
    TermInfoSourceValueParser.ParseString(
        lexed.Tokens.Single(
            token =>
                token.Kind == TermInfoSourceTokenKind.StringCapability));
Require(
    !text.HasErrors
        && text.Value == "\x1b[H",
    "The source package did not decode string source escapes.");

TermInfoSourceParseResult parsed =
    TermInfoSourceParser.Parse(
        source,
        "package-smoke.ti");
Require(
    !parsed.HasErrors,
    "The source package could not parse representative unresolved source.");
TermInfoSourceEntry parsedEntry =
    parsed.Document.Entries.Single(
        entry => entry.CanonicalName == "package-smoke");
Require(
    parsedEntry.CanonicalName == "package-smoke"
        && parsedEntry.Fields.Count == 5,
    "The source package did not expose the S04 unresolved entry model.");
TermInfoSourceField standardBoolean =
    parsedEntry.Fields.Single(
        field => field.CapabilityName == "am");
Require(
    standardBoolean.CapabilityClassification
            == TermInfoSourceCapabilityClassification.Standard
        && standardBoolean.StandardBooleanCapability
            == BooleanCapability.AutoRightMargin
        && standardBoolean.CanonicalCapabilityName == "am",
    "The S05 model did not map a standard capability to its runtime identity.");
TermInfoSourceField knownExtended =
    parsedEntry.Fields.Single(
        field => field.CapabilityName == "AX");
Require(
    knownExtended.CapabilityClassification
            == TermInfoSourceCapabilityClassification.KnownExtended
        && knownExtended.CanonicalCapabilityName == "AX",
    "The S05 model did not classify a known extended capability.");
Require(
    parsedEntry.Fields.Single(
            field =>
                field.Kind == TermInfoSourceFieldKind.NumericCapability)
        .NumericValue == 64,
    "The S04 model did not retain decoded numeric source semantics.");
Require(
    parsedEntry.Fields.Single(
            field =>
                field.Kind == TermInfoSourceFieldKind.UseReference)
        .ReferenceName == "source-parent",
    "The S04 model did not retain the use= reference.");

TermInfoSourceResolveResult resolved =
    TermInfoSourceResolver.Resolve(
        parsed.Document,
        "package-smoke");
Require(
    !resolved.HasErrors,
    "The S07 resolver could not resolve representative use= inheritance.");
TermInfoSourceResolvedEntry resolvedEntry =
    resolved.Entry
    ?? throw new InvalidOperationException(
        "The S07 resolver returned no entry without reporting an error.");
Require(
    resolvedEntry.GetNumber(NumericCapability.Columns) == 64
        && resolvedEntry.GetNumber(NumericCapability.Lines) == 24
        && resolvedEntry.GetBoolean(BooleanCapability.AutoRightMargin)
        && resolvedEntry.GetString(StringCapability.ClearScreen) == "\x1b[H",
    "The S07 resolved source did not preserve local and inherited capabilities.");

TerminalDescription materialized =
    resolvedEntry.ToTerminalDescription();
Require(
    materialized.Name == "package-smoke"
        && materialized.Description == "Package smoke source"
        && materialized.GetNumber( NumericCapability.Columns ) == 64
        && materialized.GetNumber( NumericCapability.Lines ) == 24
        && materialized.GetBoolean( BooleanCapability.AutoRightMargin )
        && materialized.GetString( StringCapability.ClearScreen ) == "\x1b[H"
        && materialized.TryGetExtendedBoolean(
            "AX",
            out bool ax
        )
        && ax,
    "The S08 materialized description did not preserve resolved source semantics."
);

Assembly runtimeAssembly =
    typeof(TerminalDescription).Assembly;
Require(
    runtimeAssembly.GetName().Name
        == "Icod.TermInfo",
    "The transitive Icod.TermInfo dependency is unavailable.");
Require(
    runtimeAssembly.GetName().Version
        == new Version(1, 0, 0, 0),
    "The runtime package must retain the stable 1.x assembly identity.");

TerminalDescription dumb =
    TerminalDatabase.BuiltIn.Load(
        "dumb");
Require(
    dumb.Name
        == "dumb",
    "The transitive runtime package is not usable.");

Console.WriteLine(
    "Icod.TermInfo.Source package smoke test passed.");
