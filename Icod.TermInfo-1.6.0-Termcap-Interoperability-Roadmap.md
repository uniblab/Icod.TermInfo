# Icod.TermInfo 1.6.0 — Termcap Interoperability Roadmap

**Development line:** `1.6.0`
**Initial development version:** `1.6.0-Alpha-1`
**Current development version:** `1.6.0-Alpha-6`
**Stable assembly version:** `1.0.0.0`
**Status:** Implementation in progress — TC06 explicit termcap acquisition
**Primary change:** Add explicit termcap parsing, semantic mapping, conversion, acquisition, and tools without changing the frozen Runtime, Source, Compiler, or Inspection contracts.

---

## 1. Release objective

Version 1.6.0 adds historical termcap interoperability as an opt-in package and
tool family. It does not change the primary terminfo representation or the
existing runtime-discovery contract.

The new reusable package is:

```text
Icod.TermInfo.Termcap
    -> Icod.TermInfo
```

The package boundary is deliberate. Termcap is a separate historical source
format with its own two-character vocabulary, `tc=` inheritance, colon-delimited
records, and environment conventions. Adding those concepts directly to the
frozen `Icod.TermInfo.Source` public surface would blur the already-released
terminfo source contract.

The existing dependency graph remains valid:

```text
Icod.TermInfo                         stable runtime

Icod.TermInfo.Source
    -> Icod.TermInfo

Icod.TermInfo.Compiler
    -> Icod.TermInfo.Source
    -> Icod.TermInfo

Icod.TermInfo.Inspection
    -> Icod.TermInfo.Source
    -> Icod.TermInfo

Icod.TermInfo.Termcap
    -> Icod.TermInfo
```

No existing package SHALL acquire a dependency on `Icod.TermInfo.Termcap` merely
because 1.6 adds termcap compatibility.

---

## 2. Compatibility and API rules

The following released public API baselines remain frozen:

- Runtime 1.0;
- Source 1.1;
- Compiler 1.2;
- Inspection 1.4.

The existing `tic`, `infocmp`, and `toe` command semantics remain frozen until a
1.6 tranche explicitly and additively composes termcap functionality.

The `Icod.TermInfo.Tools` router remains distribution-only. TC01-TC06 SHALL NOT
add a router command. Conversion commands enter only in TC07 after the reusable
conversion engines exist.

All coordinated projects continue to consume the single
`IcodTermInfoSuiteVersion` value. The four existing reusable libraries and the
new Termcap assembly retain `AssemblyVersion` `1.0.0.0` throughout the 1.x line.

---

## 3. Adopted termcap source contract

The 1.6 implementation targets the conventional BSD/GNU termcap source model:

- comments begin with `#` at the start of a physical line;
- a backslash immediately before a physical newline removes only that backslash/newline pair; following indentation remains ordinary source content;
- blank lines are ignored;
- a terminal description is logically one record;
- backslash followed by newline continues a description;
- header names are separated by `|` and terminated by `:`;
- capability fields are separated by `:`;
- capability names are two characters;
- Boolean fields are written as `xx`;
- numeric fields are written as `xx#number`, accepting non-negative decimal plus BSD-compatible C-style octal and hexadecimal forms;
- string fields are written as `xx=value`;
- cancellation is written as `xx@`;
- period-prefixed fields such as `.cr=...` are retained as disabled source fields for BSD-compatible database input;
- inheritance is written as `tc=name` and must be the final capability;
- a literal colon in a string is represented by octal `\072`, not by relying on
  `\:` to hide a field separator;
- control notation and historical backslash escapes are decoded explicitly.

Ordinary CI SHALL use checked-in fixtures and SHALL NOT require a host termcap or
ncurses installation.

---

## 4. Tranche sequence

| Tranche | Development version | Primary gate |
|---|---|---|
| **TC01** | `1.6.0-Alpha-1` | Separate package, unresolved model, bounded conventional termcap parser |
| **TC02** | `1.6.0-Alpha-2` | Centralized two-character capability metadata and type classification |
| **TC03** | `1.6.0-Alpha-3` | Deterministic `tc=` inheritance, cancellation, cycle/depth handling |
| **TC04** | `1.6.0-Alpha-4` | Termcap → canonical `TerminalDescription` conversion with explicit loss diagnostics |
| **TC05** | `1.6.0-Alpha-5` | Reverse representability and deterministic termcap rendering |
| **TC06** | `1.6.0-Alpha-6` | Explicit opt-in `TERMCAP` / `TERMPATH` acquisition |
| **TC07** | `1.6.0-Alpha-7` | `captoinfo` / `infotocap` command integration and router/archive distribution |
| **TC08** | `1.6.0-Alpha-8` | Corpus, differential validation, hostile-input audit, API/package/CLI freeze |

