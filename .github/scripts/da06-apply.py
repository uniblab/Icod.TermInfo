from pathlib import Path
import json


def replace_exact(path_name: str, old: str, new: str, count: int = 1) -> None:
    path = Path(path_name)
    text = path.read_text(encoding="utf-8")
    actual = text.count(old)
    if actual != count:
        raise RuntimeError(f"{path_name}: expected {count} occurrence(s), found {actual}: {old!r}")
    path.write_text(text.replace(old, new, count), encoding="utf-8", newline="\n")


def replace_all_required(path_name: str, old: str, new: str) -> None:
    path = Path(path_name)
    text = path.read_text(encoding="utf-8")
    actual = text.count(old)
    if actual < 1:
        raise RuntimeError(f"{path_name}: required text not found: {old!r}")
    path.write_text(text.replace(old, new), encoding="utf-8", newline="\n")


def write_new(path_name: str, content: str) -> None:
    path = Path(path_name)
    if path.exists():
        raise RuntimeError(f"{path_name}: file already exists")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content, encoding="utf-8", newline="\n")


# -----------------------------------------------------------------------------
# Inspection JSON v2 — additive database automation contract.
# -----------------------------------------------------------------------------
write_new(
    "Icod.TermInfo.Inspection/src/TermInfoJsonRenderer.DatabaseAutomationV2.cs",
    r'''namespace Icod.TermInfo.Inspection;

public static partial class TermInfoJsonRenderer {
	/// <summary>
	/// The additive schema identifier used by 1.10 database-set automation.
	/// Version-1 documents continue to use <see cref="SchemaIdentifier"/>.
	/// </summary>
	public const string DatabaseAutomationSchemaIdentifier =
		"urn:icod:terminfo:inspection:json:2";

	/// <summary>
	/// The additive schema version used by 1.10 database-set automation.
	/// </summary>
	public const int DatabaseAutomationSchemaVersion = 2;

	private const string DatabaseSetDocumentKind = "databaseSet";
	private const string DatabaseSetComparisonDocumentKind = "databaseSetComparison";
	private const string DatabaseSetPlanDocumentKind = "databaseSetPlan";

	/// <summary>
	/// Renders an ordered database set using the additive version-2 automation
	/// contract.
	/// </summary>
	public static string Render(
		TermInfoDatabaseSet databaseSet
	) =>
		Render(
			databaseSet,
			new TermInfoJsonRendererOptions(),
			CancellationToken.None
		);

	/// <summary>
	/// Renders an ordered database set using explicit deterministic JSON policy.
	/// </summary>
	public static string Render(
		TermInfoDatabaseSet databaseSet,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( databaseSet );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		TermInfoDatabaseSetSemanticAnalysis analysis =
			databaseSet.AnalyzeSemantics(
				cancellationToken: cancellationToken
			);
		return RenderDatabaseSetV2(
			databaseSet,
			analysis,
			options,
			cancellationToken
		);
	}

	/// <summary>
	/// Renders a database-set comparison using the additive version-2 automation
	/// contract.
	/// </summary>
	public static string Render(
		TermInfoDatabaseSetComparisonResult comparison
	) =>
		Render(
			comparison,
			new TermInfoJsonRendererOptions(),
			CancellationToken.None
		);

	/// <summary>
	/// Renders a database-set comparison using explicit deterministic JSON policy.
	/// </summary>
	public static string Render(
		TermInfoDatabaseSetComparisonResult comparison,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( comparison );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		return RenderDatabaseSetComparisonV2(
			comparison,
			options,
			cancellationToken
		);
	}

	/// <summary>
	/// Renders a database-set-backed source plan using the additive version-2
	/// automation contract.
	/// </summary>
	public static string Render(
		TermInfoDatabaseSetSourcePlanningResult planningResult
	) =>
		Render(
			planningResult,
			new TermInfoJsonRendererOptions(),
			CancellationToken.None
		);

	/// <summary>
	/// Renders a database-set-backed source plan using explicit deterministic JSON
	/// policy.
	/// </summary>
	public static string Render(
		TermInfoDatabaseSetSourcePlanningResult planningResult,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( planningResult );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		return RenderDatabaseSetPlanV2(
			planningResult,
			options,
			cancellationToken
		);
	}

	private static string RenderDatabaseSetV2(
		TermInfoDatabaseSet databaseSet,
		TermInfoDatabaseSetSemanticAnalysis analysis,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken
	) {
		BoundedJsonOutput output = new( options.MaximumOutputByteCount );
		DeterministicJsonWriter writer = new( output, options.WriteIndented );
		try {
			writer.WriteStartObject();
			WriteDatabaseAutomationEnvelopePrefix( writer, DatabaseSetDocumentKind );
			writer.WriteStartObject( "data" );
			writer.WriteBoolean( "isComplete", databaseSet.IsComplete );
			writer.WriteNumber( "databaseCount", databaseSet.Entries.Count );
			writer.WriteNumber( "totalEntryCount", databaseSet.TotalEntryCount );
			writer.WriteNumber( "identityCount", databaseSet.Identities.Count );
			writer.WriteNumber( "issueCount", databaseSet.Issues.Count );
			writer.WriteStartArray( "databases" );
			foreach ( TermInfoDatabaseSetEntry database in databaseSet.Entries ) {
				cancellationToken.ThrowIfCancellationRequested();
				WriteDatabaseSetEntryV2( writer, database );
			}
			writer.WriteEndArray();
			writer.WriteStartArray( "identities" );
			foreach ( TermInfoDatabaseSetIdentity identity in databaseSet.Identities ) {
				cancellationToken.ThrowIfCancellationRequested();
				WriteDatabaseSetIdentityV2(
					writer,
					identity,
					databaseSet.LookupCanonicalName( identity.Name ),
					cancellationToken
				);
			}
			writer.WriteEndArray();
			writer.WriteStartArray( "issues" );
			foreach ( TermInfoDatabaseSetIssue issue in databaseSet.Issues ) {
				cancellationToken.ThrowIfCancellationRequested();
				WriteDatabaseSetIssueV2( writer, issue );
			}
			writer.WriteEndArray();
			WriteSemanticAnalysisV2( writer, analysis, cancellationToken );
			writer.WriteEndObject();
			writer.WriteEndObject();
			cancellationToken.ThrowIfCancellationRequested();
		} catch ( JsonOutputLimitExceededException exception ) {
			throw CreateOutputLimitException( options, exception );
		}
		return output.GetString();
	}

	private static string RenderDatabaseSetComparisonV2(
		TermInfoDatabaseSetComparisonResult comparison,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken
	) {
		BoundedJsonOutput output = new( options.MaximumOutputByteCount );
		DeterministicJsonWriter writer = new( output, options.WriteIndented );
		try {
			writer.WriteStartObject();
			WriteDatabaseAutomationEnvelopePrefix( writer, DatabaseSetComparisonDocumentKind );
			writer.WriteStartObject( "data" );
			writer.WriteBoolean( "isConclusive", comparison.IsConclusive );
			writer.WriteBoolean( "areEffectivelyEquivalent", comparison.AreEffectivelyEquivalent );
			writer.WriteBoolean( "areStructurallyEquivalent", comparison.AreStructurallyEquivalent );
			writer.WriteBoolean( "areEquivalent", comparison.AreEquivalent );
			writer.WriteNumber( "semanticComparisonCount", comparison.SemanticComparisonCount );
			writer.WriteNumber( "aliasOccurrenceCount", comparison.AliasOccurrenceCount );
			writer.WriteStartArray( "differences" );
			foreach ( TermInfoDatabaseSetDifference difference in comparison.Differences ) {
				cancellationToken.ThrowIfCancellationRequested();
				WriteDatabaseSetDifferenceV2( writer, difference, cancellationToken );
			}
			writer.WriteEndArray();
			writer.WriteEndObject();
			writer.WriteEndObject();
			cancellationToken.ThrowIfCancellationRequested();
		} catch ( JsonOutputLimitExceededException exception ) {
			throw CreateOutputLimitException( options, exception );
		}
		return output.GetString();
	}

	private static string RenderDatabaseSetPlanV2(
		TermInfoDatabaseSetSourcePlanningResult planningResult,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken
	) {
		BoundedJsonOutput output = new( options.MaximumOutputByteCount );
		DeterministicJsonWriter writer = new( output, options.WriteIndented );
		try {
			writer.WriteStartObject();
			WriteDatabaseAutomationEnvelopePrefix( writer, DatabaseSetPlanDocumentKind );
			writer.WriteStartObject( "data" );
			writer.WriteStartArray( "databases" );
			foreach ( TermInfoDatabaseSetEntry database in planningResult.DatabaseSet.Entries ) {
				cancellationToken.ThrowIfCancellationRequested();
				WriteDatabaseSetEntryV2( writer, database );
			}
			writer.WriteEndArray();
			writer.WriteNumber( "candidateCount", planningResult.Candidates.Count );
			writer.WriteNumber(
				"collapsedDuplicateOccurrenceCount",
				planningResult.CollapsedDuplicateOccurrenceCount
			);
			writer.WriteNumber(
				"candidateSemanticComparisonCount",
				planningResult.CandidateSemanticComparisonCount
			);
			writer.WriteStartArray( "candidates" );
			foreach ( TermInfoDatabaseSetPlanningCandidate candidate in planningResult.Candidates ) {
				cancellationToken.ThrowIfCancellationRequested();
				WritePlanningCandidateV2( writer, candidate, cancellationToken );
			}
			writer.WriteEndArray();
			writer.WriteNumber( "selectedParentCount", planningResult.Plan.SelectedParents.Count );
			WriteNumberArray(
				writer,
				"selectedCandidateIndices",
				planningResult.Plan.Score.SelectedCandidateIndices,
				cancellationToken
			);
			writer.WriteStartArray( "selectedCandidates" );
			foreach ( TermInfoDatabaseSetPlanningCandidate candidate in planningResult.SelectedCandidates ) {
				cancellationToken.ThrowIfCancellationRequested();
				WritePlanningCandidateV2( writer, candidate, cancellationToken );
			}
			writer.WriteEndArray();
			writer.WriteString( "source", planningResult.Plan.Source );
			WritePlanningScore( writer, planningResult.Plan.Score, cancellationToken );
			writer.WriteNumber( "evaluatedPlanCount", planningResult.Plan.EvaluatedPlanCount );
			writer.WriteBoolean( "isExhaustive", planningResult.Plan.IsExhaustive );
			writer.WriteEndObject();
			writer.WriteEndObject();
			cancellationToken.ThrowIfCancellationRequested();
		} catch ( JsonOutputLimitExceededException exception ) {
			throw CreateOutputLimitException( options, exception );
		}
		return output.GetString();
	}

	private static void WriteDatabaseAutomationEnvelopePrefix(
		DeterministicJsonWriter writer,
		string documentKind
	) {
		writer.WriteString( "schema", DatabaseAutomationSchemaIdentifier );
		writer.WriteNumber( "schemaVersion", DatabaseAutomationSchemaVersion );
		writer.WriteString( "documentKind", documentKind );
	}

	private static void WriteDatabaseSetEntryV2(
		DeterministicJsonWriter writer,
		TermInfoDatabaseSetEntry database
	) {
		writer.WriteStartObjectValue();
		writer.WriteNumber( "index", database.Index );
		writer.WriteString( "root", database.Catalog.Root );
		writer.WriteString( "kind", GetDatabaseCatalogKindName( database.Catalog.Kind ) );
		writer.WriteBoolean( "isComplete", database.IsComplete );
		writer.WriteNumber( "entryCount", database.Catalog.Entries.Count );
		writer.WriteNumber( "issueCount", database.Catalog.Issues.Count );
		writer.WriteEndObject();
	}

	private static void WriteDatabaseSetIdentityV2(
		DeterministicJsonWriter writer,
		TermInfoDatabaseSetIdentity identity,
		TermInfoDatabaseSetLookupResult lookup,
		CancellationToken cancellationToken
	) {
		writer.WriteStartObjectValue();
		writer.WriteString( "name", identity.Name );
		writer.WriteString( "lookupStatus", GetLookupStatusName( lookup.Status ) );
		WriteOccurrencePropertyV2( writer, "winner", lookup.Winner, cancellationToken );
		writer.WriteStartArray( "occurrences" );
		foreach ( TermInfoDatabaseSetOccurrence occurrence in lookup.Occurrences ) {
			cancellationToken.ThrowIfCancellationRequested();
			WriteOccurrenceValueV2( writer, occurrence, cancellationToken );
		}
		writer.WriteEndArray();
		WriteNumberArray(
			writer,
			"incompleteDatabaseIndices",
			lookup.IncompleteDatabaseIndices,
			cancellationToken
		);
		WriteNumberArray(
			writer,
			"blockingDatabaseIndices",
			lookup.BlockingDatabaseIndices,
			cancellationToken
		);
		writer.WriteEndObject();
	}

	private static void WriteDatabaseSetIssueV2(
		DeterministicJsonWriter writer,
		TermInfoDatabaseSetIssue issue
	) {
		writer.WriteStartObjectValue();
		writer.WriteNumber( "databaseIndex", issue.DatabaseIndex );
		writer.WriteNumber( "catalogIssueIndex", issue.CatalogIssueIndex );
		writer.WriteString( "kind", GetDatabaseCatalogIssueKindName( issue.Issue.Kind ) );
		writer.WriteString( "path", issue.Issue.Path );
		writer.WriteString( "message", issue.Issue.Message );
		writer.WriteEndObject();
	}

	private static void WriteSemanticAnalysisV2(
		DeterministicJsonWriter writer,
		TermInfoDatabaseSetSemanticAnalysis analysis,
		CancellationToken cancellationToken
	) {
		writer.WriteStartObject( "semanticAnalysis" );
		writer.WriteBoolean( "isComplete", analysis.IsComplete );
		writer.WriteBoolean( "hasSemanticDifferences", analysis.HasSemanticDifferences );
		writer.WriteBoolean( "hasIndeterminateEvidence", analysis.HasIndeterminateEvidence );
		writer.WriteNumber( "semanticComparisonCount", analysis.SemanticComparisonCount );
		writer.WriteNumber( "aliasOccurrenceCount", analysis.AliasOccurrenceCount );
		writer.WriteNumber(
			"maximumAliasOccurrenceCount",
			TermInfoDatabaseSetSemanticAnalysisOptions.DefaultMaximumAliasOccurrenceCount
		);
		writer.WriteStartArray( "repeatedIdentities" );
		foreach ( TermInfoDatabaseSetIdentityAnalysis identity in analysis.RepeatedIdentities ) {
			cancellationToken.ThrowIfCancellationRequested();
			writer.WriteStartObjectValue();
			writer.WriteString( "name", identity.Identity.Name );
			writer.WriteString( "relationship", GetSemanticRelationshipName( identity.Relationship ) );
			writer.WriteBoolean( "isComplete", identity.IsComplete );
			WriteOccurrencePropertyV2( writer, "winner", identity.Lookup.Winner, cancellationToken );
			writer.WriteStartArray( "shadows" );
			foreach ( TermInfoDatabaseSetShadowAnalysis shadow in identity.Shadows ) {
				cancellationToken.ThrowIfCancellationRequested();
				writer.WriteStartObjectValue();
				writer.WriteString( "relationship", GetSemanticRelationshipName( shadow.Relationship ) );
				WriteOccurrencePropertyV2( writer, "occurrence", shadow.Occurrence, cancellationToken );
				WriteComparisonEvidenceV2( writer, "comparison", shadow.Comparison, cancellationToken );
				writer.WriteEndObject();
			}
			writer.WriteEndArray();
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
		writer.WriteStartArray( "aliases" );
		foreach ( TermInfoDatabaseSetAliasAnalysis alias in analysis.Aliases ) {
			cancellationToken.ThrowIfCancellationRequested();
			writer.WriteStartObjectValue();
			writer.WriteString( "alias", alias.Alias );
			writer.WriteString( "relationship", GetSemanticRelationshipName( alias.Relationship ) );
			writer.WriteBoolean( "isComplete", alias.IsComplete );
			writer.WriteBoolean( "hasMultipleCanonicalOwners", alias.HasMultipleCanonicalOwners );
			writer.WriteBoolean( "matchesCanonicalName", alias.MatchesCanonicalName );
			WriteStringArray( writer, "canonicalNames", alias.CanonicalNames, cancellationToken );
			WriteOccurrencePropertyV2( writer, "precedenceOwner", alias.PrecedenceOwner, cancellationToken );
			writer.WriteString(
				"matchingCanonicalName",
				alias.MatchingCanonicalIdentity?.Name
			);
			writer.WriteStartArray( "occurrences" );
			foreach ( TermInfoDatabaseSetOccurrence occurrence in alias.Occurrences ) {
				cancellationToken.ThrowIfCancellationRequested();
				WriteOccurrenceValueV2( writer, occurrence, cancellationToken );
			}
			writer.WriteEndArray();
			WriteNumberArray(
				writer,
				"blockingDatabaseIndices",
				alias.BlockingDatabaseIndices,
				cancellationToken
			);
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
		writer.WriteEndObject();
	}

	private static void WriteDatabaseSetDifferenceV2(
		DeterministicJsonWriter writer,
		TermInfoDatabaseSetDifference difference,
		CancellationToken cancellationToken
	) {
		writer.WriteStartObjectValue();
		writer.WriteString( "kind", GetDatabaseSetDifferenceKindName( difference.Kind ) );
		writer.WriteString( "name", difference.Name );
		WriteDatabaseEntryPropertyV2( writer, "leftDatabase", difference.LeftDatabase );
		WriteDatabaseEntryPropertyV2( writer, "rightDatabase", difference.RightDatabase );
		WriteOccurrencePropertyV2( writer, "leftOccurrence", difference.LeftOccurrence, cancellationToken );
		WriteOccurrencePropertyV2( writer, "rightOccurrence", difference.RightOccurrence, cancellationToken );
		WriteDatabaseIssuePropertyV2( writer, "leftIssue", difference.LeftIssue );
		WriteDatabaseIssuePropertyV2( writer, "rightIssue", difference.RightIssue );
		WriteLookupPropertyV2( writer, "leftLookup", difference.LeftLookup, cancellationToken );
		WriteLookupPropertyV2( writer, "rightLookup", difference.RightLookup, cancellationToken );
		WriteComparisonEvidenceV2(
			writer,
			"semanticComparison",
			difference.SemanticComparison,
			cancellationToken
		);
		writer.WriteEndObject();
	}

	private static void WritePlanningCandidateV2(
		DeterministicJsonWriter writer,
		TermInfoDatabaseSetPlanningCandidate candidate,
		CancellationToken cancellationToken
	) {
		writer.WriteStartObjectValue();
		writer.WriteNumber( "candidateIndex", candidate.CandidateIndex );
		writer.WriteNumber( "databaseIndex", candidate.DatabaseIndex );
		writer.WriteNumber( "catalogEntryIndex", candidate.CatalogEntryIndex );
		writer.WriteString( "root", candidate.Database.Catalog.Root );
		writer.WriteString( "path", candidate.Occurrence.Entry.Path );
		writer.WriteString( "canonicalName", candidate.CanonicalName );
		writer.WriteString( "useName", candidate.UseName );
		WriteStringArray(
			writer,
			"aliases",
			candidate.Occurrence.Aliases,
			cancellationToken
		);
		writer.WriteEndObject();
	}

	private static void WriteOccurrencePropertyV2(
		DeterministicJsonWriter writer,
		string propertyName,
		TermInfoDatabaseSetOccurrence? occurrence,
		CancellationToken cancellationToken
	) {
		if ( occurrence is null ) {
			writer.WriteNull( propertyName );
			return;
		}
		writer.WriteStartObject( propertyName );
		WriteOccurrenceFieldsV2( writer, occurrence, cancellationToken );
		writer.WriteEndObject();
	}

	private static void WriteOccurrenceValueV2(
		DeterministicJsonWriter writer,
		TermInfoDatabaseSetOccurrence occurrence,
		CancellationToken cancellationToken
	) {
		writer.WriteStartObjectValue();
		WriteOccurrenceFieldsV2( writer, occurrence, cancellationToken );
		writer.WriteEndObject();
	}

	private static void WriteOccurrenceFieldsV2(
		DeterministicJsonWriter writer,
		TermInfoDatabaseSetOccurrence occurrence,
		CancellationToken cancellationToken
	) {
		writer.WriteNumber( "databaseIndex", occurrence.DatabaseIndex );
		writer.WriteNumber( "catalogEntryIndex", occurrence.CatalogEntryIndex );
		writer.WriteString( "path", occurrence.Entry.Path );
		writer.WriteString( "name", occurrence.Name );
		WriteStringArray( writer, "aliases", occurrence.Aliases, cancellationToken );
	}

	private static void WriteDatabaseEntryPropertyV2(
		DeterministicJsonWriter writer,
		string propertyName,
		TermInfoDatabaseSetEntry? database
	) {
		if ( database is null ) {
			writer.WriteNull( propertyName );
			return;
		}
		writer.WriteStartObject( propertyName );
		writer.WriteNumber( "index", database.Index );
		writer.WriteString( "root", database.Catalog.Root );
		writer.WriteString( "kind", GetDatabaseCatalogKindName( database.Catalog.Kind ) );
		writer.WriteBoolean( "isComplete", database.IsComplete );
		writer.WriteEndObject();
	}

	private static void WriteDatabaseIssuePropertyV2(
		DeterministicJsonWriter writer,
		string propertyName,
		TermInfoDatabaseSetIssue? issue
	) {
		if ( issue is null ) {
			writer.WriteNull( propertyName );
			return;
		}
		writer.WriteStartObject( propertyName );
		writer.WriteNumber( "databaseIndex", issue.DatabaseIndex );
		writer.WriteNumber( "catalogIssueIndex", issue.CatalogIssueIndex );
		writer.WriteString( "kind", GetDatabaseCatalogIssueKindName( issue.Issue.Kind ) );
		writer.WriteString( "path", issue.Issue.Path );
		writer.WriteString( "message", issue.Issue.Message );
		writer.WriteEndObject();
	}

	private static void WriteLookupPropertyV2(
		DeterministicJsonWriter writer,
		string propertyName,
		TermInfoDatabaseSetLookupResult? lookup,
		CancellationToken cancellationToken
	) {
		if ( lookup is null ) {
			writer.WriteNull( propertyName );
			return;
		}
		writer.WriteStartObject( propertyName );
		writer.WriteString( "name", lookup.Name );
		writer.WriteString( "status", GetLookupStatusName( lookup.Status ) );
		WriteOccurrencePropertyV2( writer, "winner", lookup.Winner, cancellationToken );
		WriteNumberArray(
			writer,
			"incompleteDatabaseIndices",
			lookup.IncompleteDatabaseIndices,
			cancellationToken
		);
		WriteNumberArray(
			writer,
			"blockingDatabaseIndices",
			lookup.BlockingDatabaseIndices,
			cancellationToken
		);
		writer.WriteEndObject();
	}

	private static void WriteComparisonEvidenceV2(
		DeterministicJsonWriter writer,
		string propertyName,
		TermInfoComparisonResult? comparison,
		CancellationToken cancellationToken
	) {
		if ( comparison is null ) {
			writer.WriteNull( propertyName );
			return;
		}
		writer.WriteStartObject( propertyName );
		writer.WriteBoolean( "areEqual", comparison.AreEqual );
		writer.WriteStartArray( "differences" );
		foreach ( TermInfoDifference difference in comparison.Differences ) {
			cancellationToken.ThrowIfCancellationRequested();
			WriteDifference( writer, difference, cancellationToken );
		}
		writer.WriteEndArray();
		writer.WriteEndObject();
	}

	private static void WriteNumberArray(
		DeterministicJsonWriter writer,
		string propertyName,
		IEnumerable<int> values,
		CancellationToken cancellationToken
	) {
		writer.WriteStartArray( propertyName );
		foreach ( int value in values ) {
			cancellationToken.ThrowIfCancellationRequested();
			writer.WriteNumberValue( value );
		}
		writer.WriteEndArray();
	}

	private static string GetLookupStatusName(
		TermInfoDatabaseSetLookupStatus status
	) =>
		status switch {
			TermInfoDatabaseSetLookupStatus.NotObserved => "notObserved",
			TermInfoDatabaseSetLookupStatus.WinnerKnown => "winnerKnown",
			TermInfoDatabaseSetLookupStatus.Indeterminate => "indeterminate",
			_ => throw new InvalidOperationException( $"Unsupported database-set lookup status '{status}'." ),
		};

	private static string GetSemanticRelationshipName(
		TermInfoDatabaseSetSemanticRelationship relationship
	) =>
		relationship switch {
			TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual => "semanticallyEqual",
			TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent => "semanticallyDifferent",
			TermInfoDatabaseSetSemanticRelationship.Indeterminate => "indeterminate",
			_ => throw new InvalidOperationException( $"Unsupported semantic relationship '{relationship}'." ),
		};

	private static string GetDatabaseSetDifferenceKindName(
		TermInfoDatabaseSetDifferenceKind kind
	) =>
		kind switch {
			TermInfoDatabaseSetDifferenceKind.RootTopology => "rootTopology",
			TermInfoDatabaseSetDifferenceKind.Completeness => "completeness",
			TermInfoDatabaseSetDifferenceKind.Issue => "issue",
			TermInfoDatabaseSetDifferenceKind.OnlyInLeft => "onlyInLeft",
			TermInfoDatabaseSetDifferenceKind.OnlyInRight => "onlyInRight",
			TermInfoDatabaseSetDifferenceKind.EffectiveSemantic => "effectiveSemantic",
			TermInfoDatabaseSetDifferenceKind.EffectiveProvenance => "effectiveProvenance",
			TermInfoDatabaseSetDifferenceKind.AliasOwnership => "aliasOwnership",
			TermInfoDatabaseSetDifferenceKind.ShadowSet => "shadowSet",
			TermInfoDatabaseSetDifferenceKind.Indeterminate => "indeterminate",
			_ => throw new InvalidOperationException( $"Unsupported database-set difference kind '{kind}'." ),
		};
}
''',
)

