from pathlib import Path


def replace_exact(path: str, old: str, new: str, expected_count: int = 1) -> None:
    file_path = Path(path)
    text = file_path.read_text(encoding="utf-8")
    actual_count = text.count(old)
    if actual_count != expected_count:
        raise SystemExit(
            f"{path}: expected {expected_count} occurrence(s), found {actual_count}"
        )
    file_path.write_text(text.replace(old, new), encoding="utf-8", newline="\n")


# Current-development version ownership moves from the completed 1.13 line to 1.14.
replace_exact(
    "tests/Icod.TermInfo.Tests/src/T45CompletionGateTests.cs",
    '\t\tAssert.StartsWith(\n\t\t\t"1.13.0",\n\t\t\tsemanticVersion,',
    '\t\tAssert.StartsWith(\n\t\t\t"1.14.0",\n\t\t\tsemanticVersion,',
)
replace_exact(
    "tests/Icod.TermInfo.Tests/src/T45CompletionGateTests.cs",
    '\t\tAssert.StartsWith(\n\t\t\t"1.13.0",\n\t\t\tReadRequiredProperty(',
    '\t\tAssert.StartsWith(\n\t\t\t"1.14.0",\n\t\t\tReadRequiredProperty(',
)
replace_exact(
    "tests/Icod.TermInfo.Termcap.Tests/src/TC08ContractTests.cs",
    'private const string CurrentDevelopmentVersion = "1.13.0";',
    'private const string CurrentDevelopmentVersion = "1.14.0";',
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/RP08ReleaseClosureTests.cs",
    'private const string DevelopmentVersion = "1.13.0";',
    'private const string DevelopmentVersion = "1.14.0";',
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/RS08ContractTests.cs",
    'private const string CurrentDevelopmentVersion = "1.13.0";',
    'private const string CurrentDevelopmentVersion = "1.14.0";',
)
replace_exact(
    "tests/Icod.TermInfo.Tic.Tests/src/ReleaseClosureTests.cs",
    'private const string DevelopmentVersion = "1.13.0";',
    'private const string DevelopmentVersion = "1.14.0";',
)
for path in (
    "tests/Icod.TermInfo.Tic.Tests/src/CommandTests.cs",
    "tests/Icod.TermInfo.InfoCmp.Tests/src/CommandTests.cs",
    "tests/Icod.TermInfo.Toe.Tests/src/CommandTests.cs",
):
    replace_exact(
        path,
        'Assert.Contains( "1.13.0", ReadText( stdout ) );',
        'Assert.Contains( "1.14.0", ReadText( stdout ) );',
    )
replace_exact(
    "tests/Icod.TermInfo.Router.Tests/src/ContractTests.cs",
    '\t\t\t"1.13.0",\n\t\t\tReadRequiredProperty(',
    '\t\t\t"1.14.0",\n\t\t\tReadRequiredProperty(',
)
replace_exact(
    "tests/Icod.TermInfo.Router.Tests/src/CommandTests.cs",
    'Assert.Contains( "1.13.0", ReadText( stdout ) );',
    'Assert.Contains( "1.14.0", ReadText( stdout ) );',
    expected_count=2,
)

# RE08: reconstruct the exact 1.13 surface by removing only reviewed RB01 types.
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/RE08ReleaseClosureTests.cs",
    '''\t[Fact]\n\tpublic void ExactOneThirteenInspectionSurfaceHasFreezeInputs() {\n\t\tType[] exportedTypes =\n\t\t\ttypeof( PersistentRasterRuntimeObservationSet ).Assembly.GetExportedTypes();\n\t\tAssert.Equal( 90, exportedTypes.Length );\n\t\tAssert.Equal(\n\t\t\t9,\n\t\t\texportedTypes.Count(\n\t\t\t\ttype => type.FullName?.StartsWith(\n\t\t\t\t\t"Icod.TermInfo.Inspection.PersistentRasterRuntime",\n\t\t\t\t\tStringComparison.Ordinal\n\t\t\t\t) == true\n\t\t\t)\n\t\t);\n''',
    '''\t[Fact]\n\tpublic void ExactOneThirteenInspectionSurfaceHasFreezeInputs() {\n\t\tstring oneFourteenAdditions = ReadRequiredRepositoryFile(\n\t\t\t"docs/1.14.0-RB01-INSPECTION-PUBLIC-API-ADDITIONS.txt"\n\t\t);\n\t\tHashSet<string> approvedOneFourteenTypes = oneFourteenAdditions\n\t\t\t.Split( '\\n' )\n\t\t\t.Select( line => line.Trim() )\n\t\t\t.Where(\n\t\t\t\tline =>\n\t\t\t\t\tline.Length > 0\n\t\t\t\t\t&& !line.StartsWith( "#", StringComparison.Ordinal )\n\t\t\t)\n\t\t\t.ToHashSet( StringComparer.Ordinal );\n\t\tType[] currentTypes =\n\t\t\ttypeof( PersistentRasterRuntimeObservationSet ).Assembly.GetExportedTypes();\n\t\tType[] reconstructedOneThirteenTypes = currentTypes\n\t\t\t.Where(\n\t\t\t\ttype =>\n\t\t\t\t\ttype.FullName is null\n\t\t\t\t\t|| !approvedOneFourteenTypes.Contains( type.FullName )\n\t\t\t)\n\t\t\t.ToArray();\n\n\t\tAssert.Equal( 13, approvedOneFourteenTypes.Count );\n\t\tAssert.Equal( 90, reconstructedOneThirteenTypes.Length );\n\t\tAssert.Equal(\n\t\t\tapprovedOneFourteenTypes.Count,\n\t\t\tcurrentTypes.Length - reconstructedOneThirteenTypes.Length\n\t\t);\n\t\tAssert.Equal(\n\t\t\t9,\n\t\t\treconstructedOneThirteenTypes.Count(\n\t\t\t\ttype => type.FullName?.StartsWith(\n\t\t\t\t\t"Icod.TermInfo.Inspection.PersistentRasterRuntime",\n\t\t\t\t\tStringComparison.Ordinal\n\t\t\t\t) == true\n\t\t\t)\n\t\t);\n''',
)