The development suffix may advance beyond the listed alpha number if a tranche
requires a corrective follow-up. The tranche meanings and gates, not the number
of prerelease builds, are authoritative.

---

## 5. TC01 — Termcap package and parser foundation

**Development version:** `1.6.0-Alpha-1`

TC01 establishes `Icod.TermInfo.Termcap` and its test project.

The package SHALL:

- target `net8.0;net9.0;net10.0`;
- use C# 13;
- reference only `Icod.TermInfo`;
- remain opt-in for ordinary Runtime/Source/Compiler/Inspection consumers;
- carry coordinated `Version` and `PackageVersion`;
- retain `AssemblyVersion` `1.0.0.0`;
- produce `.nupkg` and `.snupkg` artifacts when packed;
- participate in the ordinary solution build/test matrix.

The initial parser SHALL produce a termcap-specific unresolved model equivalent
in responsibility to:

```text
TermcapSourceParser
TermcapSourceParseResult
TermcapSourceDocument
TermcapSourceEntry
TermcapSourceField
TermcapSourceSpan
TermcapSourceDiagnostic
```

TC01 SHALL parse and preserve:

- complete ordered header components;
- Boolean, numeric, string, cancellation, and `tc=` fields;
- source order;
- source spans;
- continuation lines;
- comments and blank lines;
- conventional string/control/octal escapes;
- BSD-style period-prefixed disabled capability fields.

TC01 SHALL enforce bounded input. The default source limit is 4 MiB and the
largest caller-selectable limit is 64 MiB.

TC01 SHALL diagnose malformed capability names, malformed field forms, invalid
or out-of-range numeric values, unsafe NUL string values, incomplete escapes,
missing `tc=` targets, and a `tc=` reference which is not final.

TC01 deliberately SHALL NOT:

- decide which header component is the canonical terminfo name or prose
  description;
- map two-character capability codes into Runtime enums;
- resolve `tc=` references;
- apply inherited cancellation/precedence;
- construct `TerminalDescription`;
- read files, `TERMCAP`, `TERMPATH`, or process-global environment;
- modify Runtime, Source, Compiler, or Inspection public APIs.

**Gate TC01:** representative conventional termcap records parse deterministically
into the unresolved model on all three target frameworks, malformed input yields
bounded source diagnostics, and the new package builds without changing any
existing reusable-package API baseline.

**Implementation record:**
[`docs/1.6.0-TC01-TERMCAP-PACKAGE-AND-PARSER-FOUNDATION.md`](docs/1.6.0-TC01-TERMCAP-PACKAGE-AND-PARSER-FOUNDATION.md)

---

## 6. TC02 — Capability metadata and semantic classification

**Development version:** `1.6.0-Alpha-2`

TC02 establishes one authoritative mapping from termcap's two-character codes to
the existing Runtime standard capability metadata.

The public classification surface is centered on:

```text
TermcapCapabilityCatalog
TermcapStandardCapabilityMapping
TermcapCapabilityClassifier
TermcapCapabilityClassificationResult
TermcapCapabilityClassification
```

Standard mappings SHALL be derived directly from
`StandardCapabilityCatalog.BooleanCapabilities`, `.NumericCapabilities`, and
`.StringCapabilities`, using each Runtime metadata record's existing
`TermcapCode`. TC02 SHALL NOT introduce a second hand-maintained table of
standard capability identities, short names, long names, indexes, or value
kinds.

Each mapping SHALL retain the exact Runtime enum identity and expose:

- the accepted two-character termcap code;
- the Runtime metadata's canonical termcap code;
- Boolean, numeric, or string value kind;
- compiled-table index;
- terminfo short and long names;
- whether the Runtime record is an obsolete `OT...` compatibility capability;
- whether the accepted code is an adopted obsolete non-standard alias.

