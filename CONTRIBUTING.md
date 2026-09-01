# Contributing to Icod.TermInfo

Contributions are welcome when they preserve the capability-driven design, the
active 1.x compatibility contracts, and the frozen historical release contracts.

## Development requirements

Install the .NET 8, .NET 9, and .NET 10 SDK/runtime lines and use C# 13. The
shipped libraries, tests, and samples target `net8.0`, `net9.0`, and `net10.0`;
repository-only maintenance tools normally target `net10.0`.

Before submitting a change, run Debug and Release builds and tests for
`Icod.TermInfo.sln`:

```text
dotnet restore Icod.TermInfo.sln
dotnet build Icod.TermInfo.sln -c Debug
dotnet test Icod.TermInfo.sln -c Debug
dotnet build Icod.TermInfo.sln -c Release
dotnet test Icod.TermInfo.sln -c Release
```

Repository text files use UTF-8 and CRLF line endings, as frozen by
`.editorconfig`. Use braces for all control-flow bodies. Public, protected, and
internal API entry points should validate their parameters before performing
work.

For multiline invocations and method declarations, place the closing `)` on its
own line. Do not attach it to the final argument or parameter. When xUnit
provides a predicate overload for an assertion, prefer that overload instead of
filtering with `Where(...)` and asserting on the filtered sequence.

Reusable-library Release builds treat warnings as errors, including CS1591.
Every public member of Runtime, Source, Compiler, Inspection, and Termcap must
carry XML documentation. Command and router projects generate XML documentation while
retaining their explicit CS1591 exemption. Public API changes must also be
nullable-correct and covered by semantic surface tests. Runtime API changes must
be reconciled
deliberately with `docs/1.0.0-PUBLIC-API-BASELINE.txt`; Source API changes must
be reconciled with `docs/1.1.0-SOURCE-PUBLIC-API-BASELINE.txt`. Do not regenerate
either baseline merely to silence a mismatch. Compiler API changes must likewise
be reconciled with `docs/1.2.0-COMPILER-PUBLIC-API-BASELINE.txt`. The frozen
1.3 Inspection baseline remains historical; the reviewed 1.4 Inspection surface
is frozen by `docs/1.4.0-INSPECTION-PUBLIC-API-BASELINE.txt`, which patch
releases such as 1.4.1 must preserve unchanged. The frozen 1.6 Termcap surface
is recorded by `docs/1.6.0-TERMCAP-PUBLIC-API-BASELINE.txt`; changes to that
public surface must likewise be deliberate compatibility decisions.

## Version metadata

`Directory.Build.props` contains the sole coordinated release-version literal
in `IcodTermInfoSuiteVersion`. Runtime, Source, Compiler, Inspection, Termcap,
`tic`, `infocmp`, `toe`, `captoinfo`, `infotocap`, and `Icod.TermInfo.Router`
must consume that property rather than introducing independent current-version
literals. The five reusable package projects and the router also consume it for
`<PackageVersion />`.

Beginning with 1.3, Runtime, Source, Compiler, and Inspection advance together;
beginning with 1.5 the `Icod.TermInfo.Tools` router package joins that coordinated
version; beginning with 1.6, Termcap joins the coordinated reusable package
family. The 1.x assembly version remains `1.0.0.0` for all five reusable
assemblies, and all five remain unsigned.

Prerelease development should use the active version roadmap's alpha/beta/RC
sequence. A final release tag must be exactly `v<PackageVersion>`.

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

All three executable API sample projects are multi-targeted. Documentation which
invokes them with `dotnet run` must specify `-f net8.0`, `-f net9.0`, or
`-f net10.0`. The Toolchain sample must remain deterministic and use an explicit
temporary database rather than the host `TERM` value or installed terminfo data.

If a public runtime type or compatibility operation changes, update the semantic
public API tests deliberately and reconcile the change with
`docs/1.0.0-PUBLIC-API-BASELINE.txt`. If a Source public API changes, reconcile
it with `docs/1.1.0-SOURCE-PUBLIC-API-BASELINE.txt`. The release verifier treats
both baselines as intentional contracts; rewriting either requires an explicit
compatibility decision.

## Packaging and release changes

Package changes should preserve:

- deterministic/continuous-integration builds;
- first-class `net8.0`, `net9.0`, and `net10.0` managed/XML assets for every
  package in the coordinated family;
- stable assembly version `1.0.0.0` throughout 1.x;
- unsigned assembly identity throughout 1.x;
- Source Link information supplied by the .NET SDK;
- portable PDBs and `.snupkg` generation for all three reusable-library targets;
- one `IcodTermInfoSuiteVersion` authority for all coordinated projects;
- synchronized Runtime, Source, Compiler, Inspection, Termcap, and
  `Icod.TermInfo.Tools` package versions;
- Runtime, Source, Compiler, Inspection, and Termcap fresh-package smoke consumers
  on all three library targets;
- installation and routed-command smoke for `Icod.TermInfo.Tools` on Windows,
  Linux, and macOS;
- each package README, icon metadata, and LGPL license expression;
- a one-way Source -> Runtime package dependency;
- a one-way Compiler -> Runtime/Source dependency;
- a one-way Inspection -> Runtime/Source dependency with no production Compiler
  dependency;
- a one-way Termcap -> Runtime dependency;
- no command-to-command dependencies among `tic`, `infocmp`, `toe`, `captoinfo`,
  and `infotocap`;
- a distribution-only Router dependency on all five command implementations;
- identical validated registry package artifacts for NuGet.org and GitHub
  Packages.

See `RELEASING.md` before modifying publication workflows.

## Scope discipline

The runtime 1.0 contract combines the immutable terminfo semantics completed in
0.8 with the compiled/system acquisition layer completed in 0.9 and freezes
their public/package contract. Version 1.1 adds `.ti` parsing, cancellation,
`use=` inheritance, and materialization in the optional `Icod.TermInfo.Source`
package without changing that runtime boundary.

Version 1.2 adds compiled-entry writing and the reusable source compiler engine
in the optional `Icod.TermInfo.Compiler` package. The low-level writer remains
pure; filesystem/database-layout output is layered separately, and no compiler
dependency enters `Icod.TermInfo` or `Icod.TermInfo.Source`.

Version 1.3 adds canonical rendering, structured semantic comparison, and
provider-aware inspection in the optional `Icod.TermInfo.Inspection` package.
Inspection depends on Runtime and Source but not on Compiler, and it does not
enlarge the frozen Runtime, Source, or Compiler public contracts.

Version 1.4 adds the `tic`, `infocmp`, and `toe` command applications above the
reusable package family. Version 1.5 adds the distribution-only
`Icod.TermInfo.Tools` router without moving command policy into reusable
libraries.

Version 1.6 adds opt-in termcap parsing, resolution, conversion, rendering, and
acquisition in the separate `Icod.TermInfo.Termcap` package. It also adds the
`captoinfo` and `infotocap` commands and routes all five command implementations
through `icod-terminfo`.

Live session ownership, input-event decoding, curses/UI behavior, PTY/ConPTY
lifecycle, terminal probing, and terminal emulation remain outside the reusable
package-family scope. The five released command applications form a separate
command layer above that family rather than becoming library-package
responsibilities.

Current post-1.0 scope and future-work ownership are governed by
`Icod.TermInfo-Post-1.0-Development-Roadmap.md`. Historical release roadmaps and
audits remain authoritative for their frozen releases.
