namespace Icod.TermInfo.Inspection;

public sealed partial class TermInfoDatabaseSet {
	/// <summary>
	/// Analyzes repeated canonical identities and alias collisions using frozen DA02
	/// precedence and <see cref="TerminalDescriptionComparer"/> semantics.
	/// </summary>
	/// <param name="options">
	/// Optional deterministic semantic-analysis resource bounds.
	/// </param>
	/// <param name="cancellationToken">
	/// A token observed at deterministic identity, shadow, occurrence, and alias
	/// boundaries.
	/// </param>
	/// <returns>Immutable deterministic semantic analysis.</returns>
	public TermInfoDatabaseSetSemanticAnalysis AnalyzeSemantics(
		TermInfoDatabaseSetSemanticAnalysisOptions? options = null,
		CancellationToken cancellationToken = default
	) {
		cancellationToken.ThrowIfCancellationRequested();
		TermInfoDatabaseSetSemanticAnalysisOptions effectiveOptions =
			options ?? new TermInfoDatabaseSetSemanticAnalysisOptions();

		List<TermInfoDatabaseSetIdentityAnalysis> repeatedIdentities = [];
		Dictionary<string, TermInfoDatabaseSetIdentityAnalysis> identityAnalyses =
			new( StringComparer.Ordinal );
		int semanticComparisonCount = 0;

		foreach ( TermInfoDatabaseSetIdentity identity in Identities ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( identity.Occurrences.Count < 2 ) {
				continue;
			}

			TermInfoDatabaseSetLookupResult lookup =
				LookupCanonicalName( identity.Name );
			List<TermInfoDatabaseSetShadowAnalysis> shadows = [];
			TermInfoDatabaseSetSemanticRelationship relationship;

			if ( lookup.Status == TermInfoDatabaseSetLookupStatus.WinnerKnown ) {
				TermInfoDatabaseSetOccurrence winner = lookup.Winner!;
				bool hasDifference = false;
				foreach (
					TermInfoDatabaseSetOccurrence shadow
					in lookup.ShadowedOccurrences
				) {
					cancellationToken.ThrowIfCancellationRequested();
					semanticComparisonCount = checked( semanticComparisonCount + 1 );
					TermInfoComparisonResult comparison =
						TerminalDescriptionComparer.Compare(
							winner.Entry.Terminal,
							shadow.Entry.Terminal
						);
					TermInfoDatabaseSetSemanticRelationship shadowRelationship =
						comparison.AreEqual
							? TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual
							: TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent;
					hasDifference |= !comparison.AreEqual;
					shadows.Add(
						new TermInfoDatabaseSetShadowAnalysis(
							shadow,
							shadowRelationship,
							comparison
						)
					);
				}

				if ( hasDifference ) {
					relationship =
						TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent;
				} else if ( lookup.IsObservationComplete ) {
					relationship =
						TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual;
				} else {
					relationship =
						TermInfoDatabaseSetSemanticRelationship.Indeterminate;
				}
			} else {
				foreach (
					TermInfoDatabaseSetOccurrence occurrence
					in identity.Occurrences.Skip( 1 )
				) {
					cancellationToken.ThrowIfCancellationRequested();
					shadows.Add(
						new TermInfoDatabaseSetShadowAnalysis(
							occurrence,
							TermInfoDatabaseSetSemanticRelationship.Indeterminate,
							null
						)
					);
				}
				relationship =
					TermInfoDatabaseSetSemanticRelationship.Indeterminate;
			}

			TermInfoDatabaseSetIdentityAnalysis analysis =
				new(
					identity,
					lookup,
					relationship,
					shadows
				);
			repeatedIdentities.Add( analysis );
			identityAnalyses.Add( identity.Name, analysis );
		}

		Dictionary<string, List<TermInfoDatabaseSetOccurrence>> aliasOccurrences =
			new( StringComparer.Ordinal );
		int aliasOccurrenceCount = 0;
		IEnumerable<TermInfoDatabaseSetOccurrence> orderedOccurrences =
			Identities
				.SelectMany( identity => identity.Occurrences )
				.OrderBy( occurrence => occurrence.DatabaseIndex )
				.ThenBy( occurrence => occurrence.CatalogEntryIndex );
		foreach ( TermInfoDatabaseSetOccurrence occurrence in orderedOccurrences ) {
			cancellationToken.ThrowIfCancellationRequested();
			foreach ( string alias in occurrence.Aliases ) {
				cancellationToken.ThrowIfCancellationRequested();
				aliasOccurrenceCount = checked( aliasOccurrenceCount + 1 );
				if ( aliasOccurrenceCount > effectiveOptions.MaximumAliasOccurrenceCount ) {
					throw new InvalidOperationException(
						$"Database-set semantic analysis exceeds the configured maximum of {effectiveOptions.MaximumAliasOccurrenceCount} alias occurrences."
					);
				}

				if ( !aliasOccurrences.TryGetValue(
					alias,
					out List<TermInfoDatabaseSetOccurrence>? occurrences
				) ) {
					occurrences = [];
					aliasOccurrences.Add( alias, occurrences );
				}
				occurrences.Add( occurrence );
			}
		}

		int[] incompleteDatabaseIndices =
			Entries
				.Where( entry => !entry.IsComplete )
				.Select( entry => entry.Index )
				.ToArray();
		List<TermInfoDatabaseSetAliasAnalysis> aliases = [];
		foreach (
			KeyValuePair<string, List<TermInfoDatabaseSetOccurrence>> pair
			in aliasOccurrences.OrderBy(
				pair => pair.Key,
				StringComparer.Ordinal
			)
		) {
			cancellationToken.ThrowIfCancellationRequested();
			TermInfoDatabaseSetIdentity? matchingCanonicalIdentity =
				FindIdentity( pair.Key );
			if ( pair.Value.Count < 2
				&& matchingCanonicalIdentity is null
				&& IsComplete ) {
				continue;
			}

			TermInfoDatabaseSetOccurrence[] occurrences = pair.Value.ToArray();
			string[] canonicalNames =
				occurrences
					.Select( occurrence => occurrence.Name )
					.Distinct( StringComparer.Ordinal )
					.OrderBy( name => name, StringComparer.Ordinal )
					.ToArray();
			int firstDatabaseIndex = occurrences[ 0 ].DatabaseIndex;
			int[] blockingDatabaseIndices =
				incompleteDatabaseIndices
					.Where( index => index <= firstDatabaseIndex )
					.ToArray();
			TermInfoDatabaseSetOccurrence? precedenceOwner =
				blockingDatabaseIndices.Length == 0
					? occurrences[ 0 ]
					: null;

			bool hasCanonicalOwnershipConflict =
				canonicalNames.Length > 1
				|| (
					matchingCanonicalIdentity is not null
					&& !canonicalNames.Contains(
						matchingCanonicalIdentity.Name,
						StringComparer.Ordinal
					)
				);
			TermInfoDatabaseSetSemanticRelationship relationship;
			if ( hasCanonicalOwnershipConflict ) {
				relationship =
					TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent;
			} else if ( identityAnalyses.TryGetValue(
				canonicalNames[ 0 ],
				out TermInfoDatabaseSetIdentityAnalysis? identityAnalysis
			) ) {
				if ( identityAnalysis.Relationship
					== TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent ) {
					relationship =
						TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent;
				} else if ( !IsComplete
					|| identityAnalysis.Relationship
						== TermInfoDatabaseSetSemanticRelationship.Indeterminate ) {
					relationship =
						TermInfoDatabaseSetSemanticRelationship.Indeterminate;
				} else {
					relationship =
						TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual;
				}
			} else if ( !IsComplete ) {
				relationship =
					TermInfoDatabaseSetSemanticRelationship.Indeterminate;
			} else {
				relationship =
					TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual;
			}

			aliases.Add(
				new TermInfoDatabaseSetAliasAnalysis(
					pair.Key,
					occurrences,
					canonicalNames,
					precedenceOwner,
					matchingCanonicalIdentity,
					relationship,
					IsComplete,
					blockingDatabaseIndices
				)
			);
		}
		cancellationToken.ThrowIfCancellationRequested();

		return new TermInfoDatabaseSetSemanticAnalysis(
			repeatedIdentities,
			aliases,
			semanticComparisonCount,
			aliasOccurrenceCount,
			IsComplete
		);
	}
}
