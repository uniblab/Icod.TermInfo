from pathlib import Path

# Temporary branch-scoped driver; removed after the convention gate is green.
# Current checkpoint normalizes the final reduced ternary inventory.


def replace_exact(path: str, old: str, new: str) -> None:
    target = Path(path)
    text = target.read_text(encoding="utf-8")
    old_count = text.count(old)
    new_count = text.count(new)
    embedded_old_count = new.count(old)
    if new_count == 1 and old_count == embedded_old_count:
        return
    if new_count == 0 and old_count == 1:
        target.write_text(text.replace(old, new, 1), encoding="utf-8", newline="\n")
        return
    raise SystemExit(
        f"{path}: expected one old target or one normalized target; "
        f"found old={old_count}, new={new_count}"
    )


replace_exact(
    "Icod.TermInfo.Termcap/src/TermcapSourceParser.cs",
    """\t\treturn hasErrors\n\t\t\t? null\n\t\t\t: value.ToString()\n\t\t;\n""",
    """\t\treturn ( hasErrors )\n\t\t\t? null\n\t\t\t: value.ToString()\n\t\t;\n""",
)

replace_exact(
    "infocmp/src/InfoCmpInspector.cs",
    """\t\t\tstring? databaseDirectory =\n\t\t\t\tindex == 0\n\t\t\t\t\t? options.DatabaseDirectory\n\t\t\t\t\t: options.ComparisonDatabaseDirectory;\n""",
    """\t\t\tstring? databaseDirectory =\n\t\t\t\t( index == 0 )\n\t\t\t\t\t? options.DatabaseDirectory\n\t\t\t\t\t: options.ComparisonDatabaseDirectory\n\t\t\t;\n""",
)

replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/I07ValidationTests.cs",
    """\t\treturn aliases is null\n\t\t\t? string.Empty\n\t\t\t: string.Join( \",\", aliases );\n""",
    """\t\treturn ( aliases is null )\n\t\t\t? string.Empty\n\t\t\t: string.Join( \",\", aliases )\n\t\t;\n""",
)

replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/RP02PlanningTests.cs",
    """\t\tint[] selectedCandidateIndices =\n\t\t\tcandidateIndex.HasValue\n\t\t\t\t? [ candidateIndex.Value ]\n\t\t\t\t: [];\n""",
    """\t\tint[] selectedCandidateIndices =\n\t\t\t( candidateIndex.HasValue )\n\t\t\t\t? [ candidateIndex.Value ]\n\t\t\t\t: []\n\t\t;\n""",
)

replace_exact(
    "tests/Icod.TermInfo.Termcap.Tests/src/TC08DifferentialValidationTests.cs",
    """\t\t\treturn _values.TryGetValue(\n\t\t\t\tname,\n\t\t\t\tout string? value\n\t\t\t)\n\t\t\t\t? value\n\t\t\t\t: null\n\t\t\t;\n""",
    """\t\t\treturn ( _values.TryGetValue(\n\t\t\t\tname,\n\t\t\t\tout string? value\n\t\t\t) )\n\t\t\t\t? value\n\t\t\t\t: null\n\t\t\t;\n""",
)

replace_exact(
    "toe/src/Command.cs",
    """\t\t\treturn listing.HasOperationalFailure\n\t\t\t\t? CommandExitCodes.Failure\n\t\t\t\t: CommandExitCodes.Success\n\t\t\t;\n""",
    """\t\t\treturn ( listing.HasOperationalFailure )\n\t\t\t\t? CommandExitCodes.Failure\n\t\t\t\t: CommandExitCodes.Success\n\t\t\t;\n""",
)

replace_exact(
    "toe/src/ToeSourceDependencyAnalyzer.cs",
    """\t\t\treturn (\n\t\t\t\tsource.Length != 0\n\t\t\t\t\t&& source[ 0 ] == '\\uFEFF'\n\t\t\t\t\t\t? source[ 1.. ]\n\t\t\t\t\t\t: source,\n\t\t\t\tnull\n\t\t\t);\n""",
    """\t\t\treturn (\n\t\t\t\t(\n\t\t\t\t\tsource.Length != 0\n\t\t\t\t\t&& source[ 0 ] == '\\uFEFF'\n\t\t\t\t)\n\t\t\t\t\t? source[ 1.. ]\n\t\t\t\t\t: source,\n\t\t\t\tnull\n\t\t\t);\n""",
)

replace_exact(
    "tools/public-api-snapshot/Program.cs",
    """\t\t\t+ ( string.IsNullOrEmpty( attributes )\n\t\t\t\t? string.Empty\n\t\t\t\t: $\" attrs={attributes}\" );\n""",
    """\t\t\t+ ( ( string.IsNullOrEmpty( attributes ) )\n\t\t\t\t? string.Empty\n\t\t\t\t: $\" attrs={attributes}\" );\n""",
)
