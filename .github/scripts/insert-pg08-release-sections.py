from pathlib import Path


def insert_before(path: str, anchor: str, block: str) -> None:
    file_path = Path(path)
    text = file_path.read_text(encoding="utf-8")
    normalized_block = block.rstrip("\n") + "\n\n"
    if normalized_block in text:
        return
    count = text.count(anchor)
    if count != 1:
        raise RuntimeError(f"{path}: expected exactly one anchor {anchor!r}, found {count}")
    file_path.write_text(
        text.replace(anchor, normalized_block + anchor, 1),
        encoding="utf-8",
        newline="\n",
    )


insert_before(
    "README.md",
    "## 1.11 release status\n",
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
)

insert_before(
    "Icod.TermInfo.Inspection/README.md",
    "## 1.11 release status\n",
    """## 1.12 release candidate

Version `1.12.0-Alpha-8` freezes the additive advanced persistent-raster
placement surface. Inspection now classifies and plans two placement semantics:
`SourceRectangle` and `SignedZOrder`. The new placement planner consumes the
already-produced 1.11 lifecycle plan rather than reinterpreting lifecycle
support, and database-set composition preserves the frozen ordered precedence
boundary.

The complete 1.12 Inspection reflection manifest contains 81 exported public
types and has normalized-LF SHA-256
`f71501dcd27a530051c1a02083325144ced2b6173b6b967b9571c620815198f0`.
Removing only the reviewed 1.12 placement type/member delta must reconstruct the
frozen 1.11 manifest exactly. JSON versions 1-3 remain immutable; version 4 adds
only `persistentRasterPlacementProfile` and `persistentRasterPlacementPlan`.

Inspection retains no production `Icod.Terminal` dependency. The dedicated PG07
package consumer and placement sample qualify `Icod.Terminal 1.12.0` only at the
downstream application boundary. Concrete rectangle coordinates and z-order
integers remain consumer-owned execution values.

See `docs/1.12.0-ADVANCED-PERSISTENT-RASTER-PLACEMENT-GUIDE.md`,
`docs/1.12.0-INSPECTION-PUBLIC-API-FREEZE.md`, and
`docs/1.12.0-RELEASE-AUDIT.md`.
""",
)

insert_before(
    "docs/VERSIONING.md",
    "## 1.11 release line\n",
    """## 1.12 release line

The PG01-PG08 development sequence is `1.12.0-Alpha-1` through
`1.12.0-Alpha-8`. Version 1.12 adds compatible public API only to
`Icod.TermInfo.Inspection` for advanced persistent-raster placement evidence,
classification, lifecycle-aware semantic planning, description/database-set
composition, and additive version-4 profile/plan JSON automation. Runtime,
Source, Compiler, and Termcap public APIs remain frozen.

PG08 freezes the complete 1.12 Inspection reflection manifest at 81 exported
public types with normalized-LF SHA-256
`f71501dcd27a530051c1a02083325144ced2b6173b6b967b9571c620815198f0`.
Release verification independently removes only the reviewed 1.12 placement
public type/member delta and requires the remainder to reproduce the frozen 1.11
manifest fingerprint exactly. JSON versions 1 through 3 remain immutable;
version 4 is additive and contains exactly the placement profile and plan
document kinds.

After the exact Alpha-8 head passes the complete Staging package, package-only
consumer, sample, installed-tool, and six-RID archive gates, stable `1.12.0` is a
promotion-only transition. Promotion may change coordinated release identity and
stable-facing documentation only; it may not introduce feature semantics,
public API, schema fields, production dependencies, target frameworks, command
behavior, or archive RIDs, and it requires its own fresh full validation.
""",
)

insert_before(
    "docs/COMPATIBILITY.md",
    "## 1.11 compatibility freeze\n",
    """## 1.12 compatibility freeze

Version 1.12 is additive above the stable 1.11 boundary. PG08 freezes the
complete 1.12 Inspection reflection manifest at 81 exported public types with
normalized-LF SHA-256
`f71501dcd27a530051c1a02083325144ced2b6173b6b967b9571c620815198f0`
and requires exact public API equality across `net8.0`, `net9.0`, and
`net10.0`. Removing only the exact reviewed 1.12 placement type/member delta
must reconstruct the frozen 1.11 public API fingerprint exactly.

Version 1.12 adds only the protocol-neutral `SourceRectangle` and `SignedZOrder`
placement semantic family to Inspection. It does not add concrete placement
coordinates, z-order values, resource identities, live probing, wire protocol
selection, terminal I/O, or a production dependency on `Icod.Terminal`.

JSON schema versions 1, 2, and 3 remain immutable historical contracts. Version
4 is additive and contains exactly `persistentRasterPlacementProfile` and
`persistentRasterPlacementPlan`. Stable 1.12 promotion may not change any frozen
schema, the exact 1.12 public surface, package dependency direction, target
frameworks, command semantics, or archive topology.
""",
)
