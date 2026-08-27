# Icod.TermInfo.Inspection

`Icod.TermInfo.Inspection` is the optional managed inspection and semantic-
comparison layer for the `Icod.TermInfo` package family.

The 1.3 line is intended to provide the reusable API engine underneath future
`infocmp`-style tooling while preserving the already-frozen Runtime 1.0, Source
1.1, and Compiler 1.2 public contracts.

## I01 package foundation

`1.3.0-Alpha-1` establishes package and release infrastructure only. It
intentionally exports no Inspection public types yet. The first public
inspection behavior is introduced deliberately in I02, beginning with canonical
effective source rendering.

The I01 dependency graph is:

```text
Inspection -> Source -> Runtime
Inspection ----------> Runtime

Compiler   -> Source -> Runtime
Compiler   ------------> Runtime
```

There is no production dependency between Inspection and Compiler.

## Install

During I01 development:

```text
dotnet add package Icod.TermInfo.Inspection --version 1.3.0-Alpha-1
```

The package targets `net8.0`, `net9.0`, and `net10.0`, uses C# 13, remains
unsigned, and retains assembly version `1.0.0.0` throughout the 1.x line.

## Ownership boundary

- `Icod.TermInfo` owns immutable effective terminal descriptions and acquisition.
- `Icod.TermInfo.Source` owns `.ti` lexical, parsing, and inheritance semantics.
- `Icod.TermInfo.Compiler` owns deterministic compiled-entry/database writing.
- `Icod.TermInfo.Inspection` owns canonical human-readable representation,
  semantic comparison, and inspection orchestration.

Command-line parsing and `infocmp` executable policy remain outside this package.