# Version-2 schema is deliberately separate; version-1 file remains byte-frozen.
v2_schema = {
    "$schema": "https://json-schema.org/draft/2020-12/schema",
    "$id": "urn:icod:terminfo:inspection:json:2",
    "title": "Icod.TermInfo.Inspection database automation JSON version 2",
    "oneOf": [
        {"$ref": "#/$defs/databaseSetDocument"},
        {"$ref": "#/$defs/databaseSetComparisonDocument"},
        {"$ref": "#/$defs/databaseSetPlanDocument"},
    ],
    "$defs": {
        "envelopeBase": {
            "type": "object",
            "required": ["schema", "schemaVersion", "documentKind", "data"],
            "properties": {
                "schema": {"const": "urn:icod:terminfo:inspection:json:2"},
                "schemaVersion": {"const": 2},
            },
        },
        "databaseSetDocument": {
            "allOf": [
                {"$ref": "#/$defs/envelopeBase"},
                {
                    "properties": {
                        "documentKind": {"const": "databaseSet"},
                        "data": {"$ref": "#/$defs/databaseSetData"},
                    }
                },
            ]
        },
        "databaseSetComparisonDocument": {
            "allOf": [
                {"$ref": "#/$defs/envelopeBase"},
                {
                    "properties": {
                        "documentKind": {"const": "databaseSetComparison"},
                        "data": {"$ref": "#/$defs/databaseSetComparisonData"},
                    }
                },
            ]
        },
        "databaseSetPlanDocument": {
            "allOf": [
                {"$ref": "#/$defs/envelopeBase"},
                {
                    "properties": {
                        "documentKind": {"const": "databaseSetPlan"},
                        "data": {"$ref": "#/$defs/databaseSetPlanData"},
                    }
                },
            ]
        },
        "databaseSetData": {
            "type": "object",
            "required": [
                "isComplete", "databaseCount", "totalEntryCount", "identityCount",
                "issueCount", "databases", "identities", "issues", "semanticAnalysis"
            ],
            "properties": {
                "isComplete": {"type": "boolean"},
                "databaseCount": {"type": "integer", "minimum": 0},
                "totalEntryCount": {"type": "integer", "minimum": 0},
                "identityCount": {"type": "integer", "minimum": 0},
                "issueCount": {"type": "integer", "minimum": 0},
                "databases": {"type": "array"},
                "identities": {"type": "array"},
                "issues": {"type": "array"},
                "semanticAnalysis": {"type": "object"},
            },
            "additionalProperties": False,
        },
        "databaseSetComparisonData": {
            "type": "object",
            "required": [
                "isConclusive", "areEffectivelyEquivalent", "areStructurallyEquivalent",
                "areEquivalent", "semanticComparisonCount", "aliasOccurrenceCount", "differences"
            ],
            "properties": {
                "isConclusive": {"type": "boolean"},
                "areEffectivelyEquivalent": {"type": "boolean"},
                "areStructurallyEquivalent": {"type": "boolean"},
                "areEquivalent": {"type": "boolean"},
                "semanticComparisonCount": {"type": "integer", "minimum": 0},
                "aliasOccurrenceCount": {"type": "integer", "minimum": 0},
                "differences": {"type": "array"},
            },
            "additionalProperties": False,
        },
        "databaseSetPlanData": {
            "type": "object",
            "required": [
                "databases", "candidateCount", "collapsedDuplicateOccurrenceCount",
                "candidateSemanticComparisonCount", "candidates", "selectedParentCount",
                "selectedCandidateIndices", "selectedCandidates", "source", "score",
                "evaluatedPlanCount", "isExhaustive"
            ],
            "properties": {
                "databases": {"type": "array"},
                "candidateCount": {"type": "integer", "minimum": 0},
                "collapsedDuplicateOccurrenceCount": {"type": "integer", "minimum": 0},
                "candidateSemanticComparisonCount": {"type": "integer", "minimum": 0},
                "candidates": {"type": "array"},
                "selectedParentCount": {"type": "integer", "minimum": 0},
                "selectedCandidateIndices": {"type": "array", "items": {"type": "integer", "minimum": 0}},
                "selectedCandidates": {"type": "array"},
                "source": {"type": "string"},
                "score": {"type": "object"},
                "evaluatedPlanCount": {"type": "integer", "minimum": 1},
                "isExhaustive": {"type": "boolean"},
            },
            "additionalProperties": False,
        },
    },
}
write_new(
    "docs/Icod.TermInfo.Inspection.schema.v2.json",
    json.dumps(v2_schema, indent=2, ensure_ascii=False) + "\n",
)

