# Contributing to Icod.TermInfo

Contributions are welcome when they preserve the capability-driven design, the active 1.0 contract, and the frozen historical release contracts.

## Development requirements

Install the .NET 8 and .NET 10 SDK/runtime lines and use C# 13. The shipped
library, tests, and samples target both `net8.0` and `net10.0`; repository-only
maintenance tools normally target `net10.0`.

Before submitting a change, run Debug and Release builds and tests for
`Icod.TermInfo.sln`:

```text
dotnet restore Icod.TermInfo.sln
dotnet build Icod.TermInfo.sln -c Debug
dotnet test Icod.TermInfo.sln -c Debug
dotnet build Icod.TermInfo.sln -c Release
dotnet test Icod.TermInfo.sln -c Release
```

Repository text files use UTF-8 and LF line endings. Use braces for all control-flow bodies. Public, protected, and internal API entry points should validate their parameters before performing work.

Release builds treat warnings as errors, including CS1591. Every public member
must carry XML documentation. Public API changes must also be nullable-correct,
covered by semantic surface tests, and reconciled deliberately with the checked
1.0 API baseline; do not regenerate the baseline merely to silence a mismatch.

## Version metadata

`Icod.TermInfo.csproj` contains `<Version />`, `<PackageVersion />`, and the
stable 1.x `<AssemblyVersion />`. Keep `Version` and `PackageVersion` identical.
The 1.x assembly version remains `1.0.0.0`, and the assembly remains unsigned.

Prerelease development should use the active version roadmap's alpha/beta/RC sequence. A final release tag must be exactly `v<PackageVersion>`.

See `docs/VERSIONING.md` and `docs/COMPATIBILITY.md` before changing public API,
assembly identity, target frameworks, or compatibility behavior.

## Adding or changing a terminal profile

Built-in profiles are data. Generic capability lookup, parameter expansion, padding, output, environment resolution, and platform code must not grow terminal-specific branches.

When adding or changing a built-in profile:

1. Confirm that the terminal belongs in the supported scope before adding aliases or behavior.
2. Add typed capability identifiers and traditional short-name mappings only when the capability catalog does not already represent the required capability.
3. Express the profile through `TerminalDescriptionBuilder` under `src/Profiles`.
4. Add the immutable description to `TerminalProfiles` and, when appropriate, to `TerminalDatabase.BuiltIn`.
5. Keep aliases exact and conservative. Do not map a merely similar terminal family to an existing profile.
6. Express parameterized strings in the shared terminfo parameter language; do not interpolate ANSI/VT values manually in generic code.
7. Preserve `$<...>` padding annotations in profile strings. The generic output layer owns padding removal, delays, and terminal-aware padding semantics.
8. Add a golden test for every capability the profile advertises and explicit absence tests for important unsupported capabilities.
9. Test canonical names and all accepted aliases deterministically.
10. Verify the profile on all three CI operating systems even when the profile itself is platform-neutral.

Do not map unsupported terminal identities such as `screen`, `tmux`, `linux`, `cygwin`, or `rxvt` to ANSI, VT100, xterm, Windows Console, or Windows Terminal merely because they understand similar escape sequences.

When consulting external terminfo databases or terminal documentation, use them as interoperability references. Keep contributed source original and compatible with this repository's LGPL-3.0-or-later licensing.

## Provider extensions

Third-party terminal families should normally enter through `ITerminalDescriptionProvider`. Providers must be deterministic, validate public inputs, and return immutable `TerminalDescription` instances. A `TryLoad` result of `false` means a clean miss; I/O, permission, corrupt-data, and parser failures must not be silently collapsed into that result.

Do not introduce a process-global provider registry. Callers own provider composition and precedence through `TerminalDatabase`.

## Parameter-expansion changes

Changes to the terminfo parameter engine require direct tests for the affected operator or formatting rule. Parse failures and evaluation failures must remain deterministic managed errors rather than producing partial output.

Persistent state must be caller-owned. Do not add process-global equivalents of `cur_term` or hidden persistent `%P/%g` variable storage.

## Padding and output changes

Padding syntax belongs in the padding parser and output layer, not in terminal profiles or the parameter evaluator. Tests for real-delay behavior must use an injected `ITermInfoDelayProvider`; the test suite should never need to sleep.

Control strings are protocol data. Avoid culture-sensitive transformations, implicit newline rewriting, or application-text encoding policy in the capability layer.

## Platform changes

Keep native interop narrow and isolated under `src/Platform`. Platform-specific helpers must fail gracefully when the requested terminal/console facility is unavailable or redirected.

Terminal profile loading must remain side-effect free. In particular, Windows virtual-terminal processing is always an explicit caller action.

## Documentation and samples

Public behavior changes should update README examples when relevant. Samples must remain safe on redirected output and should not assume the CI runner has a usable `TERM` or interactive TTY.

Both sample projects are multi-targeted. Documentation which invokes them with
`dotnet run` must specify `-f net8.0` or `-f net10.0`.

If a public type or compatibility operation changes, update the semantic public
API tests deliberately. Once `docs/1.0.0-PUBLIC-API-BASELINE.txt` is approved,
`public-api-snapshot --check` is the compatibility gate; rewriting the baseline
requires an explicit compatibility decision.

## Packaging and release changes

Package changes should preserve:

- deterministic/continuous-integration builds;
- first-class `net8.0` and `net10.0` managed/XML assets;
- stable assembly version `1.0.0.0` throughout 1.x;
- unsigned assembly identity throughout 1.x;
- Source Link information supplied by the .NET SDK;
- portable PDBs and `.snupkg` generation for both targets;
- package validation and the fresh-package smoke consumer on both targets;
- the package README, icon metadata, and LGPL license expression;
- identical release artifacts for NuGet.org and GitHub Packages.

See `docs/RELEASING.md` before modifying publication workflows.

## Scope discipline

The 1.0 contract combines the immutable terminfo semantics completed in 0.8 with
the compiled/system acquisition layer completed in 0.9 and then freezes their
public/package contract. Live session ownership, input-event decoding,
curses/UI behavior, PTY/ConPTY lifecycle, terminal probing, source-language
tooling, and terminal emulation remain outside `Icod.TermInfo` 1.0.

See `docs/FUTURE-WORK-INVENTORY.md` for the maintained project-family boundary.
