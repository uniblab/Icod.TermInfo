from pathlib import Path

# Temporary branch-scoped driver; removed after the convention gate is green.
# Current checkpoint normalizes the reduced residual ternary inventory.


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


# Inspection production.
replace_exact(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseInspector.Catalog.cs",
    """\t\tstring message =\n\t\t\tkind == TermInfoDatabaseCatalogIssueKind.PermissionFailure\n\t\t\t\t? $\"Access to the {subject} was denied.\"\n\t\t\t\t: $\"The {subject} could not be inspected because of an I/O failure.\"\n\t\t;\n""",
    """\t\tstring message =\n\t\t\t( kind == TermInfoDatabaseCatalogIssueKind.PermissionFailure )\n\t\t\t\t? $\"Access to the {subject} was denied.\"\n\t\t\t\t: $\"The {subject} could not be inspected because of an I/O failure.\"\n\t\t;\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TerminalDescriptionComparer.cs",
    """\t\t\t\tleftPresent\n\t\t\t\t\t? leftValue\n\t\t\t\t\t: null,\n\t\t\t\trightPresent\n\t\t\t\t\t? rightValue\n\t\t\t\t\t: null\n""",
    """\t\t\t\t( leftPresent )\n\t\t\t\t\t? leftValue\n\t\t\t\t\t: null,\n\t\t\t\t( rightPresent )\n\t\t\t\t\t? rightValue\n\t\t\t\t\t: null\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TerminalDescriptionSourceRenderer.Relative.cs",
    """\t\t\tTermInfoCapabilityValue orderingValue =\n\t\t\t\ttargetPresent\n\t\t\t\t\t? targetValue\n\t\t\t\t\t: inheritedValue;\n\t\t\tdirectives.Add(\n\t\t\t\tnew ExtendedRelativeDirective(\n\t\t\t\t\tname,\n\t\t\t\t\ttargetPresent\n\t\t\t\t\t\t? targetValue\n\t\t\t\t\t\t: null,\n""",
    """\t\t\tTermInfoCapabilityValue orderingValue =\n\t\t\t\t( targetPresent )\n\t\t\t\t\t? targetValue\n\t\t\t\t\t: inheritedValue\n\t\t\t;\n\t\t\tdirectives.Add(\n\t\t\t\tnew ExtendedRelativeDirective(\n\t\t\t\t\tname,\n\t\t\t\t\t( targetPresent )\n\t\t\t\t\t\t? targetValue\n\t\t\t\t\t\t: null,\n""",
)

# Termcap production.
replace_exact(
    "Icod.TermInfo.Termcap/src/TermcapSourceParser.cs",
    """\t\t\t\tint controlValue =\n\t\t\t\t\ttarget == '?'\n\t\t\t\t\t\t? 0x7f\n\t\t\t\t\t\t: target & 0x1f;\n""",
    """\t\t\t\tint controlValue =\n\t\t\t\t\t( target == '?' )\n\t\t\t\t\t\t? 0x7f\n\t\t\t\t\t\t: target & 0x1f\n\t\t\t\t;\n""",
)