# -----------------------------------------------------------------------------
# toe: one root stays v1, multiple roots become v2, explicit set comparison mode.
# -----------------------------------------------------------------------------
replace_exact(
    "toe/src/Command.cs",
    '''\t\t\tToeCommandLineNormalizationResult normalized =\n\t\t\t\tToeCommandLine.NormalizeListing( args );''',
    '''\t\t\tif ( args.Contains( "--compare-set", StringComparer.Ordinal ) ) {\n\t\t\t\treturn await CompareDatabaseSetsAsync(\n\t\t\t\t\targs,\n\t\t\t\t\tstdout,\n\t\t\t\t\tstderr,\n\t\t\t\t\tcancellationToken\n\t\t\t\t).ConfigureAwait( false );\n\t\t\t}\n\n\t\t\tToeCommandLineNormalizationResult normalized =\n\t\t\t\tToeCommandLine.NormalizeListing( args );''',
)
replace_exact(
    "toe/src/Command.cs",
    '''\t\t\tif ( options.Json ) {\n\t\t\t\treturn await RenderCatalogAsync(\n\t\t\t\t\toptions.Directories[ 0 ],\n\t\t\t\t\tstdout,\n\t\t\t\t\tstderr,\n\t\t\t\t\tcancellationToken\n\t\t\t\t).ConfigureAwait( false );\n\t\t\t}''',
    '''\t\t\tif ( options.Json ) {\n\t\t\t\treturn options.Directories.Count == 1\n\t\t\t\t\t? await RenderCatalogAsync(\n\t\t\t\t\t\toptions.Directories[ 0 ],\n\t\t\t\t\t\tstdout,\n\t\t\t\t\t\tstderr,\n\t\t\t\t\t\tcancellationToken\n\t\t\t\t\t).ConfigureAwait( false )\n\t\t\t\t\t: await RenderDatabaseSetAsync(\n\t\t\t\t\t\toptions.Directories,\n\t\t\t\t\t\tstdout,\n\t\t\t\t\t\tstderr,\n\t\t\t\t\t\tcancellationToken\n\t\t\t\t\t).ConfigureAwait( false );\n\t\t\t}''',
)
replace_exact(
    "toe/src/Command.cs",
    '''\tprivate static ToeListingResult BuildListing(''',
    r'''	private static async Task<int> RenderDatabaseSetAsync(
		IReadOnlyList<string> directories,
		Stream stdout,
		Stream stderr,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( directories );
		ArgumentNullException.ThrowIfNull( stdout );
		ArgumentNullException.ThrowIfNull( stderr );
		cancellationToken.ThrowIfCancellationRequested();

		string rendered;
		try {
			TermInfoDatabaseSet databaseSet =
				TermInfoDatabaseInspector.InspectSet(
					directories,
					parserOptions: null,
					cancellationToken: cancellationToken
				);
			rendered = TermInfoJsonRenderer.Render(
				databaseSet,
				new TermInfoJsonRendererOptions(),
				cancellationToken
			) + "\n";
		} catch ( Exception exception ) when ( IsOperationalException( exception ) ) {
			var diagnostics = new StringBuilder();
			AppendDiagnostic(
				diagnostics,
				"TOE0005",
				"database set",
				exception.Message
			);
			await WriteAsync( stderr, diagnostics.ToString(), cancellationToken ).ConfigureAwait( false );
			return CommandExitCodes.Failure;
		}

		await WriteAsync( stdout, rendered, cancellationToken ).ConfigureAwait( false );
		return CommandExitCodes.Success;
	}

	private static async Task<int> CompareDatabaseSetsAsync(
		IReadOnlyList<string> args,
		Stream stdout,
		Stream stderr,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( args );
		ArgumentNullException.ThrowIfNull( stdout );
		ArgumentNullException.ThrowIfNull( stderr );
		cancellationToken.ThrowIfCancellationRequested();

		if ( !TryParseDatabaseSetComparison(
			args,
			out IReadOnlyList<string> leftRoots,
			out IReadOnlyList<string> rightRoots,
			out string error
		) ) {
			await WriteUsageErrorAsync( stderr, error, cancellationToken ).ConfigureAwait( false );
			return CommandExitCodes.UsageError;
		}

		try {
			TermInfoDatabaseSet left = TermInfoDatabaseInspector.InspectSet(
				leftRoots,
				parserOptions: null,
				cancellationToken: cancellationToken
			);
			TermInfoDatabaseSet right = TermInfoDatabaseInspector.InspectSet(
				rightRoots,
				parserOptions: null,
				cancellationToken: cancellationToken
			);
			TermInfoDatabaseSetComparisonResult comparison =
				TermInfoDatabaseSetComparer.Compare(
					left,
					right,
					cancellationToken: cancellationToken
				);
			string rendered = TermInfoJsonRenderer.Render(
				comparison,
				new TermInfoJsonRendererOptions(),
				cancellationToken
			) + "\n";
			await WriteAsync( stdout, rendered, cancellationToken ).ConfigureAwait( false );
			return CommandExitCodes.Success;
		} catch ( Exception exception ) when ( IsOperationalException( exception ) ) {
			var diagnostics = new StringBuilder();
			AppendDiagnostic(
				diagnostics,
				"TOE0005",
				"database-set comparison",
				exception.Message
			);
			await WriteAsync( stderr, diagnostics.ToString(), cancellationToken ).ConfigureAwait( false );
			return CommandExitCodes.Failure;
		}
	}

	private static bool TryParseDatabaseSetComparison(
		IReadOnlyList<string> args,
		out IReadOnlyList<string> leftRoots,
		out IReadOnlyList<string> rightRoots,
		out string error
	) {
		ArgumentNullException.ThrowIfNull( args );
		var left = new List<string>();
		var right = new List<string>();
		bool json = false;
		bool compareSet = false;
		for ( int index = 0; index < args.Count; index++ ) {
			string argument = args[ index ];
			switch ( argument ) {
				case "--json":
					if ( json ) {
						leftRoots = Array.Empty<string>();
						rightRoots = Array.Empty<string>();
						error = "option '--json' may be specified only once";
						return false;
					}
					json = true;
					break;
				case "--compare-set":
					if ( compareSet ) {
						leftRoots = Array.Empty<string>();
						rightRoots = Array.Empty<string>();
						error = "option '--compare-set' may be specified only once";
						return false;
					}
					compareSet = true;
					break;
				case "--left-root":
				case "--right-root":
					if ( index + 1 >= args.Count || string.IsNullOrWhiteSpace( args[ index + 1 ] ) ) {
						leftRoots = Array.Empty<string>();
						rightRoots = Array.Empty<string>();
						error = $"option '{argument}' requires a non-empty directory";
						return false;
					}
					string root = args[ ++index ];
					if ( argument == "--left-root" ) {
						left.Add( root );
					} else {
						right.Add( root );
					}
					break;
				default:
					leftRoots = Array.Empty<string>();
					rightRoots = Array.Empty<string>();
					error = $"unsupported database-set comparison argument '{argument}'";
					return false;
			}
		}
		if ( !json ) {
			leftRoots = Array.Empty<string>();
			rightRoots = Array.Empty<string>();
			error = "option '--compare-set' requires '--json'";
			return false;
		}
		if ( !compareSet || left.Count == 0 || right.Count == 0 ) {
			leftRoots = Array.Empty<string>();
			rightRoots = Array.Empty<string>();
			error = "database-set comparison requires '--compare-set', at least one '--left-root', and at least one '--right-root'";
			return false;
		}
		leftRoots = Array.AsReadOnly( left.ToArray() );
		rightRoots = Array.AsReadOnly( right.ToArray() );
		error = string.Empty;
		return true;
	}

	private static ToeListingResult BuildListing(''',
)
replace_exact(
    "toe/src/Command.cs",
    '''\t\tif ( json ) {\n\t\t\tif ( allDatabases || showHeadings || sortByName ) {\n\t\t\t\toptions = ToeOptions.Empty;\n\t\t\t\terror = "options '-a', '-h', and '-s' cannot be combined with '--json'";\n\t\t\t\treturn false;\n\t\t\t}\n\t\t\tif ( directories.Count != 1 ) {\n\t\t\t\toptions = ToeOptions.Empty;\n\t\t\t\terror = "option '--json' requires exactly one explicit directory operand";\n\t\t\t\treturn false;\n\t\t\t}\n\t\t}''',
    '''\t\tif ( json ) {\n\t\t\tif ( allDatabases || showHeadings || sortByName ) {\n\t\t\t\toptions = ToeOptions.Empty;\n\t\t\t\terror = "options '-a', '-h', and '-s' cannot be combined with '--json'";\n\t\t\t\treturn false;\n\t\t\t}\n\t\t\tif ( directories.Count == 0 ) {\n\t\t\t\toptions = ToeOptions.Empty;\n\t\t\t\terror = "option '--json' requires at least one explicit directory operand";\n\t\t\t\treturn false;\n\t\t\t}\n\t\t}''',
)
replace_exact(
    "toe/src/Command.cs",
    '''\t\treturn $"Usage: {CommandName} [options] [directory ...]{Environment.NewLine}"\n\t\t\t+ $"       {CommandName} --json directory{Environment.NewLine}"''',
    '''\t\treturn $"Usage: {CommandName} [options] [directory ...]{Environment.NewLine}"\n\t\t\t+ $"       {CommandName} --json directory [directory ...]{Environment.NewLine}"\n\t\t\t+ $"       {CommandName} --json --compare-set --left-root directory [--left-root directory ...] --right-root directory [--right-root directory ...]{Environment.NewLine}"''',
)
replace_exact(
    "toe/src/Command.cs",
    '''\t\t\t+ "      --json      inspect exactly one explicit directory and emit its databaseCatalog document"''',
    '''\t\t\t+ "      --json      one directory emits frozen v1 databaseCatalog; multiple directories emit v2 databaseSet"''',
)
replace_exact(
    "toe/src/Command.cs",
    '''\t\t\t+ "JSON mode rejects listing presentation switches and writes one document followed by exactly one LF."''',
    '''\t\t\t+ "JSON mode rejects listing presentation switches and writes one document followed by exactly one LF."\n\t\t\t+ Environment.NewLine\n\t\t\t+ "Use --compare-set with repeated --left-root/--right-root operands for a v2 databaseSetComparison document."''',
)

