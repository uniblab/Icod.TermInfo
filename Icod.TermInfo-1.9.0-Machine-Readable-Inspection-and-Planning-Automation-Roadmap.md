# Icod.TermInfo 1.9.0 - Machine-Readable Inspection and Planning Automation Roadmap

**Project:** `Icod.TermInfo`
**Release:** `1.9.0`
**Theme:** Machine-Readable Inspection and Planning Automation
**Published baseline:** `1.8.0`
**Repository baseline:** `main` at `d07d923aeec758f00c4e2025fe79d6d2f97fbe83` (`v1.8.0`)
**Primary reusable package:** `Icod.TermInfo.Inspection`
**Primary commands:** `infocmp`; `toe`
**Target frameworks:** reusable libraries `net8.0`; `net9.0`; `net10.0`; commands `net10.0`
**Reusable assembly identity:** retain `1.0.0.0`
**Planned development sequence:** `1.9.0-Alpha-1` through `1.9.0-Alpha-7`, then stable `1.9.0`
**Status:** MI02 complete
**Primary objective:** expose deterministic, bounded, versioned JSON representations of effective descriptions, structured comparisons, relative-source plans, and explicit database catalogs, then compose those representations through `infocmp` and `toe` without changing the frozen 1.7 synthesis or 1.8 planning semantics.

---

## 1. Release thesis

Versions 1.3 through 1.8 established structured reusable Inspection values:

```text
TerminalDescription
TermInfoComparisonResult
TerminalDescriptionSourcePlan
TermInfoDatabaseCatalog
```

Those values are already deterministic and useful to managed callers. Command
presentation is nevertheless primarily human-readable source or prose. Scripts,
build systems, IDE extensions, repository auditors, and other Icod projects
should not have to parse human presentation to recover structured evidence.

Version 1.9 adds one explicit machine-readable boundary:

```text
immutable Inspection value
    + reviewed JSON policy
        -> versioned envelope
        -> deterministic property and item order
        -> bounded UTF-8 representation
        -> reusable renderer
        -> direct command output
        -> routed command output
```

The release also completes the deliberately deferred command automation for
planning against every canonical candidate in one explicit conventional
database directory. It does not introduce implicit host-wide discovery.

The concise promise is:

> Given the same immutable Inspection value and JSON policy, Icod produces the
> same schema-valid JSON text across supported frameworks, hosts, cultures, and
> processes, while reporting the same underlying semantics and evidence as the
> existing managed API.

JSON is an additional representation. It does not become the semantic model.

---

## 2. Architectural ownership

Machine-readable rendering belongs in `Icod.TermInfo.Inspection` because that
package already owns the structured values being represented:

- canonical effective-description rendering;
- structured semantic comparison;
- explicit database catalogs and issues;
- relative-source synthesis;
- relative-source planning scores and search evidence.

The package graph remains:

```text
Icod.TermInfo.Inspection
        |          |
        v          v
Icod.TermInfo.Source
        |
        v
Icod.TermInfo

Icod.TermInfo.Compiler       test/sample use only
Icod.TermInfo.Termcap        no JSON dependency
```

The following boundaries are frozen:

1. Runtime remains dependency-free.
2. Source continues to depend only on Runtime.
3. Compiler continues to depend only on Runtime and Source.
4. Inspection continues to depend only on Runtime and Source.
5. Termcap continues to depend only on Runtime.
6. No reusable package depends on `Icod.CommandFramework`.
7. `System.Text.Json`, when operational rendering begins, is a framework API and
   shall not add a third-party or NuGet package dependency.
8. Commands remain thin policy and stream adapters over reusable behavior.
9. The router dispatches existing command implementations without duplicating
   JSON or planning logic.
10. No seventh registry package is introduced.

---

## 3. Relationship to frozen 1.x contracts

Version 1.9 shall preserve:

- the complete Runtime 1.0 API and semantics;
- the complete Source 1.1 API and semantics;
- the complete Compiler 1.2 API and semantics;
- the complete Termcap 1.6 API and semantics;
- the complete 1.7 explicit relative-source synthesis API and semantics;
- the complete 1.8 relative-source planning API, score, ordering, bounds, and
  evidence semantics;