TC02 adopts direct obsolete alias translations from the selected ncurses
`captoinfo` compatibility baseline for AT&T, XENIX, Tektronix, and IRIX names.
The alias table SHALL contain only alias code, canonical termcap code, and
historical origin. Alias targets SHALL resolve through the Runtime-derived
canonical mappings. More complex historical transformations remain TC04
conversion policy.

Classification SHALL distinguish:

```text
Standard
ObsoleteStandard
ObsoleteAlias
Ambiguous
Unmapped
Reference
```

For active Boolean, numeric, and string fields, classification SHALL expose both
the source-syntax value kind and the Runtime mapping's expected value kind. A
unique target remains identifiable when the source uses the wrong syntactic
kind, with the mismatch reported explicitly rather than changing parser syntax.
Cancellation and disabled fields may retain a semantic target without inventing
a source value kind.

When one code has multiple Runtime/compatibility meanings, source value kind MAY
disambiguate it only when exactly one candidate has the matching kind. Otherwise
the result SHALL remain `Ambiguous` and expose every candidate in deterministic
order. Unknown/vendor fields SHALL remain `Unmapped` with the original
`TermcapSourceField` preserved.

No conversion occurs merely because a field is classified. TC02 SHALL NOT
resolve `tc=`, apply inherited cancellation or precedence, construct
`TerminalDescription`, synthesize vendor compatibility transformations, inspect
process-global termcap configuration, or add command/router behavior.

**Gate TC02:** every adopted standard termcap code maps to the same semantic
Runtime capability identity used by compiled terminfo and built-in profiles,
obsolete compatibility names and type mismatches are explicit, ambiguous codes
remain deterministic, and unknown/vendor fields remain explicit.

**Implementation record:**
[`docs/1.6.0-TC02-CAPABILITY-METADATA-AND-CLASSIFICATION.md`](docs/1.6.0-TC02-CAPABILITY-METADATA-AND-CLASSIFICATION.md)

---

## 7. TC03 — `tc=` inheritance and cancellation

**Development version:** `1.6.0-Alpha-3`

TC03 adds a termcap-specific resolver above the unresolved parser. The public
resolver surface is centered on:

```text
ITermcapSourceEntryProvider
TermcapSourceResolverOptions
TermcapSourceResolver
TermcapSourceResolveResult
TermcapSourceResolvedEntry
TermcapSourceResolvedField
```

Document-backed lookup SHALL match TC01 header components case-sensitively in
source order without prematurely assigning canonical-name, alias, or prose
description semantics. A caller-supplied provider MAY acquire entries from any
caller-controlled store, but provider exceptions propagate and clean misses are
reported as resolver diagnostics. TC03 SHALL NOT inspect files, `TERMCAP`,
`TERMPATH`, or process-global database configuration.

Resolution SHALL apply local fields first and then the inherited description.
Within one entry, the first occurrence of an exact two-character capability code
claims that code. An active local field supplies the value; `xx@` claims the code
without supplying a value and therefore suppresses inherited occurrences.
Period-prefixed disabled fields do not claim capability state. Unknown/vendor
codes participate in the same exact-code precedence rules without requiring a
TC02 Runtime mapping.

Effective inherited fields SHALL retain the original `TermcapSourceField`, the
`TermcapSourceEntry` which supplied them, and their inheritance depth. The
resolved field list therefore preserves source provenance while cancellation
materializes as absence. No Runtime conversion occurs merely because inheritance
has been resolved.

`TermcapSourceResolverOptions` SHALL default to 64 inheritance edges and SHALL
reject caller-selected bounds above 256. The resolver SHALL report deterministic
diagnostics for a missing source entry, direct or indirect cycle, and exceeded
inheritance-depth bound. The failing `tc=` source span SHALL be retained whenever
the failure occurs on an inheritance edge.

The resolver SHALL NOT route termcap through `TermInfoSourceResolver` merely
because both `tc=` and `use=` express inheritance. Their lookup, precedence,
cancellation, diagnostics, and source models remain independently testable.

**Gate TC03:** local-over-inherited precedence, multi-level cancellation,
caller-supplied lookup, missing references, cycles, depth limits, and source
provenance resolve or fail deterministically while effective fields remain in
termcap source form.

**Implementation record:**
[`docs/1.6.0-TC03-TERMCAP-INHERITANCE-AND-CANCELLATION.md`](docs/1.6.0-TC03-TERMCAP-INHERITANCE-AND-CANCELLATION.md)

