from pathlib import Path


PATH = Path("Icod.TermInfo-Post-1.0-Development-Roadmap.md")


def replace_once(text: str, old: str, new: str) -> str:
    count = text.count(old)
    if count != 1:
        raise RuntimeError(
            f"Expected exactly one roadmap match, found {count}: {old[:120]!r}"
        )
    return text.replace(old, new, 1)


text = PATH.read_text(encoding="utf-8")

text = replace_once(
    text,
    """**Current coordinated version:** `1.12.0`
**Final 1.6 prerelease:** `1.6.0-Alpha-8`
""",
    """**Current coordinated version:** `1.12.0`
**Next development line:** `1.13.0`
**Final 1.6 prerelease:** `1.6.0-Alpha-8`
""",
)

text = replace_once(
    text,
    """**Latest completed line:** `1.12.0` - Advanced Persistent-Raster Placement Semantics and Planning
**Status:** 1.12.0 implementation and stable promotion complete; PR #42 remains open pending merge
**Completed tranches:** PG01-PG08
**Primary objective:** Completed - protocol-neutral source-rectangle and signed-z-order placement semantics beside the frozen 1.11 persistent-raster lifecycle model, preserving JSON v1-v3 and downstream execution ownership.
**Release audit:** `docs/1.12.0-RELEASE-AUDIT.md`
""",
    """**Latest completed line:** `1.12.0` - Advanced Persistent-Raster Placement Semantics and Planning
**Status:** 1.13.0 planning approved; draft PR #43 open; implementation not yet started
**Planned tranches:** RE01-RE08
**Primary objective:** Add protocol-neutral runtime-evidence interchange and deterministic integration so external verification can strengthen the frozen 1.11 lifecycle and 1.12 placement models without manual evidence construction or source-ordinal management.
**Active development roadmap:** `Icod.TermInfo-1.13.0-Persistent-Raster-Runtime-Evidence-Interchange-and-Integration-Roadmap.md`
**Latest completed release audit:** `docs/1.12.0-RELEASE-AUDIT.md`
""",
)

text = replace_once(
    text,
    """| **1.11.0** | Persistent-raster lifecycle semantics and planning | Classify protocol-neutral persistent-raster lifecycle evidence and produce deterministic advisory plans without owning terminal execution |
| **1.12.0** | Advanced persistent-raster placement semantics and planning | Classify source-rectangle and signed-z-order placement support and compose those requirements with the frozen 1.11 lifecycle model |
| **later** | Exotic storage/formats and broader graphics policy | Berkeley DB, historical Unix dialects, multi-protocol preference/negotiation, and other deferred work as justified |
""",
    """| **1.11.0** | Persistent-raster lifecycle semantics and planning | Classify protocol-neutral persistent-raster lifecycle evidence and produce deterministic advisory plans without owning terminal execution |
| **1.12.0** | Advanced persistent-raster placement semantics and planning | Classify source-rectangle and signed-z-order placement support and compose those requirements with the frozen 1.11 lifecycle model |
| **1.13.0** | Persistent-raster runtime evidence interchange and integration | Normalize caller-owned runtime observations, integrate conclusive results as existing verified lifecycle/placement evidence, and remove manual evidence/ordinal bridging without owning live verification |
| **later** | Exotic storage/formats and broader graphics policy | Berkeley DB, historical Unix dialects, multi-protocol preference/negotiation after runtime-evidence interchange is mature, and other deferred work as justified |
""",
)

text = replace_once(
    text,
    """The completed 1.5 release contract is recorded in
""",
    """Version 1.13.0 is governed by
[`Icod.TermInfo-1.13.0-Persistent-Raster-Runtime-Evidence-Interchange-and-Integration-Roadmap.md`](Icod.TermInfo-1.13.0-Persistent-Raster-Runtime-Evidence-Interchange-and-Integration-Roadmap.md).
RE01 freezes the runtime-observation vocabulary and API regret gate while preserving the frozen 1.11 lifecycle and 1.12 placement semantics. RE02 adds immutable bounded runtime observations. RE03 maps conclusive observations into existing `Verified` evidence with deterministic safe ordinal assignment. RE04 proves integration through the frozen classifiers and contradiction rules. RE05 composes strengthened profiles back through the frozen planners without inventing a replacement planning model. RE06 adds additive JSON version 5 observation/integration documents while preserving v1-v4. RE07 qualifies loose coupling with `Icod.Terminal` and removes hand-written evidence/ordinal bridging from the downstream sample. RE08 hardens, fingerprints, documents, and closes the release. Multi-protocol preference/negotiation remains a later track after runtime-evidence interchange is mature and multiple meaningful backends justify policy.

The completed 1.5 release contract is recorded in
""",
)

PATH.write_text(text, encoding="utf-8", newline="\n")
