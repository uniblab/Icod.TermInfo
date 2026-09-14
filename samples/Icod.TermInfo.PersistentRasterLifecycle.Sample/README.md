# Icod.TermInfo.PersistentRasterLifecycle.Sample

This sample demonstrates the protocol-neutral persistent-raster lifecycle flow introduced by `Icod.TermInfo.Inspection` 1.11 and the caller-owned runtime-evidence integration path added in 1.13.

It intentionally keeps terminal execution outside TermInfo. The sample does not probe a live terminal, transmit Kitty or Sixel payloads, allocate terminal-side resource or placement identifiers, or perform cleanup. Those responsibilities belong to the consuming terminal-session layer.

The sample instead demonstrates the reusable semantic boundary:

1. build a controlled `TerminalDescription` containing ordinary Sixel evidence;
2. inspect it with `PersistentRasterLifecycleInspector`;
3. observe that Sixel alone leaves persistent upload and placement support `Unknown`;
4. request upload plus one placement and receive an `Indeterminate` plan requiring runtime verification;
5. represent the consumer-owned runtime result as immutable `PersistentRasterRuntimeLifecycleObservation` values;
6. pass those observations through `PersistentRasterRuntimeEvidenceIntegrator`, which maps conclusive results to existing `Verified` evidence, assigns safe final source ordinals, and reclassifies the lifecycle profile;
7. call `CreateLifecyclePlan(...)` on the integration result to delegate replanning to the frozen lifecycle planner; and
8. render the runtime integration audit through JSON version 5 and the strengthened lifecycle plan through the frozen version-3 plan contract.

The project references only `Icod.TermInfo.Inspection`; it has no dependency on `Icod.Terminal`. The observations in this example stand in for results acquired by the consumer's own live/runtime verification mechanism. TermInfo integrates, classifies, and plans from those observations; it does not perform the verification itself.

Run the sample with any supported target framework:

```text
dotnet run --project samples/Icod.TermInfo.PersistentRasterLifecycle.Sample/Icod.TermInfo.PersistentRasterLifecycle.Sample.csproj -f net8.0
```

```text
dotnet run --project samples/Icod.TermInfo.PersistentRasterLifecycle.Sample/Icod.TermInfo.PersistentRasterLifecycle.Sample.csproj -f net9.0
```

```text
dotnet run --project samples/Icod.TermInfo.PersistentRasterLifecycle.Sample/Icod.TermInfo.PersistentRasterLifecycle.Sample.csproj -f net10.0
```

For a sample that performs the sibling `Icod.Terminal` semantic verification call before creating runtime observations, see `../Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample/README.md`.