- every existing human-readable command form unless an independently reviewed
  defect requires correction.

JSON rendering consumes public immutable values. It shall not add JSON fields to
those values, mutate them, or make serialization attributes part of Runtime,
Source, Compiler, or Termcap.

The frozen `docs/1.8.0-INSPECTION-PUBLIC-API-BASELINE.txt` remains immutable
historical evidence. MI01 begins a reviewed additive Inspection surface. MI07
freezes the complete 1.9 Inspection baseline.

---

## 4. Schema and envelope contract

Every 1.9 JSON document shall use this top-level shape and property order:

```json
{
  "schema": "urn:icod:terminfo:inspection:json:1",
  "schemaVersion": 1,
  "documentKind": "terminalDescription",
  "data": {}
}
```

The four document-kind strings are:

```text
terminalDescription
comparison
sourcePlan
databaseCatalog
```

The envelope rules are:

1. `schema` is the exact case-sensitive schema identifier.
2. `schemaVersion` is the JSON integer `1`.
3. `documentKind` is one of the four exact strings above.
4. `data` contains the kind-specific payload.
5. No timestamp, process ID, host name, culture, framework, random identifier,
   or generated-at field is emitted.
6. Unknown future top-level properties must be tolerated by consumers unless a
   later schema explicitly says otherwise.
7. Existing version-1 property meanings shall not be repurposed.
8. A breaking schema change requires a new schema identifier and version rather
   than a silent change under version 1.

The repository shall publish a machine-readable JSON Schema after all four
payloads are operational. Until MI04 settles the complete payload union, the
MI01 implementation record is the authoritative provisional schema contract.

---

## 5. Deterministic text contract

Reusable rendering shall be deterministic at the returned-string and UTF-8 byte
levels.

The renderer shall freeze:

- UTF-8 without a byte-order mark when callers encode returned text;
- ordinal, case-sensitive schema and extended-capability names;
- explicit property order;
- explicit array order;
- invariant JSON number formatting;
- JSON Boolean and null tokens;
- `System.Text.Json` default safe string escaping;
- no trailing whitespace;
- no trailing line terminator in the reusable return value;
- LF internal line endings when indented output is requested;
- two-space indentation for indented output;
- compact output as the default.

Commands shall append exactly one LF after a successfully rendered document.
They shall not insert diagnostics, progress text, or a second document into the
JSON stdout stream.

Dictionary enumeration, filesystem enumeration, hash ordering, culture,
reflection order, wall-clock time, and task scheduling shall not determine
output.

---

## 6. Resource and cancellation contract

Machine-readable output may include long capability strings, many comparison
differences, a complete generated source representation, or a large explicit
catalog. Rendering must therefore remain bounded.

The reviewed MI01 policy is:

| Option | Default | Supported maximum |
| --- | ---: | ---: |
| Output size | 4,194,304 UTF-8 bytes | 67,108,864 UTF-8 bytes |
| Indentation | disabled | enabled or disabled |

The renderer shall:

1. validate required arguments before work;
2. reject a null explicit options object;
3. observe cancellation before traversal and at deterministic item boundaries;
4. use checked counters;
5. stop before returning output beyond the configured byte limit;
6. avoid retaining multiple complete output copies where practical;
7. avoid recursion proportional to uncontrolled input;
8. report cancellation through `OperationCanceledException`;
9. report an exceeded representation bound deterministically;
10. never truncate a JSON document and call it successful.

The options object shall not accept mutable `JsonSerializerOptions`, converters,
delegates, naming policies, encoders, resolver instances, or arbitrary callbacks.
Those surfaces would make determinism and schema compatibility caller-dependent.

---

## 7. Reusable API direction

MI01 adds only:

```text
TermInfoJsonRendererOptions
TermInfoJsonRenderer
```

The options type is immutable. The renderer is static and exposes schema
identity plus typed overloads for:

