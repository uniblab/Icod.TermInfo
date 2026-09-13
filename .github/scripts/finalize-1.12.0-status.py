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


replace_once(
    "README.md",
    """Production `Icod.TermInfo` and `Icod.TermInfo.Inspection` still do not depend on
`Icod.Terminal`. The Alpha-8 contract passed workflow #725 / `34763187115`.
Stable promotion is version/documentation-only and is validated separately before
merge or publication. See
""",
    """Production `Icod.TermInfo` and `Icod.TermInfo.Inspection` still do not depend on
`Icod.Terminal`. The Alpha-8 contract passed workflow #725 / `34763187115`.
The stable promotion passed workflow #731 / `34764253699` on exact head
`1674c008f1df2cbe82295b8eaa9ae9d34ac5d999`, with all 12 jobs green.
Publication remains gated by merge to `main` and the normal immutable-tag release
workflow. See
""",
)
replace_once(
    "README.md",
    """Applications which need canonical rendering, semantic comparison, provider-aware
inspection, database-set automation, or persistent-raster lifecycle planning use:
""",
    """Applications which need canonical rendering, semantic comparison, provider-aware
inspection, database-set automation, or persistent-raster lifecycle/placement
planning use:
""",
)

replace_once(
    "Icod.TermInfo.Inspection/README.md",
    """Inspection retains no production `Icod.Terminal` dependency. The dedicated PG07
package consumer and placement sample qualify `Icod.Terminal 1.12.0` only at the
downstream application boundary. Concrete rectangle coordinates and z-order
integers remain consumer-owned execution values.

See `docs/1.12.0-ADVANCED-PERSISTENT-RASTER-PLACEMENT-GUIDE.md`,
""",
    """Inspection retains no production `Icod.Terminal` dependency. The dedicated PG07
package consumer and placement sample qualify `Icod.Terminal 1.12.0` only at the
downstream application boundary. Concrete rectangle coordinates and z-order
integers remain consumer-owned execution values.

The Alpha-8 product contract passed workflow #725 / `34763187115`; the stable
promotion passed workflow #731 / `34764253699` on exact head
`1674c008f1df2cbe82295b8eaa9ae9d34ac5d999`, with all 12 jobs green. Publication
remains gated by merge to `main` and the normal immutable-tag release workflow.

See `docs/1.12.0-ADVANCED-PERSISTENT-RASTER-PLACEMENT-GUIDE.md`,
""",
)

replace_once(
    "docs/1.12.0-RELEASE-AUDIT.md",
    """PG01-PG08 are complete. Exact Alpha-8 product head `d44fd1ae6c515aab803ac692543474cfa6849282` passed pull-request workflow #725 / run `34763187115` with all 12 jobs green.

The coordinated version and current-facing metadata are now promoted to stable `1.12.0` without feature, public API, schema, dependency, target-framework, command-semantic, or archive-RID changes. The stable promotion requires its own fresh full validation before merge or publication.
""",
    """PG01-PG08 are complete. Exact Alpha-8 product head `d44fd1ae6c515aab803ac692543474cfa6849282` passed pull-request workflow #725 / run `34763187115` with all 12 jobs green.

Stable promotion head `1674c008f1df2cbe82295b8eaa9ae9d34ac5d999` passed pull-request workflow #731 / run `34764253699` with all 12 jobs green. The coordinated version is `1.12.0`; the promotion changed release identity/current-facing documentation only and introduced no feature, public API, schema, dependency, target-framework, command-semantic, or archive-RID change.

PR #42 remains open and unmerged. No tag, registry publication, or GitHub release is created by this audit.
""",
)

replace_once(
    "Icod.TermInfo-Post-1.0-Development-Roadmap.md",
    """**Current coordinated version:** `1.12.0-Alpha-1`
**Final 1.6 prerelease:** `1.6.0-Alpha-8`
**Final 1.7 prerelease:** `1.7.0-Alpha-8`
**Final 1.8 prerelease:** `1.8.0-Alpha-8`
**Final 1.9 prerelease:** `1.9.0-Alpha-7`
**Final 1.10 prerelease:** `1.10.0-Alpha-8`
**Final 1.11 prerelease:** `1.11.0-Alpha-8`
**Next development line:** `1.12.0` - Advanced Persistent-Raster Placement Semantics and Planning
**Status:** 1.12.0 implementation in progress
**Current tranche:** PG01 - Architecture, vocabulary, and public API regret gate
**Primary objective:** Add protocol-neutral source-rectangle and signed-z-order placement semantics beside the frozen 1.11 persistent-raster lifecycle model, preserving JSON v1-v3 and downstream execution ownership.
""",
    """**Current coordinated version:** `1.12.0`
**Final 1.6 prerelease:** `1.6.0-Alpha-8`
**Final 1.7 prerelease:** `1.7.0-Alpha-8`
**Final 1.8 prerelease:** `1.8.0-Alpha-8`
**Final 1.9 prerelease:** `1.9.0-Alpha-7`
**Final 1.10 prerelease:** `1.10.0-Alpha-8`
**Final 1.11 prerelease:** `1.11.0-Alpha-8`
**Final 1.12 prerelease:** `1.12.0-Alpha-8`
**Latest completed line:** `1.12.0` - Advanced Persistent-Raster Placement Semantics and Planning
**Status:** 1.12.0 implementation and stable promotion complete; PR #42 remains open pending merge
**Completed tranches:** PG01-PG08
**Primary objective:** Completed - protocol-neutral source-rectangle and signed-z-order placement semantics beside the frozen 1.11 persistent-raster lifecycle model, preserving JSON v1-v3 and downstream execution ownership.
**Release audit:** `docs/1.12.0-RELEASE-AUDIT.md`
""",
)

replace_once(
    "Icod.TermInfo-1.12.0-Advanced-Persistent-Raster-Placement-Semantics-and-Planning-Roadmap.md",
    """**Frozen contracts:** existing 1.x Runtime/Source/Compiler/Termcap APIs, Inspection contracts through 1.11, JSON schemas v1-v3, database-set precedence, and persistent-raster lifecycle semantics except for unavoidable defect corrections

---
""",
    """**Frozen contracts:** existing 1.x Runtime/Source/Compiler/Termcap APIs, Inspection contracts through 1.11, JSON schemas v1-v3, database-set precedence, and persistent-raster lifecycle semantics except for unavoidable defect corrections  
**Status:** Completed; stable `1.12.0` promotion validated  
**Alpha-8 witness:** workflow #725 / `34763187115`  
**Stable promotion witness:** workflow #731 / `34764253699`  
**Release audit:** `docs/1.12.0-RELEASE-AUDIT.md`

---
""",
)
