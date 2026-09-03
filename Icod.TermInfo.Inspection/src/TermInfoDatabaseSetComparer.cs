namespace Icod.TermInfo.Inspection;

/// <summary>
/// Compares ordered terminfo database sets as effective semantic views and as
/// physical/provenance collections.
/// </summary>
public static class TermInfoDatabaseSetComparer {
	private enum AliasResolutionStatus {
		NotObserved = 0,
		OwnerKnown = 1,
		Indeterminate = 2,
	}

	private sealed class AliasSnapshot {
		internal AliasSnapshot(
			IReadOnlyDictionary<string, TermInfoDatabaseSetOccurrence> firstOwners,
			IReadOnlyCollection<string> names,
			int occurrenceCount
		) {
			ArgumentNullException.ThrowIfNull( firstOwners );
			ArgumentNullException.ThrowIfNull( names );
			if ( occurrenceCount < 0 ) {
				throw new ArgumentOutOfRangeException( nameof( occurrenceCount ) );
			}

			FirstOwners = firstOwners;
			Names = names;
			OccurrenceCount = occurrenceCount;
		}

		internal IReadOnlyDictionary<string, TermInfoDatabaseSetOccurrence> FirstOwners {
			get;
		}

		internal IReadOnlyCollection<string> Names {
			get;
		}

		internal int OccurrenceCount {
			get;
		}
	}

	private sealed class AliasResolution {
		internal AliasResolution(
			AliasResolutionStatus status,
			TermInfoDatabaseSetOccurrence? owner
		) {
			Status = status;
			Owner = owner;
		}

		internal AliasResolutionStatus Status {
			get;
		}

		internal TermInfoDatabaseSetOccurrence? Owner {
			get;
		}
	}

