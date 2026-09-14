from pathlib import Path


def replace_exact(path: str, old: str, new: str, expected: int = 1) -> None:
    file_path = Path(path)
    text = file_path.read_text(encoding="utf-8")
    count = text.count(old)
    if count != expected:
        raise SystemExit(
            f"{path}: expected {expected} occurrence(s), found {count}: {old!r}"
        )
    file_path.write_text(text.replace(old, new), encoding="utf-8", newline="\n")


readme = "README.md"
replace_exact(
    readme,
    """Version `1.13.0` is the coordinated stable-promotion candidate. It
promotes the validated Alpha-8 persistent-raster runtime-evidence interchange
surface without changing feature semantics, public API, schemas, dependencies,
target frameworks, command behavior, package-consumer topology, or archive RIDs.
Stable `1.12.0` remains the published release until merge and tag publication.""",
    """Version `1.13.0` is the validated coordinated stable release candidate. It
promotes the persistent-raster runtime-evidence interchange surface without
changing feature semantics, public API, schemas, dependencies, target frameworks,
command behavior, package-consumer topology, or archive RIDs. Stable `1.12.0`
remains the published release until PR #43 is merged and the normal tag-based
publication flow is performed.""",
)
replace_exact(
    readme,
    "## 1.13 stable-promotion status",
    "## 1.13 release-ready status",
)
replace_exact(
    readme,
    "The complete candidate Inspection surface contains 90 exported public types with",
    "The complete 1.13 Inspection surface contains 90 exported public types with",
)
replace_exact(
    readme,
    """The Alpha-8 contract was accepted on exact product head
`f236c33d8239e80379bf8cf0f1123abd6c93c3cb` by qualification run `34797445315`,
with all 12 jobs green. Stable `1.13.0` promotion now requires its own fresh full
validation; publication remains gated by merge to `main` and the normal immutable
tag workflow. The install commands below now target `1.13.0`. See""",
    """The Alpha-8 contract was accepted on exact product head
`f236c33d8239e80379bf8cf0f1123abd6c93c3cb` by qualification run `34797445315`.
Stable product head `6e9217b16c3023fb10fa34dbaa74afe48448d858` then passed qualification run
`34799272472` with all 12 jobs green, including Windows whole-surface/historical
Inspection compatibility, package verification, three installed-tool smokes, and
all six archive RIDs. PR #43 remains unmerged; tag and package publication remain
gated by the normal release workflow. The install commands below target
`1.13.0`. See""",
)

inspection_readme = "Icod.TermInfo.Inspection/README.md"
replace_exact(
    inspection_readme,
    "## 1.13 stable-promotion status",
    "## 1.13 release-ready status",
)
replace_exact(
    inspection_readme,
    """fresh Inspection package on net8.0, net9.0, and net10.0. Exact Alpha-8
product head `f236c33d8239e80379bf8cf0f1123abd6c93c3cb` passed qualification run
`34797445315` across all 12 jobs. Stable promotion is now undergoing its own
fresh full validation. See""",
    """fresh Inspection package on net8.0, net9.0, and net10.0. Exact Alpha-8
product head `f236c33d8239e80379bf8cf0f1123abd6c93c3cb` passed qualification run
`34797445315`; stable product head `6e9217b16c3023fb10fa34dbaa74afe48448d858`
then passed qualification run `34799272472`, again with all 12 jobs green. PR #43
remains unmerged and no tag or package publication has been performed. See""",
)

roadmap = "Icod.TermInfo-Post-1.0-Development-Roadmap.md"
replace_exact(
    roadmap,
    "**Latest completed line:** `1.12.0` - Advanced Persistent-Raster Placement Semantics and Planning",
    "**Latest completed line:** `1.13.0` - Persistent-Raster Runtime Evidence Interchange and Integration",
)
replace_exact(
    roadmap,
    "**Status:** RE01-RE08 complete; Alpha-8 accepted; stable `1.13.0` promotion validation in progress in draft PR #43",
    "**Status:** RE01-RE08 complete; Alpha-8 and stable `1.13.0` product candidates validated; release-ready in draft PR #43; merge/tag/publication not performed",
)

release_roadmap = "Icod.TermInfo-1.13.0-Persistent-Raster-Runtime-Evidence-Interchange-and-Integration-Roadmap.md"
replace_exact(
    release_roadmap,
    "**Status:** RE01-RE08 complete; Alpha-8 accepted; stable `1.13.0` promotion validation in progress",
    "**Status:** RE01-RE08 complete; Alpha-8 and stable `1.13.0` product candidates validated; release-ready pending merge/tag/publication",
)

audit = "docs/1.13.0-RELEASE-AUDIT.md"
replace_exact(
    audit,
    "RE08 completed and exact `1.13.0-Alpha-8` was accepted. Coordinated stable `1.13.0` is now the promotion candidate and remains subject to a fresh full verification gate before release readiness.",
    "RE08 completed and exact `1.13.0-Alpha-8` was accepted. Coordinated stable `1.13.0` has now passed its full product qualification and is release-ready subject only to final documentation-head validation, merge, tag, and publication gates.",
)
replace_exact(
    audit,
    """## Stable promotion candidate

Stable promotion changes coordinated version identity from `1.13.0-Alpha-8` to `1.13.0` and updates current-facing release documentation/tests only. It introduces no feature semantics, public API, schema, production dependency, target-framework, command, package-consumer-topology, or archive-RID change. A fresh complete matrix is required before stable `1.13.0` may be called release-ready.

## Stable promotion rule""",
    """## Stable promotion validation

Stable promotion changed coordinated version identity from `1.13.0-Alpha-8` to `1.13.0` and updated current-facing release documentation/tests only. It introduced no feature semantics, public API, schema, production dependency, target-framework, command, package-consumer-topology, or archive-RID change.

The first stable qualification attempt correctly exposed one stale historical completion-gate assertion which still required root README install commands to use `1.12.0`. That test ownership was advanced to the current stable line without weakening its package-version or policy checks.

Exact corrected stable product head:

```text
6e9217b16c3023fb10fa34dbaa74afe48448d858
```

passed exact-head qualification run `34799272472` with all 12 jobs green: Windows/Linux/macOS Build+Test, Windows exact whole-1.13 and historical Inspection compatibility, coordinated package packing and verification, isolated package consumption, all three installed-tool smokes, and all six matching archive RID smokes.

This release-audit/documentation closure changes no product API, schema, dependency, target framework, command behavior, package-consumer topology, or archive contents. Its resulting exact head must receive one final full qualification before PR #43 may be considered merge-ready.

## Stable promotion rule""",
)
