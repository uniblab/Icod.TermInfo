# Persistent Raster Runtime Integration Sample

This sample demonstrates the `Icod.TermInfo 1.13` boundary between static semantic planning and caller-owned live terminal verification.

The flow is deliberately explicit:

```text
TermInfo static inspection and plan
    -> runtime verification required
Icod.Terminal 1.12 VerifyCapabilityAsync(...)
    -> caller maps TerminalCapabilityStatus to runtime observations
TermInfo PersistentRasterRuntimeEvidenceIntegrator
    -> strengthened lifecycle profile
existing TermInfo planner
    -> final lifecycle plan
```

`Icod.TermInfo.Inspection` does not reference `Icod.Terminal`. The sample is the consumer-owned adapter boundary and pins the published `Icod.Terminal 1.12.0` package used for the 1.13 qualification contract.

## Dry run

The default mode is safe for CI and non-interactive environments. It uses the published Terminal `Verified` support value as a representative conclusive caller result, maps it into the runtime-observation model, integrates it, and replans:

```console
dotnet run --project samples/Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample -f net10.0
```

## Live verification

On an interactive terminal, pass `--live` to open an `Icod.Terminal` session and perform the actual bounded semantic verification call:

```console
dotnet run --project samples/Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample -f net10.0 -- --live
```

The caller adapter intentionally maps Terminal's semantic result into TermInfo's protocol-neutral runtime-outcome vocabulary as follows:

| `Icod.Terminal` result | TermInfo runtime outcome |
| --- | --- |
| `LiveObservation` + `Verified` | `Supported` |
| `LiveObservation` + `Unsupported` | `Unsupported` |
| `LiveObservation` + `Unknown` | `Inconclusive` |
| `LiveObservation` + `Advertised` | `Inconclusive` |
| any non-live evidence kind | `Inconclusive` |

The mapping is deliberately conservative. Static advertisement alone is not promoted to runtime support, and TermInfo does not interpret Terminal-specific evidence kinds itself.

Terminal 1.12's `PersistentRasterGraphics` capability is intentionally coarser than TermInfo's lifecycle vocabulary. This sample therefore makes the second piece of consumer policy visible: a conclusive coarse result is expanded to the persistent-upload, acknowledged-upload, placement-create/multiple/update/delete, and resource-delete lifecycle subjects. TermInfo itself does not infer that expansion.

The resulting `PersistentRasterRuntimeObservationSet` is handed to `PersistentRasterRuntimeEvidenceIntegrator`. Conclusive observations become existing `Verified` evidence with safe final source ordinals; inconclusive observations remain visible in the integration audit. `CreateLifecyclePlan(...)` then delegates to the existing frozen lifecycle planner.

The sample does not execute image transport or persistent-raster operations. Its purpose is the interchange contract: static plan → live semantic result → runtime observations → integration → replan.