	/// <summary>
	/// Compares two immutable ordered database sets.
	/// </summary>
	/// <param name="left">The left database set.</param>
	/// <param name="right">The right database set.</param>
	/// <param name="semanticAnalysisOptions">
	/// Optional DA03 alias-scan resource bounds reused independently for each set.
	/// </param>
	/// <param name="cancellationToken">
	/// A token observed at deterministic topology, issue, identity, shadow, and
	/// alias boundaries.
	/// </param>
	/// <returns>A stable structured database-set comparison.</returns>
	public static TermInfoDatabaseSetComparisonResult Compare(
		TermInfoDatabaseSet left,
		TermInfoDatabaseSet right,
		TermInfoDatabaseSetSemanticAnalysisOptions? semanticAnalysisOptions = null,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( left );
		ArgumentNullException.ThrowIfNull( right );
		cancellationToken.ThrowIfCancellationRequested();

		TermInfoDatabaseSetSemanticAnalysisOptions effectiveOptions =
			semanticAnalysisOptions ?? new TermInfoDatabaseSetSemanticAnalysisOptions();
		List<TermInfoDatabaseSetDifference> differences = [];
		int semanticComparisonCount = 0;

		CompareTopology(
			differences,
			left,
			right,
			cancellationToken
		);
		CompareCompleteness(
			differences,
			left,
			right,
			cancellationToken
		);
		CompareIssues(
			differences,
			left,
			right,
			cancellationToken
		);
		if ( !left.IsComplete || !right.IsComplete ) {
			differences.Add(
				new TermInfoDatabaseSetDifference(
					TermInfoDatabaseSetDifferenceKind.Indeterminate
				)
			);
		}

		string[] identityNames =
			left.Identities
				.Select( identity => identity.Name )
				.Concat( right.Identities.Select( identity => identity.Name ) )
				.Distinct( StringComparer.Ordinal )
				.OrderBy( name => name, StringComparer.Ordinal )
				.ToArray();
		foreach ( string name in identityNames ) {
			cancellationToken.ThrowIfCancellationRequested();
			TermInfoDatabaseSetLookupResult leftLookup = left.LookupCanonicalName( name );
			TermInfoDatabaseSetLookupResult rightLookup = right.LookupCanonicalName( name );

			if ( leftLookup.Status == TermInfoDatabaseSetLookupStatus.Indeterminate
				|| rightLookup.Status == TermInfoDatabaseSetLookupStatus.Indeterminate ) {
				differences.Add(
					new TermInfoDatabaseSetDifference(
						TermInfoDatabaseSetDifferenceKind.Indeterminate,
						name,
						leftLookup: leftLookup,
						rightLookup: rightLookup
					)
				);
				continue;
			}

			if ( leftLookup.Status == TermInfoDatabaseSetLookupStatus.WinnerKnown
				&& rightLookup.Status == TermInfoDatabaseSetLookupStatus.NotObserved ) {
				differences.Add(
					new TermInfoDatabaseSetDifference(
						TermInfoDatabaseSetDifferenceKind.OnlyInLeft,
						name,
						leftOccurrence: leftLookup.Winner,
						leftLookup: leftLookup,
						rightLookup: rightLookup
					)
				);
				continue;
			}
			if ( leftLookup.Status == TermInfoDatabaseSetLookupStatus.NotObserved
				&& rightLookup.Status == TermInfoDatabaseSetLookupStatus.WinnerKnown ) {
				differences.Add(
					new TermInfoDatabaseSetDifference(
						TermInfoDatabaseSetDifferenceKind.OnlyInRight,
						name,
						rightOccurrence: rightLookup.Winner,
						leftLookup: leftLookup,
						rightLookup: rightLookup
					)
				);
				continue;
			}
			if ( leftLookup.Status != TermInfoDatabaseSetLookupStatus.WinnerKnown
				|| rightLookup.Status != TermInfoDatabaseSetLookupStatus.WinnerKnown ) {
				continue;
			}

			TermInfoDatabaseSetOccurrence leftWinner = leftLookup.Winner!;
			TermInfoDatabaseSetOccurrence rightWinner = rightLookup.Winner!;
			semanticComparisonCount = checked( semanticComparisonCount + 1 );
			TermInfoComparisonResult effectiveComparison =
				TerminalDescriptionComparer.Compare(
					leftWinner.Entry.Terminal,
					rightWinner.Entry.Terminal
				);
			if ( !effectiveComparison.AreEqual ) {
				differences.Add(
					new TermInfoDatabaseSetDifference(
						TermInfoDatabaseSetDifferenceKind.EffectiveSemantic,
						name,
						leftOccurrence: leftWinner,
						rightOccurrence: rightWinner,
						leftLookup: leftLookup,
						rightLookup: rightLookup,
						semanticComparison: effectiveComparison
					)
				);
			} else if ( !SameProvenance( left, leftWinner, right, rightWinner ) ) {
				differences.Add(
					new TermInfoDatabaseSetDifference(
						TermInfoDatabaseSetDifferenceKind.EffectiveProvenance,
						name,
						leftOccurrence: leftWinner,
						rightOccurrence: rightWinner,
						leftLookup: leftLookup,
						rightLookup: rightLookup,
						semanticComparison: effectiveComparison
					)
				);
			}

			CompareShadows(
				differences,
				left,
				right,
				name,
				leftLookup,
				rightLookup,
				ref semanticComparisonCount,
				cancellationToken
			);
			if ( !leftLookup.IsObservationComplete || !rightLookup.IsObservationComplete ) {
				differences.Add(
					new TermInfoDatabaseSetDifference(
						TermInfoDatabaseSetDifferenceKind.Indeterminate,
						name,
						leftOccurrence: leftWinner,
						rightOccurrence: rightWinner,
						leftLookup: leftLookup,
						rightLookup: rightLookup
					)
				);
			}
		}

		AliasSnapshot leftAliases = BuildAliasSnapshot(
			left,
			effectiveOptions.MaximumAliasOccurrenceCount,
			cancellationToken
		);
		AliasSnapshot rightAliases = BuildAliasSnapshot(
			right,
			effectiveOptions.MaximumAliasOccurrenceCount,
			cancellationToken
		);
		string[] aliasNames =
			leftAliases.Names
				.Concat( rightAliases.Names )
				.Distinct( StringComparer.Ordinal )
				.OrderBy( name => name, StringComparer.Ordinal )
				.ToArray();
		foreach ( string alias in aliasNames ) {
			cancellationToken.ThrowIfCancellationRequested();
			AliasResolution leftAlias = ResolveAlias( left, leftAliases, alias );
			AliasResolution rightAlias = ResolveAlias( right, rightAliases, alias );
			if ( leftAlias.Status == AliasResolutionStatus.Indeterminate
				|| rightAlias.Status == AliasResolutionStatus.Indeterminate ) {
				differences.Add(
					new TermInfoDatabaseSetDifference(
						TermInfoDatabaseSetDifferenceKind.Indeterminate,
						alias,
						leftOccurrence: leftAlias.Owner,
						rightOccurrence: rightAlias.Owner
					)
				);
				continue;
			}

			if ( leftAlias.Status == AliasResolutionStatus.NotObserved
				&& rightAlias.Status == AliasResolutionStatus.NotObserved ) {
				continue;
			}
			if ( leftAlias.Status == AliasResolutionStatus.NotObserved
				|| rightAlias.Status == AliasResolutionStatus.NotObserved ) {
				differences.Add(
					new TermInfoDatabaseSetDifference(
						TermInfoDatabaseSetDifferenceKind.AliasOwnership,
						alias,
						leftOccurrence: leftAlias.Owner,
						rightOccurrence: rightAlias.Owner
					)
				);
				continue;
			}

			TermInfoDatabaseSetOccurrence leftOwner = leftAlias.Owner!;
			TermInfoDatabaseSetOccurrence rightOwner = rightAlias.Owner!;
			semanticComparisonCount = checked( semanticComparisonCount + 1 );
			TermInfoComparisonResult aliasComparison =
				TerminalDescriptionComparer.Compare(
					leftOwner.Entry.Terminal,
					rightOwner.Entry.Terminal
				);
			if ( !string.Equals(
				leftOwner.Name,
				rightOwner.Name,
				StringComparison.Ordinal
			) || !aliasComparison.AreEqual
				|| !SameProvenance( left, leftOwner, right, rightOwner ) ) {
				differences.Add(
					new TermInfoDatabaseSetDifference(
						TermInfoDatabaseSetDifferenceKind.AliasOwnership,
						alias,
						leftOccurrence: leftOwner,
						rightOccurrence: rightOwner,
						semanticComparison: aliasComparison
					)
				);
			}
		}

		TermInfoDatabaseSetDifference[] orderedDifferences =
			differences
				.OrderBy( difference => difference.Kind )
				.ThenBy( difference => difference.Name, StringComparer.Ordinal )
				.ThenBy(
					difference => difference.LeftDatabase?.Index
						?? difference.LeftOccurrence?.DatabaseIndex
						?? difference.LeftIssue?.DatabaseIndex
						?? -1
				)
				.ThenBy(
					difference => difference.RightDatabase?.Index
						?? difference.RightOccurrence?.DatabaseIndex
						?? difference.RightIssue?.DatabaseIndex
						?? -1
				)
				.ThenBy(
					difference => difference.LeftOccurrence?.CatalogEntryIndex
						?? difference.LeftIssue?.CatalogIssueIndex
						?? -1
				)
				.ThenBy(
					difference => difference.RightOccurrence?.CatalogEntryIndex
						?? difference.RightIssue?.CatalogIssueIndex
						?? -1
				)
				.ToArray();
		cancellationToken.ThrowIfCancellationRequested();

		return new TermInfoDatabaseSetComparisonResult(
			orderedDifferences,
			semanticComparisonCount,
			checked( leftAliases.OccurrenceCount + rightAliases.OccurrenceCount )
		);
	}

