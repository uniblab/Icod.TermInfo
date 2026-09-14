from pathlib import Path


readme_path = Path("README.md")
text = readme_path.read_text(encoding="utf-8")


def replace_once(old: str, new: str) -> None:
    global text
    count = text.count(old)
    if count != 1:
        raise SystemExit(f"Expected exactly one README anchor, found {count}: {old!r}")
    text = text.replace(old, new, 1)


replace_once(
    "The repository contains six executable API samples plus one command-suite\nwalkthrough with deliberately different purposes.",
    "The repository contains seven executable API samples plus one command-suite\nwalkthrough with deliberately different purposes.",
)
replace_once(
    "All six executable API sample projects target `net8.0`, `net9.0`, and\n`net10.0`; `dotnet run` therefore needs an explicit framework.",
    "All seven executable API sample projects target `net8.0`, `net9.0`, and\n`net10.0`; `dotnet run` therefore needs an explicit framework.",
)
replace_once(
    "Applications which need canonical rendering, semantic comparison, provider-aware\ninspection, database-set automation, or persistent-raster lifecycle/placement\nplanning use:",
    "Applications which need canonical rendering, semantic comparison, provider-aware\ninspection, database-set automation, persistent-raster lifecycle/placement\nplanning, or 1.13 runtime-evidence interchange and integration use:",
)
replace_once(
    """`samples/Icod.TermInfo.PersistentRasterLifecycle.Sample` is the focused 1.11
reusable-API example. It starts from ordinary Sixel evidence, demonstrates that
persistent upload and placement remain `Unknown`, plans an indeterminate request,
then appends caller-owned `Verified` evidence, reclassifies, and obtains a
successful protocol-neutral upload/placement plan. It also renders the version-3
profile and plan JSON documents. The sample performs no terminal I/O and has no
`Icod.Terminal` dependency.""",
    """`samples/Icod.TermInfo.PersistentRasterLifecycle.Sample` is the focused 1.11
lifecycle example updated for the 1.13 integration path. It starts from ordinary
Sixel evidence, demonstrates that persistent upload and placement remain
`Unknown`, plans an indeterminate request, then represents consumer-owned runtime
results as immutable lifecycle observations. `PersistentRasterRuntimeEvidenceIntegrator`
maps the conclusive observations into existing `Verified` evidence with safe final
ordinals, and `CreateLifecyclePlan(...)` delegates replanning to the frozen
lifecycle planner. The sample performs no terminal I/O and has no `Icod.Terminal`
dependency.""",
)
replace_once(
    """The consumer then contributes its own `Verified` evidence, reclassifies the
placement profile, and obtains a `Satisfied` plan. The sample renders both the
version-4 placement profile and placement plan before constructing any concrete
Terminal execution values. Only after semantic planning succeeds does it create
a `TerminalRasterSourceRectangle` and signed `ZIndex`.""",
    """The consumer then contributes immutable placement runtime observations for
`SourceRectangle` and `SignedZOrder`. `PersistentRasterRuntimeEvidenceIntegrator`
maps those conclusive observations into the frozen placement evidence model, and
`CreatePlacementPlan(...)` delegates replanning to produce `Satisfied`. Only after
TermInfo has finished semantic planning does the sample create a
`TerminalRasterSourceRectangle` and signed `ZIndex`.""",
)
replace_once(
    """Release verification executes the sample on `net8.0`, `net9.0`, and `net10.0`.
See `samples/Icod.TermInfo.PersistentRasterPlacement.Sample/README.md` and
`docs/1.12.0-ADVANCED-PERSISTENT-RASTER-PLACEMENT-GUIDE.md`.

### Managed tool-suite walkthrough""",
    """Release verification executes the sample on `net8.0`, `net9.0`, and `net10.0`.
See `samples/Icod.TermInfo.PersistentRasterPlacement.Sample/README.md` and
`docs/1.12.0-ADVANCED-PERSISTENT-RASTER-PLACEMENT-GUIDE.md`.

### Persistent-raster runtime-integration sample

`samples/Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample` is the focused
1.13 caller-adapter example. It begins with a static persistent-raster lifecycle
plan that requires runtime verification, then optionally asks published
`Icod.Terminal 1.12.0` to verify its coarse `PersistentRasterGraphics` semantic
capability. Consumer code maps that sibling-layer result into TermInfo's
protocol-neutral `Supported` / `Unsupported` / `Inconclusive` runtime outcomes,
expands the coarse capability into the explicitly chosen lifecycle subjects, and
passes the resulting observations to `PersistentRasterRuntimeEvidenceIntegrator`.

The default mode is deterministic and performs no terminal I/O; `--live` performs
the actual `VerifyCapabilityAsync(...)` call on an interactive terminal. Static
advertisement is never promoted to runtime support: non-live evidence and
`Unknown` / `Advertised` support map to `Inconclusive`. The sample then renders
the version-5 integration audit and replans through `CreateLifecyclePlan(...)`.

Run the deterministic form with:

```text
dotnet run --project samples/Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample/Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample.csproj -f net10.0
```

For interactive verification:

```text
dotnet run --project samples/Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample/Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample.csproj -f net10.0 -- --live
```

Release verification executes the deterministic form on `net8.0`, `net9.0`, and
`net10.0`. See
`samples/Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample/README.md` and
`docs/1.13.0-PERSISTENT-RASTER-RUNTIME-EVIDENCE-GUIDE.md`.

### Managed tool-suite walkthrough""",
)
replace_once(
    """`samples/Icod.TermInfo.PersistentRasterLifecycle.Sample/README.md`,
`samples/Icod.TermInfo.PersistentRasterPlacement.Sample/README.md`, and
`docs/0.9.0-ACQUISITION-GUIDE.md` for the complete examples.""",
    """`samples/Icod.TermInfo.PersistentRasterLifecycle.Sample/README.md`,
`samples/Icod.TermInfo.PersistentRasterPlacement.Sample/README.md`,
`samples/Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample/README.md`, and
`docs/0.9.0-ACQUISITION-GUIDE.md` for the complete examples.""",
)
replace_once(
    """- **`Icod.TermInfo.Inspection`** — canonical effective/source rendering, relative-source synthesis and parent planning, structured semantic comparison, provider/database-set inspection, persistent-raster lifecycle and advanced-placement evidence/classification/planning, and version-3/version-4 machine-readable views;""",
    """- **`Icod.TermInfo.Inspection`** — canonical effective/source rendering, relative-source synthesis and parent planning, structured semantic comparison, provider/database-set inspection, persistent-raster lifecycle and advanced-placement evidence/classification/planning, protocol-neutral runtime-evidence interchange/integration, and versioned machine-readable views through JSON version 5;""",
)

