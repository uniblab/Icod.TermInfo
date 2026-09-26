# Changelog

This file records release-level product changes. Exact qualification evidence,
API fingerprints, and preserved boundaries live in the linked release audits.

## 1.16.0

- Adds deterministic pure-managed whole-file Berkeley DB Hash-v9 publication
  from validated compiled entries, with exact UTF-8 canonical and alias keys.
- Supports linked collision pages and overflow records within configured
  record/image/parser limits; source enumeration is bounded by record cost.
- Stages, flushes, reopens, and verifies the complete database before commit
  under a persistent cooperative lock, with explicit overwrite and cancellation.
- Adds `tic --database-format directory|hashed`; hashed output requires an
  exact `-o` file, `--force` permits replacement, and `-s` reports publication.
- Preserves the nine-type reader API and adds exactly three writer types;
  all other reusable APIs, JSON contracts, dependencies, and archive RIDs remain
  unchanged. No native production dependency is introduced.
- Adds a public writer sample, package-only writer consumer, API reconstruction
  gate, and native/cross-host pathological qualification.

Migration and catalog automation are deferred to 1.17. See the
[1.16 release audit](docs/1.16.0-RELEASE-AUDIT.md).

## 1.15.0

This is the stable coordinated release of Icod.TermInfo 1.15. It adds the
optional `Icod.TermInfo.BerkeleyDb` package for
pure-managed, read-only acquisition from the qualified ncurses-compatible
Berkeley DB Hash-v9 subset.

- Adds bounded exact lookup by canonical name or alias, including inline and
  overflow records, both byte orders, UTF-8-first names, and exact representable
  Latin-1 fallback.
- Adds opt-in hashed-aware system discovery without changing Runtime's frozen
  conventional `SystemTerminalDescriptionProvider`.
- Adds deterministic logical catalog enumeration for canonical and alias
  publications.
- Integrates explicit hashed file operands into `infocmp` and human `toe`
  while leaving `tic`, ambient discovery, and command JSON contracts unchanged.
- Preserves a Runtime-only package dependency, pure-managed deployment,
  read-only behavior, assembly version `1.0.0.0`, net8.0/net9.0/net10.0 API
  equivalence, and the frozen Runtime/Source/Compiler/Termcap/Inspection APIs.
- Adds a controlled deterministic sample, exact public API baseline, security
  and resource audit, compatibility guide, and three-host native/transported
  interoperability qualification.

See the [1.15 release audit](docs/1.15.0-RELEASE-AUDIT.md).

## Earlier releases

Historical release details and exact evidence remain in the versioned documents
under `docs/`, including the
[1.14 release audit](docs/1.14.0-RELEASE-AUDIT.md) and earlier release audits.