# -----------------------------------------------------------------------------
# infocmp: repeatable --candidate-root drives DA05 database-set planning.
# -----------------------------------------------------------------------------
replace_exact(
    "infocmp/src/InfoCmpOptions.cs",
    '''\tprivate readonly IReadOnlyList<string> _terminalNames;''',
    '''\tprivate readonly IReadOnlyList<string> _terminalNames;\n\tprivate readonly IReadOnlyList<string> _candidateRoots;''',
)
replace_exact(
    "infocmp/src/InfoCmpOptions.cs",
    '''\t\tbool allCandidates,\n\t\tint maximumSelectedParentCount,''',
    '''\t\tbool allCandidates,\n\t\tIEnumerable<string> candidateRoots,\n\t\tint maximumSelectedParentCount,''',
)
replace_exact(
    "infocmp/src/InfoCmpOptions.cs",
    '''\t\tArgumentNullException.ThrowIfNull( terminalNames );''',
    '''\t\tArgumentNullException.ThrowIfNull( terminalNames );\n\t\tArgumentNullException.ThrowIfNull( candidateRoots );''',
)
replace_exact(
    "infocmp/src/InfoCmpOptions.cs",
    '''\t\tstring[] names = terminalNames.ToArray();''',
    '''\t\tstring[] candidateRootArray = candidateRoots.ToArray();\n\t\tforeach ( string root in candidateRootArray ) {\n\t\t\tArgumentException.ThrowIfNullOrWhiteSpace( root );\n\t\t}\n\t\tif ( candidateRootArray.Length != 0 && ( !planning || !allCandidates ) ) {\n\t\t\tthrow new ArgumentException(\n\t\t\t\t"Candidate roots require all-candidates relative-source planning.",\n\t\t\t\tnameof( candidateRoots )\n\t\t\t);\n\t\t}\n\n\t\tstring[] names = terminalNames.ToArray();''',
)
replace_exact(
    "infocmp/src/InfoCmpOptions.cs",
    '''\t\t_terminalNames = Array.AsReadOnly( names );''',
    '''\t\t_terminalNames = Array.AsReadOnly( names );\n\t\t_candidateRoots = Array.AsReadOnly( candidateRootArray );''',
)
replace_exact(
    "infocmp/src/InfoCmpOptions.cs",
    '''\tinternal IReadOnlyList<string> TerminalNames =>\n\t\t_terminalNames;''',
    '''\tinternal IReadOnlyList<string> TerminalNames =>\n\t\t_terminalNames;\n\n\tinternal IReadOnlyList<string> CandidateRoots =>\n\t\t_candidateRoots;''',
)
replace_exact(
    "infocmp/src/InfoCmpOptions.cs",
    '''\t\tvar terminalNames = new List<string>();''',
    '''\t\tvar terminalNames = new List<string>();\n\t\tvar candidateRoots = new List<string>();''',
)
# Insert long-option parsing immediately before --max-parents.
replace_exact(
    "infocmp/src/InfoCmpOptions.cs",
    '''\t\t\t\tcase "--max-parents":''',
    '''\t\t\t\tcase "--candidate-root":\n\t\t\t\t\tif ( !TryReadValue(\n\t\t\t\t\t\targs,\n\t\t\t\t\t\tref index,\n\t\t\t\t\t\targument,\n\t\t\t\t\t\tout string? candidateRoot,\n\t\t\t\t\t\tout string? candidateRootError\n\t\t\t\t\t) ) {\n\t\t\t\t\t\treturn InfoCmpOptionsParseResult.Failure( candidateRootError! );\n\t\t\t\t\t}\n\t\t\t\t\tcandidateRoots.Add( candidateRoot! );\n\t\t\t\t\tbreak;\n\n\t\t\t\tcase "--max-parents":''',
)
# Replace all-candidates validation block.
replace_exact(
    "infocmp/src/InfoCmpOptions.cs",
    '''\t\tif ( allCandidates ) {\n\t\t\tif ( !planning ) {\n\t\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t\t"option '--all-candidates' requires '--plan-use'"\n\t\t\t\t);\n\t\t\t}\n\t\t\tif ( comparisonDatabaseDirectory is null ) {\n\t\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t\t"option '--all-candidates' requires one explicit '-B' directory"\n\t\t\t\t);\n\t\t\t}\n\t\t\tif ( terminalNames.Count != 1 ) {\n\t\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t\t"option '--all-candidates' requires exactly one target terminal"\n\t\t\t\t);\n\t\t\t}\n\t\t}''',
    '''\t\tif ( candidateRoots.Count != 0 && ( !planning || !allCandidates ) ) {\n\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t"option '--candidate-root' requires '--plan-use --all-candidates'"\n\t\t\t);\n\t\t}\n\t\tif ( allCandidates ) {\n\t\t\tif ( !planning ) {\n\t\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t\t"option '--all-candidates' requires '--plan-use'"\n\t\t\t\t);\n\t\t\t}\n\t\t\tif ( comparisonDatabaseDirectory is not null && candidateRoots.Count != 0 ) {\n\t\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t\t"options '-B' and '--candidate-root' are mutually exclusive for all-candidates planning"\n\t\t\t\t);\n\t\t\t}\n\t\t\tif ( comparisonDatabaseDirectory is null && candidateRoots.Count == 0 ) {\n\t\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t\t"option '--all-candidates' requires one explicit '-B' directory or at least one '--candidate-root' directory"\n\t\t\t\t);\n\t\t\t}\n\t\t\tif ( terminalNames.Count != 1 ) {\n\t\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t\t"option '--all-candidates' requires exactly one target terminal"\n\t\t\t\t);\n\t\t\t}\n\t\t}''',
)
replace_exact(
    "infocmp/src/InfoCmpOptions.cs",
    '''\t\t\t\tallCandidates,\n\t\t\t\tmaximumSelectedParentCount,''',
    '''\t\t\t\tallCandidates,\n\t\t\t\tcandidateRoots,\n\t\t\t\tmaximumSelectedParentCount,''',
)

