# Repository Maintenance Tools

The `tools` directory contains maintainer/build validation utilities. None of
these projects is a runtime dependency of `Icod.TermInfo`, and none is packed in
the NuGet package.

- `terminfo-metadata` — canonical standard-capability metadata generator and
  deterministic `--check` validation.
- `compiled-terminfo-fixtures` — maintainer-only regeneration of the checked-in
  compiled/malformed T29 fixture corpus; requires the documented `tic` version
  only when deliberately regenerating fixtures.
- `package-verifier` — `.nupkg`/`.snupkg`, Source Link, dependency, and
  architecture validation used by release scripts.
- `package-smoke` — package-reference-only fresh consumer source. It is
  deliberately excluded from the solution so normal solution restore cannot
  accidentally turn the package smoke test into a project-reference test.

Repository maintenance utilities are implemented in C#/.NET. Normal build,
test, package validation, and fixture consumption do not require Python.
