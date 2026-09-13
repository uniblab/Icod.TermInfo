from pathlib import Path


def read(path: str) -> str:
    return Path(path).read_text(encoding="utf-8")


def write(path: str, text: str) -> None:
    Path(path).write_text(text, encoding="utf-8", newline="\n")


def replace_once(path: str, old: str, new: str) -> None:
    text = read(path)
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f"{path}: expected one occurrence, found {count}: {old!r}")
    write(path, text.replace(old, new, 1))


def replace_count(path: str, old: str, new: str, expected: int) -> None:
    text = read(path)
    count = text.count(old)
    if count != expected:
        raise RuntimeError(f"{path}: expected {expected} occurrences, found {count}: {old!r}")
    write(path, text.replace(old, new))


replace_once(
    "Directory.Build.props",
    "<IcodTermInfoSuiteVersion>1.12.0-Alpha-8</IcodTermInfoSuiteVersion>",
    "<IcodTermInfoSuiteVersion>1.12.0</IcodTermInfoSuiteVersion>",
)

replace_once(
    "README.md",
    """Version `1.11.0` is the current stable coordinated release. It adds
protocol-neutral persistent-raster lifecycle evidence, classification, planning,
description/database-set composition, and version-3 machine-readable profile and
plan documents through `Icod.TermInfo.Inspection` while preserving the frozen
Runtime, Source, Compiler, Termcap, synthesis, planning, and version-1/version-2
JSON contracts.
""",
    """Version `1.12.0` is the current stable coordinated release. It adds
protocol-neutral advanced persistent-raster placement evidence, classification,
lifecycle-aware planning, description/database-set composition, and additive
version-4 profile/plan JSON through `Icod.TermInfo.Inspection` while preserving
the frozen 1.11 lifecycle surface and version-1/version-2/version-3 JSON
contracts.
""",
)
replace_once(
    "README.md",
    """## 1.12 release candidate

`1.12.0-Alpha-8` is the frozen candidate for advanced persistent-raster
placement semantics and planning. The 1.12 Inspection layer adds exactly two
protocol-neutral placement requirements: pixel-space source rectangles and
signed z-order. It composes those requirements with the frozen 1.11 lifecycle
planner, ordered database-set precedence, and additive version-4 profile/plan
JSON without adding live graphics execution to TermInfo.

The complete candidate Inspection API is frozen at 81 exported public types with
normalized-LF SHA-256
`f71501dcd27a530051c1a02083325144ced2b6173b6b967b9571c620815198f0`.
JSON versions 1 through 3 remain unchanged; version 4 contains exactly
`persistentRasterPlacementProfile` and `persistentRasterPlacementPlan`.

Production `Icod.TermInfo` and `Icod.TermInfo.Inspection` still do not depend on
`Icod.Terminal`. PG07 qualifies the semantic handoff separately against published
`Icod.Terminal 1.12.0` through a package-only consumer and the
`Icod.TermInfo.PersistentRasterPlacement.Sample` on `net8.0`, `net9.0`, and
`net10.0`.

Stable install examples below intentionally remain at `1.11.0` until Alpha-8
passes the full release matrix and a separate promotion-only commit advances the
coordinated release to stable `1.12.0`. See
`docs/1.12.0-ADVANCED-PERSISTENT-RASTER-PLACEMENT-GUIDE.md` and
`docs/1.12.0-RELEASE-AUDIT.md`.
""",
    """## 1.12 release status

Version `1.12.0` promotes the fully validated `1.12.0-Alpha-8` contract without
feature, public API, schema, dependency, target-framework, command-semantic, or
archive-RID changes. The 1.12 Inspection layer adds exactly two protocol-neutral
placement requirements: pixel-space source rectangles and signed z-order, and
composes them with the frozen 1.11 lifecycle planner and ordered database-set
precedence.

The complete Inspection API remains frozen at 81 exported public types with
normalized-LF SHA-256
`f71501dcd27a530051c1a02083325144ced2b6173b6b967b9571c620815198f0`.
JSON versions 1 through 3 remain unchanged; version 4 contains exactly
`persistentRasterPlacementProfile` and `persistentRasterPlacementPlan`.

Production `Icod.TermInfo` and `Icod.TermInfo.Inspection` still do not depend on
`Icod.Terminal`. The Alpha-8 contract passed workflow #725 / `34763187115`.
Stable promotion is version/documentation-only and is validated separately before
merge or publication. See
`docs/1.12.0-ADVANCED-PERSISTENT-RASTER-PLACEMENT-GUIDE.md` and
`docs/1.12.0-RELEASE-AUDIT.md`.
""",
)
replace_count( "README.md", "--version 1.11.0", "--version 1.12.0", 6 )
replace_once(
    "README.md",
    "`docs/1.11.0-RELEASE-AUDIT.md`.",
    "`docs/1.12.0-RELEASE-AUDIT.md`.",
)
replace_once(
    "README.md",
    "Each 1.11.0 archive contains the traditional `tic`, `infocmp`, `toe`,",
    "Each 1.12.0 archive contains the traditional `tic`, `infocmp`, `toe`,",
)
replace_once(
    "README.md",
    "Version 1.11 adds protocol-neutral persistent-raster lifecycle evidence and planning to Inspection while preserving that live-session ownership boundary. PTYs, terminal emulation, and graphics protocol execution remain separate later or sibling work.",
    "Version 1.11 adds protocol-neutral persistent-raster lifecycle evidence and planning to Inspection while preserving that live-session ownership boundary. Version 1.12 adds protocol-neutral source-rectangle and signed-z-order placement semantics and planning while keeping concrete execution values and live protocol work downstream. PTYs, terminal emulation, and graphics protocol execution remain separate later or sibling work.",
)