replace_exact(
    "infocmp/src/InfoCmpInspector.cs",
    '''\t\tTerminalDescriptionSourcePlan plan;''',
    '''\t\tTerminalDescriptionSourcePlan plan;\n\t\tTermInfoDatabaseSetSourcePlanningResult? databaseSetPlan = null;''',
)
replace_exact(
    "infocmp/src/InfoCmpInspector.cs",
    '''\t\t\tif ( options.AllCandidates ) {\n\t\t\t\tplan =\n\t\t\t\t\tTerminalDescriptionSourcePlanner.PlanFromDirectory(\n\t\t\t\t\t\ttarget.Description,\n\t\t\t\t\t\toptions.ComparisonDatabaseDirectory\n\t\t\t\t\t\t\t?? throw new InvalidOperationException(\n\t\t\t\t\t\t\t\t"All-candidates planning requires an explicit candidate directory."\n\t\t\t\t\t\t\t),\n\t\t\t\t\t\tplanningOptions,\n\t\t\t\t\t\tparserOptions: null,\n\t\t\t\t\t\tcancellationToken: cancellationToken\n\t\t\t\t\t);\n\t\t\t} else {''',
    '''\t\t\tif ( options.AllCandidates ) {\n\t\t\t\tif ( options.CandidateRoots.Count != 0 ) {\n\t\t\t\t\tdatabaseSetPlan =\n\t\t\t\t\t\tTerminalDescriptionSourcePlanner.PlanFromDirectories(\n\t\t\t\t\t\t\ttarget.Description,\n\t\t\t\t\t\t\toptions.CandidateRoots,\n\t\t\t\t\t\t\tplanningOptions,\n\t\t\t\t\t\t\tparserOptions: null,\n\t\t\t\t\t\t\tcancellationToken: cancellationToken\n\t\t\t\t\t\t);\n\t\t\t\t\tplan = databaseSetPlan.Plan;\n\t\t\t\t} else {\n\t\t\t\t\tplan =\n\t\t\t\t\t\tTerminalDescriptionSourcePlanner.PlanFromDirectory(\n\t\t\t\t\t\t\ttarget.Description,\n\t\t\t\t\t\t\toptions.ComparisonDatabaseDirectory\n\t\t\t\t\t\t\t\t?? throw new InvalidOperationException(\n\t\t\t\t\t\t\t\t\t"All-candidates planning requires an explicit candidate directory."\n\t\t\t\t\t\t\t\t),\n\t\t\t\t\t\t\tplanningOptions,\n\t\t\t\t\t\t\tparserOptions: null,\n\t\t\t\t\t\t\tcancellationToken: cancellationToken\n\t\t\t\t\t\t);\n\t\t\t\t}\n\t\t\t} else {''',
)
replace_exact(
    "infocmp/src/InfoCmpInspector.cs",
    '''\t\t\tstring rendered = options.Json\n\t\t\t\t? TermInfoJsonRenderer.Render(\n\t\t\t\t\tplan,\n\t\t\t\t\tnew TermInfoJsonRendererOptions(),\n\t\t\t\t\tcancellationToken\n\t\t\t\t) + "\\n"\n\t\t\t\t: plan.Source;''',
    '''\t\t\tstring rendered = options.Json\n\t\t\t\t? ( databaseSetPlan is null\n\t\t\t\t\t? TermInfoJsonRenderer.Render(\n\t\t\t\t\t\tplan,\n\t\t\t\t\t\tnew TermInfoJsonRendererOptions(),\n\t\t\t\t\t\tcancellationToken\n\t\t\t\t\t)\n\t\t\t\t\t: TermInfoJsonRenderer.Render(\n\t\t\t\t\t\tdatabaseSetPlan,\n\t\t\t\t\t\tnew TermInfoJsonRendererOptions(),\n\t\t\t\t\t\tcancellationToken\n\t\t\t\t\t) ) + "\\n"\n\t\t\t\t: plan.Source;''',
)

replace_exact(
    "infocmp/src/Command.cs",
    '''\t\t\t\t+ $"       {CommandName} --json --plan-use --all-candidates -B directory target{Environment.NewLine}"''',
    '''\t\t\t\t+ $"       {CommandName} --json --plan-use --all-candidates -B directory target{Environment.NewLine}"\n\t\t\t\t+ $"       {CommandName} --json --plan-use --all-candidates --candidate-root directory [--candidate-root directory ...] target{Environment.NewLine}"''',
)
replace_exact(
    "infocmp/src/Command.cs",
    '''\t\t\t\t+ $"      --all-candidates     use every canonical entry from the one explicit -B directory{Environment.NewLine}"''',
    '''\t\t\t\t+ $"      --all-candidates     use canonical entries from explicit candidate database roots{Environment.NewLine}"\n\t\t\t\t+ $"      --candidate-root dir   add one explicit ordered candidate database root; repeatable{Environment.NewLine}"''',
)
replace_exact(
    "infocmp/src/Command.cs",
    '''\t\t\t\t+ "All-candidates planning requires --plan-use, exactly one target, and one explicit "\n\t\t\t\t+ $"-B conventional directory; it never performs host discovery.{Environment.NewLine}"''',
    '''\t\t\t\t+ "All-candidates planning requires --plan-use, exactly one target, and either one legacy "\n\t\t\t\t+ $"-B directory or one or more --candidate-root directories; it never performs host discovery.{Environment.NewLine}"\n\t\t\t\t+ $"Legacy -B all-candidates JSON remains version 1; --candidate-root JSON emits the version-2 databaseSetPlan contract.{Environment.NewLine}"''',
)

# -----------------------------------------------------------------------------
# Packaging and version transition.
# -----------------------------------------------------------------------------
replace_exact(
    "Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj",
    '''\t\t<PackageReleaseNotes>1.10.0-Alpha-5 adds deterministic complete database-set candidate discovery, target-identity exclusion, semantic duplicate validation/collapse, selected-parent provenance mapping, and explicit roots/catalog composition over the unchanged frozen 1.8 planner while preserving DA01-DA04, frozen 1.9 JSON v1, lower-layer, synthesis, planner scoring, and command contracts.</PackageReleaseNotes>''',
    '''\t\t<PackageReleaseNotes>1.10.0-Alpha-6 adds the separate version-2 database automation JSON contract for databaseSet, databaseSetComparison, and databaseSetPlan documents plus thin toe/infocmp/router command composition, while preserving byte-compatible version-1 JSON invocations and all frozen DA01-DA05 engines.</PackageReleaseNotes>''',
)
replace_exact(
    "Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj",
    '''\t\t<None Include="..\\docs\\Icod.TermInfo.Inspection.schema.json" Link="Icod.TermInfo.Inspection.schema.json" Pack="true" PackagePath="docs\\" />''',
    '''\t\t<None Include="..\\docs\\Icod.TermInfo.Inspection.schema.json" Link="Icod.TermInfo.Inspection.schema.json" Pack="true" PackagePath="docs\\" />\n\t\t<None Include="..\\docs\\Icod.TermInfo.Inspection.schema.v2.json" Link="Icod.TermInfo.Inspection.schema.v2.json" Pack="true" PackagePath="docs\\" />''',
)
replace_exact(
    "tools/inspection-package-verifier/Program.cs",
    '''\t\t\t"docs/Icod.TermInfo.Inspection.schema.json",''',
    '''\t\t\t"docs/Icod.TermInfo.Inspection.schema.json",\n\t\t\t"docs/Icod.TermInfo.Inspection.schema.v2.json",''',
)
replace_exact(
    "Directory.Build.props",
    "<IcodTermInfoSuiteVersion>1.10.0-Alpha-5</IcodTermInfoSuiteVersion>",
    "<IcodTermInfoSuiteVersion>1.10.0-Alpha-6</IcodTermInfoSuiteVersion>",
)
current_version_files = [
    "tests/Icod.TermInfo.Tests/src/T45CompletionGateTests.cs",
    "tests/Icod.TermInfo.Termcap.Tests/src/TC08ContractTests.cs",
    "tests/Icod.TermInfo.Inspection.Tests/src/RS08ContractTests.cs",
    "tests/Icod.TermInfo.Inspection.Tests/src/RP08ReleaseClosureTests.cs",
    "tests/Icod.TermInfo.Tic.Tests/src/ReleaseClosureTests.cs",
    "tests/Icod.TermInfo.Tic.Tests/src/CommandTests.cs",
    "tests/Icod.TermInfo.InfoCmp.Tests/src/CommandTests.cs",
    "tests/Icod.TermInfo.Toe.Tests/src/CommandTests.cs",
    "tests/Icod.TermInfo.Router.Tests/src/ContractTests.cs",
    "tests/Icod.TermInfo.Router.Tests/src/CommandTests.cs",
]
for path_name in current_version_files:
    replace_all_required(path_name, "1.10.0-Alpha-5", "1.10.0-Alpha-6")
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/RP08ReleaseClosureTests.cs",
    '"DA05",',
    '"DA06",',
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/MI07ReleaseClosureTests.cs",
    '"DA05 - Multi-database candidate planning",',
    '"DA06 - Command and machine-readable automation composition",',
)
replace_exact(
    "Icod.TermInfo-1.10.0-Deterministic-Multi-Database-Inspection-Comparison-and-Planning-Automation-Roadmap.md",
    "**Status:** DA05 complete and frozen; DA06 not yet started",
    "**Status:** DA06 implementation complete; Staging validation pending",
)
replace_exact(
    "Icod.TermInfo-Post-1.0-Development-Roadmap.md",
    "**Current coordinated version:** `1.10.0-Alpha-5`",
    "**Current coordinated version:** `1.10.0-Alpha-6`",
)
replace_exact(
    "Icod.TermInfo-Post-1.0-Development-Roadmap.md",
    "**Current tranche:** DA05 - Multi-database candidate planning",
    "**Current tranche:** DA06 - Command and machine-readable automation composition",
)

