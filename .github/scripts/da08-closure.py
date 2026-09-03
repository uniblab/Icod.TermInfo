from pathlib import Path


def replace_once(path, old, new):
    file = Path(path)
    text = file.read_text(encoding='utf-8')
    if old not in text:
        raise SystemExit(f"Required marker not found in {path}: {old[:80]!r}")
    file.write_text(text.replace(old, new, 1), encoding='utf-8', newline='\n')


def insert_before(path, marker, section):
    replace_once(path, marker, section + marker)


replace_once(
    'Directory.Build.props',
    '<IcodTermInfoSuiteVersion>1.10.0-Alpha-7</IcodTermInfoSuiteVersion>',
    '<IcodTermInfoSuiteVersion>1.10.0-Alpha-8</IcodTermInfoSuiteVersion>',
)

current_version_paths = [
    'tests/Icod.TermInfo.Tests/src/T45CompletionGateTests.cs',
    'tests/Icod.TermInfo.Termcap.Tests/src/TC08ContractTests.cs',
    'tests/Icod.TermInfo.Inspection.Tests/src/RP08ReleaseClosureTests.cs',
    'tests/Icod.TermInfo.Inspection.Tests/src/RS08ContractTests.cs',
    'tests/Icod.TermInfo.Tic.Tests/src/ReleaseClosureTests.cs',
    'tests/Icod.TermInfo.Tic.Tests/src/CommandTests.cs',
    'tests/Icod.TermInfo.InfoCmp.Tests/src/CommandTests.cs',
    'tests/Icod.TermInfo.Toe.Tests/src/CommandTests.cs',
    'tests/Icod.TermInfo.Router.Tests/src/ContractTests.cs',
    'tests/Icod.TermInfo.Router.Tests/src/CommandTests.cs',
]
for path in current_version_paths:
    file = Path(path)
    text = file.read_text(encoding='utf-8')
    updated = text.replace('1.10.0-Alpha-7', '1.10.0-Alpha-8')
    if updated == text:
        raise SystemExit(f'Current-version Alpha-7 marker not found in {path}.')
    file.write_text(updated, encoding='utf-8', newline='\n')

replace_once(
    'Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj',
    '<PackageReleaseNotes>1.10.0-Alpha-6 adds the separate version-2 database automation JSON contract for databaseSet, databaseSetComparison, and databaseSetPlan documents plus thin toe/infocmp/router command composition, while preserving byte-compatible version-1 JSON invocations and all frozen DA01-DA05 engines.</PackageReleaseNotes>',
    '<PackageReleaseNotes>1.10.0-Alpha-8 freezes the complete 1.10 Inspection API, the additive version-2 database automation schema, package verification, command composition, and DA07 cross-host/package/archive hardening while preserving the frozen version-1 JSON and all lower-layer contracts.</PackageReleaseNotes>',
)
replace_once(
    'icod-terminfo/Icod.TermInfo.Router.csproj',
    '<PackageReleaseNotes>1.9.0 publishes the frozen routed JSON automation, five-command dispatch, installable-package behavior, and six-archive topology as part of the complete 1.9 release contract.</PackageReleaseNotes>',
    '<PackageReleaseNotes>1.10.0-Alpha-8 freezes the additive multi-database toe/infocmp automation, routed package behavior, and DA07 installed-tool plus six-RID archive hardening while preserving the complete 1.9 routed command contract.</PackageReleaseNotes>',
)

replace_once(
    'Icod.TermInfo-1.10.0-Deterministic-Multi-Database-Inspection-Comparison-and-Planning-Automation-Roadmap.md',
    '**Status:** DA06 complete and frozen; DA07 not yet started  ',
    '**Status:** DA08 release contract frozen at `1.10.0-Alpha-8`; stable promotion pending exact-head validation',
)

root_section = '''## 1.10 development status — DA08 release freeze

The `1.10.0` development branch is frozen at `1.10.0-Alpha-8` for release
validation. Version 1.10 extends the 1.9 automation layer to caller-ordered sets
of explicit conventional terminfo databases: deterministic precedence and
shadow evidence, semantic duplicate and alias-collision analysis, set
comparison, bounded multi-database parent planning, and the additive version-2
JSON documents `databaseSet`, `databaseSetComparison`, and `databaseSetPlan`.

The complete 1.10 Inspection public surface is frozen in
`docs/1.10.0-INSPECTION-PUBLIC-API-BASELINE.txt`. The 1.9 version-1 JSON schema
and command forms remain unchanged; the additive version-2 schema is frozen in
`docs/Icod.TermInfo.Inspection.schema.v2.json`. DA07 hardening exercises real
generated databases, isolated package consumers on `net8.0`/`net9.0`/`net10.0`,
installed tools on all three hosts, and all six standalone archive RIDs.

Stable `1.10.0` is promotion-only after the exact Alpha-8 release gate is green.
See `docs/1.10.0-RELEASE-AUDIT.md`.

'''
insert_before('README.md', '## Install\n', root_section)

