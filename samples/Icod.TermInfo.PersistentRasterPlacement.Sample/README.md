# Icod.TermInfo.PersistentRasterPlacement.Sample

This sample demonstrates the boundary between protocol-neutral TermInfo placement planning and consumer-owned Terminal execution values, using the 1.13 runtime-observation/integration path rather than manual final evidence construction.

The sample begins with empty lifecycle and placement evidence. A caller-owned lifecycle runtime observation first establishes `PlacementCreation`, allowing the frozen lifecycle planner to produce a successful one-placement lifecycle plan. The advanced-placement subjects still remain `Unknown`:

- pixel-space source rectangles; and
- signed z-order.

A placement request requiring both semantics therefore initially produces `RequiresRuntimeVerification`.

The consumer then supplies two immutable `PersistentRasterRuntimePlacementObservation` values for `SourceRectangle` and `SignedZOrder`. `PersistentRasterRuntimeEvidenceIntegrator` maps the conclusive observations into the existing frozen placement evidence model, assigns safe final source ordinals, and reclassifies the placement profile. `CreatePlacementPlan(...)` delegates the strengthened lifecycle and placement state back through the frozen planners and produces `Satisfied`.

The sample renders both runtime integration audits and the before/after placement plans so consumers can see the machine-readable transition from unknown runtime state to a satisfied semantic plan.

Only after semantic planning succeeds does the consumer construct concrete Terminal execution values:

```csharp
TerminalRasterSourceRectangle sourceRectangle = new(
	8,
	4,
	320,
	180
);
TerminalRasterPlacementOptions executionOptions = new() {
	SourceRectangle = sourceRectangle,
	ZIndex = -2,
};
```

The coordinates and z-order value are intentionally absent from every TermInfo plan. TermInfo owns protocol-neutral observations, evidence integration, classification, and planning; actual rectangle coordinates, signed stacking values, live verification, and protocol execution belong to the consuming application and `Icod.Terminal`.

The sample targets `net8.0`, `net9.0`, and `net10.0`. For example:

```text
dotnet run --project samples/Icod.TermInfo.PersistentRasterPlacement.Sample/Icod.TermInfo.PersistentRasterPlacement.Sample.csproj -f net10.0
```

The sample references the in-repository `Icod.TermInfo.Inspection` project and stable `Icod.Terminal 1.12.0`. That Terminal dependency is qualification/sample-only; no production TermInfo project depends on `Icod.Terminal`.

For the focused caller adapter that performs `Icod.Terminal` semantic capability verification and maps it into TermInfo runtime observations, see `../Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample/README.md`.