```text
TerminalDescription
TermInfoComparisonResult
TerminalDescriptionSourcePlan
TermInfoDatabaseCatalog
```

Each value kind shall have:

```text
Render(value)
Render(value, options, cancellationToken)
```

The default overload applies the canonical compact policy. Explicit-policy
overloads require a non-null options object. Cancellation is method-local and is
not mutable state retained by the options object.

MI01 validates the API and schema boundary but deliberately does not emit a
partial production schema:

- effective-description rendering begins in MI02;
- comparison and plan rendering begin in MI03;
- catalog rendering begins in MI04.

Until those tranches, the corresponding typed method throws a documented
`NotSupportedException` after required argument and cancellation validation.

No JSON parser, deserializer, mutable document object, DOM wrapper, or generic
`object` serializer is included in 1.9.

---

## 8. Effective-description payload

MI02 shall render a `TerminalDescription` as:

```text
identity
    name
    aliases
    description
capabilities
    booleans
    numbers
    strings
    extended
```

Required rules:

- identity is preserved exactly;
- aliases retain their immutable order;
- a missing description is represented explicitly as JSON null;
- absent standard capabilities do not receive invented default values;
- standard capabilities are ordered by compiled database position within value
  kind;
- each standard capability uses its canonical terminfo short name;
- extended capabilities are ordered first by value kind and then by exact
  ordinal name;
- string values are ordinary JSON strings containing the exact managed value;
- control bytes are JSON-escaped, not reinterpreted as source-language escapes;
- numeric values remain JSON integers;
- effective descriptions contain no cancellation tombstones or source ancestry,
  and the JSON shall not invent either.

MI02 shall use explicit writer code rather than reflection-driven serialization
of public property layout.

---

## 9. Comparison and plan payloads

MI03 shall render `TermInfoComparisonResult` with:

- `areEqual`;
- deterministic ordered differences;
- exact difference kind;
- capability identity and extended classification when applicable;
- left and right identity/capability values when applicable;
- retained source-entry, field, index, and span evidence when the comparison is
  source-aware;
- explicit null for side-specific values which are absent by semantic design.

MI03 shall render `TerminalDescriptionSourcePlan` with:

- selected parent count and exact ordered `UseName` values;
- generated LF source;
- all frozen score components;
- selected candidate indices;
- evaluated plan count;
- exhaustive or bounded evidence;
- accepted candidate count.

The JSON renderer reports the existing result. It shall not recompute a
comparison, re-run planning, parse source, reinterpret the score, or claim more
completeness than the managed result.

---

## 10. Database-catalog manifest payload

MI04 shall render `TermInfoDatabaseCatalog` as a deterministic explicit manifest
containing:

- normalized root supplied to the catalog;
- catalog kind;
- derived completeness evidence;
- entries in existing catalog order;
- each entry's normalized path, canonical name, aliases, and description;
- issues in existing deterministic order;
- each issue's kind, path, and message;
- duplicate canonical names in existing deterministic order.

The manifest does not recursively embed every complete terminal capability set.
Callers may render an entry's `TerminalDescription` separately when full
capability data is required. This keeps catalog automation bounded and prevents
one schema kind from duplicating another.

An unavailable, missing, unsupported, malformed, permission-limited, or partial
catalog shall not be represented as a complete candidate universe.

MI04 shall publish the completed version-1 JSON Schema and deterministic fixture
documents for all four kinds.

---

## 11. Command automation

MI05 shall compose the reusable renderer through existing commands.

### 11.1 `infocmp`

The reviewed direction is:

```text
infocmp --json target
infocmp --json -d left right
infocmp --json --plan-use target candidate [candidate ...]
infocmp --json --plan-use --all-candidates -B directory target
```

JSON mode selects one JSON document appropriate to the requested operation.
Human source-layout switches shall be rejected where they cannot affect JSON
semantics without ambiguity.

The all-candidates form shall:

