# Icod.TermInfo.PersistentRasterPlacement.Sample

This 1.12 sample demonstrates the boundary between protocol-neutral TermInfo placement planning and consumer-owned Terminal execution values.

The sample begins with a successful 1.11 lifecycle plan but no advanced-placement evidence. Both 1.12 placement subjects therefore remain `Unknown`:

- pixel-space source rectangles; and
- signed z-order.

A placement request requiring both semantics initially produces `RequiresRuntimeVerification`. The sample renders both the version-4 placement profile and placement plan so consumers can see the machine-readable unknown state and the planner's verification requirement.

The sample then adds caller-owned `Verified` evidence representing a result obtained by the consuming application's own live/runtime verification layer. Reclassification produces a supported placement profile, replanning produces `Satisfied`, and the verified profile and plan are rendered again through the version-4 JSON contract.

Only after the semantic plan is satisfied does the consumer construct concrete Terminal execution values:

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

The coordinates and z-order value are intentionally absent from every TermInfo plan. TermInfo owns semantic evidence, classification, and planning; actual rectangle coordinates, signed stacking values, live verification, and protocol execution belong to the consuming application and `Icod.Terminal`.

The sample targets `net8.0`, `net9.0`, and `net10.0`. For example:

```text
dotnet run --project samples/Icod.TermInfo.PersistentRasterPlacement.Sample/Icod.TermInfo.PersistentRasterPlacement.Sample.csproj -f net10.0
```

The sample references the in-repository `Icod.TermInfo.Inspection` project and stable `Icod.Terminal 1.12.0`. That Terminal dependency is qualification/sample-only; no production TermInfo project depends on `Icod.Terminal`.
