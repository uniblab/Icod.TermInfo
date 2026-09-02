# Icod.TermInfo.Termcap

`Icod.TermInfo.Termcap` is the optional termcap interoperability layer for the
Icod.TermInfo package family.

Version 1.7.0 retains the TC01 parser, TC02 capability classifier, TC03
inheritance resolver, TC04 semantic converter, TC05 reverse renderer, TC06
explicit acquisition APIs, and the TC08-frozen public/package contract used by
the TC07 conversion-command composition.

## 1.8 release-candidate status

`1.8.0-Alpha-8` carries the TC08-frozen Termcap contract unchanged into the
complete stable-intended 1.8 release. Planning adds no Termcap API, behavior, or
dependency; the package remains Runtime-only with three target frameworks and
assembly identity `1.0.0.0`.

## 1.7 release status

Version 1.7.0 carries the TC08-frozen Termcap API and Runtime-only dependency
unchanged into the stable 1.7 release. Relative terminfo source synthesis does
not alter termcap parsing, conversion, rendering, acquisition, or package
direction.

## Install

```text
dotnet add package Icod.TermInfo.Termcap --version 1.7.0
```

The package targets `net8.0`, `net9.0`, and `net10.0` and depends only on
`Icod.TermInfo`. Command-only dependencies on Source or Inspection live in the
executable projects, so existing Runtime, Source, Compiler, Inspection, and
Termcap package dependency boundaries remain unchanged.

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

## Reverse rendering

Preflight representability separately when desired, or render directly:

```csharp
TermcapRepresentabilityResult analysis = TermcapRenderer.Analyze(description);

if (analysis.IsRepresentable)
{
    TermcapRenderResult rendered = TermcapRenderer.Render(description);
    Console.Write(rendered.Text);
}
```

TC05 reverses canonical Runtime standard capabilities through the existing
Runtime-derived termcap catalog and emits a code only when TC02 would classify it
back to the same Runtime identity. Representable two-character extended fields
remain extended; mapped, reserved, or otherwise ambiguous names fail preflight.

Strings use deterministic historical-safe escaping. Literal colon is emitted as
`\072`, traditional leading padding is recovered from TC04's mandatory Runtime
delay suffix, and parameterized standard capabilities are rendered only when the
Runtime program is exactly expressible by the adopted TC04 classic operator
subset. No partial termcap text is returned when representability fails.

Fields are emitted in stable ordinal code order. `TermcapRenderOptions` controls
the preferred physical line width; wrapping occurs only between complete fields
and uses an unindented continuation so the TC01 logical-record parser sees no
synthetic whitespace.

TC05 itself performs no environment or filesystem discovery. TC06 adds that
behavior only through the explicit acquisition API below.

## Explicit acquisition

Acquisition is a separate opt-in operation. Inline-only source needs no filesystem
provider:

```csharp
TermcapAcquisitionResult acquired = TermcapAcquirer.Acquire(
    "vt100",
    new TermcapAcquisitionOptions(
        inlineTermcap: "vt100|DEC VT100:am:co#80:"
    )
);
```

To snapshot the historical environment variables, supply both provider seams
explicitly:

```csharp
TermcapAcquisitionOptions options =
    TermcapAcquisitionOptions.FromEnvironment(
        new SystemTermcapEnvironmentProvider(),
        new SystemTermcapFileProvider(),
        TermcapDefaultPathPolicy.Ncurses
    );

TermcapAcquisitionResult acquired =
    TermcapAcquirer.Acquire("vt100", options);
```

The environment factory snapshots `TERMCAP`, `TERMPATH`, and `HOME`; it does not
modify or compose Runtime `TERMINFO` discovery. Historical slash-rooted `TERMCAP`
is treated as a database path, while another non-empty value is treated as inline
source. `TERMPATH` databases are searched in order. Missing files are clean
search misses; parser, resolver, conversion, and provider failures remain visible.

`TermcapDefaultPathPolicy.None` is the default. Selecting
`TermcapDefaultPathPolicy.Ncurses` appends `/etc/termcap`,
`/usr/share/misc/termcap`, and then `$HOME/.termcap` when a home directory was
supplied. File-backed acquisition requires an `ITermcapFileProvider`, and
process-environment access occurs only through an `ITermcapEnvironmentProvider`.
This keeps ordinary tests deterministic and leaves existing Runtime discovery
unchanged.

## Conversion commands

TC07 adds two `net10.0` command composition projects without moving command
policy into this reusable package:

```text
captoinfo [OPTION]... [FILE]...
infotocap [OPTION]... FILE...
```

`captoinfo` composes the termcap parser/resolver/converter with Inspection's
effective terminfo source renderer. `infotocap` composes the existing terminfo
Source parser/resolver with `TermcapRenderer`. Both support `-w WIDTH`, help,
version reporting, `--`, and `-` for standard input.

With no file operand, `captoinfo` uses command-level `TERM` plus an explicit
TC06 snapshot of `TERMCAP`, `TERMPATH`, and `HOME` with the ncurses default path
policy. This does not join or replace Runtime `TERMINFO` discovery.

Both commands emit effective resolved state. They do not reconstruct comments,
source formatting, cancellations/disabled fields, or `tc=` / `use=` ancestry.
Representational loss and incompatibility remain visible through diagnostics;
`infotocap` does not emit a silently lossy substitute when TC05 preflight fails.

The installable NuGet tool continues to expose only the unambiguous
`icod-terminfo` launcher:

```text
icod-terminfo captoinfo ...
icod-terminfo infotocap ...
```

Standalone release archives expose `captoinfo` and `infotocap` directly beside
`tic`, `infocmp`, and `toe`.

## Resource limits

`TermcapSourceParserOptions` bounds caller-supplied source length. The default is
4 MiB and the supported upper bound is 64 MiB. Inputs beyond the configured
limit fail deterministically with a source diagnostic rather than being parsed
partially.
