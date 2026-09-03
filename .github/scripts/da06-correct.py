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


# Test projects import Icod.Path; qualify filesystem Path explicitly.
for path_name in [
    "tests/Icod.TermInfo.Toe.Tests/src/DA06DatabaseAutomationCommandTests.cs",
    "tests/Icod.TermInfo.InfoCmp.Tests/src/DA06MultiDatabasePlanningAutomationTests.cs",
]:
    path = Path(path_name)
    text = path.read_text(encoding="utf-8")
    text = text.replace("Path.GetFullPath(", "System.IO.Path.GetFullPath(")
    text = text.replace("Path.Combine(", "System.IO.Path.Combine(")
    text = text.replace("Path.GetTempPath()", "System.IO.Path.GetTempPath()")
    path.write_text(text, encoding="utf-8", newline="\n")

replace_exact(
    "tests/Icod.TermInfo.Toe.Tests/src/DA06DatabaseAutomationCommandTests.cs",
    '''\t\t\tAssert.DoesNotEndWith( "\\n\\n", single.Stdout, StringComparison.Ordinal );''',
    '''\t\t\tAssert.False( single.Stdout.EndsWith( "\\n\\n", StringComparison.Ordinal ) );''',
)
replace_exact(
    "tests/Icod.TermInfo.Toe.Tests/src/DA06DatabaseAutomationCommandTests.cs",
    '''\t\t\tAssert.DoesNotEndWith( "\\n\\n", multiple.Stdout, StringComparison.Ordinal );''',
    '''\t\t\tAssert.False( multiple.Stdout.EndsWith( "\\n\\n", StringComparison.Ordinal ) );''',
)

# Candidate roots must fail as a parser usage error even when --plan-use is absent.
replace_exact(
    "infocmp/src/InfoCmpOptions.cs",
    '''\t\tif ( allCandidates && !planning ) {\n\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t"option '--all-candidates' requires '--plan-use'"\n\t\t\t);\n\t\t}\n\t\tif ( !planning''',
    '''\t\tif ( allCandidates && !planning ) {\n\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t"option '--all-candidates' requires '--plan-use'"\n\t\t\t);\n\t\t}\n\t\tif ( candidateRoots.Count != 0 && ( !planning || !allCandidates ) ) {\n\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t"option '--candidate-root' requires '--plan-use --all-candidates'"\n\t\t\t);\n\t\t}\n\t\tif ( !planning''',
)

# databaseSetPlan must retain the exact planner bounds that affected selection.
renderer = "Icod.TermInfo.Inspection/src/TermInfoJsonRenderer.DatabaseAutomationV2.cs"
replace_exact(
    renderer,
    '''\tpublic static string Render(\n\t\tTermInfoDatabaseSetSourcePlanningResult planningResult,\n\t\tTermInfoJsonRendererOptions options,\n\t\tCancellationToken cancellationToken = default\n\t) {\n\t\tArgumentNullException.ThrowIfNull( planningResult );\n\t\tArgumentNullException.ThrowIfNull( options );\n\t\tcancellationToken.ThrowIfCancellationRequested();\n\n\t\treturn RenderDatabaseSetPlanV2(\n\t\t\tplanningResult,\n\t\t\toptions,\n\t\t\tcancellationToken\n\t\t);\n\t}''',
    '''\tpublic static string Render(\n\t\tTermInfoDatabaseSetSourcePlanningResult planningResult,\n\t\tTermInfoJsonRendererOptions options,\n\t\tCancellationToken cancellationToken = default\n\t) {\n\t\tArgumentNullException.ThrowIfNull( planningResult );\n\t\tArgumentNullException.ThrowIfNull( options );\n\t\tcancellationToken.ThrowIfCancellationRequested();\n\n\t\treturn RenderDatabaseSetPlanV2(\n\t\t\tplanningResult,\n\t\t\tnew TerminalDescriptionSourcePlanningOptions(),\n\t\t\toptions,\n\t\t\tcancellationToken\n\t\t);\n\t}\n\n\t/// <summary>\n\t/// Renders a database-set-backed source plan while retaining the exact frozen\n\t/// 1.8 planning bounds used to produce the result.\n\t/// </summary>\n\tpublic static string Render(\n\t\tTermInfoDatabaseSetSourcePlanningResult planningResult,\n\t\tTerminalDescriptionSourcePlanningOptions planningOptions,\n\t\tTermInfoJsonRendererOptions options,\n\t\tCancellationToken cancellationToken = default\n\t) {\n\t\tArgumentNullException.ThrowIfNull( planningResult );\n\t\tArgumentNullException.ThrowIfNull( planningOptions );\n\t\tArgumentNullException.ThrowIfNull( options );\n\t\tcancellationToken.ThrowIfCancellationRequested();\n\n\t\treturn RenderDatabaseSetPlanV2(\n\t\t\tplanningResult,\n\t\t\tplanningOptions,\n\t\t\toptions,\n\t\t\tcancellationToken\n\t\t);\n\t}''',
)
replace_exact(
    renderer,
    '''\tprivate static string RenderDatabaseSetPlanV2(\n\t\tTermInfoDatabaseSetSourcePlanningResult planningResult,\n\t\tTermInfoJsonRendererOptions options,''',
    '''\tprivate static string RenderDatabaseSetPlanV2(\n\t\tTermInfoDatabaseSetSourcePlanningResult planningResult,\n\t\tTerminalDescriptionSourcePlanningOptions planningOptions,\n\t\tTermInfoJsonRendererOptions options,''',
)
replace_exact(
    renderer,
    '''\t\t\twriter.WriteNumber( "candidateCount", planningResult.Candidates.Count );''',
    '''\t\t\twriter.WriteStartObject( "planningBounds" );\n\t\t\twriter.WriteNumber( "maximumCandidateCount", planningOptions.MaximumCandidateCount );\n\t\t\twriter.WriteNumber(\n\t\t\t\t"maximumSelectedParentCount",\n\t\t\t\tplanningOptions.MaximumSelectedParentCount\n\t\t\t);\n\t\t\twriter.WriteNumber(\n\t\t\t\t"maximumEvaluatedPlanCount",\n\t\t\t\tplanningOptions.MaximumEvaluatedPlanCount\n\t\t\t);\n\t\t\twriter.WriteNumber(\n\t\t\t\t"maximumGeneratedSourceLength",\n\t\t\t\tplanningOptions.MaximumGeneratedSourceLength\n\t\t\t);\n\t\t\twriter.WriteBoolean(\n\t\t\t\t"allowNonExhaustiveResult",\n\t\t\t\tplanningOptions.AllowNonExhaustiveResult\n\t\t\t);\n\t\t\twriter.WriteEndObject();\n\t\t\twriter.WriteNumber( "candidateCount", planningResult.Candidates.Count );''',
)

