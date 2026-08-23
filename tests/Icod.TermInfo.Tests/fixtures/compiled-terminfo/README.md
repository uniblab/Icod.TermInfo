# Compiled Terminfo Fixture Area

This directory contains the deterministic compiled-entry corpus frozen by the
0.8 T29 readiness gate and intended to seed 0.9 parser work.

No production compiled terminfo parser belonged in 0.8. The checked-in corpus is
now the normative fixture baseline for 0.9 parser work.

The fixture roles are:

```text
source/       small documented terminfo source descriptions
compiled/     compiled entry bytes generated from the recorded `tic` baseline
manifests/    expected semantic results, hashes, and provenance
malformed/    deterministic malformed/adversarial binary seeds
```

Normal tests consume these checked-in assets and do not require `tic`, ncurses,
Python, or network access on the test host.

The valid binaries were generated with:

```text
ncurses 6.5.20250216
```

using `tic -x`. Fixture regeneration is a maintainer operation implemented by:

```text
tools/compiled-terminfo-fixtures/Icod.TermInfo.FixtureGenerator.csproj
```

Run it from anywhere inside the repository only when intentionally refreshing
the corpus:

```text
dotnet run --project tools/compiled-terminfo-fixtures/Icod.TermInfo.FixtureGenerator.csproj
```

The generator refuses to run with a different `tic -V` string so provenance
changes remain explicit. It also performs the documented Boolean-cancellation
sentinel edit, derives the malformed/adversarial seeds, and prints SHA-256 values
for the regenerated corpus.

After regeneration, compare those values with `manifests/manifest.json` and
update the manifest only when the fixture change is intentional and reviewed.

See `docs/0.8.0-T29-0.9-READINESS.md` for the frozen binary/vendor/provider
contract and detailed fixture rationale.
