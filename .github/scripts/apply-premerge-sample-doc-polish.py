from pathlib import Path


def read(path: str) -> str:
    return Path(path).read_text(encoding="utf-8")


def write(path: str, text: str) -> None:
    Path(path).write_text(text, encoding="utf-8", newline="\n")


def replace_once_or_already(path: str, old: str, new: str) -> None:
    text = read(path)
    old_count = text.count(old)
    new_count = text.count(new)
    if old_count == 1 and new_count == 0:
        write(path, text.replace(old, new, 1))
        return
    if old_count == 0 and new_count == 1:
        return
    raise RuntimeError(
        f"{path}: expected one old occurrence or one applied occurrence; "
        f"found old={old_count}, new={new_count}"
    )


replace_once_or_already(
    "README.md",
    """The stable promotion passed workflow #731 / `34764253699` on exact head
`1674c008f1df2cbe82295b8eaa9ae9d34ac5d999`, with all 12 jobs green.
""",
    """The release-ready stable contract passed workflow #741 / `34764960954` on
exact head `292ea7b490747ada55d1960461e1cdb676a8e3e9`, with all 12 jobs green.
The release audit records the subsequent pre-merge sample/documentation polish
separately.
""",
)
replace_once_or_already(
    "README.md",
    "The repository contains five executable API samples plus one command-suite\nwalkthrough with deliberately different purposes.",
    "The repository contains six executable API samples plus one command-suite\nwalkthrough with deliberately different purposes.",
)
replace_once_or_already(
    "README.md",
    "All five executable API sample projects target `net8.0`, `net9.0`, and\n`net10.0`; `dotnet run` therefore needs an explicit framework.",
    "All six executable API sample projects target `net8.0`, `net9.0`, and\n`net10.0`; `dotnet run` therefore needs an explicit framework.",
)
replace_once_or_already(
    "README.md",
    """### Managed tool-suite walkthrough
""",
    """### Persistent-raster placement sample

`samples/Icod.TermInfo.PersistentRasterPlacement.Sample` is the focused 1.12
Inspection/Terminal boundary example. It begins with a successful lifecycle plan
but no advanced-placement evidence, so both `SourceRectangle` and
`SignedZOrder` remain `Unknown` and the placement planner returns
`RequiresRuntimeVerification`.

The consumer then contributes its own `Verified` evidence, reclassifies the
placement profile, and obtains a `Satisfied` plan. The sample renders both the
version-4 placement profile and placement plan before constructing any concrete
Terminal execution values. Only after semantic planning succeeds does it create
a `TerminalRasterSourceRectangle` and signed `ZIndex`.

Run it with:

```text
dotnet run --project samples/Icod.TermInfo.PersistentRasterPlacement.Sample/Icod.TermInfo.PersistentRasterPlacement.Sample.csproj -f net10.0
```

Release verification executes the sample on `net8.0`, `net9.0`, and `net10.0`.
See `samples/Icod.TermInfo.PersistentRasterPlacement.Sample/README.md` and
`docs/1.12.0-ADVANCED-PERSISTENT-RASTER-PLACEMENT-GUIDE.md`.

### Managed tool-suite walkthrough
""",
)
replace_once_or_already(
    "README.md",
    """`samples/Icod.TermInfo.PersistentRasterLifecycle.Sample/README.md`, and
`docs/0.9.0-ACQUISITION-GUIDE.md` for the complete examples.
""",
    """`samples/Icod.TermInfo.PersistentRasterLifecycle.Sample/README.md`,
`samples/Icod.TermInfo.PersistentRasterPlacement.Sample/README.md`, and
`docs/0.9.0-ACQUISITION-GUIDE.md` for the complete examples.
""",
)
replace_once_or_already(
    "README.md",
    """`Icod.TermInfo` owns immutable terminal-description data, acquisition of that data, and pure transformations required to interpret, expand, and output terminal capabilities. `Icod.TermInfo.Source` owns optional source-language parsing and inheritance resolution, `Icod.TermInfo.Compiler` owns compiled output, `Icod.TermInfo.Inspection` owns canonical rendering, semantic comparison, database-set automation, and protocol-neutral persistent-raster lifecycle evidence/classification/planning, and `Icod.TermInfo.Termcap` owns optional termcap interoperability. None of those packages owns a live terminal session, terminal graphics resource identity, a child pseudo-terminal, or a virtual screen.
""",
    """`Icod.TermInfo` owns immutable terminal-description data, acquisition of that data, and pure transformations required to interpret, expand, and output terminal capabilities. `Icod.TermInfo.Source` owns optional source-language parsing and inheritance resolution, `Icod.TermInfo.Compiler` owns compiled output, `Icod.TermInfo.Inspection` owns canonical rendering, semantic comparison, database-set automation, protocol-neutral persistent-raster lifecycle and placement evidence/classification/planning, and versioned machine-readable views, and `Icod.TermInfo.Termcap` owns optional termcap interoperability. None of those packages owns a live terminal session, terminal graphics resource identity, a child pseudo-terminal, or a virtual screen.
""",
)
replace_once_or_already(
    "README.md",
    """- **`Icod.TermInfo.Inspection`** — canonical effective/source rendering, relative-source synthesis and parent planning, structured semantic comparison, provider/database-set inspection, and persistent-raster lifecycle evidence, classification, planning, and version-3 machine-readable views;
""",
    """- **`Icod.TermInfo.Inspection`** — canonical effective/source rendering, relative-source synthesis and parent planning, structured semantic comparison, provider/database-set inspection, persistent-raster lifecycle and advanced-placement evidence/classification/planning, and version-3/version-4 machine-readable views;
""",
)

