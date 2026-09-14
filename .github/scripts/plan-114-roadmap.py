from pathlib import Path

path = Path("Icod.TermInfo-Post-1.0-Development-Roadmap.md")
text = path.read_text(encoding="utf-8")

old_header = """**Current coordinated version:** `1.13.0`
**Next development line:** `TBD`
**Final 1.6 prerelease:** `1.6.0-Alpha-8`
**Final 1.7 prerelease:** `1.7.0-Alpha-8`
**Final 1.8 prerelease:** `1.8.0-Alpha-8`
**Final 1.9 prerelease:** `1.9.0-Alpha-7`
**Final 1.10 prerelease:** `1.10.0-Alpha-8`
**Final 1.11 prerelease:** `1.11.0-Alpha-8`
**Final 1.12 prerelease:** `1.12.0-Alpha-8`
**Final 1.13 prerelease:** `1.13.0-Alpha-8`
**Latest completed line:** `1.13.0` - Persistent-Raster Runtime Evidence Interchange and Integration
**Status:** RE01-RE08 complete; Alpha-8 and stable `1.13.0` product candidates validated; release-ready in draft PR #43; merge/tag/publication not performed
**Planned tranches:** RE01-RE08
**Primary objective:** Add protocol-neutral runtime-evidence interchange and deterministic integration so external verification can strengthen the frozen 1.11 lifecycle and 1.12 placement models without manual evidence construction or source-ordinal management.
**Active development roadmap:** `Icod.TermInfo-1.13.0-Persistent-Raster-Runtime-Evidence-Interchange-and-Integration-Roadmap.md`
**Latest completed release audit:** `docs/1.13.0-RELEASE-AUDIT.md`
"""
new_header = """**Current coordinated version:** `1.13.0`
**Next development line:** `1.14.0` - Raster Backend Capability Evidence, Selection, and Planning
**Final 1.6 prerelease:** `1.6.0-Alpha-8`
**Final 1.7 prerelease:** `1.7.0-Alpha-8`
**Final 1.8 prerelease:** `1.8.0-Alpha-8`
**Final 1.9 prerelease:** `1.9.0-Alpha-7`
**Final 1.10 prerelease:** `1.10.0-Alpha-8`
**Final 1.11 prerelease:** `1.11.0-Alpha-8`
**Final 1.12 prerelease:** `1.12.0-Alpha-8`
**Final 1.13 prerelease:** `1.13.0-Alpha-8`
**Planned final 1.14 prerelease:** `1.14.0-Alpha-8`
**Latest completed line:** `1.13.0` - Persistent-Raster Runtime Evidence Interchange and Integration
**Status:** stable `1.13.0` merged to `main`; 1.14 planning approved and active on branch `1.14.0`
**Planned tranches:** RB01-RB08
**Primary objective:** Add backend-scoped raster availability evidence and deterministic advisory selection so callers can choose among concrete raster backends using explicit preference policy while reusing the frozen 1.11 lifecycle, 1.12 placement, and 1.13 runtime-integration semantics.
**Active development roadmap:** `Icod.TermInfo-1.14.0-Raster-Backend-Capability-Evidence-Selection-and-Planning-Roadmap.md`
**Latest completed release audit:** `docs/1.13.0-RELEASE-AUDIT.md`
"""
if text.count(old_header) != 1:
    raise SystemExit("Expected exactly one active-roadmap header block")
text = text.replace(old_header, new_header, 1)

old_row = "| **1.13.0** | Persistent-raster runtime evidence interchange and integration | Normalize caller-owned runtime observations, integrate conclusive results as existing verified lifecycle/placement evidence, and remove manual evidence/ordinal bridging without owning live verification |\n| **later** | Exotic storage/formats and broader graphics policy | Berkeley DB, historical Unix dialects, multi-protocol preference/negotiation after runtime-evidence interchange is mature, and other deferred work as justified |"
new_row = "| **1.13.0** | Persistent-raster runtime evidence interchange and integration | Normalize caller-owned runtime observations, integrate conclusive results as existing verified lifecycle/placement evidence, and remove manual evidence/ordinal bridging without owning live verification |\n| **1.14.0** | Raster backend capability evidence, selection, and planning | Classify concrete raster-backend availability and deterministically plan selection using explicit caller preference while reusing frozen lifecycle, placement, and runtime-integration semantics |\n| **later** | Exotic storage/formats and broader graphics policy | Berkeley DB, historical Unix dialects, additional raster backends and richer graphics semantics after 1.14, and other deferred work as justified |"
if text.count(old_row) != 1:
    raise SystemExit("Expected exactly one 1.13/later version-table boundary")
text = text.replace(old_row, new_row, 1)

old_governance = "Version 1.13.0 is governed by\n[`Icod.TermInfo-1.13.0-Persistent-Raster-Runtime-Evidence-Interchange-and-Integration-Roadmap.md`](Icod.TermInfo-1.13.0-Persistent-Raster-Runtime-Evidence-Interchange-and-Integration-Roadmap.md)."
new_governance = "The completed 1.13.0 line is governed by\n[`Icod.TermInfo-1.13.0-Persistent-Raster-Runtime-Evidence-Interchange-and-Integration-Roadmap.md`](Icod.TermInfo-1.13.0-Persistent-Raster-Runtime-Evidence-Interchange-and-Integration-Roadmap.md)."
if text.count(old_governance) != 1:
    raise SystemExit("Expected exactly one 1.13 governance introduction")
text = text.replace(old_governance, new_governance, 1)

anchor = "Multi-protocol preference/negotiation remains a later track after runtime-evidence interchange is mature and multiple meaningful backends justify policy."
addition = anchor + "\n\nVersion 1.14.0 is governed by\n[`Icod.TermInfo-1.14.0-Raster-Backend-Capability-Evidence-Selection-and-Planning-Roadmap.md`](Icod.TermInfo-1.14.0-Raster-Backend-Capability-Evidence-Selection-and-Planning-Roadmap.md).\nRB01 freezes the backend-availability evidence and selection vocabulary. RB02 implements conservative backend classification and static Sixel inspection without terminal-brand heuristics. RB03 evaluates backend candidates by delegating to the frozen lifecycle and placement planners. RB04 adds explicit preference-aware selection with no hidden ranking. RB05 composes strengthened 1.13 integration results into backend candidates without changing backend-neutral observations. RB06 adds additive JSON version 6 backend profile and selection-plan documents while preserving v1-v5. RB07 qualifies the loose-coupling boundary against published `Icod.Terminal 1.13.0` and adds a focused sample. RB08 adversarially hardens, fingerprints, documents, and closes the release."
if text.count(anchor) != 1:
    raise SystemExit("Expected exactly one 1.13 closing roadmap sentence")
text = text.replace(anchor, addition, 1)

path.write_text(text, encoding="utf-8", newline="\n")