# README section.
readme_path = Path("Icod.TermInfo.Inspection/README.md")
readme = readme_path.read_text(encoding="utf-8")
marker = "## 1.10 DA05 multi-database candidate planning\n"
if readme.count(marker) != 1:
    raise RuntimeError("Inspection README DA05 heading marker mismatch")
section = '''## 1.10 DA06 command and machine-readable automation composition\n\n`1.10.0-Alpha-6` adds a separate version-2 database automation contract with\n`databaseSet`, `databaseSetComparison`, and `databaseSetPlan` document kinds. The\nfrozen version-1 identifier, schema file, four document kinds, renderer output,\nand legacy `toe`/`infocmp` JSON invocations remain unchanged. `toe --json` emits\nv1 for one directory and v2 for multiple explicit directories; explicit set\ncomparison is available through `--compare-set` with repeated left/right roots.\n`infocmp --candidate-root` composes DA05 multi-database planning, while legacy\n`--all-candidates -B directory` remains the version-1 path.\n\nSee `docs/1.10.0-DA06-COMMAND-AND-MACHINE-READABLE-AUTOMATION-COMPOSITION.md`.\n\n'''
readme_path.write_text(readme.replace(marker, section + marker, 1), encoding="utf-8", newline="\n")

# -----------------------------------------------------------------------------
# DA06 implementation record.
# -----------------------------------------------------------------------------
write_new(
    "docs/1.10.0-DA06-COMMAND-AND-MACHINE-READABLE-AUTOMATION-COMPOSITION.md",
    '''# Icod.TermInfo 1.10.0 DA06 — Command and Machine-Readable Automation Composition\n\n**Development version:** `1.10.0-Alpha-6`  \n**Tranche:** DA06  \n**Published baseline:** `1.9.0`  \n**DA05 baseline:** `1.10.0-Alpha-5`  \n**Primary package:** `Icod.TermInfo.Inspection`  \n**Commands:** `toe`, `infocmp`, `icod-terminfo`  \n**Status:** implementation complete; PR Staging validation pending  \n\n## 1. Versioned JSON boundary\n\nThe frozen version-1 contract remains exactly identified by:\n\n```text\nurn:icod:terminfo:inspection:json:1\nschemaVersion = 1\n```\n\nDA06 introduces a separate additive database-automation contract:\n\n```text\nurn:icod:terminfo:inspection:json:2\nschemaVersion = 2\n```\n\nVersion 2 has exactly three DA06 document kinds:\n\n```text\ndatabaseSet\ndatabaseSetComparison\ndatabaseSetPlan\n```\n\nThe version-1 schema file `docs/Icod.TermInfo.Inspection.schema.json` is not\nmodified. Version 2 is published separately as\n`docs/Icod.TermInfo.Inspection.schema.v2.json` and both files are packaged.\n\n## 2. databaseSet\n\n`TermInfoJsonRenderer.Render(TermInfoDatabaseSet)` emits caller-order database\nevidence, ordinal canonical identities, DA02 lookup/winner/blocking evidence,\nordered physical occurrences, constituent issues, and DA03 semantic duplicate,\nshadow, and alias evidence. The semantic-analysis section records its comparison\nand alias-scan work counts plus the active default alias bound.\n\nNo path re-normalization occurs during rendering. Paths are emitted exactly from\nthe frozen catalog/occurrence evidence.\n\n## 3. databaseSetComparison\n\n`TermInfoJsonRenderer.Render(TermInfoDatabaseSetComparisonResult)` emits the DA04\nconclusion flags, work counts, and the already-frozen DA04 difference sequence.\nEach difference retains typed left/right database, occurrence, issue, lookup, and\nstructured semantic-comparison evidence when present. DA06 does not re-sort or\nreclassify differences.\n\n## 4. databaseSetPlan\n\n`TermInfoJsonRenderer.Render(TermInfoDatabaseSetSourcePlanningResult)` emits the\nordered database roots, complete DA05 candidate provenance, exact frozen planner\nselected indices, selected candidate evidence, generated source, score, evaluated\nplan count, and exhaustive status. No planner decision is reconstructed in the\nrenderer.\n\n## 5. toe command composition\n\nThe additive JSON rules are:\n\n```text\ntoe --json directory\n    -> frozen version-1 databaseCatalog\n\ntoe --json directory [directory ...]\n    -> when two or more directories are supplied: version-2 databaseSet\n\ntoe --json --compare-set \\\n    --left-root directory [--left-root directory ...] \\\n    --right-root directory [--right-root directory ...]\n    -> version-2 databaseSetComparison\n```\n\nOne-directory behavior intentionally retains the exact 1.9 route. Multi-root\nrendering calls `TermInfoDatabaseInspector.InspectSet`; comparison calls the DA04\n`TermInfoDatabaseSetComparer`. Human listing behavior is unchanged.\n\n## 6. infocmp command composition\n\nLegacy all-candidates planning remains:\n\n```text\ninfocmp --json --plan-use --all-candidates -B directory target\n    -> frozen version-1 sourcePlan\n```\n\nDA06 adds repeatable explicit roots:\n\n```text\ninfocmp --json --plan-use --all-candidates \\\n    --candidate-root directory [--candidate-root directory ...] target\n    -> version-2 databaseSetPlan\n```\n\n`--candidate-root` is valid only with `--plan-use --all-candidates` and is\nmutually exclusive with `-B` in that mode. It calls DA05\n`PlanFromDirectories(...)`; the command layer performs no candidate discovery or\nplanning itself. Human output remains the selected frozen plan source. Target\nacquisition through `-A` is unchanged.\n\n## 7. Router composition\n\n`icod-terminfo` requires no new planning or JSON implementation. It continues to\nforward subcommand argv unchanged; DA06 tests exercise the new `toe` and\n`infocmp` forms through the router to freeze that composition boundary.\n\n## 8. Frozen 1.9 compatibility\n\nThe following invocations remain on their exact version-1 renderer paths:\n\n```text\ninfocmp --json target\ninfocmp --json -d left right\ninfocmp --json --plan-use target candidate [candidate ...]\ninfocmp --json --plan-use --all-candidates -B directory target\ntoe --json directory\n```\n\nTheir schema identifier, schema version, document kinds, field order, escaping,\ncompact representation, output bound semantics, and one-document-plus-one-LF\ncommand framing are unchanged.\n\n## 9. Bounds and framing\n\nVersion-2 rendering reuses `TermInfoJsonRendererOptions.MaximumOutputByteCount`.\nDatabase-set construction reuses DA01 bounds; semantic alias scanning reuses the\nDA03 default bound; comparison reuses DA04 work; planning reuses DA05/1.8 bounds.\nCommands add exactly one LF after successful JSON documents and write no partial\nJSON on operational failure.\n\n**DA06 gate:** the DA01–DA05 reusable engines are available through stable command\nand machine-readable contracts while the frozen 1.9 JSON automation paths remain\nbyte-compatible.\n''',
)