# PG08: current -> 1.13 (remove RB01) -> 1.12 (remove RE01-RE03 types).
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/PG08ReleaseClosureTests.cs",
    '''\t\tstring oneThirteenAdditions = ReadRepositoryFile(\n\t\t\t"docs/1.13.0-RE01-INSPECTION-PUBLIC-API-ADDITIONS.txt"\n\t\t);\n\t\tHashSet<string> approvedOneThirteenTypes = oneThirteenAdditions\n''',
    '''\t\tstring oneThirteenAdditions = ReadRepositoryFile(\n\t\t\t"docs/1.13.0-RE01-INSPECTION-PUBLIC-API-ADDITIONS.txt"\n\t\t);\n\t\tstring oneFourteenAdditions = ReadRepositoryFile(\n\t\t\t"docs/1.14.0-RB01-INSPECTION-PUBLIC-API-ADDITIONS.txt"\n\t\t);\n\t\tHashSet<string> approvedOneFourteenTypes = oneFourteenAdditions\n\t\t\t.Split( '\\n' )\n\t\t\t.Select( line => line.Trim() )\n\t\t\t.Where(\n\t\t\t\tline =>\n\t\t\t\t\tline.Length > 0\n\t\t\t\t\t&& !line.StartsWith( "#", StringComparison.Ordinal )\n\t\t\t)\n\t\t\t.ToHashSet( StringComparer.Ordinal );\n\t\tHashSet<string> approvedOneThirteenTypes = oneThirteenAdditions\n''',
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/PG08ReleaseClosureTests.cs",
    '''\t\tType[] currentTypes =\n\t\t\ttypeof( PersistentRasterPlacementProfile ).Assembly.GetExportedTypes();\n\t\tType[] reconstructedOneTwelveTypes = currentTypes\n\t\t\t.Where(\n\t\t\t\ttype =>\n\t\t\t\t\ttype.FullName is null\n\t\t\t\t\t|| !approvedOneThirteenTypes.Contains( type.FullName )\n\t\t\t)\n\t\t\t.ToArray();\n''',
    '''\t\tType[] currentTypes =\n\t\t\ttypeof( PersistentRasterPlacementProfile ).Assembly.GetExportedTypes();\n\t\tType[] reconstructedOneThirteenTypes = currentTypes\n\t\t\t.Where(\n\t\t\t\ttype =>\n\t\t\t\t\ttype.FullName is null\n\t\t\t\t\t|| !approvedOneFourteenTypes.Contains( type.FullName )\n\t\t\t)\n\t\t\t.ToArray();\n\t\tType[] reconstructedOneTwelveTypes = reconstructedOneThirteenTypes\n\t\t\t.Where(\n\t\t\t\ttype =>\n\t\t\t\t\ttype.FullName is null\n\t\t\t\t\t|| !approvedOneThirteenTypes.Contains( type.FullName )\n\t\t\t)\n\t\t\t.ToArray();\n''',
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/PG08ReleaseClosureTests.cs",
    '''\t\tAssert.Equal( 81, reconstructedOneTwelveTypes.Length );\n\t\tAssert.Equal(\n\t\t\tapprovedOneThirteenTypes.Count,\n\t\t\tcurrentTypes.Count(\n''',
    '''\t\tAssert.Equal( 13, approvedOneFourteenTypes.Count );\n\t\tAssert.Equal( 81, reconstructedOneTwelveTypes.Length );\n\t\tAssert.Equal(\n\t\t\tapprovedOneThirteenTypes.Count,\n\t\t\treconstructedOneThirteenTypes.Count(\n''',
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/PG08ReleaseClosureTests.cs",
    '''\t\tforeach ( string approvedType in approvedOneThirteenTypes ) {\n\t\t\tAssert.Contains(\n\t\t\t\tcurrentTypes,\n''',
    '''\t\tforeach ( string approvedType in approvedOneThirteenTypes ) {\n\t\t\tAssert.Contains(\n\t\t\t\treconstructedOneThirteenTypes,\n''',
)