- require `--plan-use`;
- require one explicit `-B directory`;
- accept exactly one target operand;
- inspect only that explicit conventional directory;
- use canonical catalog entries in deterministic catalog order;
- exclude the target through the frozen planning contract;
- reject incomplete or ambiguous catalogs rather than claim exhaustive
  selection;
- reuse existing planning limits and cancellation behavior;
- remain unavailable through implicit host discovery.

Without `--json`, successful planning continues to write only selected source.

### 11.2 `toe`

The reviewed direction is:

```text
toe --json directory
```

The command shall render the explicit catalog manifest produced from the same
catalog object used by human-readable listing. JSON mode shall not parse the
human listing.

### 11.3 Router and exit statuses

Direct and routed invocation shall remain equivalent. Existing exit statuses
remain:

```text
0    operation and JSON rendering succeeded
1    acquisition, catalog, planning, bounds, or rendering failure
2    usage error
130  cancellation
```

Diagnostics use stderr and must not contaminate stdout JSON.

---

## 12. Package and distribution contract

The coordinated registry package family remains exactly:

```text
Icod.TermInfo
Icod.TermInfo.Source
Icod.TermInfo.Compiler
Icod.TermInfo.Inspection
Icod.TermInfo.Termcap
Icod.TermInfo.Tools
```

All packages and commands consume the central suite version. The five reusable
assemblies retain `AssemblyVersion 1.0.0.0`.

MI01-MI04 change only the Inspection package's reusable public behavior. MI05
changes `infocmp`, `toe`, the Tools router behavior, and matching archive
behavior. Runtime, Source, Compiler, and Termcap receive coordinated version and
release metadata only.

The six existing framework-dependent command archives remain unchanged in
topology:

```text
win-x64
win-arm64
linux-x64
linux-arm64
osx-x64
osx-arm64
```

No new executable, registry package, runtime bundle, native library, or platform
installer is introduced.

---

## 13. Permanent correctness evidence

The 1.9 test program shall include:

### 13.1 Schema fixtures

Checked-in compact and indented fixtures for all four document kinds shall be
validated through `JsonDocument` and the published JSON Schema.

### 13.2 Determinism

For every fixture:

```text
same immutable value
    -> repeated render
    -> cross-process render
    -> net8/net9/net10 render
    -> Windows/Linux/macOS render
    -> exact UTF-8 byte equality
```

Culture and insertion-order variants shall not change output.

### 13.3 Semantic correspondence

Tests shall compare JSON fields directly with the managed value which was
rendered. Tests shall not merely compare one serializer invocation with another.

### 13.4 Bounds and cancellation

Exercise:

- output exactly at and one byte beyond the configured limit;
- long strings and control bytes;
- large extended-capability sets;
- many differences;
- cancellation before rendering and during collection traversal;
- large catalog entry and issue sets;
- checked size arithmetic;
- no partial successful JSON after failure.

### 13.5 Command and package evidence

Tests shall cover direct commands, routed commands, package-only consumers, and
matching-host archives. Every successful stdout payload shall parse as exactly
one document and contain no diagnostic prefix or suffix.

---

## 14. Samples and documentation

The deterministic Toolchain sample shall demonstrate:

```text
source parse and resolve
    -> effective description JSON
    -> relative-source planning
    -> plan JSON
    -> compile and reacquire
    -> comparison JSON
```

The ToolSuite sample shall demonstrate controlled direct and routed forms for:

- description JSON;
- comparison JSON;
- explicit-candidate planning JSON;
- explicit-directory all-candidates planning;
- database manifest JSON.

Documentation shall include:

- the schema identifier and versioning policy;
- exact command examples;
- compact and indented examples;
- field semantics for all payload kinds;
- completeness and path caveats for catalogs;
- the absence of JSON input/deserialization;
- boundaries with `Icod.Terminal`, future `Icod.Pty`, and `Icod.DCurses`.

---

## 15. Development tranche sequence

## MI01 - JSON Contract and Renderer Foundation

**Development version:** `1.9.0-Alpha-1`

### Goals

Freeze the schema envelope, deterministic text policy, immutable options,
resource bounds, typed renderer surface, dependency direction, and temporary
operational tranche boundaries.

