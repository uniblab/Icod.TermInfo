# Icod.TermInfo 1.6.0 — Termcap Interoperability Roadmap

**Development line:** `1.6.0`
**Initial development version:** `1.6.0-Alpha-1`
**Stable assembly version:** `1.0.0.0`
**Status:** Implementation in progress — TC01 foundation
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

It SHALL:

- derive standard mappings from the canonical Runtime catalog wherever a
  standard termcap code is already recorded;
- avoid a second hand-maintained standard capability table;
- distinguish Boolean, numeric, and string type expectations;
- identify obsolete aliases explicitly;
- distinguish known standard mappings from unmapped/vendor termcap fields;
- preserve unmapped fields for diagnostics rather than silently discarding them;
- define deterministic handling for conflicting or ambiguous historical codes.

No conversion occurs merely because a field is classified.

**Gate TC02:** every adopted standard termcap code maps to the same semantic
Runtime capability identity used by compiled terminfo and built-in profiles, and
unknown/vendor fields remain explicit.

---

## 7. TC03 — `tc=` inheritance and cancellation

**Development version:** `1.6.0-Alpha-3`

TC03 adds a termcap-specific resolver above the unresolved parser.

Required behavior:

- resolve terminal names from a parsed document or caller-supplied provider;
- apply the referring description before inherited descriptions;
- preserve the historical rule that local values override inherited values;
- apply `xx@` cancellation across inheritance;
- detect missing references;
- detect direct and indirect cycles;
- impose a configurable inheritance-depth bound;
- preserve deterministic diagnostics and source provenance;
- remain independent of process-global database discovery.

The resolver SHALL NOT route termcap through the terminfo `use=` resolver merely
because both mechanisms express inheritance. Their syntax and precedence rules
remain separately testable.

**Gate TC03:** multi-level and cyclic `tc=` graphs resolve or fail deterministically
with the adopted precedence and cancellation semantics.

---

## 8. TC04 — Termcap to terminfo semantic conversion

**Development version:** `1.6.0-Alpha-4`

TC04 converts a resolved termcap entry into the canonical immutable Runtime
model.

Conversion SHALL distinguish:

```text
exact mapping
supported historical alias
explicit approximation
unsupported/unmapped field
unrepresentable value
```

Loss SHALL be reported through a structured conversion result and SHALL NOT be
silently hidden.

The resulting `TerminalDescription` SHALL use the same standard capability enums
and extended-capability storage used by every other Runtime acquisition path.
Termcap-specific source state SHALL NOT leak into `TerminalDescription`.

**Gate TC04:** representative resolved termcap entries materialize into semantic
Runtime descriptions with every non-exact conversion decision observable.

---

## 9. TC05 — Reverse conversion and rendering

**Development version:** `1.6.0-Alpha-5`

TC05 supplies the reverse interoperability path for descriptions which can be
represented by termcap.

It SHALL:

- determine representability before emitting text;
- map canonical Runtime capabilities to adopted two-character termcap codes;
- report capabilities with no faithful termcap representation;
- encode strings deterministically with historical-safe escapes;
- use `\072` for literal colon bytes;
- avoid synthesizing silent approximations;
- produce stable field ordering and wrapping;
- support semantic terminfo → termcap → parse/resolve round trips where lossless.

**Gate TC05:** representable descriptions render deterministically and parse back
to equivalent adopted termcap semantics; nonrepresentable descriptions return
explicit loss/representability information.

---

## 10. TC06 — Explicit termcap acquisition

**Development version:** `1.6.0-Alpha-6`

TC06 adds opt-in acquisition compatible with historical termcap environments.

The API SHALL make environment dependence explicit. It MAY support:

- a caller-supplied inline `TERMCAP` description;
- a caller-supplied `TERMCAP` database path;
- ordered `TERMPATH` database paths;
- an explicitly selected conventional default path policy.

It SHALL NOT alter the existing Runtime `TERMINFO`, `TERMINFO_DIRS`, user
`.terminfo`, or built-in fallback behavior.

Filesystem and environment access SHALL be isolated behind explicit provider or
options types so tests can remain deterministic.

**Gate TC06:** a caller can opt into termcap acquisition without changing the
behavior or dependency graph of existing Runtime terminal discovery.

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