# The command knows the exact active planning options; use the evidence-preserving overload.
replace_exact(
    "infocmp/src/InfoCmpInspector.cs",
    '''\t\t\t\t\t: TermInfoJsonRenderer.Render(\n\t\t\t\t\t\tdatabaseSetPlan,\n\t\t\t\t\t\tnew TermInfoJsonRendererOptions(),\n\t\t\t\t\t\tcancellationToken\n\t\t\t\t\t) ) + "\\n"''',
    '''\t\t\t\t\t: TermInfoJsonRenderer.Render(\n\t\t\t\t\t\tdatabaseSetPlan,\n\t\t\t\t\t\tplanningOptions,\n\t\t\t\t\t\tnew TermInfoJsonRendererOptions(),\n\t\t\t\t\t\tcancellationToken\n\t\t\t\t\t) ) + "\\n"''',
)

# Schema and tests freeze the planning-bound object.
schema_path = Path("docs/Icod.TermInfo.Inspection.schema.v2.json")
schema_text = schema_path.read_text(encoding="utf-8")
schema_text = schema_text.replace(
    '"databases",\n        "candidateCount",',
    '"databases",\n        "planningBounds",\n        "candidateCount",',
    1,
)
schema_text = schema_text.replace(
    '"databases": {\n          "type": "array"\n        },\n        "candidateCount":',
    '''"databases": {\n          "type": "array"\n        },\n        "planningBounds": {\n          "type": "object",\n          "required": [\n            "maximumCandidateCount",\n            "maximumSelectedParentCount",\n            "maximumEvaluatedPlanCount",\n            "maximumGeneratedSourceLength",\n            "allowNonExhaustiveResult"\n          ],\n          "properties": {\n            "maximumCandidateCount": { "type": "integer", "minimum": 0 },\n            "maximumSelectedParentCount": { "type": "integer", "minimum": 0 },\n            "maximumEvaluatedPlanCount": { "type": "integer", "minimum": 1 },\n            "maximumGeneratedSourceLength": { "type": "integer", "minimum": 1 },\n            "allowNonExhaustiveResult": { "type": "boolean" }\n          },\n          "additionalProperties": false\n        },\n        "candidateCount":''',
    1,
)
schema_path.write_text(schema_text, encoding="utf-8", newline="\n")

replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/DA06DatabaseAutomationJsonTests.cs",
    '''\t\tAssert.Equal( "databaseSetPlan", document.RootElement.GetProperty( "documentKind" ).GetString() );''',
    '''\t\tAssert.Equal( "databaseSetPlan", document.RootElement.GetProperty( "documentKind" ).GetString() );\n\t\tJsonElement bounds = data.GetProperty( "planningBounds" );\n\t\tAssert.Equal(\n\t\t\tTerminalDescriptionSourcePlanningOptions.DefaultMaximumCandidateCount,\n\t\t\tbounds.GetProperty( "maximumCandidateCount" ).GetInt32()\n\t\t);\n\t\tAssert.Equal(\n\t\t\tTerminalDescriptionSourcePlanningOptions.DefaultMaximumEvaluatedPlanCount,\n\t\t\tbounds.GetProperty( "maximumEvaluatedPlanCount" ).GetInt32()\n\t\t);''',
)

replace_exact(
    "docs/1.10.0-DA06-COMMAND-AND-MACHINE-READABLE-AUTOMATION-COMPOSITION.md",
    '''`TermInfoJsonRenderer.Render(TermInfoDatabaseSetSourcePlanningResult)` emits the\nordered database roots, complete DA05 candidate provenance, exact frozen planner\nselected indices, selected candidate evidence, generated source, score, evaluated\nplan count, and exhaustive status. No planner decision is reconstructed in the\nrenderer.''',
    '''`TermInfoJsonRenderer.Render(TermInfoDatabaseSetSourcePlanningResult)` emits the\nordered database roots, complete DA05 candidate provenance, exact frozen planner\nselected indices, selected candidate evidence, generated source, score, evaluated\nplan count, and exhaustive status. An explicit overload also accepts the exact\n`TerminalDescriptionSourcePlanningOptions` used to produce the result and emits\n`planningBounds`; `infocmp --candidate-root` always uses that evidence-preserving\noverload. No planner decision is reconstructed in the renderer.''',
)
