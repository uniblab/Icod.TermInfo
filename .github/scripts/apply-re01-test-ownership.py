from pathlib import Path


def read(path: str) -> str:
    return Path(path).read_text(encoding="utf-8")


def write(path: str, text: str) -> None:
    Path(path).write_text(text, encoding="utf-8", newline="\n")


def replace_once(path: str, old: str, new: str) -> None:
    text = read(path)
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f"{path}: expected one match, found {count}: {old[:100]!r}")
    write(path, text.replace(old, new, 1))


def replace_count(path: str, old: str, new: str, expected: int) -> None:
    text = read(path)
    count = text.count(old)
    if count != expected:
        raise RuntimeError(
            f"{path}: expected {expected} matches, found {count}: {old[:100]!r}"
        )
    write(path, text.replace(old, new))


def replace_between(path: str, start: str, end: str, replacement: str) -> None:
    text = read(path)
    start_index = text.find(start)
    if start_index < 0:
        raise RuntimeError(f"{path}: start marker not found: {start!r}")
    if text.find(start, start_index + 1) >= 0:
        raise RuntimeError(f"{path}: start marker is not unique: {start!r}")
    end_index = text.find(end, start_index)
    if end_index < 0:
        raise RuntimeError(f"{path}: end marker not found: {end!r}")
    write(path, text[:start_index] + replacement + text[end_index:])


# Current-development ownership: these tests deliberately identify the active line.
replace_once(
    "tests/Icod.TermInfo.Termcap.Tests/src/TC08ContractTests.cs",
    'private const string CurrentDevelopmentVersion = "1.12.0";',
    'private const string CurrentDevelopmentVersion = "1.13.0";',
)
replace_once(
    "tests/Icod.TermInfo.Inspection.Tests/src/RS08ContractTests.cs",
    'private const string CurrentDevelopmentVersion = "1.12.0";',
    'private const string CurrentDevelopmentVersion = "1.13.0";',
)
replace_once(
    "tests/Icod.TermInfo.Inspection.Tests/src/RP08ReleaseClosureTests.cs",
    'private const string DevelopmentVersion = "1.12.0";',
    'private const string DevelopmentVersion = "1.13.0";',
)
replace_once(
    "tests/Icod.TermInfo.Inspection.Tests/src/RP08ReleaseClosureTests.cs",
    '''\t\tAssert.Contains(\n\t\t\t"1.12.0",\n\t\t\tactiveRoadmap,\n\t\t\tStringComparison.Ordinal\n\t\t);''',
    '''\t\tAssert.Contains(\n\t\t\t"1.13.0",\n\t\t\tactiveRoadmap,\n\t\t\tStringComparison.Ordinal\n\t\t);''',
)
replace_once(
    "tests/Icod.TermInfo.Tic.Tests/src/ReleaseClosureTests.cs",
    'private const string DevelopmentVersion = "1.12.0";',
    'private const string DevelopmentVersion = "1.13.0";',
)
replace_once(
    "tests/Icod.TermInfo.Tests/src/T45CompletionGateTests.cs",
    '''\t\tAssert.StartsWith(\n\t\t\t"1.12.0",\n\t\t\tsemanticVersion,\n\t\t\tStringComparison.Ordinal\n\t\t);''',
    '''\t\tAssert.StartsWith(\n\t\t\t"1.13.0",\n\t\t\tsemanticVersion,\n\t\t\tStringComparison.Ordinal\n\t\t);''',
)
replace_once(
    "tests/Icod.TermInfo.Tests/src/T45CompletionGateTests.cs",
    '''\t\tAssert.StartsWith(\n\t\t\t"1.12.0",\n\t\t\tReadRequiredProperty(\n\t\t\t\tbuildProperties,\n\t\t\t\t"IcodTermInfoSuiteVersion"\n\t\t\t),\n\t\t\tStringComparison.Ordinal\n\t\t);''',
    '''\t\tAssert.StartsWith(\n\t\t\t"1.13.0",\n\t\t\tReadRequiredProperty(\n\t\t\t\tbuildProperties,\n\t\t\t\t"IcodTermInfoSuiteVersion"\n\t\t\t),\n\t\t\tStringComparison.Ordinal\n\t\t);''',
)
replace_count(
    "tests/Icod.TermInfo.Tic.Tests/src/CommandTests.cs",
    'Assert.Contains( "1.12.0", ReadText( stdout ) );',
    'Assert.Contains( "1.13.0", ReadText( stdout ) );',
    1,
)
replace_count(
    "tests/Icod.TermInfo.InfoCmp.Tests/src/CommandTests.cs",
    'Assert.Contains( "1.12.0", ReadText( stdout ) );',
    'Assert.Contains( "1.13.0", ReadText( stdout ) );',
    1,
)
replace_count(
    "tests/Icod.TermInfo.Toe.Tests/src/CommandTests.cs",
    'Assert.Contains( "1.12.0", ReadText( stdout ) );',
    'Assert.Contains( "1.13.0", ReadText( stdout ) );',
    1,
)
replace_once(
    "tests/Icod.TermInfo.Router.Tests/src/ContractTests.cs",
    '''\t\tAssert.StartsWith(\n\t\t\t"1.12.0",\n\t\t\tReadRequiredProperty(\n\t\t\t\tbuildProperties,\n\t\t\t\t"IcodTermInfoSuiteVersion"\n\t\t\t),\n\t\t\tStringComparison.Ordinal\n\t\t);''',
    '''\t\tAssert.StartsWith(\n\t\t\t"1.13.0",\n\t\t\tReadRequiredProperty(\n\t\t\t\tbuildProperties,\n\t\t\t\t"IcodTermInfoSuiteVersion"\n\t\t\t),\n\t\t\tStringComparison.Ordinal\n\t\t);''',
)
replace_count(
    "tests/Icod.TermInfo.Router.Tests/src/CommandTests.cs",
    'Assert.Contains( "1.12.0", ReadText( stdout ) );',
    'Assert.Contains( "1.13.0", ReadText( stdout ) );',
    2,
)