---

## 8. TC04 — Termcap to terminfo semantic conversion

**Development version:** `1.6.0-Alpha-4`

TC04 converts a TC03-resolved termcap entry into the canonical immutable Runtime
model. The public conversion surface is centered on:

```text
TermcapConverter
TermcapConversionResult
TermcapConversionDiagnostic
TermcapConversionDiagnosticCodes
TermcapConversionDiagnosticSeverity
TermcapConversionDecision
```

Header interpretation SHALL be deterministic. The first header component is the
canonical Runtime name. Subsequent components are aliases except when the final
component contains whitespace, in which case it is the verbose description. A
final component without whitespace remains an alias rather than causing a
synthetic description. Duplicate header identities SHALL be ignored only with an
explicit approximation diagnostic.

Conversion SHALL distinguish:

```text
Exact
HistoricalAlias
Extended
Approximation
Unsupported
Unrepresentable
```

Canonical TC02 mappings, including Runtime-retained `OT...` compatibility
capabilities, SHALL materialize directly through the existing Runtime enums.
Adopted obsolete aliases SHALL map to their selected canonical Runtime identity
and remain observable as lossless `HistoricalAlias` decisions. Ambiguous codes
and source/mapping value-kind mismatches SHALL fail rather than being guessed.

An unmapped two-character Boolean, numeric, or string field SHALL be preserved as
a Runtime extended capability when its exact name does not collide with a
standard terminfo short name. This is an observable but lossless `Extended`
decision. A colliding name is unsupported because Runtime extended capabilities
may not shadow standard names.

When two effective termcap codes map to the same Runtime capability identity, the
higher-priority TC03 field SHALL win. The lower-priority field SHALL be ignored
with an explicit approximation diagnostic rather than overwriting the earlier
semantic value.

Termcap strings SHALL NOT be copied blindly into Runtime. Traditional leading
termcap padding SHALL be moved to an equivalent mandatory terminfo `$<.../>`
delay suffix. TC04 SHALL translate the classic BSD parameter operators `%%`,
`%d`, `%2`, `%3`, `%.`, `%+x`, `%>xy`, `%r`, `%i`, `%n`, `%B`, and `%D` into
the Runtime terminfo parameter language for adopted one- and two-numeric-parameter
capability profiles. `%02` and `%03` SHALL be accepted as compatibility spellings.
Fixed-width `%2` / `%3` execution SHALL retain BSD-style modulo 100 / 1000 and
zero-padding semantics. A recognizable parameter program on a capability outside
the adopted profile set, or any unsupported `%` operator within a supported
profile, SHALL fail explicitly rather than being silently passed through.

Loss SHALL be reported through `TermcapConversionResult` and SHALL NOT be
silently hidden. `HistoricalAlias` and `Extended` are observable but lossless;
`Approximation`, `Unsupported`, and `Unrepresentable` set `HasLoss`. An error
prevents publication of a partial `TerminalDescription`. Diagnostics SHALL retain
the originating source entry, effective source field when available, and source
span.

The resulting `TerminalDescription` SHALL use the same standard capability enums
and extended-capability storage used by every other Runtime acquisition path.
Termcap-specific source state SHALL NOT leak into `TerminalDescription`. TC04
SHALL NOT inspect `TERMCAP`, `TERMPATH`, conventional database paths, or add
command/router behavior.

**Gate TC04:** representative resolved termcap entries materialize into semantic
Runtime descriptions; inherited cancellation, standard mappings, adopted aliases,
unmapped extended fields, classic parameter programs, and padding retain their
adopted semantics; and every non-exact or failed decision remains observable.

**Implementation record:**
[`docs/1.6.0-TC04-TERMCAP-SEMANTIC-CONVERSION.md`](docs/1.6.0-TC04-TERMCAP-SEMANTIC-CONVERSION.md)

---

## 9. TC05 — Reverse conversion and rendering

**Development version:** `1.6.0-Alpha-5`

TC05 supplies the reverse interoperability path for descriptions which can be
represented by termcap.

The public reverse-rendering surface is centered on:

```text
TermcapRenderer
TermcapRenderOptions
TermcapRepresentabilityResult
TermcapRenderResult
TermcapRenderDiagnostic
TermcapRenderDiagnosticCodes
TermcapRenderDiagnosticSeverity
```