inspection_section = '''## 1.10 DA08 release freeze

`1.10.0-Alpha-8` freezes the complete 51-type additive 1.10 Inspection surface,
the exact version-2 database automation schema, package verification, command
composition, and DA07 distribution evidence. The exact API baseline is
`docs/1.10.0-INSPECTION-PUBLIC-API-BASELINE.txt`; both JSON schema fingerprints
are recorded in `docs/1.10.0-DA08-FREEZE-FINGERPRINTS.txt` and enforced by the
package verifier. Version-1 JSON remains byte-compatible with the frozen 1.9
contract.

See `docs/1.10.0-DA08-API-SCHEMA-COMMAND-PACKAGE-AND-DOCUMENTATION-FREEZE.md`
and `docs/1.10.0-RELEASE-AUDIT.md`.

## 1.10 DA07 generated-state and distribution hardening

`1.10.0-Alpha-7` adds no feature API. It hardens DA01-DA06 with generated
compiled databases, culture changes, incomplete and repeated roots, large
candidate sets, isolated package-only consumption, installed-tool automation,
and all six matching standalone archive RIDs.

See
`docs/1.10.0-DA07-GENERATED-STATE-CROSS-HOST-PACKAGE-AND-PATHOLOGICAL-HARDENING.md`.

'''
insert_before('Icod.TermInfo.Inspection/README.md', '## 1.10 DA06 command and machine-readable automation composition\n', inspection_section)

toe_section = '''## 1.10 multi-database JSON automation

Version `1.10.0-Alpha-8` freezes the additive explicit database-set forms:

```text
toe --json root-a root-b [root ...]
toe --json --compare-set --left-root root [--left-root root ...] \\
    --right-root root [--right-root root ...]
```

Two or more ordinary explicit roots emit a version-2 `databaseSet` document in
caller order. `--compare-set` emits a version-2 `databaseSetComparison` and
keeps left and right root order independently. The historical one-root
`toe --json directory` form remains the exact version-1 `databaseCatalog`
contract from 1.9.

DA07 distribution smoke creates real databases with the shipped `tic`, executes
multi-root `toe --json` through the installed router and every supported archive,
and verifies conflicting canonical evidence without relying on source-tree
project references.

'''
insert_before('toe/README.md', '## 1.9 JSON automation\n', toe_section)

infocmp_section = '''## 1.10 multi-database planning automation

Version `1.10.0-Alpha-8` freezes repeatable explicit candidate roots:

```text
infocmp --json --plan-use --all-candidates \\
    --candidate-root root [--candidate-root root ...] target
```

The command composes the frozen 1.8 planner over the complete ordered database
set and emits a version-2 `databaseSetPlan`. Physical database/catalog candidate
order is retained, the target identity is excluded, semantically equal duplicate
publications collapse behind the first representative, and conflicting or
incomplete candidate sets are rejected. The legacy single-directory
`--all-candidates -B directory` route remains the frozen version-1 `sourcePlan`
path from 1.9.

DA07 package and six-RID archive smoke exercise this path against real compiled
databases outside the source tree.

'''
insert_before('infocmp/README.md', '## 1.9 JSON automation\n', infocmp_section)

compatibility_section = '''## 1.10 compatibility freeze

Version 1.10 is additive above the stable 1.9 boundary. DA08 freezes the complete
1.10 Inspection surface in `docs/1.10.0-INSPECTION-PUBLIC-API-BASELINE.txt` and
requires exact equality across `net8.0`, `net9.0`, and `net10.0`. Runtime, Source,
Compiler, and Termcap retain their previously frozen APIs and assembly identity
`1.0.0.0`.

The version-1 JSON identifier, schema, four document kinds, ordering, UTF-8
bounds, and historical `toe`/`infocmp` JSON command forms remain immutable. The
version-2 schema is additive and contains only `databaseSet`,
`databaseSetComparison`, and `databaseSetPlan`. Stable 1.10 promotion may not
change either frozen schema or command semantics.

'''
insert_before('docs/COMPATIBILITY.md', '## Supported target frameworks\n', compatibility_section)

versioning_section = '''## 1.10 release line

The DA01-DA08 development sequence is `1.10.0-Alpha-1` through
`1.10.0-Alpha-8`. DA08 freezes the exact complete 1.10 Inspection API and the
additive version-2 JSON schema while preserving the frozen 1.9 version-1 schema.
After the exact Alpha-8 head passes the full package and six-RID distribution
gate, stable `1.10.0` is a promotion-only version transition: no new feature
semantics, public API, schema fields, dependencies, target frameworks, command
behavior, or archive RIDs may be introduced.

'''
insert_before('docs/VERSIONING.md', '## Package versions\n', versioning_section)