# PG08 owns the immutable 1.12 surface, not the current assembly as a whole.
pg08_path = "tests/Icod.TermInfo.Inspection.Tests/src/PG08ReleaseClosureTests.cs"
replace_between(
    pg08_path,
    '''\t[Fact]\n\tpublic void ExactOneTwelveInspectionSurfaceIsFrozen() {''',
    '''\t[Fact]\n\tpublic void AllFourJsonSchemasHaveExactFrozenFingerprints() {''',
    '''\t[Fact]\n\tpublic void ExactOneTwelveInspectionSurfaceIsFrozen() {\n\t\tstring freeze = ReadRepositoryFile(\n\t\t\t"docs/1.12.0-INSPECTION-PUBLIC-API-FREEZE.md"\n\t\t);\n\t\tstring fingerprints = ReadRepositoryFile(\n\t\t\t"docs/1.12.0-PG08-FREEZE-FINGERPRINTS.txt"\n\t\t);\n\t\tstring additions = ReadRepositoryFile(\n\t\t\t"docs/1.12.0-PG01-INSPECTION-PUBLIC-API-ADDITIONS.txt"\n\t\t);\n\t\tstring additiveMembers = ReadRepositoryFile(\n\t\t\t"docs/1.12.0-PG06-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt"\n\t\t);\n\t\tstring oneThirteenAdditions = ReadRepositoryFile(\n\t\t\t"docs/1.13.0-RE01-INSPECTION-PUBLIC-API-ADDITIONS.txt"\n\t\t);\n\t\tHashSet<string> approvedOneThirteenTypes = oneThirteenAdditions\n\t\t\t.Split( '\\n' )\n\t\t\t.Select( line => line.Trim() )\n\t\t\t.Where(\n\t\t\t\tline =>\n\t\t\t\t\tline.Length > 0\n\t\t\t\t\t&& !line.StartsWith( "#", StringComparison.Ordinal )\n\t\t\t)\n\t\t\t.ToHashSet( StringComparer.Ordinal );\n\t\tType[] currentTypes =\n\t\t\ttypeof( PersistentRasterPlacementProfile ).Assembly.GetExportedTypes();\n\t\tType[] reconstructedOneTwelveTypes = currentTypes\n\t\t\t.Where(\n\t\t\t\ttype =>\n\t\t\t\t\ttype.FullName is null\n\t\t\t\t\t|| !approvedOneThirteenTypes.Contains( type.FullName )\n\t\t\t)\n\t\t\t.ToArray();\n\t\tstring compatibility = ReadRepositoryFile(\n\t\t\t".github/scripts/verify-inspection-compatibility.ps1"\n\t\t);\n\n\t\tAssert.Equal( 81, reconstructedOneTwelveTypes.Length );\n\t\tAssert.Equal(\n\t\t\tapprovedOneThirteenTypes.Count,\n\t\t\tcurrentTypes.Count(\n\t\t\t\ttype =>\n\t\t\t\t\ttype.FullName?.StartsWith(\n\t\t\t\t\t\t"Icod.TermInfo.Inspection.PersistentRasterRuntime",\n\t\t\t\t\t\tStringComparison.Ordinal\n\t\t\t\t\t) == true\n\t\t\t)\n\t\t);\n\t\tforeach ( string approvedType in approvedOneThirteenTypes ) {\n\t\t\tAssert.Contains(\n\t\t\t\tcurrentTypes,\n\t\t\t\ttype => string.Equals(\n\t\t\t\t\ttype.FullName,\n\t\t\t\t\tapprovedType,\n\t\t\t\t\tStringComparison.Ordinal\n\t\t\t\t)\n\t\t\t);\n\t\t}\n\t\tAssert.Contains( InspectionApiSha256, freeze, StringComparison.Ordinal );\n\t\tAssert.Contains( InspectionApiSha256, fingerprints, StringComparison.Ordinal );\n\t\tAssert.Contains( InspectionApiSha256, compatibility, StringComparison.Ordinal );\n\t\tAssert.Contains(\n\t\t\t"Icod.TermInfo.Inspection.PersistentRasterPlacementProfile",\n\t\t\tadditions,\n\t\t\tStringComparison.Ordinal\n\t\t);\n\t\tAssert.Contains(\n\t\t\t"PersistentRasterPlacementSchemaIdentifier",\n\t\t\tadditiveMembers,\n\t\t\tStringComparison.Ordinal\n\t\t);\n\t}\n\n''',
)
replace_once(
    pg08_path,
    '''\t\tstring inspectionProject = ReadRepositoryFile(\n\t\t\t"Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj"\n\t\t);\n\t\tstring buildProperties = ReadRepositoryFile( "Directory.Build.props" );''',
    '''\t\tstring inspectionProject = ReadRepositoryFile(\n\t\t\t"Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj"\n\t\t);''',
)
replace_once(
    pg08_path,
    '''\n\t\tAssert.Contains(\n\t\t\t"<IcodTermInfoSuiteVersion>1.12.0</IcodTermInfoSuiteVersion>",\n\t\t\tbuildProperties,\n\t\t\tStringComparison.Ordinal\n\t\t);''',
    '',
)

