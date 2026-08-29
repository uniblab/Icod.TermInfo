# Repository Maintenance Tools

The `tools` directory contains maintainer/build validation utilities. None of
these projects is a runtime dependency of `Icod.TermInfo`, and none is packed in
the NuGet package.

- `terminfo-metadata` — canonical standard-capability metadata generator and
  deterministic `--check` validation.
- `compiled-terminfo-fixtures` — maintainer-only regeneration of the checked-in
  compiled/malformed T29 fixture corpus; requires the documented `tic` version
  only when deliberately regenerating fixtures.
- `package-verifier` — Runtime `.nupkg`/`.snupkg`, multi-target payload,
  assembly identity, Source Link, dependency, and architecture validation used
  by release scripts.
- `compiler-package-verifier` — equivalent C01 structural, dependency, assembly
  identity, documentation, symbol, and Source Link validation for
  `Icod.TermInfo.Compiler`.
- `inspection-package-verifier` — structural, dependency, assembly identity,
  documentation, symbol, and Source Link validation for
  `Icod.TermInfo.Inspection`.
- `public-api-snapshot` — deterministic exhaustive reflection manifest for the
  frozen Runtime API and explicitly supplied assemblies such as
  `Icod.TermInfo.Source`, `Icod.TermInfo.Compiler`, and
  `Icod.TermInfo.Inspection`.
- `package-smoke` — package-reference-only fresh Runtime consumer source. It is
  deliberately excluded from the solution so normal solution restore cannot
  accidentally turn the package smoke test into a project-reference test.
- `source-package-smoke` — package-reference-only consumer for
  `Icod.TermInfo.Source`; it also uses `Icod.TermInfo` through the package
  dependency to prove the Source package does not rely on a repository-local
  project reference.
- `compiler-package-smoke` — package-reference-only consumer for
  `Icod.TermInfo.Compiler`; it exercises the writer and transitive Runtime and
  Source dependencies without a repository-local project reference.
- `inspection-package-smoke` — package-reference-only consumer for
  `Icod.TermInfo.Inspection`; it exercises rendering/inspection through the
  packaged Runtime and Source dependency graph without repository-local project
  references.

Repository maintenance utilities target `net10.0` unless their purpose is to
exercise a shipped consumer target. All package-smoke consumers deliberately
target `net8.0`, `net9.0`, and `net10.0`.

Normal build, test, package validation, and fixture consumption do not require
Python.
