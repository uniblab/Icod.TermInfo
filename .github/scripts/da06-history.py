from pathlib import Path


def replace_exact(path_name: str, old: str, new: str, count: int = 1) -> None:
    path = Path(path_name)
    text = path.read_text(encoding="utf-8")
    actual = text.count(old)
    if actual != count:
        raise RuntimeError(
            f"{path_name}: expected {count} occurrence(s), found {actual}: {old!r}"
        )
    path.write_text(text.replace(old, new, count), encoding="utf-8", newline="\n")


replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/MI01JsonContractTests.cs",
    '''\t\tAssert.Equal( 8, renderMethods.Length );\n\t\tAssert.All(\n\t\t\trenderMethods,\n\t\t\tmethod => Assert.Equal( typeof( string ), method.ReturnType )\n\t\t);\n\t\tType[] oneParameterTypes =\n\t\t\trenderMethods\n\t\t\t\t.Where( method => method.GetParameters().Length == 1 )\n\t\t\t\t.Select( method => method.GetParameters()[ 0 ].ParameterType )\n\t\t\t\t.ToArray();\n\t\tAssert.Equal( 4, oneParameterTypes.Length );\n\t\tAssert.Contains( typeof( TerminalDescription ), oneParameterTypes );\n\t\tAssert.Contains( typeof( TermInfoComparisonResult ), oneParameterTypes );\n\t\tAssert.Contains( typeof( TerminalDescriptionSourcePlan ), oneParameterTypes );\n\t\tAssert.Contains( typeof( TermInfoDatabaseCatalog ), oneParameterTypes );''',
    '''\t\tAssert.InRange( renderMethods.Length, 8, int.MaxValue );\n\t\tAssert.All(\n\t\t\trenderMethods,\n\t\t\tmethod => Assert.Equal( typeof( string ), method.ReturnType )\n\t\t);\n\t\tType[] oneParameterTypes =\n\t\t\trenderMethods\n\t\t\t\t.Where( method => method.GetParameters().Length == 1 )\n\t\t\t\t.Select( method => method.GetParameters()[ 0 ].ParameterType )\n\t\t\t\t.ToArray();\n\t\tAssert.Contains( typeof( TerminalDescription ), oneParameterTypes );\n\t\tAssert.Contains( typeof( TermInfoComparisonResult ), oneParameterTypes );\n\t\tAssert.Contains( typeof( TerminalDescriptionSourcePlan ), oneParameterTypes );\n\t\tAssert.Contains( typeof( TermInfoDatabaseCatalog ), oneParameterTypes );\n\n\t\tType[] frozenVersionOneTypes =\n\t\t[\n\t\t\ttypeof( TerminalDescription ),\n\t\t\ttypeof( TermInfoComparisonResult ),\n\t\t\ttypeof( TerminalDescriptionSourcePlan ),\n\t\t\ttypeof( TermInfoDatabaseCatalog ),\n\t\t];\n\t\tforeach ( Type frozenType in frozenVersionOneTypes ) {\n\t\t\tAssert.Contains(\n\t\t\t\trenderMethods,\n\t\t\t\tmethod => method.GetParameters().Length == 1\n\t\t\t\t\t&& method.GetParameters()[ 0 ].ParameterType == frozenType\n\t\t\t);\n\t\t\tAssert.Contains(\n\t\t\t\trenderMethods,\n\t\t\t\tmethod => method.GetParameters().Length == 3\n\t\t\t\t\t&& method.GetParameters()[ 0 ].ParameterType == frozenType\n\t\t\t\t\t&& method.GetParameters()[ 1 ].ParameterType\n\t\t\t\t\t\t== typeof( TermInfoJsonRendererOptions )\n\t\t\t\t\t&& method.GetParameters()[ 2 ].ParameterType\n\t\t\t\t\t\t== typeof( CancellationToken )\n\t\t\t);\n\t\t}''',
)

replace_exact(
    "tests/Icod.TermInfo.Toe.Tests/src/MI05JsonCatalogCommandTests.cs",
    '''\tpublic async Task JsonRequiresOneUnmodifiedExplicitDirectory() {\n\t\tforeach ( string[] args in new[] {\n\t\t\tnew[] { "--json" },\n\t\t\t[ "--json", "first", "second" ],\n\t\t\t[ "--json", "-a", "catalog" ],\n\t\t\t[ "--json", "-h", "catalog" ],\n\t\t\t[ "--json", "-s", "catalog" ],\n\t\t} ) {''',
    '''\tpublic async Task JsonRequiresExplicitDirectoriesAndRejectsListingPresentationSwitches() {\n\t\tforeach ( string[] args in new[] {\n\t\t\tnew[] { "--json" },\n\t\t\t[ "--json", "-a", "catalog" ],\n\t\t\t[ "--json", "-h", "catalog" ],\n\t\t\t[ "--json", "-s", "catalog" ],\n\t\t} ) {''',
)
