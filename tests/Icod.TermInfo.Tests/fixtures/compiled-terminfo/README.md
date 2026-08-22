# Compiled Terminfo Fixture Area

This directory is reserved for the deterministic compiled-entry corpus required
by the 0.8 T29 readiness gate and used to begin 0.9 parser work.

No production compiled terminfo parser belongs in 0.8.

The intended fixture roles are:

```text
source/       small documented terminfo source descriptions
compiled/     compiled entry bytes generated from a recorded `tic` baseline
manifests/    expected semantic results
malformed/    deterministic malformed/adversarial binary seeds
```

Each future fixture should record enough provenance to reproduce or audit it,
including the source description, authoritative ncurses/`tic` baseline, and
expected semantic result.

Normal tests consume checked-in assets and must not require `tic`, ncurses, or
network access on the test host.