	private static void CompareTopology(
		ICollection<TermInfoDatabaseSetDifference> differences,
		TermInfoDatabaseSet left,
		TermInfoDatabaseSet right,
		CancellationToken cancellationToken
	) {
		int count = Math.Max( left.Entries.Count, right.Entries.Count );
		for ( int index = 0; index < count; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			TermInfoDatabaseSetEntry? leftDatabase =
				index < left.Entries.Count ? left.Entries[ index ] : null;
			TermInfoDatabaseSetEntry? rightDatabase =
				index < right.Entries.Count ? right.Entries[ index ] : null;
			if ( leftDatabase is null || rightDatabase is null
				|| leftDatabase.Catalog.Kind != rightDatabase.Catalog.Kind
				|| !string.Equals(
					leftDatabase.Catalog.Root,
					rightDatabase.Catalog.Root,
					StringComparison.Ordinal
				) ) {
				differences.Add(
					new TermInfoDatabaseSetDifference(
						TermInfoDatabaseSetDifferenceKind.RootTopology,
						leftDatabase: leftDatabase,
						rightDatabase: rightDatabase
					)
				);
			}
		}
	}

	private static void CompareCompleteness(
		ICollection<TermInfoDatabaseSetDifference> differences,
		TermInfoDatabaseSet left,
		TermInfoDatabaseSet right,
		CancellationToken cancellationToken
	) {
		if ( left.IsComplete != right.IsComplete ) {
			differences.Add(
				new TermInfoDatabaseSetDifference(
					TermInfoDatabaseSetDifferenceKind.Completeness
				)
			);
		}

		int count = Math.Min( left.Entries.Count, right.Entries.Count );
		for ( int index = 0; index < count; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( left.Entries[ index ].IsComplete != right.Entries[ index ].IsComplete ) {
				differences.Add(
					new TermInfoDatabaseSetDifference(
						TermInfoDatabaseSetDifferenceKind.Completeness,
						leftDatabase: left.Entries[ index ],
						rightDatabase: right.Entries[ index ]
					)
				);
			}
		}
	}

