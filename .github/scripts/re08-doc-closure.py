from pathlib import Path


def replace_once(path: str, old: str, new: str) -> None:
    file_path = Path(path)
    text = file_path.read_text(encoding="utf-8")
    if new in text:
        return
    if old not in text:
        raise SystemExit(f"Required anchor not found in {path}: {old!r}")
    file_path.write_text(text.replace(old, new, 1), encoding="utf-8", newline="\n")


root_anchor = "The package family targets `net8.0`, `net9.0`, and `net10.0`; packages use C# 13,\n"
root_section = """## 1.13 release-candidate status

Version `1.13.0-Alpha-8` is the release-hardening candidate for the additive
`Icod.TermInfo.Inspection` runtime-evidence interchange layer. Version 1.13 adds
bounded immutable `PersistentRasterRuntime*` observations, deterministic
atomic-per-family conversion of conclusive observations into existing `Verified`
evidence, audit-visible inconclusive observations and integration issues,
planner-delegating replanning conveniences, and additive version-5 JSON for
runtime observation sets and integration results.

The complete candidate Inspection surface contains 90 exported public types with
normalized-LF SHA-256
`fd827a25abafb8e9ff3915567f45f9f2ec51b332bfd8a82dd4e4bca20290e764`.
JSON versions 1 through 4 remain unchanged; version 5 contains exactly
`persistentRasterRuntimeObservationSet` and
`persistentRasterRuntimeIntegration`. Production `Icod.TermInfo.Inspection`
still has no `Icod.Terminal` dependency. Downstream qualification remains pinned
to published `Icod.Terminal 1.12.0`.

Stable `1.12.0` remains the published release while Alpha-8 is being qualified,
so the install commands below intentionally remain at `1.12.0`. See
`docs/1.13.0-PERSISTENT-RASTER-RUNTIME-EVIDENCE-GUIDE.md`,
`docs/1.13.0-INSPECTION-PUBLIC-API-FREEZE.md`, and
`docs/1.13.0-RELEASE-AUDIT.md`.

"""
replace_once("README.md", root_anchor, root_section + root_anchor)

inspection_anchor = "## 1.12 release status\n"
inspection_section = """## 1.13 Alpha-8 release candidate

`1.13.0-Alpha-8` freezes the additive `PersistentRasterRuntime*` interchange
contract. Caller-owned lifecycle and placement runtime observations are immutable,
bounded, canonical, and protocol-neutral. Conclusive observations integrate as
existing `Verified` evidence through the frozen classifiers; inconclusive
observations remain audit-visible, and represented family capacity/ordinal
limitations fail atomically per family.

`PersistentRasterRuntimeIntegrationResult` delegates lifecycle and placement
replanning to the existing planners. JSON version 5 adds exactly
`persistentRasterRuntimeObservationSet` and
`persistentRasterRuntimeIntegration`, while JSON versions 1 through 4 remain
byte-frozen. The whole candidate Inspection reflection manifest contains 90
exported public types and has normalized-LF SHA-256
`fd827a25abafb8e9ff3915567f45f9f2ec51b332bfd8a82dd4e4bca20290e764`.

Inspection still has no production `Icod.Terminal` dependency. RE07 qualifies
the caller adapter boundary using published `Icod.Terminal 1.12.0` beside the
fresh Inspection package on net8.0, net9.0, and net10.0. See
`../docs/1.13.0-PERSISTENT-RASTER-RUNTIME-EVIDENCE-GUIDE.md`,
`../docs/1.13.0-INSPECTION-PUBLIC-API-FREEZE.md`, and
`../docs/1.13.0-RELEASE-AUDIT.md`.

"""
replace_once(
    "Icod.TermInfo.Inspection/README.md",
    inspection_anchor,
    inspection_section + inspection_anchor,
)

