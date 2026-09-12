from pathlib import Path

# Temporary branch-scoped driver; removed after the convention gate is green.
# Current checkpoint verifies forward ternary-expression terminators.


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


convention_test = "tests/Icod.TermInfo.Tests/src/CodingConventionTests.cs"

replace_exact(
    convention_test,
    """\t\t\tif ( IsNestedInParenthesesOrBrackets( maskedSource, questionIndex ) ) {\n\t\t\t\tcontinue;\n\t\t\t}\n\n\t\t\tint semicolonIndex = maskedSource.IndexOf( ';', colonIndex + 1 );\n\t\t\tif ( semicolonIndex < 0 ) {\n\t\t\t\treturn colonIndex;\n\t\t\t}\n\n\t\t\tint semicolonLineStart =\n\t\t\t\tmaskedSource.LastIndexOf( '\\n', semicolonIndex - 1 ) + 1;\n""",
    """\t\t\tint terminatorIndex =\n\t\t\t\tFindTernaryExpressionTerminator(\n\t\t\t\t\tmaskedSource,\n\t\t\t\t\tcolonIndex + 1\n\t\t\t\t);\n\t\t\tif ( terminatorIndex < 0 ) {\n\t\t\t\treturn colonIndex;\n\t\t\t}\n\t\t\tif ( maskedSource[ terminatorIndex ] != ';' ) {\n\t\t\t\tcontinue;\n\t\t\t}\n\n\t\t\tint semicolonIndex = terminatorIndex;\n\t\t\tint semicolonLineStart =\n\t\t\t\tmaskedSource.LastIndexOf( '\\n', semicolonIndex - 1 ) + 1;\n""",
)

replace_exact(
    convention_test,
    """\tprivate static bool IsNestedInParenthesesOrBrackets(\n\t\tstring source,\n\t\tint index\n\t) {\n\t\tArgumentNullException.ThrowIfNull( source );\n\t\tArgumentOutOfRangeException.ThrowIfNegative( index );\n\n\t\tint parenthesisDepth = 0;\n\t\tint bracketDepth = 0;\n\t\tfor ( int i = 0; i < index; i++ ) {\n\t\t\tif ( source[i] == '(' ) {\n\t\t\t\tparenthesisDepth++;\n\t\t\t} else if ( source[i] == ')' ) {\n\t\t\t\tparenthesisDepth--;\n\t\t\t} else if ( source[i] == '[' ) {\n\t\t\t\tbracketDepth++;\n\t\t\t} else if ( source[i] == ']' ) {\n\t\t\t\tbracketDepth--;\n\t\t\t}\n\t\t}\n\n\t\treturn parenthesisDepth > 0 || bracketDepth > 0;\n\t}\n""",
    """\tprivate static int FindTernaryExpressionTerminator(\n\t\tstring source,\n\t\tint startIndex\n\t) {\n\t\tArgumentNullException.ThrowIfNull( source );\n\t\tArgumentOutOfRangeException.ThrowIfNegative( startIndex );\n\n\t\tint parenthesisDepth = 0;\n\t\tint bracketDepth = 0;\n\t\tint braceDepth = 0;\n\t\tfor ( int i = startIndex; i < source.Length; i++ ) {\n\t\t\tif ( source[i] == '(' ) {\n\t\t\t\tparenthesisDepth++;\n\t\t\t\tcontinue;\n\t\t\t}\n\t\t\tif ( source[i] == '[' ) {\n\t\t\t\tbracketDepth++;\n\t\t\t\tcontinue;\n\t\t\t}\n\t\t\tif ( source[i] == '{' ) {\n\t\t\t\tbraceDepth++;\n\t\t\t\tcontinue;\n\t\t\t}\n\t\t\tif ( source[i] == ')' ) {\n\t\t\t\tif ( parenthesisDepth == 0 ) {\n\t\t\t\t\treturn i;\n\t\t\t\t}\n\t\t\t\tparenthesisDepth--;\n\t\t\t\tcontinue;\n\t\t\t}\n\t\t\tif ( source[i] == ']' ) {\n\t\t\t\tif ( bracketDepth == 0 ) {\n\t\t\t\t\treturn i;\n\t\t\t\t}\n\t\t\t\tbracketDepth--;\n\t\t\t\tcontinue;\n\t\t\t}\n\t\t\tif ( source[i] == '}' ) {\n\t\t\t\tif ( braceDepth == 0 ) {\n\t\t\t\t\treturn i;\n\t\t\t\t}\n\t\t\t\tbraceDepth--;\n\t\t\t\tcontinue;\n\t\t\t}\n\n\t\t\tif (\n\t\t\t\tparenthesisDepth == 0\n\t\t\t\t&& bracketDepth == 0\n\t\t\t\t&& braceDepth == 0\n\t\t\t\t&& ( source[i] == ',' || source[i] == ';' )\n\t\t\t) {\n\t\t\t\treturn i;\n\t\t\t}\n\t\t}\n\n\t\treturn -1;\n\t}\n""",
)