	private static void CompareIssues(
		ICollection<TermInfoDatabaseSetDifference> differences,
		TermInfoDatabaseSet left,
		TermInfoDatabaseSet right,
		CancellationToken cancellationToken
	) {
		int count = Math.Max( left.Issues.Count, right.Issues.Count );
		for ( int index = 0; index < count; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			TermInfoDatabaseSetIssue? leftIssue =
				index < left.Issues.Count ? left.Issues[ index ] : null;
			TermInfoDatabaseSetIssue? rightIssue =
				index < right.Issues.Count ? right.Issues[ index ] : null;
			if ( !SameIssue( leftIssue, rightIssue ) ) {
				differences.Add(
					new TermInfoDatabaseSetDifference(
						TermInfoDatabaseSetDifferenceKind.Issue,
						leftIssue: leftIssue,
						rightIssue: rightIssue
					)
				);
			}
		}
	}

	private static bool SameIssue(
		TermInfoDatabaseSetIssue? left,
		TermInfoDatabaseSetIssue? right
	) {
		if ( left is null || right is null ) {
			return left is null && right is null;
		}

		return left.DatabaseIndex == right.DatabaseIndex
			&& left.CatalogIssueIndex == right.CatalogIssueIndex
			&& left.Issue.Kind == right.Issue.Kind
			&& string.Equals( left.Issue.Path, right.Issue.Path, StringComparison.Ordinal )
			&& string.Equals( left.Issue.Message, right.Issue.Message, StringComparison.Ordinal );
	}

	private static void CompareShadows(
		ICollection<TermInfoDatabaseSetDifference> differences,
		TermInfoDatabaseSet left,
		TermInfoDatabaseSet right,
		string name,
		TermInfoDatabaseSetLookupResult leftLookup,
		TermInfoDatabaseSetLookupResult rightLookup,
		ref int semanticComparisonCount,
		CancellationToken cancellationToken
	) {
		int count = Math.Max(
			leftLookup.ShadowedOccurrences.Count,
			rightLookup.ShadowedOccurrences.Count
		);
		for ( int index = 0; index < count; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			TermInfoDatabaseSetOccurrence? leftShadow =
				index < leftLookup.ShadowedOccurrences.Count
					? leftLookup.ShadowedOccurrences[ index ]
					: null;
			TermInfoDatabaseSetOccurrence? rightShadow =
				index < rightLookup.ShadowedOccurrences.Count
					? rightLookup.ShadowedOccurrences[ index ]
					: null;
			TermInfoComparisonResult? comparison = null;
			bool different = leftShadow is null || rightShadow is null;
			if ( leftShadow is not null && rightShadow is not null ) {
				semanticComparisonCount = checked( semanticComparisonCount + 1 );
				comparison = TerminalDescriptionComparer.Compare(
					leftShadow.Entry.Terminal,
					rightShadow.Entry.Terminal
				);
				different =
					!comparison.AreEqual
					|| !SameProvenance( left, leftShadow, right, rightShadow );
			}

			if ( different ) {
				differences.Add(
					new TermInfoDatabaseSetDifference(
						TermInfoDatabaseSetDifferenceKind.ShadowSet,
						name,
						leftOccurrence: leftShadow,
						rightOccurrence: rightShadow,
						leftLookup: leftLookup,
						rightLookup: rightLookup,
						semanticComparison: comparison
					)
				);
			}
		}
	}