versioning_anchor = "## 1.12 release line\n"
versioning_section = """## 1.13 release line

The RE01-RE08 development sequence is `1.13.0-Alpha-1` through
`1.13.0-Alpha-8`. Version 1.13 adds compatible public API only to
`Icod.TermInfo.Inspection` for caller-owned persistent-raster runtime observations,
deterministic integration into the frozen 1.11 lifecycle and 1.12 placement
evidence models, planner-delegating replanning, and additive version-5 JSON.
Runtime, Source, Compiler, and Termcap public APIs remain frozen.

RE08 freezes the complete 1.13 Inspection reflection manifest at 90 exported
public types with normalized-LF SHA-256
`fd827a25abafb8e9ff3915567f45f9f2ec51b332bfd8a82dd4e4bca20290e764`.
Release verification first requires that exact current 1.13 surface, then removes
only the reviewed six RE06 renderer members and nine cumulative
`PersistentRasterRuntime*` type blocks to reproduce frozen 1.12 SHA-256
`f71501dcd27a530051c1a02083325144ced2b6173b6b967b9571c620815198f0`.
JSON versions 1 through 4 remain immutable; version 5 contains exactly the
runtime observation-set and integration-audit document kinds.

After the exact Alpha-8 head passes the complete Staging package, historical and
RE07 package-consumer/sample, installed-tool, and six-RID archive gates, stable
`1.13.0` is a promotion-only transition. Promotion may change coordinated
release identity and stable-facing documentation only; it may not introduce
feature semantics, public API, schema fields, production dependencies, target
frameworks, command behavior, or archive RIDs, and it requires its own fresh full
validation.

"""
replace_once("docs/VERSIONING.md", versioning_anchor, versioning_section + versioning_anchor)

compat_anchor = "## 1.12 compatibility freeze\n"
compat_section = """## 1.13 compatibility freeze

Version 1.13 is additive above the stable 1.12 boundary. RE08 freezes the complete
1.13 Inspection reflection manifest at 90 exported public types with normalized-LF
SHA-256
`fd827a25abafb8e9ff3915567f45f9f2ec51b332bfd8a82dd4e4bca20290e764`
and requires equivalent public API across `net8.0`, `net9.0`, and `net10.0`.
The verifier first proves that complete 1.13 surface, then removes exactly the
reviewed RE06 renderer-member delta and cumulative `PersistentRasterRuntime*`
type delta to reconstruct frozen 1.12 SHA-256
`f71501dcd27a530051c1a02083325144ced2b6173b6b967b9571c620815198f0`.
The established 1.12 -> 1.11 -> 1.10 reconstruction chain remains unchanged.

Version 1.13 adds only protocol-neutral caller-owned runtime observation and
evidence-integration semantics to Inspection. It does not add live probing,
backend ranking, protocol negotiation, raw protocol responses, terminal session
or resource identity, or a production dependency on `Icod.Terminal`.

JSON schema versions 1 through 4 remain immutable historical contracts. Version
5 is additive and contains exactly `persistentRasterRuntimeObservationSet` and
`persistentRasterRuntimeIntegration`. Stable 1.13 promotion may not change any
frozen schema, the exact 1.13 public surface, package dependency direction,
target frameworks, command semantics, package-consumer topology, or archive RIDs.

"""
replace_once("docs/COMPATIBILITY.md", compat_anchor, compat_section + compat_anchor)

roadmap = Path("Icod.TermInfo-Post-1.0-Development-Roadmap.md")
roadmap_text = roadmap.read_text(encoding="utf-8")
replacements = {
    "**Current coordinated version:** `1.12.0`": "**Current coordinated version:** `1.13.0-Alpha-8`",
    "**Final 1.12 prerelease:** `1.12.0-Alpha-8`": "**Final 1.12 prerelease:** `1.12.0-Alpha-8`\n**Final 1.13 prerelease:** `1.13.0-Alpha-8`",
    "**Status:** 1.13.0 planning approved; draft PR #43 open; implementation not yet started": "**Status:** RE01-RE07 accepted; RE08 `1.13.0-Alpha-8` release hardening and whole-surface freeze in progress in draft PR #43",
}
for old, new in replacements.items():
    if old not in roadmap_text and new not in roadmap_text:
        raise SystemExit(f"Required roadmap anchor not found: {old!r}")
    roadmap_text = roadmap_text.replace(old, new, 1)
roadmap.write_text(roadmap_text, encoding="utf-8", newline="\n")