# infocmp.
replace_exact(
    "infocmp/src/InfoCmpComparison.cs",
    """\t\treturn otherValue.HasValue\n\t\t\t&& otherValue.Value.Kind == TermInfoCapabilityValueKind.Boolean\n\t\t\t\t? \"F\"\n\t\t\t\t: \"NULL\";\n""",
    """\t\treturn (\n\t\t\totherValue.HasValue\n\t\t\t&& otherValue.Value.Kind == TermInfoCapabilityValueKind.Boolean\n\t\t)\n\t\t\t? \"F\"\n\t\t\t: \"NULL\"\n\t\t;\n""",
)
replace_exact(
    "infocmp/src/InfoCmpComparison.cs",
    """\t\t\tTermInfoCapabilityValueKind.Boolean =>\n\t\t\t\tvalue.BooleanValue\n\t\t\t\t\t? \"T\"\n\t\t\t\t\t: \"F\",\n""",
    """\t\t\tTermInfoCapabilityValueKind.Boolean =>\n\t\t\t\t( value.BooleanValue )\n\t\t\t\t\t? \"T\"\n\t\t\t\t\t: \"F\",\n""",
)
replace_exact(
    "infocmp/src/InfoCmpInspector.cs",
    """\t\t\tstring rendered = ( options.Json )\n\t\t\t\t? ( ( databaseSetPlan is null )\n\t\t\t\t\t? TermInfoJsonRenderer.Render(\n\t\t\t\t\t\tplan,\n\t\t\t\t\t\tnew TermInfoJsonRendererOptions(),\n\t\t\t\t\t\tcancellationToken\n\t\t\t\t\t)\n\t\t\t\t\t: TermInfoJsonRenderer.Render(\n\t\t\t\t\t\tdatabaseSetPlan,\n\t\t\t\t\t\tplanningOptions,\n\t\t\t\t\t\tnew TermInfoJsonRendererOptions(),\n\t\t\t\t\t\tcancellationToken\n\t\t\t\t\t) ) + \"\\n\"\n\t\t\t\t: plan.Source\n\t\t\t;\n""",
    """\t\t\tstring rendered;\n\t\t\tif ( options.Json ) {\n\t\t\t\tstring json =\n\t\t\t\t\t( databaseSetPlan is null )\n\t\t\t\t\t\t? TermInfoJsonRenderer.Render(\n\t\t\t\t\t\t\tplan,\n\t\t\t\t\t\t\tnew TermInfoJsonRendererOptions(),\n\t\t\t\t\t\t\tcancellationToken\n\t\t\t\t\t\t)\n\t\t\t\t\t\t: TermInfoJsonRenderer.Render(\n\t\t\t\t\t\t\tdatabaseSetPlan,\n\t\t\t\t\t\t\tplanningOptions,\n\t\t\t\t\t\t\tnew TermInfoJsonRendererOptions(),\n\t\t\t\t\t\t\tcancellationToken\n\t\t\t\t\t\t)\n\t\t\t\t;\n\t\t\t\trendered = json + \"\\n\";\n\t\t\t} else {\n\t\t\t\trendered = plan.Source;\n\t\t\t}\n""",
)
replace_exact(
    "infocmp/src/InfoCmpOptions.cs",
    """\tinternal string? TerminalName =>\n\t\t_terminalNames.Count == 1\n\t\t\t? _terminalNames[ 0 ]\n\t\t\t: null;\n""",
    """\tinternal string? TerminalName =>\n\t\t( _terminalNames.Count == 1 )\n\t\t\t? _terminalNames[ 0 ]\n\t\t\t: null\n\t;\n""",
)

# toe.
replace_exact(
    "toe/src/Command.cs",
    """\t\t\tif ( options.Json ) {\n\t\t\t\treturn options.Directories.Count == 1\n\t\t\t\t\t? await RenderCatalogAsync(\n\t\t\t\t\t\toptions.Directories[ 0 ],\n\t\t\t\t\t\tstdout,\n\t\t\t\t\t\tstderr,\n\t\t\t\t\t\tcancellationToken\n\t\t\t\t\t).ConfigureAwait( false )\n\t\t\t\t\t: await RenderDatabaseSetAsync(\n\t\t\t\t\t\toptions.Directories,\n\t\t\t\t\t\tstdout,\n\t\t\t\t\t\tstderr,\n\t\t\t\t\t\tcancellationToken\n\t\t\t\t\t).ConfigureAwait( false );\n\t\t\t}\n""",
    """\t\t\tif ( options.Json ) {\n\t\t\t\treturn ( options.Directories.Count == 1 )\n\t\t\t\t\t? await RenderCatalogAsync(\n\t\t\t\t\t\toptions.Directories[ 0 ],\n\t\t\t\t\t\tstdout,\n\t\t\t\t\t\tstderr,\n\t\t\t\t\t\tcancellationToken\n\t\t\t\t\t).ConfigureAwait( false )\n\t\t\t\t\t: await RenderDatabaseSetAsync(\n\t\t\t\t\t\toptions.Directories,\n\t\t\t\t\t\tstdout,\n\t\t\t\t\t\tstderr,\n\t\t\t\t\t\tcancellationToken\n\t\t\t\t\t).ConfigureAwait( false )\n\t\t\t\t;\n\t\t\t}\n""",
)
replace_exact(
    "toe/src/ToeSourceDependencyAnalyzer.cs",
    """\t\t\tstring location = span is null\n\t\t\t\t? \"source\"\n\t\t\t\t: string.Concat(\n\t\t\t\t\tspan.SourceName ?? \"source\",\n\t\t\t\t\t\":\",\n\t\t\t\t\tspan.Line.ToString( CultureInfo.InvariantCulture ),\n\t\t\t\t\t\":\",\n\t\t\t\t\tspan.Column.ToString( CultureInfo.InvariantCulture )\n\t\t\t\t)\n\t\t\t;\n""",
    """\t\t\tstring location = ( span is null )\n\t\t\t\t? \"source\"\n\t\t\t\t: string.Concat(\n\t\t\t\t\tspan.SourceName ?? \"source\",\n\t\t\t\t\t\":\",\n\t\t\t\t\tspan.Line.ToString( CultureInfo.InvariantCulture ),\n\t\t\t\t\t\":\",\n\t\t\t\t\tspan.Column.ToString( CultureInfo.InvariantCulture )\n\t\t\t\t)\n\t\t\t;\n""",
)
replace_exact(
    "toe/src/ToeSourceDependencyAnalyzer.cs",
    """\t\t\t\t.Append(\n\t\t\t\t\tdiagnostic.Severity == TermInfoSourceDiagnosticSeverity.Error\n\t\t\t\t\t\t? \"error\"\n\t\t\t\t\t\t: \"warning\"\n\t\t\t\t)\n""",
    """\t\t\t\t.Append(\n\t\t\t\t\t( diagnostic.Severity == TermInfoSourceDiagnosticSeverity.Error )\n\t\t\t\t\t\t? \"error\"\n\t\t\t\t\t\t: \"warning\"\n\t\t\t\t)\n""",
)