`TermcapRenderer.Analyze` SHALL complete representability preflight without
emitting text. `TermcapRenderer.Render` SHALL perform the same preflight and
SHALL NOT publish partial text when any error would require guessing or semantic
loss.

It SHALL:

- determine representability before emitting text;
- map canonical Runtime capabilities to adopted two-character termcap codes;
- report capabilities with no faithful termcap representation;
- encode strings deterministically with historical-safe escapes;
- use `\072` for literal colon bytes;
- avoid synthesizing silent approximations;
- produce stable field ordering and wrapping;
- support semantic terminfo → termcap → parse/resolve round trips where lossless.

Reverse standard mapping SHALL use the existing Runtime-derived TC02 catalog
rather than introducing a second capability table. A proposed canonical code is
representable only when TC02 value-kind selection would classify that field back
to the same Runtime capability identity. Historical collisions SHALL therefore
remain explicit nonrepresentability instead of being guessed in reverse.

Runtime extended capabilities SHALL render only when their exact names are
parser-safe, two-character, unmapped termcap codes. Reserved `tc` syntax,
standard/historical mapping collisions, negative numeric values, and extended
parameter strings without an adopted TC04 profile SHALL fail preflight.

String rendering SHALL reverse TC04's mandatory delay suffix back to traditional
leading padding when the suffix is exactly representable. The classic parameter
operator subset adopted by TC04 SHALL be inverted exactly; broader terminfo
parameter programs SHALL be rejected rather than approximated. Historical-safe
escaping SHALL use canonical control escapes and three-digit octal where needed,
with literal colon always rendered as `\072`.

Fields SHALL be emitted in ordinal two-character-code order. Physical wrapping
SHALL occur only between complete colon-terminated fields, using backslash plus
LF without continuation indentation so TC01 logical-record reconstruction does
not acquire whitespace.

**Gate TC05:** representable descriptions render deterministically and parse back
to equivalent adopted termcap semantics; nonrepresentable descriptions return
explicit loss/representability information.

**Implementation record:**
[`docs/1.6.0-TC05-TERMCAP-REVERSE-CONVERSION-AND-RENDERING.md`](docs/1.6.0-TC05-TERMCAP-REVERSE-CONVERSION-AND-RENDERING.md)

---

## 10. TC06 — Explicit termcap acquisition

**Development version:** `1.6.0-Alpha-6`

TC06 adds opt-in acquisition compatible with historical termcap environments.

The public acquisition surface is centered on:

```text
TermcapAcquirer
TermcapAcquisitionOptions
TermcapAcquisitionResult
TermcapAcquisitionSource
TermcapAcquisitionSourceKind
TermcapDefaultPathPolicy
ITermcapEnvironmentProvider
ITermcapFileProvider
SystemTermcapEnvironmentProvider
SystemTermcapFileProvider
```

The API SHALL make environment and filesystem dependence explicit. Acquisition
SHALL support:

- a caller-supplied inline `TERMCAP` description;
- a caller-supplied `TERMCAP` database path;
- ordered `TERMPATH` database paths;
- an explicitly selected conventional default path policy.

Source precedence SHALL be inline source, explicit `TERMCAP` database path,
ordered `TERMPATH` databases, and then conventional defaults only when the
caller selected such a policy. The same ordered provider SHALL serve both the
requested root entry and `tc=` parents so inheritance may cross database-file
boundaries while retaining TC03 precedence and cycle/depth behavior.

`TermcapAcquisitionOptions.FromEnvironment` SHALL be an explicit snapshotting
operation rather than ambient discovery. It SHALL read only the historical
termcap inputs `TERMCAP`, `TERMPATH`, and `HOME` through an
`ITermcapEnvironmentProvider`. The requested terminal name SHALL remain an
explicit `TermcapAcquirer.Acquire` argument; TC06 SHALL NOT begin consulting
`TERM` on behalf of existing Runtime callers.

For the environment helper, a non-empty slash-rooted Unix `TERMCAP` value SHALL
be interpreted as a database path; rooted Windows path spellings SHALL also be
accepted for cross-platform managed callers. Other non-empty `TERMCAP` values
SHALL be treated as inline source. `TERMPATH` SHALL preserve configured search
order. Direct options construction SHALL remain available when callers do not
want historical environment-string interpretation.

