# Icod.TermInfo Raster Backend Selection Sample

This sample demonstrates the `Icod.TermInfo 1.14.0` advisory raster-backend selection layer beside published `Icod.Terminal 1.13.0` without introducing any production dependency from TermInfo to Terminal.

The default mode is deterministic and CI-safe. It uses a synthetic published `TerminalCapabilitySupport.Verified` value as caller-owned input, explicitly maps that result into 1.13 runtime lifecycle observations, and separately applies **caller policy** that a conclusive Terminal persistent-raster result represents Kitty Graphics availability for this sample. TermInfo itself does not make that inference.

The Sixel side comes from explicit static `TerminalDescription` metadata through `RasterBackendInspector`. The Kitty side demonstrates the downstream boundary:

```text
Terminal semantic result
    -> caller-owned lifecycle observation mapping
    -> 1.13 runtime integration
    -> caller-owned Kitty availability evidence
    -> RasterBackendCandidate

Sixel candidate + Kitty candidate
    -> plan without preference
    -> RequiresPreference
    -> explicit caller preference
    -> selected Kitty Graphics plan
```

No terminal name, emulator identity, process ancestry, environment variable, or Terminal-internal backend resolver is inspected.

## Run deterministic mode

```text
dotnet run --project samples/Icod.TermInfo.RasterBackendSelection.Sample -f net10.0
```

The deterministic path first submits both viable candidates without a preference order and requires `RequiresPreference`, proving that TermInfo does not rank backends by enum value or input order. It then supplies the explicit preference order Kitty Graphics -> Sixel and requires a selected Kitty plan. Sixel remains independently represented from static capability metadata.

## Run live mode

On an interactive terminal, pass `--live`:

```text
dotnet run --project samples/Icod.TermInfo.RasterBackendSelection.Sample -f net10.0 -- --live
```

Live mode calls published `Icod.Terminal 1.13.0` `VerifyCapabilityAsync(TerminalCapability.PersistentRasterGraphics)`. The resulting Terminal status is a real live observation; the expansion into TermInfo lifecycle subjects and the decision to treat a conclusive persistent-raster result as Kitty availability remain explicit **caller policy** in this sample.

A live terminal may therefore produce a non-selected result. That is expected: this sample reports the advisory plan rather than manufacturing support evidence.
