# Contributing to Icod.TermInfo

Contributions are welcome when they preserve the capability-driven design and versioned contracts documented by the current roadmap and the frozen historical release roadmaps.

## Development requirements

Use the .NET 10 SDK and C# 13. Before submitting a change, run Debug and Release builds and tests for `Icod.TermInfo.sln`:

```text
dotnet restore Icod.TermInfo.sln
dotnet build Icod.TermInfo.sln -c Debug
dotnet test Icod.TermInfo.sln -c Debug
dotnet build Icod.TermInfo.sln -c Release
dotnet test Icod.TermInfo.sln -c Release
```

Repository text files use UTF-8 and LF line endings. Use braces for all control-flow bodies. Public, protected, and internal API entry points should validate their parameters before performing work.

Release builds treat warnings as errors except for the repository's deliberate XML-documentation exception. New public API must therefore be intentional, documented, nullable-correct, and covered by the current public API baseline tests. For 0.8, T30 freezes that surface before the final completion gate.

## Version metadata

`Icod.TermInfo.csproj` contains both `<Version />` and `<PackageVersion />`. Keep them identical in every development and release change.

Prerelease development should use the active version roadmap's alpha/beta/RC sequence. A release tag must be exactly `v<PackageVersion>`; the release workflow rejects a tag/version mismatch.

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

If a public type or compatibility operation is added, update the public API baseline tests deliberately rather than weakening them.

## Packaging and release changes

Package changes should preserve:

- deterministic/continuous-integration builds;
- Source Link information supplied by the .NET SDK;
- portable PDBs and `.snupkg` generation;
- package validation;
- the package README and LGPL license;
- identical release artifacts for NuGet.org and GitHub Packages.

See `docs/RELEASING.md` before modifying publication workflows.

## Scope discipline

The 0.8 contract deliberately stops at immutable terminfo semantics, pure transformation/output helpers, built-in profiles, and explicit narrow platform helpers. Arbitrary compiled/system terminfo acquisition (`TERMINFO`, `TERMINFO_DIRS`, filesystem providers, and the production compiled parser) is required for 0.9, not 0.8. Live session ownership, input-event decoding, curses/UI behavior, PTY/ConPTY lifecycle, and terminal probing remain outside `Icod.TermInfo` 0.8.