section = """## What 1.13 adds

Version 1.13.0 adds protocol-neutral runtime-evidence interchange above the
frozen 1.11 lifecycle and 1.12 placement models without moving live terminal I/O
into TermInfo:

- immutable bounded `PersistentRasterRuntimeLifecycleObservation` and
  `PersistentRasterRuntimePlacementObservation` values plus canonical observation
  sets;
- deterministic mapping of conclusive runtime outcomes into the existing
  `Verified` evidence model with safe final source ordinals;
- atomic-per-family handling of evidence-capacity and ordinal-space exhaustion;
- audit-visible `Inconclusive` observations and structured integration issues;
- `CreateLifecyclePlan(...)` and `CreatePlacementPlan(...)` conveniences which
  delegate directly to the existing frozen planners;
- additive JSON version 5 documents for
  `persistentRasterRuntimeObservationSet` and
  `persistentRasterRuntimeIntegration`; and
- package-only qualification against published `Icod.Terminal 1.12.0` while the
  production `Icod.TermInfo.Inspection` package remains free of any
  `Icod.Terminal` dependency.

The intended consumer flow is:

```text
static TermInfo evidence -> classify / plan -> runtime verification required
    -> caller-owned verifier -> runtime observations
    -> PersistentRasterRuntimeEvidenceIntegrator -> frozen classifiers / planners
```

TermInfo does not infer protocol/backend identity, perform live probing, own
terminal resource identities, or expand a sibling layer's coarse capability into
TermInfo subjects. Those adapter decisions remain explicit consumer policy. See
`samples/Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample/README.md` and
`docs/1.13.0-PERSISTENT-RASTER-RUNTIME-EVIDENCE-GUIDE.md`.

"""
marker = "## Getting started\n"
if "## What 1.13 adds\n" in text:
    raise SystemExit("README already contains a What 1.13 adds section")
if text.count(marker) != 1:
    raise SystemExit("Expected exactly one Getting started marker")
text = text.replace(marker, section + marker, 1)

readme_path.write_text(text, encoding="utf-8", newline="\n")
