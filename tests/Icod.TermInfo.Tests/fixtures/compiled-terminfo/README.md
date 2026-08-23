# Compiled Terminfo Fixture Area

This directory contains the deterministic compiled-entry corpus frozen by the
0.8 T29 readiness gate and intended to seed 0.9 parser work.

No production compiled terminfo parser belongs in 0.8.

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

using `tic -x`. `generate-fixtures.py` is a maintainer-only regeneration tool.
It refuses to run with a different `tic -V` string so provenance changes remain
explicit. The script also performs the documented Boolean-cancellation sentinel
edit and derives the malformed/adversarial seeds.

Run it from this directory only when intentionally refreshing the corpus:

```text
python3 generate-fixtures.py
```

After regeneration, compare the printed SHA-256 values with
`manifests/manifest.json` and update the manifest only when the fixture change is
intentional and reviewed.

See `docs/0.8.0-T29-0.9-READINESS.md` for the frozen binary/vendor/provider
contract and detailed fixture rationale.