replace_once_or_already(
    "samples/README.md",
    """The 1.12 addition is `Icod.TermInfo.PersistentRasterPlacement.Sample`. It shows
the deliberate boundary between protocol-neutral TermInfo planning and
consumer-owned `Icod.Terminal 1.12.0` execution values: TermInfo establishes that
source rectangles and signed z-order are admissible requirements, then the
consumer supplies an actual `TerminalRasterSourceRectangle` and `ZIndex`.
""",
    """The 1.12 addition is `Icod.TermInfo.PersistentRasterPlacement.Sample`. It shows
the deliberate boundary between protocol-neutral TermInfo planning and
consumer-owned `Icod.Terminal 1.12.0` execution values. With no advanced-placement
evidence, source rectangles and signed z-order remain `Unknown` and planning
requires runtime verification. The consumer then supplies its own `Verified`
evidence, replans to `Satisfied`, and only then constructs an actual
`TerminalRasterSourceRectangle` and `ZIndex`.
""",
)
replace_once_or_already(
    "samples/README.md",
    """`Icod.TermInfo.PersistentRasterPlacement.Sample` is the focused 1.12 downstream
integration example. It first constructs a successful lifecycle plan and a
placement profile that verifies both `SourceRectangle` and `SignedZOrder`, then
requires those semantics through `PersistentRasterPlacementPlanner`.

Only after the semantic plan is satisfied does the consumer construct concrete
`Icod.Terminal 1.12.0` execution values: a `TerminalRasterSourceRectangle` and a
signed `TerminalRasterPlacementOptions.ZIndex`. This demonstrates that TermInfo
never owns the actual crop coordinates or z-order integer.
""",
    """`Icod.TermInfo.PersistentRasterPlacement.Sample` is the focused 1.12 downstream
integration example. It first constructs a successful lifecycle plan with no
advanced-placement evidence. Both `SourceRectangle` and `SignedZOrder` therefore
remain `Unknown`, and a request requiring both semantics produces
`RequiresRuntimeVerification`.

The consumer then adds caller-owned `Verified` evidence, reclassifies, and
replans to `Satisfied`. The sample renders both version-4 placement profile and
plan documents before constructing concrete `Icod.Terminal 1.12.0` execution
values: a `TerminalRasterSourceRectangle` and a signed
`TerminalRasterPlacementOptions.ZIndex`. This demonstrates that TermInfo never
owns the actual crop coordinates or z-order integer.
""",
)

replace_once_or_already(
    "docs/1.12.0-RELEASE-AUDIT.md",
    """Stable promotion head `1674c008f1df2cbe82295b8eaa9ae9d34ac5d999` passed pull-request workflow #731 / run `34764253699` with all 12 jobs green. The coordinated version is `1.12.0`; the promotion changed release identity/current-facing documentation only and introduced no feature, public API, schema, dependency, target-framework, command-semantic, or archive-RID change.

PR #42 remains open and unmerged. No tag, registry publication, or GitHub release is created by this audit.
""",
    """Stable promotion head `1674c008f1df2cbe82295b8eaa9ae9d34ac5d999` passed pull-request workflow #731 / run `34764253699` with all 12 jobs green. The coordinated version is `1.12.0`; the promotion changed release identity/current-facing documentation only and introduced no feature, public API, schema, dependency, target-framework, command-semantic, or archive-RID change.

Release-ready head `292ea7b490747ada55d1960461e1cdb676a8e3e9` passed pull-request workflow #741 / run `34764960954` with all 12 jobs green after final historical-closure corrections.

PR #42 remains open and unmerged. No tag, registry publication, or GitHub release is created by this audit.
""",
)
replace_once_or_already(
    "docs/1.12.0-RELEASE-AUDIT.md",
    """## Release scope
""",
    """## Pre-merge sample and documentation polish

After the release-ready #741 checkpoint, the dedicated 1.12 placement sample was expanded without changing library API or semantics. The sample now demonstrates the complete conservative planning progression: advanced-placement evidence begins unknown, the first plan requires runtime verification, caller-owned verified evidence is added, replanning becomes satisfied, and both version-4 placement profile and plan documents are rendered before concrete `Icod.Terminal` rectangle/z-order values are constructed.

The root and sample READMEs are aligned with the six executable API samples and the 1.12 Inspection ownership boundary. This documentation/sample-only polish requires a fresh full PR validation before merge.

## Release scope
""",
)