replace_once(
    "Icod.TermInfo.Inspection/README.md",
    "## 1.12 release candidate",
    "## 1.12 release status",
)
replace_once(
    "Icod.TermInfo.Inspection/README.md",
    "Version `1.12.0-Alpha-8` freezes the additive advanced persistent-raster\nplacement surface.",
    "Version `1.12.0` promotes the additive advanced persistent-raster placement\nsurface frozen by `1.12.0-Alpha-8` without semantic, API, schema, dependency,\ntarget-framework, or command changes.",
)
replace_once(
    "Icod.TermInfo.Inspection/README.md",
    "dotnet add package Icod.TermInfo.Inspection --version 1.11.0",
    "dotnet add package Icod.TermInfo.Inspection --version 1.12.0",
)

replace_count(
    "tests/Icod.TermInfo.Tests/src/T45CompletionGateTests.cs",
    "--version 1.11.0",
    "--version 1.12.0",
    6,
)
replace_once(
    "tests/Icod.TermInfo.Tests/src/T45CompletionGateTests.cs",
    """\t\tAssert.Contains(
\t\t\t\"docs/1.11.0-RELEASE-AUDIT.md\",
\t\t\treadme
\t\t);
""",
    """\t\tAssert.Contains(
\t\t\t\"docs/1.11.0-RELEASE-AUDIT.md\",
\t\t\treadme
\t\t);
\t\tAssert.Contains(
\t\t\t\"docs/1.12.0-RELEASE-AUDIT.md\",
\t\t\treadme
\t\t);
""",
)

replace_once(
    "tests/Icod.TermInfo.Inspection.Tests/src/PG08ReleaseClosureTests.cs",
    "public void ReleaseFacingMetadataDescribesOneTwelveAlphaEight()",
    "public void ReleaseFacingMetadataDescribesStableOneTwelve()",
)
replace_once(
    "tests/Icod.TermInfo.Inspection.Tests/src/PG08ReleaseClosureTests.cs",
    "<IcodTermInfoSuiteVersion>1.12.0-Alpha-8</IcodTermInfoSuiteVersion>",
    "<IcodTermInfoSuiteVersion>1.12.0</IcodTermInfoSuiteVersion>",
)

replace_once(
    "docs/1.12.0-RELEASE-AUDIT.md",
    """The coordinated 1.12 line is assembled as `1.12.0-Alpha-8` for final product validation. This audit does not claim Alpha-8 acceptance until the exact Alpha-8 head has completed the full Staging matrix successfully.

The stable baseline remains `1.11.0` until a separately validated promotion-only commit advances the coordinated version and current-facing release metadata to `1.12.0`.
""",
    """PG01-PG08 are complete. Exact Alpha-8 product head `d44fd1ae6c515aab803ac692543474cfa6849282` passed pull-request workflow #725 / run `34763187115` with all 12 jobs green.

The coordinated version and current-facing metadata are now promoted to stable `1.12.0` without feature, public API, schema, dependency, target-framework, command-semantic, or archive-RID changes. The stable promotion requires its own fresh full validation before merge or publication.
""",
)