# -----------------------------------------------------------------------------
# Tests — renderer/versioning, toe, infocmp, router, package smoke.
# -----------------------------------------------------------------------------
write_new(
    "tests/Icod.TermInfo.Inspection.Tests/src/DA06DatabaseAutomationJsonTests.cs",
    r'''using System.Globalization;
using System.Text;
using System.Text.Json;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class DA06DatabaseAutomationJsonTests {
	[Fact]
	public void VersionOneIdentityAndSchemaFileRemainFrozen() {
		Assert.Equal( "urn:icod:terminfo:inspection:json:1", TermInfoJsonRenderer.SchemaIdentifier );
		Assert.Equal( 1, TermInfoJsonRenderer.SchemaVersion );
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:2",
			TermInfoJsonRenderer.DatabaseAutomationSchemaIdentifier
		);
		Assert.Equal( 2, TermInfoJsonRenderer.DatabaseAutomationSchemaVersion );

		string root = FindRepositoryRoot();
		using JsonDocument v1 = JsonDocument.Parse(
			File.ReadAllText(
				Path.Combine( root, "docs", "Icod.TermInfo.Inspection.schema.json" )
			)
		);
		using JsonDocument v2 = JsonDocument.Parse(
			File.ReadAllText(
				Path.Combine( root, "docs", "Icod.TermInfo.Inspection.schema.v2.json" )
			)
		);
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:1",
			v1.RootElement.GetProperty( "$id" ).GetString()
		);
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:2",
			v2.RootElement.GetProperty( "$id" ).GetString()
		);
	}

	[Fact]
	public void DatabaseSetJsonPreservesRootIdentityOccurrenceAndSemanticOrdering() {
		string firstRoot = AbsolutePath( "set-first" );
		string secondRoot = AbsolutePath( "set-second" );
		TermInfoDatabaseSet set = TermInfoDatabaseInspector.CreateSet(
			[
				CreateCatalog( firstRoot, CreateTerminal( "zeta", 80, "shared" ) ),
				CreateCatalog(
					secondRoot,
					CreateTerminal( "alpha", 100 ),
					CreateTerminal( "zeta", 132, "shared" )
				),
			]
		);

		string json = TermInfoJsonRenderer.Render( set );
		using JsonDocument document = JsonDocument.Parse( json );
		JsonElement root = document.RootElement;
		JsonElement data = root.GetProperty( "data" );
		Assert.Equal( "urn:icod:terminfo:inspection:json:2", root.GetProperty( "schema" ).GetString() );
		Assert.Equal( 2, root.GetProperty( "schemaVersion" ).GetInt32() );
		Assert.Equal( "databaseSet", root.GetProperty( "documentKind" ).GetString() );
		Assert.Equal(
			new[] { firstRoot, secondRoot },
			data.GetProperty( "databases" ).EnumerateArray()
				.Select( element => element.GetProperty( "root" ).GetString() )
				.ToArray()
		);
		Assert.Equal(
			new[] { "alpha", "zeta" },
			data.GetProperty( "identities" ).EnumerateArray()
				.Select( element => element.GetProperty( "name" ).GetString() )
				.ToArray()
		);
		JsonElement zeta = data.GetProperty( "identities" )[ 1 ];
		Assert.Equal( "winnerKnown", zeta.GetProperty( "lookupStatus" ).GetString() );
		Assert.Equal( 0, zeta.GetProperty( "winner" ).GetProperty( "databaseIndex" ).GetInt32() );
		Assert.Equal(
			new[] { 0, 1 },
			zeta.GetProperty( "occurrences" ).EnumerateArray()
				.Select( element => element.GetProperty( "databaseIndex" ).GetInt32() )
				.ToArray()
		);
		JsonElement repeated = Assert.Single(
			data.GetProperty( "semanticAnalysis" ).GetProperty( "repeatedIdentities" ).EnumerateArray()
		);
		Assert.Equal( "zeta", repeated.GetProperty( "name" ).GetString() );
		Assert.Equal( "semanticallyDifferent", repeated.GetProperty( "relationship" ).GetString() );
		JsonElement shadow = Assert.Single( repeated.GetProperty( "shadows" ).EnumerateArray() );
		Assert.Equal( 1, shadow.GetProperty( "occurrence" ).GetProperty( "databaseIndex" ).GetInt32() );
		Assert.False( shadow.GetProperty( "comparison" ).GetProperty( "areEqual" ).GetBoolean() );
	}

	[Fact]
	public void DatabaseSetComparisonJsonPreservesFrozenDifferenceOrder() {
		string root = AbsolutePath( "comparison" );
		TermInfoDatabaseSet left = TermInfoDatabaseInspector.CreateSet(
			[ CreateCatalog( root, CreateTerminal( "alpha", 80 ), CreateTerminal( "zeta", 80 ) ) ]
		);
		TermInfoDatabaseSet right = TermInfoDatabaseInspector.CreateSet(
			[ CreateCatalog( root, CreateTerminal( "beta", 80 ), CreateTerminal( "zeta", 132 ) ) ]
		);
		TermInfoDatabaseSetComparisonResult comparison =
			TermInfoDatabaseSetComparer.Compare( left, right );

		using JsonDocument document = JsonDocument.Parse( TermInfoJsonRenderer.Render( comparison ) );
		JsonElement rootElement = document.RootElement;
		Assert.Equal( "databaseSetComparison", rootElement.GetProperty( "documentKind" ).GetString() );
		string[] kinds = rootElement.GetProperty( "data" ).GetProperty( "differences" )
			.EnumerateArray()
			.Select( element => element.GetProperty( "kind" ).GetString()! )
			.ToArray();
		Assert.Equal(
			comparison.Differences.Select( difference => KindName( difference.Kind ) ).ToArray(),
			kinds
		);
	}

	[Fact]
	public void DatabaseSetPlanJsonMapsFrozenSelectedIndicesToProvenance() {
		string firstRoot = AbsolutePath( "plan-first" );
		string secondRoot = AbsolutePath( "plan-second" );
		TerminalDescription target = CreateTarget();
		TermInfoDatabaseSet set = TermInfoDatabaseInspector.CreateSet(
			[
				CreateCatalog( firstRoot, CreateParent( "parent-a", true, false ) ),
				CreateCatalog( secondRoot, CreateParent( "parent-b", false, true ) ),
			]
		);
		TermInfoDatabaseSetSourcePlanningResult plan =
			TerminalDescriptionSourcePlanner.PlanFromDatabaseSet( target, set );

		using JsonDocument document = JsonDocument.Parse( TermInfoJsonRenderer.Render( plan ) );
		JsonElement data = document.RootElement.GetProperty( "data" );
		Assert.Equal( "databaseSetPlan", document.RootElement.GetProperty( "documentKind" ).GetString() );
		Assert.Equal(
			plan.Plan.Score.SelectedCandidateIndices.ToArray(),
			data.GetProperty( "selectedCandidateIndices" ).EnumerateArray()
				.Select( element => element.GetInt32() )
				.ToArray()
		);
		Assert.Equal(
			plan.SelectedCandidates.Select( candidate => candidate.DatabaseIndex ).ToArray(),
			data.GetProperty( "selectedCandidates" ).EnumerateArray()
				.Select( element => element.GetProperty( "databaseIndex" ).GetInt32() )
				.ToArray()
		);
	}

	[Fact]
	public void VersionTwoRenderingIsCultureDeterministicBoundedAndCancelable() {
		TermInfoDatabaseSet set = TermInfoDatabaseInspector.CreateSet(
			[ CreateCatalog( AbsolutePath( "culture" ), CreateTerminal( "I", 80, "İ" ) ) ]
		);
		string invariant = TermInfoJsonRenderer.Render( set );
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "tr-TR" );
			Assert.Equal( invariant, TermInfoJsonRenderer.Render( set ) );
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}

		int exactBytes = Encoding.UTF8.GetByteCount( invariant );
		Assert.Equal(
			invariant,
			TermInfoJsonRenderer.Render(
				set,
				new TermInfoJsonRendererOptions( exactBytes )
			)
		);
		Assert.Throws<InvalidOperationException>(
			() => TermInfoJsonRenderer.Render(
				set,
				new TermInfoJsonRendererOptions( exactBytes - 1 )
			)
		);
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>(
			() => TermInfoJsonRenderer.Render(
				set,
				new TermInfoJsonRendererOptions(),
				cancellation.Token
			)
		);
	}

	[Fact]
	public void Da06AddsRendererOverloadsButNoNewPublicTypes() {
		Type[] exportedTypes = typeof( TermInfoJsonRenderer ).Assembly.GetExportedTypes();
		Assert.InRange( exportedTypes.Length, 51, 51 );
	}

	private static string KindName( TermInfoDatabaseSetDifferenceKind kind ) =>
		kind switch {
			TermInfoDatabaseSetDifferenceKind.RootTopology => "rootTopology",
			TermInfoDatabaseSetDifferenceKind.Completeness => "completeness",
			TermInfoDatabaseSetDifferenceKind.Issue => "issue",
			TermInfoDatabaseSetDifferenceKind.OnlyInLeft => "onlyInLeft",
			TermInfoDatabaseSetDifferenceKind.OnlyInRight => "onlyInRight",
			TermInfoDatabaseSetDifferenceKind.EffectiveSemantic => "effectiveSemantic",
			TermInfoDatabaseSetDifferenceKind.EffectiveProvenance => "effectiveProvenance",
			TermInfoDatabaseSetDifferenceKind.AliasOwnership => "aliasOwnership",
			TermInfoDatabaseSetDifferenceKind.ShadowSet => "shadowSet",
			TermInfoDatabaseSetDifferenceKind.Indeterminate => "indeterminate",
			_ => throw new ArgumentOutOfRangeException( nameof( kind ) ),
		};

	private static TermInfoDatabaseCatalog CreateCatalog(
		string root,
		params TerminalDescription[] terminals
	) {
		TermInfoDatabaseCatalogEntry[] entries = terminals
			.Select(
				( terminal, index ) => new TermInfoDatabaseCatalogEntry(
					Path.Combine( root, index.ToString( CultureInfo.InvariantCulture ) ),
					terminal
				)
			)
			.OrderBy( entry => entry.Name, StringComparer.Ordinal )
			.ThenBy( entry => entry.Path, StringComparer.Ordinal )
			.ToArray();
		return new TermInfoDatabaseCatalog(
			root,
			TermInfoDatabaseCatalogKind.ConventionalDirectory,
			entries,
			Array.Empty<TermInfoDatabaseCatalogIssue>(),
			entries.GroupBy( entry => entry.Name, StringComparer.Ordinal )
				.Where( group => group.Count() > 1 )
				.Select( group => group.Key )
				.OrderBy( name => name, StringComparer.Ordinal )
				.ToArray()
		);
	}

	private static TerminalDescription CreateTerminal(
		string name,
		int columns,
		params string[] aliases
	) {
		TerminalDescriptionBuilder builder = new TerminalDescriptionBuilder( name )
			.SetDescription( $"DA06 {name}" )
			.SetNumber( NumericCapability.Columns, columns );
		foreach ( string alias in aliases ) {
			builder.AddAlias( alias );
		}
		return builder.Build();
	}

	private static TerminalDescription CreateTarget() =>
		new TerminalDescriptionBuilder( "da06-target" )
			.SetDescription( "DA06 target" )
			.SetBoolean( BooleanCapability.AutoRightMargin )
			.SetNumber( NumericCapability.Columns, 80 )
			.Build();

	private static TerminalDescription CreateParent(
		string name,
		bool margin,
		bool columns
	) {
		TerminalDescriptionBuilder builder = new TerminalDescriptionBuilder( name )
			.SetDescription( $"DA06 {name}" );
		if ( margin ) {
			builder.SetBoolean( BooleanCapability.AutoRightMargin );
		}
		if ( columns ) {
			builder.SetNumber( NumericCapability.Columns, 80 );
		}
		return builder.Build();
	}

	private static string AbsolutePath( string suffix ) =>
		Path.Combine( Path.GetTempPath(), $"icod-terminfo-da06-{suffix}" );

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( AppContext.BaseDirectory );
		while ( current is not null ) {
			if ( File.Exists( Path.Combine( current.FullName, "Icod.TermInfo.sln" ) ) ) {
				return current.FullName;
			}
			current = current.Parent;
		}
		throw new DirectoryNotFoundException( "Could not locate repository root." );
	}
}
''',
)