# Compiler production.
replace_exact(
    "Icod.TermInfo.Compiler/src/CompiledTermInfoWriter.cs",
    """\t\t\treturn requiresWide\n\t\t\t\t? CompiledTermInfoFormat.Wide\n\t\t\t\t: CompiledTermInfoFormat.Legacy\n\t\t\t;\n""",
    """\t\t\treturn ( requiresWide )\n\t\t\t\t? CompiledTermInfoFormat.Wide\n\t\t\t\t: CompiledTermInfoFormat.Legacy\n\t\t\t;\n""",
)

# Inspection production.
replace_exact(
    "Icod.TermInfo.Inspection/src/PersistentRasterLifecyclePlanner.cs",
    """\t\tPersistentRasterLifecyclePlanStatus planStatus =\n\t\t\thasUnsupported\n\t\t\t\t? PersistentRasterLifecyclePlanStatus.Impossible\n\t\t\t\t: hasUncertain\n\t\t\t\t\t? PersistentRasterLifecyclePlanStatus.Indeterminate\n\t\t\t\t\t: PersistentRasterLifecyclePlanStatus.Success;\n""",
    """\t\tPersistentRasterLifecyclePlanStatus planStatus =\n\t\t\t( hasUnsupported )\n\t\t\t\t? PersistentRasterLifecyclePlanStatus.Impossible\n\t\t\t\t: ( hasUncertain )\n\t\t\t\t\t? PersistentRasterLifecyclePlanStatus.Indeterminate\n\t\t\t\t\t: PersistentRasterLifecyclePlanStatus.Success\n\t\t;\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseInspector.Catalog.cs",
    """\t\tStringComparison pathComparison =\n\t\t\tOperatingSystem.IsWindows()\n\t\t\t\t? StringComparison.OrdinalIgnoreCase\n\t\t\t\t: StringComparison.Ordinal\n\t\t;\n""",
    """\t\tStringComparison pathComparison =\n\t\t\t( OperatingSystem.IsWindows() )\n\t\t\t\t? StringComparison.OrdinalIgnoreCase\n\t\t\t\t: StringComparison.Ordinal\n\t\t;\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSet.SemanticAnalysis.cs",
    """\t\t\t\t\tTermInfoDatabaseSetSemanticRelationship shadowRelationship =\n\t\t\t\t\t\tcomparison.AreEqual\n\t\t\t\t\t\t\t? TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual\n\t\t\t\t\t\t\t: TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent;\n""",
    """\t\t\t\t\tTermInfoDatabaseSetSemanticRelationship shadowRelationship =\n\t\t\t\t\t\t( comparison.AreEqual )\n\t\t\t\t\t\t\t? TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual\n\t\t\t\t\t\t\t: TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent\n\t\t\t\t\t;\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSetComparer.cs",
    """\t\t\tTermInfoDatabaseSetOccurrence? leftShadow =\n\t\t\t\tindex < leftLookup.ShadowedOccurrences.Count\n\t\t\t\t\t? leftLookup.ShadowedOccurrences[ index ]\n\t\t\t\t\t: null;\n\t\t\tTermInfoDatabaseSetOccurrence? rightShadow =\n\t\t\t\tindex < rightLookup.ShadowedOccurrences.Count\n\t\t\t\t\t? rightLookup.ShadowedOccurrences[ index ]\n\t\t\t\t\t: null;\n""",
    """\t\t\tTermInfoDatabaseSetOccurrence? leftShadow =\n\t\t\t\t( index < leftLookup.ShadowedOccurrences.Count )\n\t\t\t\t\t? leftLookup.ShadowedOccurrences[ index ]\n\t\t\t\t\t: null\n\t\t\t;\n\t\t\tTermInfoDatabaseSetOccurrence? rightShadow =\n\t\t\t\t( index < rightLookup.ShadowedOccurrences.Count )\n\t\t\t\t\t? rightLookup.ShadowedOccurrences[ index ]\n\t\t\t\t\t: null\n\t\t\t;\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TermInfoDifference.cs",
    """\t\t_leftAliases =\n\t\t\tleftAliases is null\n\t\t\t\t? null\n\t\t\t\t: Array.AsReadOnly(\n\t\t\t\t\tleftAliases.ToArray()\n\t\t\t\t);\n\t\t_rightAliases =\n\t\t\trightAliases is null\n\t\t\t\t? null\n\t\t\t\t: Array.AsReadOnly(\n\t\t\t\t\trightAliases.ToArray()\n\t\t\t\t);\n""",
    """\t\t_leftAliases =\n\t\t\t( leftAliases is null )\n\t\t\t\t? null\n\t\t\t\t: Array.AsReadOnly(\n\t\t\t\t\tleftAliases.ToArray()\n\t\t\t\t)\n\t\t;\n\t\t_rightAliases =\n\t\t\t( rightAliases is null )\n\t\t\t\t? null\n\t\t\t\t: Array.AsReadOnly(\n\t\t\t\t\trightAliases.ToArray()\n\t\t\t\t)\n\t\t;\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TermInfoJsonRenderer.ComparisonAndPlan.cs",
    """\t\t\tfield.CapabilityClassification.HasValue\n\t\t\t\t? GetSourceCapabilityClassificationName(\n""",
    """\t\t\t( field.CapabilityClassification.HasValue )\n\t\t\t\t? GetSourceCapabilityClassificationName(\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TermInfoJsonRenderer.ComparisonAndPlan.cs",
    """\t\t\tfield.StandardValueKind.HasValue\n\t\t\t\t? GetCapabilityValueKindName(\n""",
    """\t\t\t( field.StandardValueKind.HasValue )\n\t\t\t\t? GetCapabilityValueKindName(\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TermInfoJsonRenderer.TerminalDescription.cs",
    """\t\t\t\tvalue\n\t\t\t\t\t? \"true\"\n""",
    """\t\t\t\t( value )\n\t\t\t\t\t? \"true\"\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TermInfoSourceComparer.cs",
    """\t\t\treturn string.Equals(\n\t\t\t\tleft.ReferenceName,\n\t\t\t\tright.ReferenceName,\n\t\t\t\tStringComparison.Ordinal\n\t\t\t)\n\t\t\t\t? null\n\t\t\t\t: TermInfoDifferenceKind.SourceUseReference;\n""",
    """\t\t\treturn ( string.Equals(\n\t\t\t\tleft.ReferenceName,\n\t\t\t\tright.ReferenceName,\n\t\t\t\tStringComparison.Ordinal\n\t\t\t) )\n\t\t\t\t? null\n\t\t\t\t: TermInfoDifferenceKind.SourceUseReference\n\t\t\t;\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TermInfoSourceComparer.cs",
    """\t\t\t\tNumericValuesEqual( left, right )\n\t\t\t\t\t? null\n""",
    """\t\t\t\t( NumericValuesEqual( left, right ) )\n\t\t\t\t\t? null\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TermInfoSourceComparer.cs",
    """\t\t\t\tStringValuesEqual( left, right )\n\t\t\t\t\t? null\n""",
    """\t\t\t\t( StringValuesEqual( left, right ) )\n\t\t\t\t\t? null\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TerminalDescriptionComparer.cs",
    """\t\t\t\tleftValue.HasValue\n\t\t\t\t\t? new TermInfoCapabilityValue( leftValue.Value )\n""",
    """\t\t\t\t( leftValue.HasValue )\n\t\t\t\t\t? new TermInfoCapabilityValue( leftValue.Value )\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TerminalDescriptionComparer.cs",
    """\t\t\t\trightValue.HasValue\n\t\t\t\t\t? new TermInfoCapabilityValue( rightValue.Value )\n""",
    """\t\t\t\t( rightValue.HasValue )\n\t\t\t\t\t? new TermInfoCapabilityValue( rightValue.Value )\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TerminalDescriptionSourcePlanner.cs",
    """\t\tint evaluationLimit =\n\t\t\tcompletePlanCountKnown\n\t\t\t\t? requiredPlanCount\n\t\t\t\t: request.Options.MaximumEvaluatedPlanCount;\n""",
    """\t\tint evaluationLimit =\n\t\t\t( completePlanCountKnown )\n\t\t\t\t? requiredPlanCount\n\t\t\t\t: request.Options.MaximumEvaluatedPlanCount\n\t\t;\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TerminalDescriptionSourceRenderer.Relative.cs",
    """\t\t\tint? inheritedValue =\n\t\t\t\tinherited.NumericCapabilities.TryGetValue(\n\t\t\t\t\tmetadata.Capability,\n\t\t\t\t\tout int inheritedNumber\n\t\t\t\t)\n\t\t\t\t\t? inheritedNumber\n\t\t\t\t\t: null;\n""",
    """\t\t\tint? inheritedValue =\n\t\t\t\t( inherited.NumericCapabilities.TryGetValue(\n\t\t\t\t\tmetadata.Capability,\n\t\t\t\t\tout int inheritedNumber\n\t\t\t\t) )\n\t\t\t\t\t? inheritedNumber\n\t\t\t\t\t: null\n\t\t\t;\n""",
)

# Termcap production.
replace_exact(
    "Icod.TermInfo.Termcap/src/TermcapAcquirer.cs",
    """\t\t\treturn _sourceByName.TryGetValue(\n\t\t\t\tname,\n\t\t\t\tout TermcapAcquisitionSource? source\n\t\t\t)\n\t\t\t\t? source\n\t\t\t\t: null\n\t\t\t;\n""",
    """\t\t\treturn ( _sourceByName.TryGetValue(\n\t\t\t\tname,\n\t\t\t\tout TermcapAcquisitionSource? source\n\t\t\t) )\n\t\t\t\t? source\n\t\t\t\t: null\n\t\t\t;\n""",
)
replace_exact(
    "Icod.TermInfo.Termcap/src/TermcapAcquisitionOptions.cs",
    """\t\treturn string.IsNullOrWhiteSpace( value )\n\t\t\t? null\n\t\t\t: value\n\t\t;\n""",
    """\t\treturn ( string.IsNullOrWhiteSpace( value ) )\n\t\t\t? null\n\t\t\t: value\n\t\t;\n""",
)
replace_exact(
    "Icod.TermInfo.Termcap/src/TermcapConverter.cs",
    """\t\tTerminalDescription? description =\n\t\t\thasErrors\n\t\t\t\t? null\n\t\t\t\t: builder.Build()\n\t\t;\n""",
    """\t\tTerminalDescription? description =\n\t\t\t( hasErrors )\n\t\t\t\t? null\n\t\t\t\t: builder.Build()\n\t\t;\n""",
)
replace_exact(
    "Icod.TermInfo.Termcap/src/TermcapRenderer.cs",
    """\t\t\t\t\tparameterized\n\t\t\t\t\t\t? TermcapRenderDiagnosticCodes.ParameterProgramNotRepresentable\n""",
    """\t\t\t\t\t( parameterized )\n\t\t\t\t\t\t? TermcapRenderDiagnosticCodes.ParameterProgramNotRepresentable\n""",
)
replace_exact(
    "Icod.TermInfo.Termcap/src/TermcapReverseStringConverter.cs",
    """\t\tinternal string CurrentExpression =>\n\t\t\tCurrent == 0\n\t\t\t\t? First\n\t\t\t\t: Second\n\t\t;\n""",
    """\t\tinternal string CurrentExpression =>\n\t\t\t( Current == 0 )\n\t\t\t\t? First\n\t\t\t\t: Second\n\t\t;\n""",
)
replace_exact(
    "Icod.TermInfo.Termcap/src/TermcapSourceParser.cs",
    """\t\tint capabilityOffset =\n\t\t\tdisabled\n\t\t\t\t? 1\n\t\t\t\t: 0\n\t\t;\n""",
    """\t\tint capabilityOffset =\n\t\t\t( disabled )\n\t\t\t\t? 1\n\t\t\t\t: 0\n\t\t;\n""",
)
replace_exact(
    "Icod.TermInfo.Termcap/src/TermcapStringConverter.cs",
    """\t\tint paddingEnd =\n\t\t\tproportional\n\t\t\t\t? checked( position + 1 )\n\t\t\t\t: position\n\t\t;\n""",
    """\t\tint paddingEnd =\n\t\t\t( proportional )\n\t\t\t\t? checked( position + 1 )\n\t\t\t\t: position\n\t\t;\n""",
)
