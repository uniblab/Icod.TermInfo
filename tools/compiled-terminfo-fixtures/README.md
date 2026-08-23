# Compiled Terminfo Fixture Generator

This is the maintainer-only generator for the deterministic compiled terminfo
corpus under `tests/Icod.TermInfo.Tests/fixtures/compiled-terminfo`.

The tool:

- requires the exact documented `tic -V` provenance before changing fixtures;
- compiles each checked-in `.ti` source with `tic -x`;
- applies the frozen Boolean-cancellation sentinel edit;
- derives the checked-in malformed/adversarial binary seeds;
- prints SHA-256 values for the regenerated corpus.

Run from anywhere inside the repository with:

```text
dotnet run --project tools/compiled-terminfo-fixtures/Icod.TermInfo.FixtureGenerator.csproj
```

`tic` is required only when intentionally regenerating the corpus. Normal build,
test, package validation, and fixture consumption do not require `tic`, Python,
or network access.
