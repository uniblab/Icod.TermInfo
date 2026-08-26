# Repository Maintenance Tools

The `tools` directory contains maintainer/build validation utilities. None of
these projects is a runtime dependency of `Icod.TermInfo`, and none is packed in
the NuGet package.

- `terminfo-metadata` — canonical standard-capability metadata generator and
  deterministic `--check` validation.
- `compiled-terminfo-fixtures` — maintainer-only regeneration of the checked-in
  compiled/malformed T29 fixture corpus; requires the documented `tic` version
  only when deliberately regenerating fixtures.
- `package-verifier` — `.nupkg`/`.snupkg`, dual-target payload, assembly
  identity, Source Link, dependency, and architecture validation used by
  release scripts.
- `public-api-snapshot` — deterministic exhaustive reflection manifest for the
  1.0 API-regret audit and later compatibility checks.
- `package-smoke` — package-reference-only fresh consumer source. It is
  deliberately excluded from the solution so normal solution restore cannot
  accidentally turn the package smoke test into a project-reference test.
- `source-package-smoke` — package-reference-only consumer for
  `Icod.TermInfo.Source`; it also uses `Icod.TermInfo` through the package
  dependency to prove the source package does not rely on a repository-local
  project reference.

Repository maintenance utilities target `net10.0` unless their purpose is to
exercise a shipped consumer target. Both package-smoke consumers deliberately
target `net8.0` and `net10.0`.

Normal build, test, package validation, and fixture consumption do not require
Python.
