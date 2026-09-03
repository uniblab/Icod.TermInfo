from pathlib import Path

replacements = {
    "tests/Icod.TermInfo.Inspection.Tests/src/MI02TerminalDescriptionJsonTests.cs": [
        (
            'Assert.True( fixture.EndsWith( "\\n", StringComparison.Ordinal ) );',
            'Assert.EndsWith( "\\n", fixture, StringComparison.Ordinal );',
        ),
    ],
    "tests/Icod.TermInfo.Inspection.Tests/src/MI03ComparisonAndPlanJsonTests.cs": [
        (
            'Assert.True( fixture.EndsWith( "\\n", StringComparison.Ordinal ) );',
            'Assert.EndsWith( "\\n", fixture, StringComparison.Ordinal );',
        ),
    ],
    "tests/Icod.TermInfo.Inspection.Tests/src/MI04DatabaseCatalogJsonAndSchemaTests.cs": [
        (
            'Assert.True( fixture.EndsWith( "\\n", StringComparison.Ordinal ) );',
            'Assert.EndsWith( "\\n", fixture, StringComparison.Ordinal );',
        ),
        (
            'entries[ index ]\n\t\t\t\t\t.GetProperty( "aliases" )\n\t\t\t\t\t.EnumerateArray()\n\t\t\t\t\t.Select( value => value.GetString() )\n\t\t\t\t\t.ToArray()',
            'entries[ index ]\n\t\t\t\t\t.GetProperty( "aliases" )\n\t\t\t\t\t.EnumerateArray()\n\t\t\t\t\t.Select( value => value.GetString()! )\n\t\t\t\t\t.ToArray()',
        ),
        (
            'data\n\t\t\t\t.GetProperty( "duplicateCanonicalNames" )\n\t\t\t\t.EnumerateArray()\n\t\t\t\t.Select( value => value.GetString() )\n\t\t\t\t.ToArray()',
            'data\n\t\t\t\t.GetProperty( "duplicateCanonicalNames" )\n\t\t\t\t.EnumerateArray()\n\t\t\t\t.Select( value => value.GetString()! )\n\t\t\t\t.ToArray()',
        ),
    ],
    "tests/Icod.TermInfo.Toe.Tests/src/MI05JsonCatalogCommandTests.cs": [
        (
            'Assert.True(\n\t\t\t\tresult.Stdout.EndsWith( "\\n", StringComparison.Ordinal )\n\t\t\t);',
            'Assert.EndsWith(\n\t\t\t\t"\\n",\n\t\t\t\tresult.Stdout,\n\t\t\t\tStringComparison.Ordinal\n\t\t\t);',
        ),
    ],
}

for filename, file_replacements in replacements.items():
    path = Path(filename)
    text = path.read_text(encoding="utf-8")
    for old, new in file_replacements:
        count = text.count(old)
        if count != 1:
            raise RuntimeError(
                f"Expected exactly one occurrence in {filename}, found {count}: {old!r}"
            )
        text = text.replace(old, new, 1)
    path.write_text(text, encoding="utf-8", newline="\n")
