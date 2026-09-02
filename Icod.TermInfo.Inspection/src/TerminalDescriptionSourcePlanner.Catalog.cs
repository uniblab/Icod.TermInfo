namespace Icod.TermInfo.Inspection;

public static partial class TerminalDescriptionSourcePlanner {
	/// <summary>
	/// Plans relative source from the canonical entries of one explicit complete
	/// conventional database catalog.
	/// </summary>
	/// <param name="target">The effective target terminal description.</param>
	/// <param name="catalog">
	/// The explicit immutable catalog. Only canonical entry names become candidate
	/// references; aliases do not create additional candidate identities.
	/// </param>
	/// <param name="planningOptions">
	/// Optional bounded planning policy. A <see langword="null"/> value uses the
	/// canonical defaults.
	/// </param>
	/// <param name="cancellationToken">
	/// A token observed while validating catalog entries and during planning.
	/// </param>
	/// <returns>The selected deterministic relative-source plan.</returns>
	/// <exception cref="ArgumentException">
	/// The canonical non-self candidate count exceeds the active planning limit.
	/// </exception>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="target"/> or <paramref name="catalog"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The catalog is not a complete conventional-directory snapshot, contains
	/// issues, contains conflicting physical copies of one canonical entry, or
	/// planning cannot satisfy the active policy.
	/// </exception>
	/// <exception cref="OperationCanceledException">
	/// <paramref name="cancellationToken"/> requests cancellation.
	/// </exception>
	public static TerminalDescriptionSourcePlan PlanFromCatalog(
		TerminalDescription target,
		TermInfoDatabaseCatalog catalog,
		TerminalDescriptionSourcePlanningOptions? planningOptions = null,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( catalog );

		cancellationToken.ThrowIfCancellationRequested();
		TerminalDescriptionSourcePlanningOptions effectivePlanningOptions =
			planningOptions
			?? new TerminalDescriptionSourcePlanningOptions();
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> candidates =
			CreateCatalogCandidates(
				target,
				catalog,
				effectivePlanningOptions,
				cancellationToken
			);
		cancellationToken.ThrowIfCancellationRequested();

		return Plan(
			target,
			candidates,
			effectivePlanningOptions,
			cancellationToken
		);
	}

	/// <summary>
	/// Inspects one explicit conventional database root and plans from its complete
	/// canonical catalog.
	/// </summary>
	/// <param name="target">The effective target terminal description.</param>
	/// <param name="root">
	/// The explicit conventional database root. No environment or host discovery
	/// locations are consulted.
	/// </param>
	/// <param name="planningOptions">
	/// Optional bounded planning policy. A <see langword="null"/> value uses the
	/// canonical defaults.
	/// </param>
	/// <param name="parserOptions">
	/// Optional compiled-entry resource limits snapshotted for catalog inspection.
	/// </param>
	/// <param name="cancellationToken">
	/// A token observed during catalog inspection, candidate preparation, and
	/// planning.
	/// </param>
	/// <returns>The selected deterministic relative-source plan.</returns>
	/// <exception cref="ArgumentException">
	/// <paramref name="root"/> is empty or whitespace, or the canonical non-self
	/// candidate count exceeds the active planning limit.
	/// </exception>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="target"/> or <paramref name="root"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The explicit root does not produce a complete conventional catalog, the
	/// catalog contains conflicting physical copies, or planning cannot satisfy
	/// the active policy.
	/// </exception>
	/// <exception cref="OperationCanceledException">
	/// <paramref name="cancellationToken"/> requests cancellation.
	/// </exception>
	public static TerminalDescriptionSourcePlan PlanFromDirectory(
		TerminalDescription target,
		string root,
		TerminalDescriptionSourcePlanningOptions? planningOptions = null,
		CompiledTermInfoParserOptions? parserOptions = null,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( root );
		if ( string.IsNullOrWhiteSpace( root ) ) {
			throw new ArgumentException(
				"The terminfo database root cannot be empty or whitespace.",
				nameof( root )
			);
		}

		cancellationToken.ThrowIfCancellationRequested();
		TermInfoDatabaseCatalog catalog =
			TermInfoDatabaseInspector.InspectDirectory(
				root,
				parserOptions,
				cancellationToken
			);

		return PlanFromCatalog(
			target,
			catalog,
			planningOptions,
			cancellationToken
		);
	}

	private static IReadOnlyList<TerminalDescriptionSourceSynthesisParent> CreateCatalogCandidates(
			TerminalDescription target,
			TermInfoDatabaseCatalog catalog,
			TerminalDescriptionSourcePlanningOptions planningOptions,
			CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( catalog );
		ArgumentNullException.ThrowIfNull( planningOptions );

		ValidateCompleteCatalog( catalog );
		HashSet<string> targetIdentities =
			new(
				StringComparer.Ordinal
			) {
				target.Name,
			};
		foreach ( string alias in target.Aliases ) {
			targetIdentities.Add( alias );
		}

		Dictionary<string, TerminalDescription> representatives =
			new( StringComparer.Ordinal );
		List<TerminalDescriptionSourceSynthesisParent> candidates = [];
		foreach ( TermInfoDatabaseCatalogEntry entry in catalog.Entries ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( representatives.TryGetValue(
				entry.Name,
				out TerminalDescription? representative
			) ) {
				if ( !TerminalDescriptionComparer.Compare(
					representative,
					entry.Terminal
				).AreEqual ) {
					throw new InvalidOperationException(
						$"The explicit catalog contains conflicting physical entries for canonical name '{entry.Name}'."
					);
				}
				continue;
			}

			representatives.Add(
				entry.Name,
				entry.Terminal
			);
			if ( SharesIdentity(
				targetIdentities,
				entry.Terminal
			) ) {
				continue;
			}
			if ( candidates.Count >= planningOptions.MaximumCandidateCount ) {
				throw new ArgumentException(
					$"The catalog planning request exceeds the configured maximum of {planningOptions.MaximumCandidateCount} canonical non-self candidates.",
					nameof( catalog )
				);
			}

			candidates.Add(
				new TerminalDescriptionSourceSynthesisParent(
					entry.Name,
					entry.Terminal
				)
			);
		}
		cancellationToken.ThrowIfCancellationRequested();

		return Array.AsReadOnly(
			candidates.ToArray()
		);
	}

	private static void ValidateCompleteCatalog(
		TermInfoDatabaseCatalog catalog
	) {
		ArgumentNullException.ThrowIfNull( catalog );

		if ( catalog.Kind != TermInfoDatabaseCatalogKind.ConventionalDirectory ) {
			throw new InvalidOperationException(
				$"Catalog planning requires a complete conventional-directory catalog; root '{catalog.Root}' was reported as {catalog.Kind}."
			);
		}
		if ( catalog.HasIssues ) {
			throw new InvalidOperationException(
				$"Catalog planning requires an issue-free catalog; root '{catalog.Root}' contains {catalog.Issues.Count} issue(s)."
			);
		}
	}

	private static bool SharesIdentity(
		IReadOnlySet<string> targetIdentities,
		TerminalDescription candidate
	) {
		ArgumentNullException.ThrowIfNull( targetIdentities );
		ArgumentNullException.ThrowIfNull( candidate );

		if ( targetIdentities.Contains( candidate.Name ) ) {
			return true;
		}

		return candidate.Aliases.Any(
			alias => targetIdentities.Contains( alias )
		);
	}
}
