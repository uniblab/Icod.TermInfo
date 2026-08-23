# Icod.TermInfo Sample

This is the repository's general executable sample for the low-level
`Icod.TermInfo` semantic, profile, output, and environment APIs. It is
intentionally descriptive rather than a live terminal session manager.

The sample demonstrates:

- exact built-in profile selection and conservative environment resolution;
- ordinary system-to-built-in provider composition;
- standard catalog and per-description capability enumeration;
- reusable standard and extended parameter expansion;
- exact Latin-1 capability-byte output;
- terminal-aware padding;
- indexed/direct color inspection and expansion;
- Windows Console and Windows Terminal profile identities;
- cursor-addressing/full-screen capability discovery;
- live/environment/profile terminal-size fallback chosen by the application;
- custom `ITerminalDescriptionProvider` composition;
- explicit Windows VT output enablement for the interactive demonstration.

Run the ordinary demonstration with:

```text
dotnet run --project samples/Icod.TermInfo.Sample/Icod.TermInfo.Sample.csproj
```

Select an exact built-in profile with:

```text
dotnet run --project samples/Icod.TermInfo.Sample/Icod.TermInfo.Sample.csproj -- --profile xterm-256color
```

Use the non-interactive mode in CI, documentation checks, redirected output, or
any environment where terminal-control strings should not be emitted:

```text
dotnet run --project samples/Icod.TermInfo.Sample/Icod.TermInfo.Sample.csproj -- --describe-only --profile ms-terminal-direct
```

For caller-supplied compiled bytes, explicit database roots, restricted system
discovery, normal host discovery, and explicit built-in fallback composition,
use the focused `Icod.TermInfo.Acquisition.Sample` executable next to this
project.

See `../README.md` for the sample index and
`../../docs/0.9.0-ACQUISITION-GUIDE.md` for the complete acquisition contract.