# RL08: current -> 1.13 (remove RB01) -> 1.12 -> 1.11.
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/RL08ReleaseClosureTests.cs",
    '''\t\tstring oneThirteenAdditions = ReadRepositoryFile(\n\t\t\t"docs/1.13.0-RE01-INSPECTION-PUBLIC-API-ADDITIONS.txt"\n\t\t);\n\t\tHashSet<string> approvedOneTwelveTypes = oneTwelveAdditions\n''',
    '''\t\tstring oneThirteenAdditions = ReadRepositoryFile(\n\t\t\t"docs/1.13.0-RE01-INSPECTION-PUBLIC-API-ADDITIONS.txt"\n\t\t);\n\t\tstring oneFourteenAdditions = ReadRepositoryFile(\n\t\t\t"docs/1.14.0-RB01-INSPECTION-PUBLIC-API-ADDITIONS.txt"\n\t\t);\n\t\tHashSet<string> approvedOneFourteenTypes = oneFourteenAdditions\n\t\t\t.Split( '\\n' )\n\t\t\t.Select( line => line.Trim() )\n\t\t\t.Where(\n\t\t\t\tline =>\n\t\t\t\t\tline.Length > 0\n\t\t\t\t\t&& !line.StartsWith( "#", StringComparison.Ordinal )\n\t\t\t)\n\t\t\t.ToHashSet( StringComparer.Ordinal );\n\t\tHashSet<string> approvedOneTwelveTypes = oneTwelveAdditions\n''',
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/RL08ReleaseClosureTests.cs",
    '''\t\tType[] currentTypes =\n\t\t\ttypeof( PersistentRasterLifecycleProfile ).Assembly.GetExportedTypes();\n\t\tType[] reconstructedOneTwelveTypes = currentTypes\n\t\t\t.Where(\n\t\t\t\ttype =>\n\t\t\t\t\ttype.FullName is null\n\t\t\t\t\t|| !approvedOneThirteenTypes.Contains( type.FullName )\n\t\t\t)\n\t\t\t.ToArray();\n''',
    '''\t\tType[] currentTypes =\n\t\t\ttypeof( PersistentRasterLifecycleProfile ).Assembly.GetExportedTypes();\n\t\tType[] reconstructedOneThirteenTypes = currentTypes\n\t\t\t.Where(\n\t\t\t\ttype =>\n\t\t\t\t\ttype.FullName is null\n\t\t\t\t\t|| !approvedOneFourteenTypes.Contains( type.FullName )\n\t\t\t)\n\t\t\t.ToArray();\n\t\tType[] reconstructedOneTwelveTypes = reconstructedOneThirteenTypes\n\t\t\t.Where(\n\t\t\t\ttype =>\n\t\t\t\t\ttype.FullName is null\n\t\t\t\t\t|| !approvedOneThirteenTypes.Contains( type.FullName )\n\t\t\t)\n\t\t\t.ToArray();\n''',
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/RL08ReleaseClosureTests.cs",
    '''\t\tAssert.Equal( 67, reconstructedOneElevenTypes.Length );\n\t\tAssert.Equal(\n\t\t\tapprovedOneThirteenTypes.Count,\n\t\t\tcurrentTypes.Count(\n''',
    '''\t\tAssert.Equal( 13, approvedOneFourteenTypes.Count );\n\t\tAssert.Equal( 67, reconstructedOneElevenTypes.Length );\n\t\tAssert.Equal(\n\t\t\tapprovedOneThirteenTypes.Count,\n\t\t\treconstructedOneThirteenTypes.Count(\n''',
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/RL08ReleaseClosureTests.cs",
    '''\t\tforeach ( string approvedType in approvedOneThirteenTypes ) {\n\t\t\tAssert.Contains(\n\t\t\t\tcurrentTypes,\n''',
    '''\t\tforeach ( string approvedType in approvedOneThirteenTypes ) {\n\t\t\tAssert.Contains(\n\t\t\t\treconstructedOneThirteenTypes,\n''',
)
