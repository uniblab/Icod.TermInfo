from pathlib import Path

# Temporary branch-scoped driver; removed after the convention gate is green.
# Current checkpoint normalizes the reduced non-test/tool ternary inventory.


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
    """\t\treturn exception is UnauthorizedAccessException\n\t\t\t? TermInfoDatabaseCatalogIssueKind.PermissionFailure\n\t\t\t: TermInfoDatabaseCatalogIssueKind.IoFailure\n\t\t;\n""",
    """\t\treturn ( exception is UnauthorizedAccessException )\n\t\t\t? TermInfoDatabaseCatalogIssueKind.PermissionFailure\n\t\t\t: TermInfoDatabaseCatalogIssueKind.IoFailure\n\t\t;\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSet.SemanticAnalysis.cs",
    """\t\t\tTermInfoDatabaseSetOccurrence? precedenceOwner =\n\t\t\t\tblockingDatabaseIndices.Length == 0\n\t\t\t\t\t? occurrences[ 0 ]\n\t\t\t\t\t: null;\n""",
    """\t\t\tTermInfoDatabaseSetOccurrence? precedenceOwner =\n\t\t\t\t( blockingDatabaseIndices.Length == 0 )\n\t\t\t\t\t? occurrences[ 0 ]\n\t\t\t\t\t: null\n\t\t\t;\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSetComparer.cs",
    """\t\t\treturn new AliasResolution(\n\t\t\t\tset.IsComplete\n\t\t\t\t\t? AliasResolutionStatus.NotObserved\n\t\t\t\t\t: AliasResolutionStatus.Indeterminate,\n\t\t\t\tnull\n\t\t\t);\n""",
    """\t\t\treturn new AliasResolution(\n\t\t\t\t( set.IsComplete )\n\t\t\t\t\t? AliasResolutionStatus.NotObserved\n\t\t\t\t\t: AliasResolutionStatus.Indeterminate,\n\t\t\t\tnull\n\t\t\t);\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSetComparer.cs",
    """\t\treturn new AliasResolution(\n\t\t\tblocked\n\t\t\t\t? AliasResolutionStatus.Indeterminate\n\t\t\t\t: AliasResolutionStatus.OwnerKnown,\n\t\t\towner\n\t\t);\n""",
    """\t\treturn new AliasResolution(\n\t\t\t( blocked )\n\t\t\t\t? AliasResolutionStatus.Indeterminate\n\t\t\t\t: AliasResolutionStatus.OwnerKnown,\n\t\t\towner\n\t\t);\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TermInfoDifference.cs",
    """\tpublic TermInfoSourceSpan? LeftSourceSpan =>\n\t\tLeftSourceField is not null\n\t\t\t|| RightSourceField is not null\n\t\t\t? LeftSourceField?.Span\n\t\t\t: LeftSourceEntry?.Span;\n""",
    """\tpublic TermInfoSourceSpan? LeftSourceSpan =>\n\t\t(\n\t\t\tLeftSourceField is not null\n\t\t\t|| RightSourceField is not null\n\t\t)\n\t\t\t? LeftSourceField?.Span\n\t\t\t: LeftSourceEntry?.Span\n\t;\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TermInfoDifference.cs",
    """\tpublic TermInfoSourceSpan? RightSourceSpan =>\n\t\tLeftSourceField is not null\n\t\t\t|| RightSourceField is not null\n\t\t\t? RightSourceField?.Span\n\t\t\t: RightSourceEntry?.Span;\n""",
    """\tpublic TermInfoSourceSpan? RightSourceSpan =>\n\t\t(\n\t\t\tLeftSourceField is not null\n\t\t\t|| RightSourceField is not null\n\t\t)\n\t\t\t? RightSourceField?.Span\n\t\t\t: RightSourceEntry?.Span\n\t;\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TerminalDescriptionComparer.cs",
    """\t\t\t\tleftValue is not null\n\t\t\t\t\t? new TermInfoCapabilityValue( leftValue )\n\t\t\t\t\t: default,\n""",
    """\t\t\t\t( leftValue is not null )\n\t\t\t\t\t? new TermInfoCapabilityValue( leftValue )\n\t\t\t\t\t: default,\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TerminalDescriptionComparer.cs",
    """\t\t\t\trightValue is not null\n\t\t\t\t\t? new TermInfoCapabilityValue( rightValue )\n\t\t\t\t\t: default\n""",
    """\t\t\t\t( rightValue is not null )\n\t\t\t\t\t? new TermInfoCapabilityValue( rightValue )\n\t\t\t\t\t: default\n""",
)
replace_exact(
    "Icod.TermInfo.Inspection/src/TerminalDescriptionSourceRenderer.Relative.cs",
    """\t\t\tstring? inheritedValue =\n\t\t\t\tinherited.StringCapabilities.TryGetValue(\n\t\t\t\t\tmetadata.Capability,\n\t\t\t\t\tout string? inheritedString\n\t\t\t\t)\n\t\t\t\t\t? inheritedString\n\t\t\t\t\t: null;\n""",
    """\t\t\tstring? inheritedValue =\n\t\t\t\t( inherited.StringCapabilities.TryGetValue(\n\t\t\t\t\tmetadata.Capability,\n\t\t\t\t\tout string? inheritedString\n\t\t\t\t) )\n\t\t\t\t\t? inheritedString\n\t\t\t\t\t: null\n\t\t\t;\n""",
)

# Termcap production.
replace_exact(
    "Icod.TermInfo.Termcap/src/TermcapReverseStringConverter.cs",
    """\t\t\treturn Current == 0\n\t\t\t\t? new ParameterState(\n""",
    """\t\t\treturn ( Current == 0 )\n\t\t\t\t? new ParameterState(\n""",
)
replace_exact(
    "Icod.TermInfo.Termcap/src/TermcapSourceParser.cs",
    """\t\t\t\t\treferenceName.Length == 0\n\t\t\t\t\t\t? null\n\t\t\t\t\t\t: referenceName,\n""",
    """\t\t\t\t\t( referenceName.Length == 0 )\n\t\t\t\t\t\t? null\n\t\t\t\t\t\t: referenceName,\n""",
)

# infocmp.
replace_exact(
    "infocmp/src/Command.cs",
    """\t\t\treturn options.IsComparison\n\t\t\t\t? await InfoCmpInspector.CompareAsync(\n""",
    """\t\t\treturn ( options.IsComparison )\n\t\t\t\t? await InfoCmpInspector.CompareAsync(\n""",
)
replace_exact(
    "infocmp/src/InfoCmpComparison.cs",
    """\t\tstring leftText =\n\t\t\tleft.HasValue\n\t\t\t\t? FormatCapabilityValue(\n\t\t\t\t\tleft.Value,\n\t\t\t\t\tincludeKind: difference.Kind == TermInfoDifferenceKind.DifferentValueKind\n\t\t\t\t)\n\t\t\t\t: FormatMissingCapabilityValue( right );\n""",
    """\t\tstring leftText =\n\t\t\t( left.HasValue )\n\t\t\t\t? FormatCapabilityValue(\n\t\t\t\t\tleft.Value,\n\t\t\t\t\tincludeKind: difference.Kind == TermInfoDifferenceKind.DifferentValueKind\n\t\t\t\t)\n\t\t\t\t: FormatMissingCapabilityValue( right )\n\t\t;\n""",
)
replace_exact(
    "infocmp/src/InfoCmpComparison.cs",
    """\t\tstring rightText =\n\t\t\tright.HasValue\n\t\t\t\t? FormatCapabilityValue(\n\t\t\t\t\tright.Value,\n\t\t\t\t\tincludeKind: difference.Kind == TermInfoDifferenceKind.DifferentValueKind\n\t\t\t\t)\n\t\t\t\t: FormatMissingCapabilityValue( left );\n""",
    """\t\tstring rightText =\n\t\t\t( right.HasValue )\n\t\t\t\t? FormatCapabilityValue(\n\t\t\t\t\tright.Value,\n\t\t\t\t\tincludeKind: difference.Kind == TermInfoDifferenceKind.DifferentValueKind\n\t\t\t\t)\n\t\t\t\t: FormatMissingCapabilityValue( left )\n\t\t;\n""",
)
replace_exact(
    "infocmp/src/InfoCmpInspector.cs",
    """\t\t\tstring rendered = options.Json\n\t\t\t\t? ( databaseSetPlan is null\n""",
    """\t\t\tstring rendered = ( options.Json )\n\t\t\t\t? ( ( databaseSetPlan is null )\n""",
)
replace_exact(
    "infocmp/src/InfoCmpInspector.cs",
    """\t\t\t\t: plan.Source;\n""",
    """\t\t\t\t: plan.Source\n\t\t\t;\n""",
)
replace_exact(
    "infocmp/src/InfoCmpOptions.cs",
    """\t\t\t\t\tallCandidates\n\t\t\t\t\t\t? \"All-candidates planning requires exactly one target terminal.\"\n\t\t\t\t\t\t: \"Relative-source planning requires a target and at least one candidate terminal.\",\n""",
    """\t\t\t\t\t( allCandidates )\n\t\t\t\t\t\t? \"All-candidates planning requires exactly one target terminal.\"\n\t\t\t\t\t\t: \"Relative-source planning requires a target and at least one candidate terminal.\",\n""",
)

# tic/toe tools.
replace_exact(
    "tic/src/TicDiagnostic.cs",
    """\t\t\tstring location =\n\t\t\t\tdiagnostic.Line.HasValue\n""",
    """\t\t\tstring location =\n\t\t\t\t( diagnostic.Line.HasValue )\n""",
)
replace_exact(
    "tic/src/TicDiagnostic.cs",
    """\t\t\tstring severity =\n\t\t\t\tdiagnostic.IsError\n""",
    """\t\t\tstring severity =\n\t\t\t\t( diagnostic.IsError )\n""",
)
replace_exact(
    "tic/src/TicPublisher.cs",
    """\t\t\tTicDestinationResolution destination =\n\t\t\t\toptions.OutputDirectory is string explicitDirectory\n""",
    """\t\t\tTicDestinationResolution destination =\n\t\t\t\t( options.OutputDirectory is string explicitDirectory )\n""",
)
replace_exact(
    "tic/src/TicSourceValidator.cs",
    """\t\treturn diagnostics.Any( diagnostic => diagnostic.IsError )\n\t\t\t? CommandExitCodes.Failure\n""",
    """\t\treturn ( diagnostics.Any( diagnostic => diagnostic.IsError ) )\n\t\t\t? CommandExitCodes.Failure\n""",
)
replace_exact(
    "toe/src/Command.cs",
    """\t\t\t\treturn dependency.HasOperationalFailure\n\t\t\t\t\t? CommandExitCodes.Failure\n""",
    """\t\t\t\treturn ( dependency.HasOperationalFailure )\n\t\t\t\t\t? CommandExitCodes.Failure\n""",
)
replace_exact(
    "toe/src/ToeSourceDependencyAnalyzer.cs",
    """\t\t\t\tTermInfoSourceEntry? parent = identities.TryGetValue(\n\t\t\t\t\treferenceName,\n\t\t\t\t\tout TermInfoSourceEntry? resolvedParent\n\t\t\t\t)\n\t\t\t\t\t? resolvedParent\n\t\t\t\t\t: null;\n""",
    """\t\t\t\tTermInfoSourceEntry? parent = ( identities.TryGetValue(\n\t\t\t\t\treferenceName,\n\t\t\t\t\tout TermInfoSourceEntry? resolvedParent\n\t\t\t\t) )\n\t\t\t\t\t? resolvedParent\n\t\t\t\t\t: null\n\t\t\t\t;\n""",
)

# Package/snapshot tools.
replace_exact(
    "tools/compiler-package-verifier/Program.cs",
    """\t\t\tstring artifactDirectory =\n\t\t\t\targs.Length == 0\n\t\t\t\t\t? Path.Combine( root, \"artifacts\" )\n\t\t\t\t\t: Path.GetFullPath( args[0], root );\n""",
    """\t\t\tstring artifactDirectory =\n\t\t\t\t( args.Length == 0 )\n\t\t\t\t\t? Path.Combine( root, \"artifacts\" )\n\t\t\t\t\t: Path.GetFullPath( args[0], root )\n\t\t\t;\n""",
)
replace_exact(
    "tools/inspection-package-verifier/Program.cs",
    """\t\t\tstring artifactDirectory =\n\t\t\t\targs.Length == 0\n\t\t\t\t\t? Path.Combine(\n\t\t\t\t\t\troot,\n\t\t\t\t\t\t\"artifacts\"\n\t\t\t\t\t)\n\t\t\t\t\t: Path.GetFullPath(\n\t\t\t\t\t\targs[ 0 ],\n\t\t\t\t\t\troot\n\t\t\t\t\t);\n""",
    """\t\t\tstring artifactDirectory =\n\t\t\t\t( args.Length == 0 )\n\t\t\t\t\t? Path.Combine(\n\t\t\t\t\t\troot,\n\t\t\t\t\t\t\"artifacts\"\n\t\t\t\t\t)\n\t\t\t\t\t: Path.GetFullPath(\n\t\t\t\t\t\targs[ 0 ],\n\t\t\t\t\t\troot\n\t\t\t\t\t)\n\t\t\t;\n""",
)
replace_exact(
    "tools/public-api-snapshot/Program.cs",
    """\t\t\ttype.IsGenericTypeDefinition\n\t\t\t\t? type.GetGenericArguments()\n\t\t\t\t: Array.Empty<Type>(),\n""",
    """\t\t\t( type.IsGenericTypeDefinition )\n\t\t\t\t? type.GetGenericArguments()\n\t\t\t\t: Array.Empty<Type>(),\n""",
)
replace_exact(
    "tools/termcap-package-verifier/Program.cs",
    """\t\t\tstring artifactDirectory =\n\t\t\t\targs.Length == 0\n\t\t\t\t\t? Path.Combine( root, \"artifacts\" )\n\t\t\t\t\t: Path.GetFullPath( args[0], root )\n\t\t\t;\n""",
    """\t\t\tstring artifactDirectory =\n\t\t\t\t( args.Length == 0 )\n\t\t\t\t\t? Path.Combine( root, \"artifacts\" )\n\t\t\t\t\t: Path.GetFullPath( args[0], root )\n\t\t\t;\n""",
)
