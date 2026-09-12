from pathlib import Path

# Temporary branch-scoped driver; removed after the convention gate is green.
# Current checkpoint normalizes the final three ternary findings.


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
    "tests/Icod.TermInfo.Inspection.Tests/src/I07ValidationTests.cs",
    """\t\treturn value.HasValue\n\t\t\t? value.Value.ToString( CultureInfo.InvariantCulture )\n\t\t\t: string.Empty;\n""",
    """\t\treturn ( value.HasValue )\n\t\t\t? value.Value.ToString( CultureInfo.InvariantCulture )\n\t\t\t: string.Empty\n\t\t;\n""",
)

replace_exact(
    "toe/src/Command.cs",
    """\t\tDictionary<string, ToeDuplicateReference>? duplicateReferences =\n\t\t\toptions.AllDatabases && options.SortByName\n\t\t\t\t? new Dictionary<string, ToeDuplicateReference>( StringComparer.Ordinal )\n\t\t\t\t: null;\n""",
    """\t\tDictionary<string, ToeDuplicateReference>? duplicateReferences =\n\t\t\t( options.AllDatabases && options.SortByName )\n\t\t\t\t? new Dictionary<string, ToeDuplicateReference>( StringComparer.Ordinal )\n\t\t\t\t: null\n\t\t;\n""",
)

replace_exact(
    "tools/public-api-snapshot/Program.cs",
    """\t\t\tbool boolean =>\n\t\t\t\tboolean\n\t\t\t\t\t? \"true\"\n\t\t\t\t\t: \"false\",\n""",
    """\t\t\tbool boolean =>\n\t\t\t\t( boolean )\n\t\t\t\t\t? \"true\"\n\t\t\t\t\t: \"false\",\n""",
)