The default `TermcapDefaultPathPolicy` SHALL be `None`. The explicitly selected
`Ncurses` policy SHALL append `/etc/termcap`, `/usr/share/misc/termcap`, and then
`$HOME/.termcap` when a home directory was supplied. No conventional path SHALL
be inspected merely because the Termcap package is referenced.

File-backed acquisition SHALL use `ITermcapFileProvider` and SHALL feed its
`TextReader` directly to the bounded TC01 parser. A missing file is a clean
search miss; other provider failures propagate. Parser errors from a configured
source SHALL remain observable and SHALL prevent publication of a Runtime
description rather than being silently hidden by a later matching database.

Successful acquisition SHALL resolve through TC03 and convert through TC04 to
the ordinary immutable Runtime `TerminalDescription`. Parser/resolver and
conversion diagnostics SHALL remain separately observable through
`TermcapAcquisitionResult`; TC06 SHALL NOT introduce another semantic model.

It SHALL NOT alter the existing Runtime `TERMINFO`, `TERMINFO_DIRS`, user
`.terminfo`, or built-in fallback behavior.

TC06 SHALL NOT add a command or router route. `captoinfo` / `infotocap` command
composition remains TC07.

**Gate TC06:** a caller can opt into termcap acquisition without changing the
behavior or dependency graph of existing Runtime terminal discovery.

**Implementation record:**
[`docs/1.6.0-TC06-EXPLICIT-TERMCAP-ACQUISITION.md`](docs/1.6.0-TC06-EXPLICIT-TERMCAP-ACQUISITION.md)

---

## 11. TC07 — Conversion tools and coordinated distribution

**Development version:** `1.6.0-Alpha-7`

TC07 exposes managed command functionality equivalent in purpose to:

```text
captoinfo
infotocap
```

The preferred command design SHALL reuse the parser/resolver/conversion/rendering
engines rather than duplicate them in console projects.

At this tranche, review whether the commands should be:

- new standalone command projects routed by `icod-terminfo`;
- explicit modes of an existing inspection command; or
- both, if compatibility and distribution value justify both surfaces.

Whichever command topology is chosen, update:

- router dispatch and tests;
- standalone archive construction and smoke tests;
- tool-package structure checks;
- release artifact accounting;
- README/install documentation.

**Gate TC07:** both conversion directions execute through packaged command
surfaces on Windows, Linux, and macOS without changing frozen `tic`, `infocmp`,
or `toe` behavior.

---

## 12. TC08 — Differential validation, fuzzing, and freeze

**Development version:** `1.6.0-Alpha-8`

TC08 closes the feature line.

Required validation:

- checked-in BSD/GNU-style termcap fixtures;
- comments, blank lines, continuations, and empty fields;
- all adopted string escapes and byte boundaries;
- malformed delimiter and escape cases;
- numeric boundaries and overflow;
- cancellation and inheritance precedence;
- missing/cyclic/deep `tc=` references;
- exact and lossy capability mappings;
- deterministic reverse rendering;
- termcap → Runtime and Runtime → termcap round trips;
- `TERMCAP` / `TERMPATH` acquisition without host-environment dependence;
- bounded mutation/fuzz tests for parser and resolver;
- optional differential comparison with authoritative host tools where available.

TC08 SHALL perform the public API regret review for `Icod.TermInfo.Termcap` and
freeze its active 1.6 baseline. Package-reference-only consumers and structural
package verification SHALL cover all supported target frameworks.

Before stable publication, trusted publishing SHALL authorize the new
`Icod.TermInfo.Termcap` package ID. Release workflow package counts and GitHub
Release asset accounting SHALL be updated for the additional `.nupkg` and
`.snupkg`.

**1.6 completion gate:** common conventional termcap databases can be parsed,
resolved, converted into the canonical Runtime model, rendered back where
representable, acquired explicitly through termcap environment conventions, and
used through the selected conversion tools with all loss and incompatibility
reported rather than hidden.

---

## 13. Deferred work

The following remain outside 1.6 unless required by real compatibility evidence:

- Berkeley DB / hashed ncurses storage;
- arbitrary vendor-specific binary formats;
- implicit replacement of Runtime terminfo discovery with termcap discovery;
- emulation of every historical library quirk where it conflicts with bounded,
  deterministic parsing;
- PTYs, terminal emulation, live probing, curses presentation, or graphics
  protocols.