# RL08 reconstructs 1.11 from the current additive assembly by subtracting
# both the reviewed 1.13 and reviewed 1.12 type families.
rl08_path = "tests/Icod.TermInfo.Inspection.Tests/src/RL08ReleaseClosureTests.cs"
replace_between(
    rl08_path,
    '''\t[Fact]\n\tpublic void ExactOneElevenInspectionSurfaceIsFrozen() {''',
    '''\t[Fact]\n\tpublic void AllThreeJsonSchemasHaveExactFrozenFingerprints() {''',
    '''\t[Fact]\n\tpublic void ExactOneElevenInspectionSurfaceIsFrozen() {\n\t\tstring freeze = ReadRepositoryFile(\n\t\t\t"docs/1.11.0-INSPECTION-PUBLIC-API-FREEZE.md"\n\t\t);\n\t\tstring fingerprints = ReadRepositoryFile(\n\t\t\t"docs/1.11.0-RL08-FREEZE-FINGERPRINTS.txt"\n\t\t);\n\t\tstring additions = ReadRepositoryFile(\n\t\t\t"docs/1.11.0-INSPECTION-PUBLIC-API-ADDITIONS.txt"\n\t\t);\n\t\tstring additiveMembers = ReadRepositoryFile(\n\t\t\t"docs/1.11.0-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt"\n\t\t);\n\t\tstring oneTwelveAdditions = ReadRepositoryFile(\n\t\t\t"docs/1.12.0-PG01-INSPECTION-PUBLIC-API-ADDITIONS.txt"\n\t\t);\n\t\tstring oneThirteenAdditions = ReadRepositoryFile(\n\t\t\t"docs/1.13.0-RE01-INSPECTION-PUBLIC-API-ADDITIONS.txt"\n\t\t);\n\t\tHashSet<string> approvedOneTwelveTypes = oneTwelveAdditions\n\t\t\t.Split( '\\n' )\n\t\t\t.Select( line => line.Trim() )\n\t\t\t.Where(\n\t\t\t\tline =>\n\t\t\t\t\tline.Length > 0\n\t\t\t\t\t&& !line.StartsWith( "#", StringComparison.Ordinal )\n\t\t\t)\n\t\t\t.ToHashSet( StringComparer.Ordinal );\n\t\tHashSet<string> approvedOneThirteenTypes = oneThirteenAdditions\n\t\t\t.Split( '\\n' )\n\t\t\t.Select( line => line.Trim() )\n\t\t\t.Where(\n\t\t\t\tline =>\n\t\t\t\t\tline.Length > 0\n\t\t\t\t\t&& !line.StartsWith( "#", StringComparison.Ordinal )\n\t\t\t)\n\t\t\t.ToHashSet( StringComparer.Ordinal );\n\t\tType[] currentTypes =\n\t\t\ttypeof( PersistentRasterLifecycleProfile ).Assembly.GetExportedTypes();\n\t\tType[] reconstructedOneTwelveTypes = currentTypes\n\t\t\t.Where(\n\t\t\t\ttype =>\n\t\t\t\t\ttype.FullName is null\n\t\t\t\t\t|| !approvedOneThirteenTypes.Contains( type.FullName )\n\t\t\t)\n\t\t\t.ToArray();\n\t\tType[] reconstructedOneElevenTypes = reconstructedOneTwelveTypes\n\t\t\t.Where(\n\t\t\t\ttype =>\n\t\t\t\t\ttype.FullName is null\n\t\t\t\t\t|| !approvedOneTwelveTypes.Contains( type.FullName )\n\t\t\t)\n\t\t\t.ToArray();\n\n\t\tAssert.Equal( 67, reconstructedOneElevenTypes.Length );\n\t\tAssert.Equal(\n\t\t\tapprovedOneThirteenTypes.Count,\n\t\t\tcurrentTypes.Count(\n\t\t\t\ttype =>\n\t\t\t\t\ttype.FullName?.StartsWith(\n\t\t\t\t\t\t"Icod.TermInfo.Inspection.PersistentRasterRuntime",\n\t\t\t\t\t\tStringComparison.Ordinal\n\t\t\t\t\t) == true\n\t\t\t)\n\t\t);\n\t\tAssert.Equal(\n\t\t\tapprovedOneTwelveTypes.Count,\n\t\t\treconstructedOneTwelveTypes.Count(\n\t\t\t\ttype =>\n\t\t\t\t\ttype.FullName?.StartsWith(\n\t\t\t\t\t\t"Icod.TermInfo.Inspection.PersistentRasterPlacement",\n\t\t\t\t\t\tStringComparison.Ordinal\n\t\t\t\t\t) == true\n\t\t\t)\n\t\t);\n\t\tforeach ( string approvedType in approvedOneTwelveTypes ) {\n\t\t\tAssert.Contains(\n\t\t\t\treconstructedOneTwelveTypes,\n\t\t\t\ttype => string.Equals(\n\t\t\t\t\ttype.FullName,\n\t\t\t\t\tapprovedType,\n\t\t\t\t\tStringComparison.Ordinal\n\t\t\t\t)\n\t\t\t);\n\t\t}\n\t\tforeach ( string approvedType in approvedOneThirteenTypes ) {\n\t\t\tAssert.Contains(\n\t\t\t\tcurrentTypes,\n\t\t\t\ttype => string.Equals(\n\t\t\t\t\ttype.FullName,\n\t\t\t\t\tapprovedType,\n\t\t\t\t\tStringComparison.Ordinal\n\t\t\t\t)\n\t\t\t);\n\t\t}\n\t\tAssert.Contains(\n\t\t\t"Icod.TermInfo.Inspection.PersistentRasterPlacementSubject",\n\t\t\toneTwelveAdditions,\n\t\t\tStringComparison.Ordinal\n\t\t);\n\t\tAssert.Contains( InspectionApiSha256, freeze, StringComparison.Ordinal );\n\t\tAssert.Contains( InspectionApiSha256, fingerprints, StringComparison.Ordinal );\n\t\tAssert.Contains(\n\t\t\t"Icod.TermInfo.Inspection.PersistentRasterLifecycleProfile",\n\t\t\tadditions,\n\t\t\tStringComparison.Ordinal\n\t\t);\n\t\tAssert.Contains(\n\t\t\t"PersistentRasterLifecycleSchemaIdentifier",\n\t\t\tadditiveMembers,\n\t\t\tStringComparison.Ordinal\n\t\t);\n\t}\n\n''',
)
replace_once(
    rl08_path,
    '''\t\tAssert.Contains(\n\t\t\t"1.12.0-PG01-INSPECTION-PUBLIC-API-ADDITIONS.txt",\n\t\t\tcompatibility,\n\t\t\tStringComparison.Ordinal\n\t\t);''',
    '''\t\tAssert.Contains(\n\t\t\t"1.12.0-PG01-INSPECTION-PUBLIC-API-ADDITIONS.txt",\n\t\t\tcompatibility,\n\t\t\tStringComparison.Ordinal\n\t\t);\n\t\tAssert.Contains(\n\t\t\t"1.13.0-RE01-INSPECTION-PUBLIC-API-ADDITIONS.txt",\n\t\t\tcompatibility,\n\t\t\tStringComparison.Ordinal\n\t\t);''',
)
