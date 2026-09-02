# Icod.TermInfo CI/CD support scripts

The repository now follows the canonical Icod C#/.NET build-cycle structure. Normal build and distribution orchestration lives under `/packaging`; this directory retains TermInfo-specific structural and execution gates used by that orchestration.

## Preferred entry points

Exact coordinated package validation:

```text
.github\scripts\verify-package-artifact.cmd artifacts Debug
./.github/scripts/verify-package-artifact.sh artifacts Debug
pwsh .github/scripts/verify-package-artifact.ps1 artifacts Debug
```

The wrappers delegate to:

```text
packaging/VerifyPackageArtifact.ps1
```

which in turn preserves the existing deep TermInfo package-validation contract implemented by `verify-release-package.cmd` and `verify-release-package.sh`.

The legacy `verify-release-package.*` names remain internal compatibility engines; new workflows and local build tooling should use `verify-package-artifact.*` or `/packaging` entry points.

## Coordinated package validation

The deep verifier covers the coordinated Runtime, Source, Termcap, Compiler, Inspection, and `Icod.TermInfo.Tools` artifacts. It retains API-baseline checks, net8.0/net9.0/net10.0 equivalence where applicable, structural package checks, isolated package-reference consumers, deterministic samples, and router/package validation.

Package production is centralized in:

```text
packaging/PackPackages.ps1
```

## Tool-suite archives

The deterministic archive implementation remains:

```text
build-tool-archives.sh
verify-tool-archives.sh
```

The preferred orchestration entry point is:

```text
pwsh packaging/BuildToolArchives.ps1 -Configuration Release -OutputDirectory artifacts/tools
```

The six archives cover:

```text
win-x64
win-arm64
linux-x64
linux-arm64
osx-x64
osx-arm64
```

and contain the five traditional command launchers:

```text
tic
infocmp
toe
captoinfo
infotocap
```

`verify-tool-archives.sh` performs structural verification without executing foreign-architecture binaries.

`smoke-tool-archive.ps1` selects the matching archive for the current operating system/architecture and executes the command/version and controlled database smoke path.

## Installable tool package

`smoke-tool-package.ps1` installs the freshly produced local `Icod.TermInfo.Tools` package into an isolated tool path and package cache, then validates the `icod-terminfo` router and its routed commands.

The package smoke and standalone archive smoke remain intentionally separate because they validate different distribution surfaces.

## Lifecycle

- local `build.cmd` / `build.sh`: Debug;
- pull requests: Staging;
- `main`: Release;
- pushed release tags contained in `main`: Release publication.

See `packaging/README.md` for the dependency graph and repository-level build/distribution contract.
