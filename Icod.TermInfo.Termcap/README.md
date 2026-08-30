# Icod.TermInfo.Termcap

`Icod.TermInfo.Termcap` is the optional termcap interoperability layer for the
Icod.TermInfo package family.

The `1.6.0-Alpha-4` TC04 tranche retains the TC01 parser, TC02 capability
classifier, and TC03 inheritance resolver, and adds explicit conversion into the
canonical Runtime `TerminalDescription` model. It still does not render termcap
text, read `TERMCAP` or `TERMPATH`, or provide conversion commands.

The package targets `net8.0`, `net9.0`, and `net10.0` and depends only on
`Icod.TermInfo`. Existing Runtime, Source, Compiler, and Inspection package APIs
remain unchanged.

## Parsing and classification

Parse source first, then classify individual unresolved fields without mutating
them:

```csharp
TermcapSourceParseResult result = TermcapSourceParser.Parse(
    "vt|vt100|DEC VT100:am:co#80:cl=\\E[H\\E[2J:"
);

if (!result.HasErrors)
{
    TermcapSourceEntry entry = result.Document.Entries[0];
    TermcapCapabilityClassificationResult classification =
        TermcapCapabilityClassifier.Classify(entry.Fields[1]);

    Console.WriteLine(classification.Mapping?.TermInfoLongName);
}
```

`TermcapCapabilityCatalog` derives standard mappings from
`StandardCapabilityCatalog` and its recorded `TermcapCode` values rather than
maintaining a second standard capability table. Classification reports standard
capabilities, Runtime-retained obsolete termcap capabilities, adopted historical
aliases, ambiguous codes, unmapped/vendor codes, and `tc=` references. The
source syntax value kind and the Runtime mapping's expected value kind remain
separately observable.

The parser continues to preserve source spans and field order. Classification
does not itself resolve inheritance or perform conversion.

## Inheritance resolution

Resolve a parsed entry explicitly by one of its header components:

```csharp
TermcapSourceResolveResult resolved = TermcapSourceResolver.Resolve(
    result.Document,
    "vt100"
);

if (!resolved.HasErrors && resolved.Entry is not null)
{
    foreach (TermcapSourceResolvedField field in resolved.Entry.Fields)
    {
        Console.WriteLine(
            $"{field.CapabilityName} depth={field.InheritanceDepth}"
        );
    }
}
```

Local fields take precedence over inherited fields by exact two-character code.
`xx@` cancellation suppresses inherited occurrences, while period-prefixed
disabled fields do not claim a capability. Effective fields retain the original
source field, originating entry, source span, and inheritance depth. Unknown or
vendor codes are resolved by the same exact-code rules without being discarded.

`ITermcapSourceEntryProvider` supports caller-controlled lookup when a parsed
document is not the desired store. TC03 performs no process-global or file-system
discovery.

## Semantic conversion

Convert only after inheritance has been resolved:

```csharp
TermcapConversionResult converted = TermcapConverter.Convert(
    resolved.Entry
);

if (!converted.HasErrors && converted.Description is not null)
{
    TerminalDescription description = converted.Description;
    Console.WriteLine(description.Name);
}
```

TC04 maps canonical two-character capabilities to the existing Runtime enums,
preserves adopted historical aliases as observable lossless decisions, and keeps
unmapped Boolean/numeric/string fields as Runtime extended capabilities when
that is representable. Ambiguous historical codes and value-kind mismatches fail
conversion rather than being guessed.

Traditional termcap padding is translated into mandatory Runtime terminfo delay
syntax. Classic `%` operators are translated for traditional parameterized
capabilities, while `%` remains literal in ordinary non-parameterized strings.
Unsupported operators in a parameterized capability are returned as structured
conversion errors instead of being copied silently.

`TermcapConversionResult` exposes `HasErrors`, `HasLoss`, and deterministic
conversion diagnostics. Historical aliases and extended-field preservation are
observable but lossless; approximations, unsupported constructs, and
unrepresentable values set `HasLoss`.

## Resource limits

`TermcapSourceParserOptions` bounds caller-supplied source length. The default is
4 MiB and the supported upper bound is 64 MiB. Inputs beyond the configured
limit fail deterministically with a source diagnostic rather than being parsed
partially.
