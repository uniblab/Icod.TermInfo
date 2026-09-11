# Icod.TermInfo.PersistentRasterLifecycle.Sample

This sample demonstrates the protocol-neutral persistent-raster lifecycle flow added by `Icod.TermInfo.Inspection` 1.11.

It intentionally keeps terminal execution outside TermInfo. The sample does not probe a live terminal, transmit Kitty or Sixel payloads, allocate terminal-side resource or placement identifiers, or perform cleanup. Those responsibilities belong to the consuming terminal-session layer.

The sample instead demonstrates the reusable semantic boundary:

1. build a controlled `TerminalDescription` containing ordinary Sixel evidence;
2. inspect it with `PersistentRasterLifecycleInspector`;
3. observe that Sixel alone leaves persistent upload and placement support `Unknown`;
4. request upload plus one placement and receive an `Indeterminate` plan requiring runtime verification;
5. simulate a consumer-owned live verification result by adding explicit `Verified` evidence for persistent upload and placement creation;
6. reclassify the evidence and obtain a successful protocol-neutral plan;
7. render the strengthened profile and plan through the version-3 lifecycle JSON contract.

The project references only `Icod.TermInfo.Inspection`; it has no dependency on `Icod.Terminal`.

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

The `Verified` evidence in this example stands in for evidence acquired by the consumer's own live/runtime verification mechanism. TermInfo classifies and plans from that evidence; it does not perform the verification itself.