write_new(
    "tests/Icod.TermInfo.Toe.Tests/src/DA06DatabaseAutomationCommandTests.cs",
    r'''using System.Text;
using System.Text.Json;
using Icod.CommandFramework.Diagnostics;
using Xunit;

namespace Icod.TermInfo.Toe.Tests;

public sealed class DA06DatabaseAutomationCommandTests {
	[Fact]
	public async Task OneRootJsonRemainsVersionOneAndMultipleRootsUseVersionTwo() {
		string first = CreateTemporaryDirectory();
		string second = CreateTemporaryDirectory();
		try {
			CommandResult single = await RunAsync( "--json", first );
			CommandResult multiple = await RunAsync( "--json", first, second );

			Assert.Equal( CommandExitCodes.Success, single.Status );
			Assert.Equal( CommandExitCodes.Success, multiple.Status );
			using JsonDocument v1 = JsonDocument.Parse( single.Stdout );
			using JsonDocument v2 = JsonDocument.Parse( multiple.Stdout );
			Assert.Equal( 1, v1.RootElement.GetProperty( "schemaVersion" ).GetInt32() );
			Assert.Equal( "databaseCatalog", v1.RootElement.GetProperty( "documentKind" ).GetString() );
			Assert.Equal( 2, v2.RootElement.GetProperty( "schemaVersion" ).GetInt32() );
			Assert.Equal( "databaseSet", v2.RootElement.GetProperty( "documentKind" ).GetString() );
			Assert.Equal( new[] { Path.GetFullPath( first ), Path.GetFullPath( second ) },
				v2.RootElement.GetProperty( "data" ).GetProperty( "databases" ).EnumerateArray()
					.Select( element => element.GetProperty( "root" ).GetString() )
					.ToArray() );
			Assert.EndsWith( "\n", single.Stdout, StringComparison.Ordinal );
			Assert.DoesNotEndWith( "\n\n", single.Stdout, StringComparison.Ordinal );
			Assert.EndsWith( "\n", multiple.Stdout, StringComparison.Ordinal );
			Assert.DoesNotEndWith( "\n\n", multiple.Stdout, StringComparison.Ordinal );
			Assert.Equal( string.Empty, single.Stderr );
			Assert.Equal( string.Empty, multiple.Stderr );
		} finally {
			DeleteTemporaryDirectory( first );
			DeleteTemporaryDirectory( second );
		}
	}

	[Fact]
	public async Task ExplicitSetComparisonEmitsVersionTwoComparison() {
		string left = CreateTemporaryDirectory();
		string right = CreateTemporaryDirectory();
		try {
			CommandResult result = await RunAsync(
				"--json",
				"--compare-set",
				"--left-root",
				left,
				"--right-root",
				right
			);
			Assert.Equal( CommandExitCodes.Success, result.Status );
			using JsonDocument document = JsonDocument.Parse( result.Stdout );
			Assert.Equal( 2, document.RootElement.GetProperty( "schemaVersion" ).GetInt32() );
			Assert.Equal( "databaseSetComparison", document.RootElement.GetProperty( "documentKind" ).GetString() );
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteTemporaryDirectory( left );
			DeleteTemporaryDirectory( right );
		}
	}

	[Theory]
	[InlineData( "--compare-set", "--left-root", "left", "--right-root", "right" )]
	[InlineData( "--json", "--compare-set", "--left-root", "left" )]
	public async Task InvalidSetComparisonFormsAreUsageErrors( params string[] args ) {
		CommandResult result = await RunAsync( args );
		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Equal( string.Empty, result.Stdout );
		Assert.NotEqual( string.Empty, result.Stderr );
	}

	private static async Task<CommandResult> RunAsync( params string[] args ) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		int status = await Command.RunAsync( args, stdin, stdout, stderr );
		return new CommandResult(
			status,
			Encoding.UTF8.GetString( stdout.ToArray() ),
			Encoding.UTF8.GetString( stderr.ToArray() )
		);
	}

	private static string CreateTemporaryDirectory() {
		string path = Path.Combine(
			Path.GetTempPath(),
			$"icod-terminfo-toe-da06-{Guid.NewGuid():N}"
		);
		Directory.CreateDirectory( path );
		return path;
	}

	private static void DeleteTemporaryDirectory( string path ) {
		try {
			Directory.Delete( path, recursive: true );
		} catch ( IOException ) {
		} catch ( UnauthorizedAccessException ) {
		}
	}

	private sealed record CommandResult( int Status, string Stdout, string Stderr );
}
''',
)

write_new(
    "tests/Icod.TermInfo.InfoCmp.Tests/src/DA06MultiDatabasePlanningAutomationTests.cs",
    r'''using System.Text;
using System.Text.Json;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo.Compiler;
using Xunit;

namespace Icod.TermInfo.InfoCmp.Tests;

public sealed class DA06MultiDatabasePlanningAutomationTests {
	[Fact]
	public async Task CandidateRootsEmitVersionTwoPlanWhileLegacyBRemainsVersionOne() {
		string targetRoot = CreateTemporaryDirectory();
		string first = CreateTemporaryDirectory();
		string second = CreateTemporaryDirectory();
		try {
			TerminalDescription target = new TerminalDescriptionBuilder( "da06-target" )
				.SetDescription( "DA06 target" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
			TerminalDescription firstParent = new TerminalDescriptionBuilder( "da06-parent-a" )
				.SetDescription( "DA06 parent a" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.Build();
			TerminalDescription secondParent = new TerminalDescriptionBuilder( "da06-parent-b" )
				.SetDescription( "DA06 parent b" )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
			CompiledTermInfoDatabaseWriter.Write( targetRoot, target );
			CompiledTermInfoDatabaseWriter.Write( first, firstParent );
			CompiledTermInfoDatabaseWriter.Write( second, secondParent );

			CommandResult v2 = await RunAsync(
				"--json", "--plan-use", "--all-candidates",
				"-A", targetRoot,
				"--candidate-root", first,
				"--candidate-root", second,
				target.Name
			);
			CommandResult legacy = await RunAsync(
				"--json", "--plan-use", "--all-candidates",
				"-A", targetRoot,
				"-B", first,
				target.Name
			);

			Assert.Equal( CommandExitCodes.Success, v2.Status );
			Assert.Equal( CommandExitCodes.Success, legacy.Status );
			using JsonDocument v2Document = JsonDocument.Parse( v2.Stdout );
			using JsonDocument legacyDocument = JsonDocument.Parse( legacy.Stdout );
			Assert.Equal( 2, v2Document.RootElement.GetProperty( "schemaVersion" ).GetInt32() );
			Assert.Equal( "databaseSetPlan", v2Document.RootElement.GetProperty( "documentKind" ).GetString() );
			Assert.Equal(
				new[] { Path.GetFullPath( first ), Path.GetFullPath( second ) },
				v2Document.RootElement.GetProperty( "data" ).GetProperty( "databases" )
					.EnumerateArray()
					.Select( element => element.GetProperty( "root" ).GetString() )
					.ToArray()
			);
			Assert.Equal( 1, legacyDocument.RootElement.GetProperty( "schemaVersion" ).GetInt32() );
			Assert.Equal( "sourcePlan", legacyDocument.RootElement.GetProperty( "documentKind" ).GetString() );
			Assert.Equal( string.Empty, v2.Stderr );
			Assert.Equal( string.Empty, legacy.Stderr );
		} finally {
			DeleteTemporaryDirectory( targetRoot );
			DeleteTemporaryDirectory( first );
			DeleteTemporaryDirectory( second );
		}
	}

	[Fact]
	public async Task CandidateRootHumanModeWritesOnlyFrozenSelectedSource() {
		string targetRoot = CreateTemporaryDirectory();
		string candidateRoot = CreateTemporaryDirectory();
		try {
			TerminalDescription target = new TerminalDescriptionBuilder( "da06-human-target" )
				.SetDescription( "DA06 human target" )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
			TerminalDescription parent = new TerminalDescriptionBuilder( "da06-human-parent" )
				.SetDescription( "DA06 human parent" )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
			CompiledTermInfoDatabaseWriter.Write( targetRoot, target );
			CompiledTermInfoDatabaseWriter.Write( candidateRoot, parent );

			CommandResult result = await RunAsync(
				"--plan-use", "--all-candidates",
				"-A", targetRoot,
				"--candidate-root", candidateRoot,
				target.Name
			);
			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Contains( "da06-human-target", result.Stdout, StringComparison.Ordinal );
			Assert.DoesNotContain( "schemaVersion", result.Stdout, StringComparison.Ordinal );
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteTemporaryDirectory( targetRoot );
			DeleteTemporaryDirectory( candidateRoot );
		}
	}

	[Fact]
	public async Task CandidateRootRejectsAmbiguousOrUnboundForms() {
		foreach ( string[] args in new[] {
			new[] { "--candidate-root", "one", "target" },
			[ "--plan-use", "--candidate-root", "one", "target", "parent" ],
			[ "--plan-use", "--all-candidates", "-B", "legacy", "--candidate-root", "one", "target" ],
		} ) {
			CommandResult result = await RunAsync( args );
			Assert.Equal( CommandExitCodes.UsageError, result.Status );
			Assert.Equal( string.Empty, result.Stdout );
			Assert.NotEqual( string.Empty, result.Stderr );
		}
	}

	private static async Task<CommandResult> RunAsync( params string[] args ) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		int status = await Command.RunAsync( args, stdin, stdout, stderr );
		return new CommandResult(
			status,
			Encoding.UTF8.GetString( stdout.ToArray() ),
			Encoding.UTF8.GetString( stderr.ToArray() )
		);
	}

	private static string CreateTemporaryDirectory() {
		string path = Path.Combine(
			Path.GetTempPath(),
			$"icod-terminfo-infocmp-da06-{Guid.NewGuid():N}"
		);
		Directory.CreateDirectory( path );
		return path;
	}

	private static void DeleteTemporaryDirectory( string path ) {
		try {
			Directory.Delete( path, recursive: true );
		} catch ( IOException ) {
		} catch ( UnauthorizedAccessException ) {
		}
	}

	private sealed record CommandResult( int Status, string Stdout, string Stderr );
}
''',
)

# Router: lightweight help contract proves it exposes updated subcommand surfaces.
write_new(
    "tests/Icod.TermInfo.Router.Tests/src/DA06AutomationRoutingTests.cs",
    r'''using System.Text;
using Xunit;

namespace Icod.TermInfo.Router.Tests;

public sealed class DA06AutomationRoutingTests {
	[Theory]
	[InlineData( "toe", "--compare-set" )]
	[InlineData( "infocmp", "--candidate-root" )]
	public async Task RouterForwardsDa06HelpSurface(
		string commandName,
		string expectedOption
	) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		int status = await Command.RunAsync(
			[ commandName, "--help" ],
			stdin,
			stdout,
			stderr
		);
		Assert.Equal( 0, status );
		Assert.Contains(
			expectedOption,
			Encoding.UTF8.GetString( stdout.ToArray() ),
			StringComparison.Ordinal
		);
		Assert.Empty( stderr.ToArray() );
	}
}
''',
)

# Package smoke: constants and empty database-set v2 render, while type count stays 51.
replace_exact(
    "tools/inspection-package-smoke/Program.cs",
    '''\t"The MI01 JSON renderer contract did not retain its reviewed identity and bounds."\n);''',
    '''\t"The MI01 JSON renderer contract did not retain its reviewed identity and bounds."\n);\nRequire(\n\tTermInfoJsonRenderer.DatabaseAutomationSchemaIdentifier\n\t\t== "urn:icod:terminfo:inspection:json:2"\n\t\t&& TermInfoJsonRenderer.DatabaseAutomationSchemaVersion == 2,\n\t"The DA06 additive database automation JSON identity is unavailable."\n);\nTermInfoDatabaseSet emptyDatabaseSet =\n\tTermInfoDatabaseInspector.CreateSet( Array.Empty<TermInfoDatabaseCatalog>() );\nusing JsonDocument databaseSetJsonDocument = JsonDocument.Parse(\n\tTermInfoJsonRenderer.Render( emptyDatabaseSet )\n);\nRequire(\n\tdatabaseSetJsonDocument.RootElement.GetProperty( "schemaVersion" ).GetInt32() == 2\n\t\t&& databaseSetJsonDocument.RootElement.GetProperty( "documentKind" ).GetString()\n\t\t\t== "databaseSet",\n\t"The DA06 package renderer did not emit the additive databaseSet document."\n);''',
)
