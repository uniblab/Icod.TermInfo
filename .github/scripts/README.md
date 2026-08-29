# Release Validation Scripts

## Package validation

`verify-release-package.sh` and `verify-release-package.cmd` are equivalent host
wrappers for the same repository package-validation contract.

Both wrappers require:

```text
<artifact-directory> <Debug|Staging|Release>
```

For local Debug validation:

```text
.github\scripts\verify-release-package.cmd artifacts Debug
bash .github/scripts/verify-release-package.sh artifacts Debug
```

For pull-request/development validation:

```text
.github\scripts\verify-release-package.cmd artifacts Staging
bash .github/scripts/verify-release-package.sh artifacts Staging
```

For final main-branch release validation:

```text
.github\scripts\verify-release-package.cmd artifacts Release
bash .github/scripts/verify-release-package.sh artifacts Release
```

The scripts reject any configuration other than `Debug`, `Staging`, or
`Release`. The selected configuration controls maintenance tools, API-snapshot
build-output paths, the Runtime, Compiler, and Inspection package verifiers, all
four package artifacts, all four fresh-package consumers, and the
non-interactive repository sample.

The 1.1 source-language line keeps the frozen `Icod.TermInfo` package checks and
adds `Icod.TermInfo.Source` net8.0/net9.0/net10.0 API-equivalence, reviewed
public-API baseline, coordinated-version, artifact-presence, and
package-reference-only consumer gates. C01 adds equivalent Compiler API-baseline,
three-target API-equivalence, package-structure, coordinated-version, artifact,
and package-reference-only consumer gates. I01 adds the independent Inspection
API baseline, three-target API equivalence, exact Runtime+Source dependency
verification, structural package validation, coordinated-version/artifact gates,
and the fourth package-reference-only consumer.

Fresh-package consumers use isolated NuGet package caches.
`package-smoke.NuGet.Config` maps every `Icod.TermInfo*` package exclusively to
the validated artifact directory while allowing `Microsoft.*` framework and
runtime reference packs to restore from NuGet.org when they are not installed
locally. This keeps the smoke test tied to the local package artifacts without
blocking SDK reference-pack acquisition.

- Use `verify-release-package.sh` on Bash-capable hosts and in Ubuntu GitHub
  Actions package-validation jobs.
- Use `verify-release-package.cmd` from Windows Command Prompt; Bash and Python
  are not required.

## Tool-suite archives

`build-tool-archives.sh` publishes `tic`, `infocmp`, and `toe` for the six
supported release RIDs and creates the coordinated framework-dependent tool-suite
archives. It first requires all four libraries and all three commands to declare
the same version.

```text
bash .github/scripts/build-tool-archives.sh Release artifacts/tools
```

The six output archives cover `win-x64`, `win-arm64`, `linux-x64`,
`linux-arm64`, `osx-x64`, and `osx-arm64`. Archive construction normalizes
ordering and metadata used by the release workflow.

`verify-tool-archives.sh` is the structural gate. It requires exactly the six
archives for the coordinated version and validates their paths, launchers, and
release payload without executing a foreign-architecture binary.

```text
bash .github/scripts/verify-tool-archives.sh artifacts/tools
```

`smoke-tool-archive.ps1` is the matching-host execution gate. It selects the
archive for the current operating system and architecture, unpacks it, verifies
all three `--version` results, publishes a controlled entry with `tic`, acquires
it with `infocmp`, and enumerates it with `toe`.

```text
pwsh -File .github/scripts/smoke-tool-archive.ps1 artifacts/tools
```

The release workflow runs structural validation for all six archives and the
execution smoke on matching Windows, Linux, and macOS runners before package
publication.
