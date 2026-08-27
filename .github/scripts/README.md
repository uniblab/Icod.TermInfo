# Package Validation Scripts

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

The scripts reject any configuration other than `Debug`, `Staging`, or `Release`, and the
selected configuration controls maintenance tools, API-snapshot build-output
paths, the Runtime, Compiler, and Inspection package verifiers, all four package
artifacts, all four fresh-package consumers, and the non-interactive repository
sample.

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
