namespace Icod.TermInfo.Inspection;

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
			new TerminalDescriptionSourcePlanningOptions(),
			options,
			cancellationToken
		);
	}

	/// <summary>
	/// Renders a database-set-backed source plan while retaining the exact frozen
	/// 1.8 planning bounds used to produce the result.
	/// </summary>
	public static string Render(
		TermInfoDatabaseSetSourcePlanningResult planningResult,
		TerminalDescriptionSourcePlanningOptions planningOptions,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( planningResult );
		ArgumentNullException.ThrowIfNull( planningOptions );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		return RenderDatabaseSetPlanV2(
			planningResult,
			planningOptions,
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
		TerminalDescriptionSourcePlanningOptions planningOptions,
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
			writer.WriteStartObject( "planningBounds" );
			writer.WriteNumber( "maximumCandidateCount", planningOptions.MaximumCandidateCount );
			writer.WriteNumber(
				"maximumSelectedParentCount",
				planningOptions.MaximumSelectedParentCount
			);
			writer.WriteNumber(
				"maximumEvaluatedPlanCount",
				planningOptions.MaximumEvaluatedPlanCount
			);
			writer.WriteNumber(
				"maximumGeneratedSourceLength",
				planningOptions.MaximumGeneratedSourceLength
			);
			writer.WriteBoolean(
				"allowNonExhaustiveResult",
				planningOptions.AllowNonExhaustiveResult
			);
			writer.WriteEndObject();
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