### Required work

- add this roadmap and activate the 1.9 development line;
- add `TermInfoJsonRendererOptions`;
- add `TermInfoJsonRenderer`;
- freeze schema identifier/version and document-kind strings;
- validate nulls, bounds, and pre-cancellation;
- retain the frozen 1.8 public baseline as historical evidence;
- preserve exact cross-framework public API equality;
- update package smoke for the reviewed Alpha-1 surface;
- document the contract in `docs/1.9.0-MI01-JSON-CONTRACT-AND-RENDERER-FOUNDATION.md`.

### Gate

MI01 is complete when the reviewed API can represent all later payload kinds
without exposing mutable JSON policy or emitting a misleading partial schema.

---

## MI02 - Effective Description JSON

**Development version:** `1.9.0-Alpha-2`

Implement deterministic bounded rendering for `TerminalDescription`, including
identity, every standard capability kind, every extended capability kind,
escaping, ordering, compact/indented output, cancellation, and exact byte-limit
tests.

**Gate:** every generated payload parses, corresponds exactly to its effective
description, and is byte-identical across target frameworks and repeated runs.

---

## MI03 - Comparison and Planning Evidence JSON

**Development version:** `1.9.0-Alpha-3`

Implement deterministic bounded rendering for `TermInfoComparisonResult` and
`TerminalDescriptionSourcePlan`, including effective and source-aware difference
evidence, plan source, selected parents, score components, indices, evaluation
count, candidate count, and exhaustive/bounded status.

**Gate:** JSON fields correspond directly to independently asserted managed
result values; rendering never recomputes comparison or planning semantics.

---

## MI04 - Database Catalog Manifests and JSON Schema

**Development version:** `1.9.0-Alpha-4`

Implement deterministic bounded catalog manifests, explicit completeness
evidence, entries, issues, duplicates, and path handling. Publish the completed
version-1 JSON Schema and checked-in fixtures for all document kinds.

**Gate:** complete and incomplete explicit catalog states validate against the
schema without hiding issues or ambiguity.

---

## MI05 - `infocmp` and `toe` Automation

**Development version:** `1.9.0-Alpha-5`

Add command JSON modes plus explicit-directory all-candidates planning. Freeze
option interactions, stdout/stderr separation, exit statuses, direct/router
equivalence, and all six archive behaviors.

**Gate:** every supported direct and routed JSON command produces one valid
schema document or no stdout document on failure.

---

## MI06 - Samples, Package Consumers, and Cross-Host Hardening

**Development version:** `1.9.0-Alpha-6`

Update Toolchain and ToolSuite samples, package READMEs, root documentation,
package-only smoke consumers, command package smoke, archive smoke, culture and
process determinism, large/pathological inputs, and cross-host fixture evidence.

**Gate:** the exact packaged Inspection API and distributed commands exercise
real JSON rendering and automation without repository project references.

---

## MI07 - API, Schema, Packaging, and Release Closure

**Development version:** `1.9.0-Alpha-7`

Freeze the exact 1.9 Inspection API baseline, version-1 JSON Schema, command
semantics, package graph, router/archive topology, documentation, samples,
release verifiers, and release audit. No new feature begins in MI07.

Create:

```text
docs/1.9.0-INSPECTION-PUBLIC-API-BASELINE.txt
docs/1.9.0-RELEASE-AUDIT.md
```

**Gate:** Alpha-7 contains the complete stable-intended source, schema, API,
commands, documentation, fixtures, packages, archives, and release evidence.

---

## 16. Version and package policy

Every tranche updates the single coordinated authority:

```xml
<IcodTermInfoSuiteVersion>1.9.0-Alpha-N</IcodTermInfoSuiteVersion>
```

All six registry packages consume that value. Standalone commands consume it for
version reporting and remain non-packable. Historical roadmap, implementation,
baseline, and release-audit versions remain unchanged.

The stable transition is:

```text
1.9.0-Alpha-7
    -> 1.9.0
```

