# Icod.TermInfo.Termcap

`Icod.TermInfo.Termcap` is the optional termcap interoperability layer for the
Icod.TermInfo package family.

The `1.6.0-Alpha-3` TC03 tranche retains the TC01 parser and TC02 capability
classifier and adds bounded, deterministic `tc=` inheritance and cancellation.
It still does not construct `TerminalDescription` values, read `TERMCAP` or
`TERMPATH`, or provide conversion commands.

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

## Resource limits

`TermcapSourceParserOptions` bounds caller-supplied source length. The default is
4 MiB and the supported upper bound is 64 MiB. Inputs beyond the configured
limit fail deterministically with a source diagnostic rather than being parsed
partially.