	private static bool SameProvenance(
		TermInfoDatabaseSet leftSet,
		TermInfoDatabaseSetOccurrence left,
		TermInfoDatabaseSet rightSet,
		TermInfoDatabaseSetOccurrence right
	) {
		ArgumentNullException.ThrowIfNull( leftSet );
		ArgumentNullException.ThrowIfNull( left );
		ArgumentNullException.ThrowIfNull( rightSet );
		ArgumentNullException.ThrowIfNull( right );

		return left.DatabaseIndex == right.DatabaseIndex
			&& left.CatalogEntryIndex == right.CatalogEntryIndex
			&& string.Equals(
				leftSet.Entries[ left.DatabaseIndex ].Catalog.Root,
				rightSet.Entries[ right.DatabaseIndex ].Catalog.Root,
				StringComparison.Ordinal
			)
			&& string.Equals( left.Entry.Path, right.Entry.Path, StringComparison.Ordinal );
	}

	private static AliasSnapshot BuildAliasSnapshot(
		TermInfoDatabaseSet set,
		int maximumAliasOccurrenceCount,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( set );
		if ( maximumAliasOccurrenceCount < 1 ) {
			throw new ArgumentOutOfRangeException( nameof( maximumAliasOccurrenceCount ) );
		}

		Dictionary<(int DatabaseIndex, int CatalogEntryIndex), TermInfoDatabaseSetOccurrence>
			occurrencesByCoordinate = [];
		foreach ( TermInfoDatabaseSetIdentity identity in set.Identities ) {
			foreach ( TermInfoDatabaseSetOccurrence occurrence in identity.Occurrences ) {
				occurrencesByCoordinate.Add(
					( occurrence.DatabaseIndex, occurrence.CatalogEntryIndex ),
					occurrence
				);
			}
		}

		Dictionary<string, TermInfoDatabaseSetOccurrence> firstOwners =
			new( StringComparer.Ordinal );
		HashSet<string> names = new( StringComparer.Ordinal );
		int occurrenceCount = 0;
		foreach ( TermInfoDatabaseSetEntry database in set.Entries ) {
			for ( int entryIndex = 0; entryIndex < database.Catalog.Entries.Count; entryIndex++ ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( !occurrencesByCoordinate.TryGetValue(
					( database.Index, entryIndex ),
					out TermInfoDatabaseSetOccurrence? occurrence
				) ) {
					throw new InvalidOperationException(
						"The database-set occurrence index is inconsistent with its constituent catalog."
					);
				}

				foreach ( string alias in occurrence.Aliases ) {
					cancellationToken.ThrowIfCancellationRequested();
					occurrenceCount = checked( occurrenceCount + 1 );
					if ( occurrenceCount > maximumAliasOccurrenceCount ) {
						throw new InvalidOperationException(
							$"Database-set comparison exceeds the configured maximum of {maximumAliasOccurrenceCount} alias occurrences in one input set."
						);
					}
					names.Add( alias );
					firstOwners.TryAdd( alias, occurrence );
				}
			}
		}

		return new AliasSnapshot(
			firstOwners,
			names,
			occurrenceCount
		);
	}

	private static AliasResolution ResolveAlias(
		TermInfoDatabaseSet set,
		AliasSnapshot snapshot,
		string alias
	) {
		ArgumentNullException.ThrowIfNull( set );
		ArgumentNullException.ThrowIfNull( snapshot );
		ArgumentException.ThrowIfNullOrWhiteSpace( alias );

		if ( !snapshot.FirstOwners.TryGetValue(
			alias,
			out TermInfoDatabaseSetOccurrence? owner
		) ) {
			return new AliasResolution(
				set.IsComplete
					? AliasResolutionStatus.NotObserved
					: AliasResolutionStatus.Indeterminate,
				null
			);
		}

		bool blocked =
			set.Entries.Any(
				database => !database.IsComplete && database.Index <= owner.DatabaseIndex
			);
		return new AliasResolution(
			blocked
				? AliasResolutionStatus.Indeterminate
				: AliasResolutionStatus.OwnerKnown,
			owner
		);
	}
}