# Public API tool.
replace_exact(
    "tools/public-api-snapshot/Program.cs",
    """\t\t\t\tmethod.IsGenericMethodDefinition\n\t\t\t\t\t? method.GetGenericArguments()\n\t\t\t\t\t: Array.Empty<Type>(),\n""",
    """\t\t\t\t( method.IsGenericMethodDefinition )\n\t\t\t\t\t? method.GetGenericArguments()\n\t\t\t\t\t: Array.Empty<Type>(),\n""",
)

# Inspection tests.
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/I07ValidationTests.cs",
    """\t\tTerminalDescriptionBuilder builder =\n\t\t\tnew TerminalDescriptionBuilder(\n\t\t\t\tleft\n\t\t\t\t\t? \"i07-left\"\n\t\t\t\t\t: \"i07-right\"\n\t\t\t)\n\t\t\t\t.SetDescription(\n\t\t\t\t\tleft\n\t\t\t\t\t\t? \"I07 left comparison\"\n\t\t\t\t\t\t: \"I07 right comparison\"\n\t\t\t\t)\n\t\t\t\t.AddAlias(\n\t\t\t\t\tleft\n\t\t\t\t\t\t? \"i07-left-alias\"\n\t\t\t\t\t\t: \"i07-right-alias\"\n\t\t\t\t)\n""",
    """\t\tTerminalDescriptionBuilder builder =\n\t\t\tnew TerminalDescriptionBuilder(\n\t\t\t\t( left )\n\t\t\t\t\t? \"i07-left\"\n\t\t\t\t\t: \"i07-right\"\n\t\t\t)\n\t\t\t\t.SetDescription(\n\t\t\t\t\t( left )\n\t\t\t\t\t\t? \"I07 left comparison\"\n\t\t\t\t\t\t: \"I07 right comparison\"\n\t\t\t\t)\n\t\t\t\t.AddAlias(\n\t\t\t\t\t( left )\n\t\t\t\t\t\t? \"i07-left-alias\"\n\t\t\t\t\t\t: \"i07-right-alias\"\n\t\t\t\t)\n""",
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/JsonSchemaFixtureValidator.cs",
    """\t\tJsonObject? properties =\n\t\t\tschema.TryGetPropertyValue(\n\t\t\t\t\"properties\",\n\t\t\t\tout JsonNode? propertiesNode\n\t\t\t)\n\t\t\t\t? propertiesNode!.AsObject()\n\t\t\t\t: null;\n""",
    """\t\tJsonObject? properties =\n\t\t\t( schema.TryGetPropertyValue(\n\t\t\t\t\"properties\",\n\t\t\t\tout JsonNode? propertiesNode\n\t\t\t) )\n\t\t\t\t? propertiesNode!.AsObject()\n\t\t\t\t: null\n\t\t;\n""",
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/MI03ComparisonAndPlanJsonTests.cs",
    """\t\tstring expected =\n\t\t\trenderComparison\n\t\t\t\t? TermInfoJsonRenderer.Render( CreateFixtureComparison() )\n\t\t\t\t: TermInfoJsonRenderer.Render( CreateFixturePlan() );\n""",
    """\t\tstring expected =\n\t\t\t( renderComparison )\n\t\t\t\t? TermInfoJsonRenderer.Render( CreateFixtureComparison() )\n\t\t\t\t: TermInfoJsonRenderer.Render( CreateFixturePlan() )\n\t\t;\n""",
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/MI03ComparisonAndPlanJsonTests.cs",
    """\t\tstring exact =\n\t\t\trenderComparison\n\t\t\t\t? TermInfoJsonRenderer.Render(\n\t\t\t\t\tCreateFixtureComparison(),\n\t\t\t\t\tnew TermInfoJsonRendererOptions( byteCount )\n\t\t\t\t)\n\t\t\t\t: TermInfoJsonRenderer.Render(\n\t\t\t\t\tCreateFixturePlan(),\n\t\t\t\t\tnew TermInfoJsonRendererOptions( byteCount )\n\t\t\t\t);\n""",
    """\t\tstring exact =\n\t\t\t( renderComparison )\n\t\t\t\t? TermInfoJsonRenderer.Render(\n\t\t\t\t\tCreateFixtureComparison(),\n\t\t\t\t\tnew TermInfoJsonRendererOptions( byteCount )\n\t\t\t\t)\n\t\t\t\t: TermInfoJsonRenderer.Render(\n\t\t\t\t\tCreateFixturePlan(),\n\t\t\t\t\tnew TermInfoJsonRendererOptions( byteCount )\n\t\t\t\t)\n\t\t;\n""",
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/MI04DatabaseCatalogJsonAndSchemaTests.cs",
    """\t\tTermInfoDatabaseCatalog catalog =\n\t\t\tcomplete\n\t\t\t\t? CreateCompleteCatalog()\n\t\t\t\t: CreateIncompleteCatalog();\n""",
    """\t\tTermInfoDatabaseCatalog catalog =\n\t\t\t( complete )\n\t\t\t\t? CreateCompleteCatalog()\n\t\t\t\t: CreateIncompleteCatalog()\n\t\t;\n""",
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/RP02PlanningTests.cs",
    """\t\tTerminalDescriptionSourceSynthesisParent[] parents =\n\t\t\tparent is null\n\t\t\t\t? []\n\t\t\t\t: [ parent ];\n""",
    """\t\tTerminalDescriptionSourceSynthesisParent[] parents =\n\t\t\t( parent is null )\n\t\t\t\t? []\n\t\t\t\t: [ parent ]\n\t\t;\n""",
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/RP05CatalogPlanningTests.cs",
    """\t\tTerminalDescription[] entries =\n\t\t\treverseOrder\n\t\t\t\t? [\n""",
    """\t\tTerminalDescription[] entries =\n\t\t\t( reverseOrder )\n\t\t\t\t? [\n""",
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/RP07GeneratedPlanningTests.cs",
    """\t\t\t_state = seed == 0\n\t\t\t\t? 0xA341316Cu\n\t\t\t\t: seed\n\t\t\t;\n""",
    """\t\t\t_state = ( seed == 0 )\n\t\t\t\t? 0xA341316Cu\n\t\t\t\t: seed\n\t\t\t;\n""",
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/RS07ContractTests.cs",
    """\t\t\t_state = seed == 0\n\t\t\t\t? 0xA341316Cu\n\t\t\t\t: seed\n\t\t\t;\n""",
    """\t\t\t_state = ( seed == 0 )\n\t\t\t\t? 0xA341316Cu\n\t\t\t\t: seed\n\t\t\t;\n""",
)

# Termcap tests.
replace_exact(
    "tests/Icod.TermInfo.Termcap.Tests/src/TC06AcquisitionTests.cs",
    """\t\t\treturn _values.TryGetValue(\n\t\t\t\tname,\n\t\t\t\tout string? value\n\t\t\t)\n\t\t\t\t? value\n\t\t\t\t: null\n\t\t\t;\n""",
    """\t\t\treturn ( _values.TryGetValue(\n\t\t\t\tname,\n\t\t\t\tout string? value\n\t\t\t) )\n\t\t\t\t? value\n\t\t\t\t: null\n\t\t\t;\n""",
)
replace_exact(
    "tests/Icod.TermInfo.Termcap.Tests/src/TC08DifferentialValidationTests.cs",
    """\t\tstring fields =\n\t\t\tresult.Entry is null\n\t\t\t\t? string.Empty\n\t\t\t\t: string.Join(\n""",
    """\t\tstring fields =\n\t\t\t( result.Entry is null )\n\t\t\t\t? string.Empty\n\t\t\t\t: string.Join(\n""",
)
