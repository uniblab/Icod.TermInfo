namespace Icod.TermInfo.Inspection;

public static partial class TerminalDescriptionSourcePlanner {
	/// <summary>
	/// Plans relative source from canonical candidates discovered across one
	/// complete explicit ordered database set.
	/// </summary>
	public static TermInfoDatabaseSetSourcePlanningResult PlanFromDatabaseSet(
		TerminalDescription target,
		TermInfoDatabaseSet databaseSet,
		TerminalDescriptionSourcePlanningOptions? planningOptions = null,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( databaseSet );
		cancellationToken.ThrowIfCancellationRequested();

		TerminalDescriptionSourcePlanningOptions effectivePlanningOptions =
			planningOptions ?? new TerminalDescriptionSourcePlanningOptions();
		IReadOnlyList<TermInfoDatabaseSetPlanningCandidate> candidates =
			CreateDatabaseSetCandidates(
				target,
				databaseSet,
				effectivePlanningOptions,
				cancellationToken,
				out int collapsedDuplicateOccurrenceCount,
				out int candidateSemanticComparisonCount
			);
		cancellationToken.ThrowIfCancellationRequested();

		TerminalDescriptionSourcePlan plan =
			Plan(
				target,
				candidates.Select( candidate => candidate.Parent ),
				effectivePlanningOptions,
				cancellationToken
			);

		return new TermInfoDatabaseSetSourcePlanningResult(
			databaseSet,
			plan,
			candidates,
			collapsedDuplicateOccurrenceCount,
			candidateSemanticComparisonCount
		);
	}

	/// <summary>
	/// Aggregates already-inspected catalogs without filesystem I/O and plans from
	/// the resulting complete ordered database set.
	/// </summary>
	public static TermInfoDatabaseSetSourcePlanningResult PlanFromCatalogs(
		TerminalDescription target,
		IEnumerable<TermInfoDatabaseCatalog> catalogs,
		TerminalDescriptionSourcePlanningOptions? planningOptions = null,
		TermInfoDatabaseSetOptions? databaseSetOptions = null,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( catalogs );
		cancellationToken.ThrowIfCancellationRequested();

		TermInfoDatabaseSet databaseSet =
			TermInfoDatabaseInspector.CreateSet(
				catalogs,
				databaseSetOptions,
				cancellationToken
			);
		return PlanFromDatabaseSet(
			target,
			databaseSet,
			planningOptions,
			cancellationToken
		);
	}

	/// <summary>
	/// Inspects explicit database roots once in caller order and plans from the
	/// resulting complete ordered database set.
	/// </summary>
	public static TermInfoDatabaseSetSourcePlanningResult PlanFromDirectories(
		TerminalDescription target,
		IEnumerable<string> roots,
		TerminalDescriptionSourcePlanningOptions? planningOptions = null,
		TermInfoDatabaseSetOptions? databaseSetOptions = null,
		CompiledTermInfoParserOptions? parserOptions = null,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( roots );
		cancellationToken.ThrowIfCancellationRequested();

		TermInfoDatabaseSet databaseSet =
			TermInfoDatabaseInspector.InspectSet(
				roots,
				databaseSetOptions,
				parserOptions,
				cancellationToken
			);
		return PlanFromDatabaseSet(
			target,
			databaseSet,
			planningOptions,
			cancellationToken
		);
	}

	private static IReadOnlyList<TermInfoDatabaseSetPlanningCandidate> CreateDatabaseSetCandidates(
		TerminalDescription target,
		TermInfoDatabaseSet databaseSet,
		TerminalDescriptionSourcePlanningOptions planningOptions,
		CancellationToken cancellationToken,
		out int collapsedDuplicateOccurrenceCount,
		out int candidateSemanticComparisonCount
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( databaseSet );
		ArgumentNullException.ThrowIfNull( planningOptions );
		cancellationToken.ThrowIfCancellationRequested();
		ValidateCompleteDatabaseSet( databaseSet );

		Dictionary<(int DatabaseIndex, int CatalogEntryIndex), TermInfoDatabaseSetOccurrence>
			occurrencesByCoordinate = [];
		foreach ( TermInfoDatabaseSetIdentity identity in databaseSet.Identities ) {
			foreach ( TermInfoDatabaseSetOccurrence occurrence in identity.Occurrences ) {
				occurrencesByCoordinate.Add(
					( occurrence.DatabaseIndex, occurrence.CatalogEntryIndex ),
					occurrence
				);
			}
		}

		HashSet<string> targetIdentities =
			new(
				StringComparer.Ordinal
			) {
				target.Name,
			};
		foreach ( string alias in target.Aliases ) {
			targetIdentities.Add( alias );
		}

		Dictionary<string, TermInfoDatabaseSetOccurrence> representatives =
			new( StringComparer.Ordinal );
		List<TermInfoDatabaseSetPlanningCandidate> candidates = [];
		collapsedDuplicateOccurrenceCount = 0;
		candidateSemanticComparisonCount = 0;

		foreach ( TermInfoDatabaseSetEntry database in databaseSet.Entries ) {
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

				if ( representatives.TryGetValue(
					occurrence.Name,
					out TermInfoDatabaseSetOccurrence? representative
				) ) {
					candidateSemanticComparisonCount =
						checked( candidateSemanticComparisonCount + 1 );
					TermInfoComparisonResult comparison =
						TerminalDescriptionComparer.Compare(
							representative.Entry.Terminal,
							occurrence.Entry.Terminal
						);
					if ( !comparison.AreEqual ) {
						throw new InvalidOperationException(
							$"Database-set planning cannot use conflicting physical candidate publications for canonical name '{occurrence.Name}' at database indices {representative.DatabaseIndex} and {occurrence.DatabaseIndex}."
						);
					}
					collapsedDuplicateOccurrenceCount =
						checked( collapsedDuplicateOccurrenceCount + 1 );
					continue;
				}

				representatives.Add( occurrence.Name, occurrence );
				if ( SharesIdentity(
					targetIdentities,
					occurrence.Entry.Terminal
				) ) {
					continue;
				}
				if ( candidates.Count >= planningOptions.MaximumCandidateCount ) {
					throw new ArgumentException(
						$"The database-set planning request exceeds the configured maximum of {planningOptions.MaximumCandidateCount} canonical non-self candidates.",
						nameof( databaseSet )
					);
				}

				TerminalDescriptionSourceSynthesisParent parent =
					new(
						occurrence.Name,
						occurrence.Entry.Terminal
					);
				candidates.Add(
					new TermInfoDatabaseSetPlanningCandidate(
						candidates.Count,
						database,
						occurrence,
						parent
					)
				);
			}
		}
		cancellationToken.ThrowIfCancellationRequested();

		return Array.AsReadOnly( candidates.ToArray() );
	}

	private static void ValidateCompleteDatabaseSet(
		TermInfoDatabaseSet databaseSet
	) {
		ArgumentNullException.ThrowIfNull( databaseSet );
		if ( databaseSet.IsComplete ) {
			return;
		}

		string indices =
			string.Join(
				", ",
				databaseSet.Entries
					.Where( database => !database.IsComplete )
					.Select( database => database.Index )
			);
		throw new InvalidOperationException(
			$"Database-set planning requires complete issue-free conventional catalogs; incomplete database indices: {indices}."
		);
	}
}
