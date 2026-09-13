# Icod.TermInfo.PersistentRasterPlacement.Sample

This 1.12 sample demonstrates the boundary between protocol-neutral TermInfo placement planning and consumer-owned Terminal execution values.

The sample first classifies verified lifecycle and advanced-placement evidence, then requests both 1.12 placement semantics:

- pixel-space source rectangles; and
- signed z-order.

`PersistentRasterPlacementPlanner` must produce a satisfied semantic plan before the sample creates any concrete Terminal execution values. Only after that planning boundary does the consumer construct:

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

The coordinates and z-order value are intentionally absent from the TermInfo plan. They belong to the consuming application and `Icod.Terminal`.

The sample targets `net8.0`, `net9.0`, and `net10.0`. For example:

```text
dotnet run --project samples/Icod.TermInfo.PersistentRasterPlacement.Sample/Icod.TermInfo.PersistentRasterPlacement.Sample.csproj -f net10.0
```

The sample references the in-repository `Icod.TermInfo.Inspection` project and stable `Icod.Terminal 1.12.0`. That Terminal dependency is qualification/sample-only; no production TermInfo project depends on `Icod.Terminal`.
