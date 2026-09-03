# Icod.TermInfo.DatabaseSet.Sample

This sample demonstrates the reusable public APIs added by `Icod.TermInfo 1.10`
for deterministic inspection, comparison, planning, and JSON rendering over an
ordered set of explicit conventional terminfo databases.

The sample creates four temporary conventional databases through
`Icod.TermInfo.Compiler`, so it does not depend on the host `TERM` value, ambient
terminfo discovery, or the host's installed terminfo database.

It demonstrates:

- `TermInfoDatabaseInspector.InspectSet(...)`;
- `TermInfoDatabaseSet.LookupCanonicalName(...)` and first-root precedence;
- semantic winner/shadow classification through `AnalyzeSemantics()`;
- alias-collision evidence;
- `TermInfoDatabaseSetComparer.Compare(...)`;
- `TerminalDescriptionSourcePlanner.PlanFromDatabaseSet(...)`;
- version-2 `databaseSet`, `databaseSetComparison`, and `databaseSetPlan` JSON.

## Run the sample

```text
dotnet run --project samples/Icod.TermInfo.DatabaseSet.Sample/Icod.TermInfo.DatabaseSet.Sample.csproj -f net10.0
```

The project also targets `net8.0` and `net9.0`.

The sample normalizes its generated temporary root to `$ROOT` before displaying
JSON so output is deterministic across Windows, Linux, macOS, and separate
process executions.

## Inspection fixture

The first two databases deliberately publish conflicting state:

```text
inspection-first
    sample-shared        cols#80, alias sample-collision

inspection-second
    sample-shared        cols#132
    sample-other-owner   cols#100, alias sample-collision
```

Because roots are ordered, `sample-shared` from `inspection-first` is the
conclusive winner and the later `sample-shared` is a shadow. The semantic
analysis classifies that shadow as `SemanticallyDifferent`. The alias
`sample-collision` is owned by two different canonical identities and therefore
retains explicit collision evidence.

This is different from an incomplete earlier root. If an earlier database is
incomplete, a later observed entry cannot be promoted to a definitive winner;
`LookupCanonicalName(...)` reports `Indeterminate` instead.

## Comparison fixture

The sample compares the two-root inspection set with the one-root
`inspection-first` set. `TermInfoDatabaseSetComparer` reports effective and/or
structural differences rather than reducing the result to one boolean.

Use `AreEffectivelyEquivalent`, `AreStructurallyEquivalent`, `AreEquivalent`,
and the ordered `Differences` evidence according to the question your
application needs to answer.

## Planning fixture

Planning uses a separate conflict-free ordered candidate set:

```text
planning-first
    sample-parent-a      am

planning-second
    sample-parent-b      cols#80
```

The target contains both `am` and `cols#80`, so the frozen planner selects both
complementary parents. Planning deliberately does not use the conflicting
inspection roots: a candidate universe containing semantically conflicting
physical publications for the same canonical name is rejected rather than
silently resolved by precedence.

## Machine-readable output

The sample renders three frozen version-2 document kinds:

```text
databaseSet
databaseSetComparison
databaseSetPlan
```

The schema identifier is:

```text
urn:icod:terminfo:inspection:json:2
```

The version-1 schema and all 1.9 JSON document kinds remain separate and frozen.

## Checked-in fixtures

The release gate runs the sample against these normalized fixtures on
`net8.0`, `net9.0`, and `net10.0`:

```text
expected/database-set.json
expected/database-set-comparison.json
expected/database-set-plan.json
```

Maintainers can regenerate the normalized fixtures explicitly with:

```text
dotnet run --project samples/Icod.TermInfo.DatabaseSet.Sample/Icod.TermInfo.DatabaseSet.Sample.csproj -f net10.0 -- --write-fixtures samples/Icod.TermInfo.DatabaseSet.Sample/expected
```

Normal validation uses `--verify-fixtures`; fixture regeneration is never part
of the release gate.

For the complete consumer guide, see
`../../docs/1.10.0-MULTI-DATABASE-GUIDE.md`.