No feature, API, schema, command, or packaging change enters between successful
Alpha-7 validation and stable publication.

---

## 17. Non-goals for 1.9

Version 1.9 shall not include:

- JSON input, parsing, or deserialization;
- arbitrary CLR object serialization;
- caller-supplied JSON converters, naming policies, encoders, or delegates;
- silent schema negotiation;
- timestamps or host-dependent provenance in deterministic documents;
- implicit host-wide all-candidates planning;
- source-set factoring or synthetic shared-parent creation;
- alternative planning scores or application callbacks;
- changes to frozen 1.7 synthesis behavior;
- changes to frozen 1.8 planning score or enumeration behavior;
- hashed/Berkeley terminfo stores;
- historical vendor binary dialects;
- new termcap conversion behavior;
- `tput`, `clear`, `tabs`, or `reset` commands;
- live terminal sessions, probing, input, PTYs, curses, emulation, or graphics;
- a new package family or executable.

---

## 18. Risks and mitigations

### 18.1 Accidental schema instability

**Risk:** implementation or reflection order becomes observable API.

**Mitigation:** explicit writers, fixed property and item order, checked-in
fixtures, published schema, and exact cross-framework/process/host byte tests.

### 18.2 JSON becomes a second semantic model

**Risk:** renderer code independently interprets capabilities, differences, or
plans.

**Mitigation:** serialize existing immutable values directly and independently
assert field correspondence in tests.

### 18.3 Unbounded output

**Risk:** large catalogs or strings create excessive allocation.

**Mitigation:** immutable byte limits, checked counters, deterministic
cancellation points, and no successful truncation.

### 18.4 Human and machine modes contaminate each other

**Risk:** diagnostics or source text makes stdout invalid JSON.

**Mitigation:** exactly one document on successful stdout, diagnostics on
stderr, command-level parse tests, and router/archive smoke.

### 18.5 Incomplete catalog appears exhaustive

**Risk:** permission or parse failures are hidden by an all-candidates command.

**Mitigation:** explicit completeness evidence and refusal to claim complete
planning over incomplete or ambiguous catalogs.

### 18.6 Public API regret

**Risk:** serializer internals, DOM types, or mutable framework options become
permanent.

**Mitigation:** two-type MI01 surface, typed immutable inputs, string output,
minimal overloads, and no public writer/search/document-node types.

---

## 19. Completion gate

Version 1.9.0 is complete when:

1. all four document kinds use the frozen version-1 envelope;
2. description JSON represents identity and every capability kind exactly;
3. comparison JSON represents every effective and source-aware difference kind;
4. plan JSON represents exact source, parents, score, and completeness evidence;
5. catalog JSON represents entries, issues, duplicates, and completeness without
   hiding partial state;
6. compact and indented output are deterministic and schema-valid;
7. output limits and cancellation are enforced without successful truncation;
8. `infocmp` and `toe` expose reviewed JSON forms;
9. all-candidates planning requires an explicit candidate directory;
10. direct and routed command behavior is equivalent;
11. package and archive smoke execute real machine-readable workflows;
12. Toolchain and ToolSuite samples document reproducible use;
13. Runtime, Source, Compiler, Termcap, 1.7 synthesis, and 1.8 planning contracts
    remain unchanged;
14. Inspection retains Runtime-and-Source-only production dependencies;
15. all reusable assemblies retain version `1.0.0.0`;
16. net8/net9/net10 and Windows/Linux/macOS validation pass;
17. the exact 1.9 API baseline and JSON Schema are frozen;
18. stable `v1.9.0` identifies the exact validated `main` release commit.

---

## 20. Post-1.9 candidates

Later releases may consider:

- multi-target source-set factoring and shared-parent synthesis;
- reviewed alternative built-in planning objectives;
- `tput`, `clear`, and `tabs` command expansion;
- optional hashed ncurses database acquisition;
- documented historical Unix binary dialects;
- additional vendor source compatibility.

These candidates shall not be pulled into 1.9 merely because nearby Inspection
or command code is being touched.
