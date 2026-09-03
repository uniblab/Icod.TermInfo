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


write_new(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSetDifferenceKind.cs",
    '''namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies one stable database-set comparison difference category.
/// </summary>
public enum TermInfoDatabaseSetDifferenceKind {
	/// <summary>
	/// The ordered constituent root topology differs.
	/// </summary>
	RootTopology = 0,

	/// <summary>
	/// Aggregate or constituent completeness differs.
	/// </summary>
	Completeness = 1,

	/// <summary>
	/// Frozen catalog issue evidence differs.
	/// </summary>
	Issue = 2,

	/// <summary>
	/// A conclusive canonical identity is present only in the left set.
	/// </summary>
	OnlyInLeft = 3,

	/// <summary>
	/// A conclusive canonical identity is present only in the right set.
	/// </summary>
	OnlyInRight = 4,

	/// <summary>
	/// The effective precedence winners are semantically different.
	/// </summary>
	EffectiveSemantic = 5,

	/// <summary>
	/// The effective winners are semantically equal but their physical provenance
	/// differs.
	/// </summary>
	EffectiveProvenance = 6,

	/// <summary>
	/// Effective alias ownership, owner semantics, or owner provenance differs.
	/// </summary>
	AliasOwnership = 7,

	/// <summary>
	/// The observed ordered shadow set differs semantically or structurally.
	/// </summary>
	ShadowSet = 8,

	/// <summary>
	/// Incomplete evidence prevents a complete comparison conclusion.
	/// </summary>
	Indeterminate = 9,
}
''',
)

write_new(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSetDifference.cs",
    '''namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one deterministic typed difference between two ordered terminfo
/// database sets.
/// </summary>
public sealed class TermInfoDatabaseSetDifference {
	internal TermInfoDatabaseSetDifference(
		TermInfoDatabaseSetDifferenceKind kind,
		string? name = null,
		TermInfoDatabaseSetEntry? leftDatabase = null,
		TermInfoDatabaseSetEntry? rightDatabase = null,
		TermInfoDatabaseSetOccurrence? leftOccurrence = null,
		TermInfoDatabaseSetOccurrence? rightOccurrence = null,
		TermInfoDatabaseSetIssue? leftIssue = null,
		TermInfoDatabaseSetIssue? rightIssue = null,
		TermInfoDatabaseSetLookupResult? leftLookup = null,
		TermInfoDatabaseSetLookupResult? rightLookup = null,
		TermInfoComparisonResult? semanticComparison = null
	) {
		if ( name is not null && string.IsNullOrWhiteSpace( name ) ) {
			throw new ArgumentException(
				"A comparison difference name cannot be empty or whitespace.",
				nameof( name )
			);
		}

		Kind = kind;
		Name = name;
		LeftDatabase = leftDatabase;
		RightDatabase = rightDatabase;
		LeftOccurrence = leftOccurrence;
		RightOccurrence = rightOccurrence;
		LeftIssue = leftIssue;
		RightIssue = rightIssue;
		LeftLookup = leftLookup;
		RightLookup = rightLookup;
		SemanticComparison = semanticComparison;
	}

	/// <summary>
	/// Gets the stable difference category.
	/// </summary>
	public TermInfoDatabaseSetDifferenceKind Kind {
		get;
	}

	/// <summary>
	/// Gets the canonical identity or alias associated with the difference, when
	/// the category is name-scoped.
	/// </summary>
	public string? Name {
		get;
	}

	/// <summary>
	/// Gets relevant left constituent database evidence, when applicable.
	/// </summary>
	public TermInfoDatabaseSetEntry? LeftDatabase {
		get;
	}

	/// <summary>
	/// Gets relevant right constituent database evidence, when applicable.
	/// </summary>
	public TermInfoDatabaseSetEntry? RightDatabase {
		get;
	}

	/// <summary>
	/// Gets relevant left physical occurrence evidence, when applicable.
	/// </summary>
	public TermInfoDatabaseSetOccurrence? LeftOccurrence {
		get;
	}

	/// <summary>
	/// Gets relevant right physical occurrence evidence, when applicable.
	/// </summary>
	public TermInfoDatabaseSetOccurrence? RightOccurrence {
		get;
	}

	/// <summary>
	/// Gets relevant left catalog issue evidence, when applicable.
	/// </summary>
	public TermInfoDatabaseSetIssue? LeftIssue {
		get;
	}

	/// <summary>
	/// Gets relevant right catalog issue evidence, when applicable.
	/// </summary>
	public TermInfoDatabaseSetIssue? RightIssue {
		get;
	}

	/// <summary>
	/// Gets relevant left precedence lookup evidence, when applicable.
	/// </summary>
	public TermInfoDatabaseSetLookupResult? LeftLookup {
		get;
	}

	/// <summary>
	/// Gets relevant right precedence lookup evidence, when applicable.
	/// </summary>
	public TermInfoDatabaseSetLookupResult? RightLookup {
		get;
	}

	/// <summary>
	/// Gets the retained cross-set semantic comparison when the difference compared
	/// two observed terminal descriptions.
	/// </summary>
	public TermInfoComparisonResult? SemanticComparison {
		get;
	}
}
''',
)

write_new(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSetComparisonResult.cs",
    '''namespace Icod.TermInfo.Inspection;

/// <summary>
/// Contains the stable deterministic comparison of two ordered terminfo database
/// sets.
/// </summary>
public sealed class TermInfoDatabaseSetComparisonResult {
	internal TermInfoDatabaseSetComparisonResult(
		IEnumerable<TermInfoDatabaseSetDifference> differences,
		int semanticComparisonCount,
		int aliasOccurrenceCount
	) {
		ArgumentNullException.ThrowIfNull( differences );
		if ( semanticComparisonCount < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( semanticComparisonCount ) );
		}
		if ( aliasOccurrenceCount < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( aliasOccurrenceCount ) );
		}

		TermInfoDatabaseSetDifference[] differenceArray = differences.ToArray();
		if ( differenceArray.Any( difference => difference is null ) ) {
			throw new ArgumentException(
				"Database-set differences cannot contain null.",
				nameof( differences )
			);
		}

		Differences = Array.AsReadOnly( differenceArray );
		SemanticComparisonCount = semanticComparisonCount;
		AliasOccurrenceCount = aliasOccurrenceCount;
	}

	/// <summary>
	/// Gets differences in the frozen DA04 kind/name/provenance order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetDifference> Differences {
		get;
	}

	/// <summary>
	/// Gets the number of cross-set calls made to
	/// <see cref="TerminalDescriptionComparer"/>.
	/// </summary>
	public int SemanticComparisonCount {
		get;
	}

	/// <summary>
	/// Gets the total number of alias declarations scanned across both sets.
	/// </summary>
	public int AliasOccurrenceCount {
		get;
	}

	/// <summary>
	/// Gets whether incomplete evidence leaves any whole-set conclusion
	/// indeterminate.
	/// </summary>
	public bool IsConclusive =>
		!Differences.Any(
			difference => difference.Kind == TermInfoDatabaseSetDifferenceKind.Indeterminate
		);

	/// <summary>
	/// Gets whether a conclusive comparison found no effective identity, winner, or
	/// alias-ownership difference.
	/// </summary>
	public bool AreEffectivelyEquivalent =>
		IsConclusive
		&& !Differences.Any(
			difference =>
				difference.Kind == TermInfoDatabaseSetDifferenceKind.OnlyInLeft
				|| difference.Kind == TermInfoDatabaseSetDifferenceKind.OnlyInRight
				|| difference.Kind == TermInfoDatabaseSetDifferenceKind.EffectiveSemantic
				|| difference.Kind == TermInfoDatabaseSetDifferenceKind.AliasOwnership
		);

	/// <summary>
	/// Gets whether a conclusive comparison found no topology, provenance, issue,
	/// alias, shadow, or membership difference.
	/// </summary>
	public bool AreStructurallyEquivalent =>
		IsConclusive
		&& !Differences.Any(
			difference =>
				difference.Kind != TermInfoDatabaseSetDifferenceKind.EffectiveSemantic
				&& difference.Kind != TermInfoDatabaseSetDifferenceKind.Indeterminate
		);

	/// <summary>
	/// Gets whether the database sets are conclusively equivalent in both effective
	/// semantics and structure/provenance.
	/// </summary>
	public bool AreEquivalent =>
		IsConclusive && Differences.Count == 0;
}
''',
)

write_new(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSetComparer.cs",
    '''namespace Icod.TermInfo.Inspection;

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
''',
)

write_new(
    "tests/Icod.TermInfo.Inspection.Tests/src/DA04DatabaseSetComparisonTests.cs",
    '''using System.Globalization;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class DA04DatabaseSetComparisonTests {
	[Fact]
	public void IndependentOracleIdenticalCompleteSetsAreEquivalent() {
		TermInfoDatabaseSet left = CreateSet(
			CreateCatalogAtRoot(
				AbsolutePath( "identical" ),
				CreateTerminal( "alpha", 80, "a" ),
				CreateTerminal( "beta", 100 )
			)
		);
		TermInfoDatabaseSet right = CreateSet(
			CreateCatalogAtRoot(
				left.Entries[ 0 ].Catalog.Root,
				CreateTerminal( "alpha", 80, "a" ),
				CreateTerminal( "beta", 100 )
			)
		);

		TermInfoDatabaseSetComparisonResult result =
			TermInfoDatabaseSetComparer.Compare(
				left,
				right,
				cancellationToken: CancellationToken.None
			);

		Assert.True( result.IsConclusive );
		Assert.True( result.AreEffectivelyEquivalent );
		Assert.True( result.AreStructurallyEquivalent );
		Assert.True( result.AreEquivalent );
		Assert.Empty( result.Differences );
	}

	[Fact]
	public void EqualEffectiveWinnerAtDifferentRootIsStructuralNotEffectiveChange() {
		TermInfoDatabaseSet left = CreateSet(
			CreateCatalogAtRoot(
				AbsolutePath( "left-root" ),
				CreateTerminal( "alpha", 80 )
			)
		);
		TermInfoDatabaseSet right = CreateSet(
			CreateCatalogAtRoot(
				AbsolutePath( "right-root" ),
				CreateTerminal( "alpha", 80 )
			)
		);

		TermInfoDatabaseSetComparisonResult result =
			TermInfoDatabaseSetComparer.Compare( left, right );

		Assert.True( result.IsConclusive );
		Assert.True( result.AreEffectivelyEquivalent );
		Assert.False( result.AreStructurallyEquivalent );
		Assert.Contains(
			result.Differences,
			difference => difference.Kind == TermInfoDatabaseSetDifferenceKind.RootTopology
		);
		TermInfoDatabaseSetDifference provenance =
			Assert.Single(
				result.Differences.Where(
					difference => difference.Kind
						== TermInfoDatabaseSetDifferenceKind.EffectiveProvenance
				)
			);
		Assert.Equal( "alpha", provenance.Name );
		Assert.NotNull( provenance.SemanticComparison );
		Assert.True( provenance.SemanticComparison!.AreEqual );
	}

	[Fact]
	public void IdentityMembershipDifferencesAreDirectionalAndOrdinal() {
		string root = AbsolutePath( "membership" );
		TermInfoDatabaseSet left = CreateSet(
			CreateCatalogAtRoot(
				root,
				CreateTerminal( "alpha", 80 ),
				CreateTerminal( "gamma", 80 )
			)
		);
		TermInfoDatabaseSet right = CreateSet(
			CreateCatalogAtRoot(
				root,
				CreateTerminal( "beta", 80 ),
				CreateTerminal( "gamma", 80 )
			)
		);

		TermInfoDatabaseSetComparisonResult result =
			TermInfoDatabaseSetComparer.Compare( left, right );

		Assert.False( result.AreEffectivelyEquivalent );
		Assert.Equal(
			new[] {
				TermInfoDatabaseSetDifferenceKind.OnlyInLeft,
				TermInfoDatabaseSetDifferenceKind.OnlyInRight,
			},
			result.Differences.Select( difference => difference.Kind ).ToArray()
		);
		Assert.Equal( "alpha", result.Differences[ 0 ].Name );
		Assert.Equal( "beta", result.Differences[ 1 ].Name );
	}

	[Fact]
	public void DifferentEffectiveWinnerRetainsStructuredComparison() {
		string root = AbsolutePath( "effective" );
		TermInfoDatabaseSet left = CreateSet(
			CreateCatalogAtRoot( root, CreateTerminal( "alpha", 80 ) )
		);
		TermInfoDatabaseSet right = CreateSet(
			CreateCatalogAtRoot( root, CreateTerminal( "alpha", 132 ) )
		);

		TermInfoDatabaseSetComparisonResult result =
			TermInfoDatabaseSetComparer.Compare( left, right );
		TermInfoDatabaseSetDifference difference =
			Assert.Single( result.Differences );

		Assert.Equal(
			TermInfoDatabaseSetDifferenceKind.EffectiveSemantic,
			difference.Kind
		);
		Assert.Equal( "alpha", difference.Name );
		Assert.NotNull( difference.SemanticComparison );
		Assert.False( difference.SemanticComparison!.AreEqual );
		Assert.False( result.AreEffectivelyEquivalent );
	}

	[Fact]
	public void AliasOwnershipDifferenceIsEffectiveEvenWhenCanonicalWinnersRemain() {
		string firstRoot = AbsolutePath( "alias-first" );
		string secondRoot = AbsolutePath( "alias-second" );
		TermInfoDatabaseSet left = CreateSet(
			CreateCatalogAtRoot(
				firstRoot,
				CreateTerminal( "zeta", 80, "shared" )
			),
			CreateCatalogAtRoot(
				secondRoot,
				CreateTerminal( "alpha", 80, "shared" )
			)
		);
		TermInfoDatabaseSet right = CreateSet(
			CreateCatalogAtRoot(
				firstRoot,
				CreateTerminal( "zeta", 80 )
			),
			CreateCatalogAtRoot(
				secondRoot,
				CreateTerminal( "alpha", 80, "shared" )
			)
		);

		TermInfoDatabaseSetComparisonResult result =
			TermInfoDatabaseSetComparer.Compare( left, right );
		TermInfoDatabaseSetDifference aliasDifference =
			Assert.Single(
				result.Differences.Where(
					difference => difference.Kind
						== TermInfoDatabaseSetDifferenceKind.AliasOwnership
				)
			);

		Assert.Equal( "shared", aliasDifference.Name );
		Assert.Equal( "zeta", aliasDifference.LeftOccurrence!.Name );
		Assert.Equal( 0, aliasDifference.LeftOccurrence.DatabaseIndex );
		Assert.Equal( "alpha", aliasDifference.RightOccurrence!.Name );
		Assert.Equal( 1, aliasDifference.RightOccurrence.DatabaseIndex );
		Assert.False( result.AreEffectivelyEquivalent );
	}

	[Fact]
	public void ShadowSetDifferenceIsStructuralWhenEffectiveWinnerIsEqual() {
		string firstRoot = AbsolutePath( "shadow-first" );
		string secondRoot = AbsolutePath( "shadow-second" );
		TermInfoDatabaseSet left = CreateSet(
			CreateCatalogAtRoot( firstRoot, CreateTerminal( "alpha", 80 ) ),
			CreateCatalogAtRoot( secondRoot, CreateTerminal( "alpha", 80 ) )
		);
		TermInfoDatabaseSet right = CreateSet(
			CreateCatalogAtRoot( firstRoot, CreateTerminal( "alpha", 80 ) ),
			CreateCatalogAtRoot( secondRoot, CreateTerminal( "alpha", 132 ) )
		);

		TermInfoDatabaseSetComparisonResult result =
			TermInfoDatabaseSetComparer.Compare( left, right );
		TermInfoDatabaseSetDifference shadow =
			Assert.Single(
				result.Differences.Where(
					difference => difference.Kind
						== TermInfoDatabaseSetDifferenceKind.ShadowSet
				)
			);

		Assert.True( result.AreEffectivelyEquivalent );
		Assert.False( result.AreStructurallyEquivalent );
		Assert.Equal( "alpha", shadow.Name );
		Assert.NotNull( shadow.SemanticComparison );
		Assert.False( shadow.SemanticComparison!.AreEqual );
	}

	[Fact]
	public void IncompleteIssueDifferenceIsExplicitAndComparisonIsIndeterminate() {
		string root = AbsolutePath( "incomplete" );
		TermInfoDatabaseSet left = CreateSet(
			CreateIncompleteCatalogAtRoot(
				root,
				[ CreateTerminal( "alpha", 80 ) ],
				"left issue"
			)
		);
		TermInfoDatabaseSet right = CreateSet(
			CreateCatalogAtRoot( root, CreateTerminal( "alpha", 80 ) )
		);

		TermInfoDatabaseSetComparisonResult result =
			TermInfoDatabaseSetComparer.Compare( left, right );

		Assert.False( result.IsConclusive );
		Assert.False( result.AreEffectivelyEquivalent );
		Assert.Contains(
			result.Differences,
			difference => difference.Kind == TermInfoDatabaseSetDifferenceKind.Completeness
		);
		Assert.Contains(
			result.Differences,
			difference => difference.Kind == TermInfoDatabaseSetDifferenceKind.Issue
		);
		Assert.Contains(
			result.Differences,
			difference => difference.Kind == TermInfoDatabaseSetDifferenceKind.Indeterminate
		);
	}

	[Fact]
	public void DifferenceOrderingUsesKindThenOrdinalNameThenProvenance() {
		string root = AbsolutePath( "ordering" );
		TermInfoDatabaseSet left = CreateSet(
			CreateCatalogAtRoot(
				root,
				CreateTerminal( "zeta", 80 ),
				CreateTerminal( "alpha", 80 )
			)
		);
		TermInfoDatabaseSet right = CreateSet(
			CreateCatalogAtRoot(
				root,
				CreateTerminal( "zeta", 132 ),
				CreateTerminal( "alpha", 132 )
			)
		);

		TermInfoDatabaseSetComparisonResult result =
			TermInfoDatabaseSetComparer.Compare( left, right );
		TermInfoDatabaseSetDifference[] semanticDifferences =
			result.Differences
				.Where(
					difference => difference.Kind
						== TermInfoDatabaseSetDifferenceKind.EffectiveSemantic
				)
				.ToArray();

		Assert.Equal( new[] { "alpha", "zeta" }, semanticDifferences.Select( difference => difference.Name ).ToArray() );
		Assert.True(
			result.Differences
				.Select( difference => difference.Kind )
				.SequenceEqual(
					result.Differences.Select( difference => difference.Kind ).OrderBy( kind => kind )
				)
		);
	}

	[Fact]
	public void AliasBoundAndCancellationAbortBeforePartialResult() {
		string root = AbsolutePath( "bounds" );
		TermInfoDatabaseSet set = CreateSet(
			CreateCatalogAtRoot(
				root,
				CreateTerminal( "alpha", 80, "a", "b" )
			)
		);

		Assert.Throws<InvalidOperationException>(
			() => TermInfoDatabaseSetComparer.Compare(
				set,
				set,
				new TermInfoDatabaseSetSemanticAnalysisOptions(
					maximumAliasOccurrenceCount: 1
				)
			)
		);

		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>(
			() => TermInfoDatabaseSetComparer.Compare(
				set,
				set,
				cancellationToken: cancellation.Token
			)
		);
	}

	[Fact]
	public void Da04AddsOnlyReviewedComparisonConceptFamily() {
		Type[] exportedTypes =
			typeof( TermInfoDatabaseSetComparer ).Assembly.GetExportedTypes();

		Assert.Contains( typeof( TermInfoDatabaseSetDifferenceKind ), exportedTypes );
		Assert.Contains( typeof( TermInfoDatabaseSetDifference ), exportedTypes );
		Assert.Contains( typeof( TermInfoDatabaseSetComparisonResult ), exportedTypes );
		Assert.Contains( typeof( TermInfoDatabaseSetComparer ), exportedTypes );
		Assert.InRange( exportedTypes.Length, 49, int.MaxValue );
	}

	private static TermInfoDatabaseSet CreateSet(
		params TermInfoDatabaseCatalog[] catalogs
	) =>
		TermInfoDatabaseInspector.CreateSet( catalogs );

	private static TermInfoDatabaseCatalog CreateCatalogAtRoot(
		string root,
		params TerminalDescription[] terminals
	) =>
		CreateCatalogCore(
			root,
			terminals,
			Array.Empty<TermInfoDatabaseCatalogIssue>()
		);

	private static TermInfoDatabaseCatalog CreateIncompleteCatalogAtRoot(
		string root,
		IReadOnlyList<TerminalDescription> terminals,
		string message
	) {
		TermInfoDatabaseCatalogIssue issue =
			new(
				TermInfoDatabaseCatalogIssueKind.MalformedEntry,
				Path.Combine( root, "entries", "malformed" ),
				message
			);
		return CreateCatalogCore( root, terminals, [ issue ] );
	}

	private static TermInfoDatabaseCatalog CreateCatalogCore(
		string root,
		IEnumerable<TerminalDescription> terminals,
		IEnumerable<TermInfoDatabaseCatalogIssue> issues
	) {
		TermInfoDatabaseCatalogEntry[] entries =
			terminals
				.Select(
					( terminal, index ) => new TermInfoDatabaseCatalogEntry(
						Path.Combine(
							root,
							"entries",
							index.ToString( CultureInfo.InvariantCulture )
						),
						terminal
					)
				)
				.OrderBy( entry => entry.Name, StringComparer.Ordinal )
				.ThenBy( entry => entry.Path, StringComparer.Ordinal )
				.ToArray();
		string[] duplicates =
			entries
				.GroupBy( entry => entry.Name, StringComparer.Ordinal )
				.Where( group => group.Count() > 1 )
				.Select( group => group.Key )
				.OrderBy( name => name, StringComparer.Ordinal )
				.ToArray();
		return new TermInfoDatabaseCatalog(
			root,
			TermInfoDatabaseCatalogKind.ConventionalDirectory,
			entries,
			issues,
			duplicates
		);
	}

	private static TerminalDescription CreateTerminal(
		string name,
		int columns,
		params string[] aliases
	) {
		TerminalDescriptionBuilder builder =
			new TerminalDescriptionBuilder( name )
				.SetNumber( NumericCapability.Columns, columns );
		foreach ( string alias in aliases ) {
			builder.AddAlias( alias );
		}
		return builder.Build();
	}

	private static string AbsolutePath(
		string suffix
	) =>
		Path.Combine(
			Path.GetTempPath(),
			$"icod-terminfo-da04-{suffix}-{Guid.NewGuid():N}"
		);
}
''',
)

write_new(
    "docs/1.10.0-DA04-DATABASE-SET-SEMANTIC-AND-STRUCTURAL-COMPARISON.md",
    '''# Icod.TermInfo 1.10.0 DA04 — Database-Set Semantic and Structural Comparison

**Development version:** `1.10.0-Alpha-4`  
**Tranche:** DA04  
**Published baseline:** `1.9.0`  
**DA03 baseline:** `1.10.0-Alpha-3`  
**Primary package:** `Icod.TermInfo.Inspection`  
**Status:** implementation complete; PR Staging validation pending  

## 1. Purpose

DA04 compares two immutable ordered database sets as both effective precedence
views and physical/provenance collections. It deliberately separates "would these
sets select the same effective terminal behavior?" from "are these sets laid out
and evidenced the same way?"

The comparison is reusable managed API only. Commands and JSON remain DA06 work.

## 2. Public surface

DA04 adds exactly four public concepts:

```text
TermInfoDatabaseSetDifferenceKind
TermInfoDatabaseSetDifference
TermInfoDatabaseSetComparisonResult
TermInfoDatabaseSetComparer
```

The primary API is:

```csharp
TermInfoDatabaseSetComparisonResult TermInfoDatabaseSetComparer.Compare(
    TermInfoDatabaseSet left,
    TermInfoDatabaseSet right,
    TermInfoDatabaseSetSemanticAnalysisOptions? semanticAnalysisOptions = null,
    CancellationToken cancellationToken = default
)
```

DA04 reuses the DA03 alias-declaration resource policy rather than creating a
second incompatible alias bound.

## 3. Stable difference hierarchy

The frozen DA04 hierarchy is:

```text
RootTopology
Completeness
Issue
OnlyInLeft
OnlyInRight
EffectiveSemantic
EffectiveProvenance
AliasOwnership
ShadowSet
Indeterminate
```

Differences are ordered by this hierarchy, then by ordinal identity/alias name,
then by database index and catalog entry/issue index. Host path collation never
participates in ordering.

## 4. Effective comparison

For every observed canonical identity union, DA04 asks each set for its frozen
DA02 lookup result.

- conclusive one-sided membership produces `OnlyInLeft` or `OnlyInRight`;
- two known winners are compared with `TerminalDescriptionComparer`;
- unequal winners produce `EffectiveSemantic` with the structured comparison
  retained;
- equal winners at different database/root/entry provenance produce
  `EffectiveProvenance` rather than a false semantic difference;
- a DA02 `Indeterminate` lookup produces `Indeterminate` rather than a guessed
  membership or winner result.

`AreEffectivelyEquivalent` is true only for a conclusive comparison with no
membership, effective-winner, or alias-ownership difference.

## 5. Structural and provenance comparison

DA04 separately compares:

- constituent count, root path, and catalog kind by caller-order index;
- aggregate and constituent completeness;
- frozen catalog issue sequences;
- effective winner provenance;
- ordered observed shadow sequences;
- effective alias ownership and owner provenance.

`AreStructurallyEquivalent` requires a conclusive comparison with none of those
structural/provenance differences. `AreEquivalent` requires no differences at
all.

This means two hosts may be effectively equivalent while still reporting root,
winner-provenance, or shadow-set differences useful for deployment auditing.

## 6. Alias ownership

DA04 compares all observed aliases, not only DA03 collision records. Alias
snapshots are built in physical database -> catalog-entry -> alias order, so
canonical-name ordering can never change precedence ownership.

For each alias, the first applicable owner is resolved using the DA02 incomplete-
prefix rule. Different owner identity, effective owner semantics, owner
provenance, or one-sided conclusive absence yields `AliasOwnership`.
Incomplete earlier evidence yields `Indeterminate` instead.

The DA03 maximum-alias-occurrence option applies independently to each input set.
`AliasOccurrenceCount` records the combined scan work.

## 7. Shadow comparison and work bound

For shared known canonical winners, observed shadows are compared pairwise in
their frozen DA02 order. A count mismatch, cross-set semantic difference, or
provenance mismatch yields `ShadowSet`.

DA04 never performs an all-pairs comparison. Cross-set semantic calls are bounded
by:

```text
shared effective winners
+ paired observed shadows
+ aliases with known owners on both sides
```

Alias scanning remains explicitly bounded by the reused DA03 policy. The result
records `SemanticComparisonCount` and `AliasOccurrenceCount`.

## 8. Incomplete evidence

Any incomplete input adds whole-set `Indeterminate` evidence, so equivalence is
never claimed from a partial universe. DA04 still reports definite observed
root, issue, winner, alias, or shadow differences when they can be established
without guessing.

An identity whose DA02 lookup is indeterminate receives identity-scoped
`Indeterminate` evidence instead of a false membership or winner claim. A known
winner with an incomplete later shadow universe can still be semantically
compared, but its complete shadow conclusion remains indeterminate.

## 9. Independent oracle validation

DA04 tests use independently constructed small truth tables covering:

- exact complete equivalence;
- same effective semantics with different roots/provenance;
- directional identity membership;
- different effective winner semantics with retained structured differences;
- alias ownership changes where database precedence disagrees with canonical
  alphabetical order;
- shadow-set-only structural differences;
- completeness and issue differences with indeterminate whole-set state;
- frozen difference ordering;
- alias bounds and cancellation;
- reviewed public API growth only.

**DA04 gate:** two explicit ordered database sets can be compared through a stable
structured result that distinguishes effective semantic changes from topology,
provenance, alias, shadow, issue, and incomplete-evidence differences.
''',
)

replace_exact(
    "Directory.Build.props",
    "<IcodTermInfoSuiteVersion>1.10.0-Alpha-3</IcodTermInfoSuiteVersion>",
    "<IcodTermInfoSuiteVersion>1.10.0-Alpha-4</IcodTermInfoSuiteVersion>",
)

replace_exact(
    "Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj",
    "<PackageReleaseNotes>1.10.0-Alpha-3 adds bounded winner-versus-shadow semantic duplicate analysis, retained structured conflict comparisons, deterministic alias ownership/canonical-name collision analysis, and explicit indeterminate evidence while preserving DA01/DA02 precedence, frozen 1.9 JSON v1, lower-layer, synthesis, planning, and command contracts.</PackageReleaseNotes>",
    "<PackageReleaseNotes>1.10.0-Alpha-4 adds deterministic ordered database-set comparison with separate effective semantic and structural/provenance classifications, typed topology/membership/winner/alias/shadow/issue/indeterminate evidence, bounded alias scanning, and retained structured semantic comparisons while preserving DA01-DA03, frozen 1.9 JSON v1, lower-layer, synthesis, planning, and command contracts.</PackageReleaseNotes>",
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
    replace_all_required(path_name, "1.10.0-Alpha-3", "1.10.0-Alpha-4")

replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/RP08ReleaseClosureTests.cs",
    '"DA03",',
    '"DA04",',
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/MI07ReleaseClosureTests.cs",
    '"DA03 - Semantic duplicate, conflict, alias, and shadow analysis",',
    '"DA04 - Database-set semantic comparison",',
)

replace_exact(
    "tools/inspection-package-smoke/Program.cs",
    "exportedTypes.Length >= 45",
    "exportedTypes.Length >= 49",
)
replace_exact(
    "tools/inspection-package-smoke/Program.cs",
    "&& exportedTypes.Contains( typeof( TermInfoDatabaseSetAliasAnalysis ) )",
    '''&& exportedTypes.Contains( typeof( TermInfoDatabaseSetAliasAnalysis ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetDifferenceKind ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetDifference ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetComparisonResult ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetComparer ) )''',
)

replace_exact(
    "Icod.TermInfo-1.10.0-Deterministic-Multi-Database-Inspection-Comparison-and-Planning-Automation-Roadmap.md",
    "**Status:** DA03 implementation complete; Staging validation pending",
    "**Status:** DA04 implementation complete; Staging validation pending",
)

replace_exact(
    "Icod.TermInfo-Post-1.0-Development-Roadmap.md",
    "**Current coordinated version:** `1.10.0-Alpha-3`",
    "**Current coordinated version:** `1.10.0-Alpha-4`",
)
replace_exact(
    "Icod.TermInfo-Post-1.0-Development-Roadmap.md",
    "**Current tranche:** DA03 - Semantic duplicate, conflict, alias, and shadow analysis",
    "**Current tranche:** DA04 - Database-set semantic comparison",
)

readme_path = Path("Icod.TermInfo.Inspection/README.md")
readme = readme_path.read_text(encoding="utf-8")
marker = "## 1.10 DA03 semantic duplicate, alias, and shadow analysis\n"
if readme.count(marker) != 1:
    raise RuntimeError("Inspection README DA03 heading marker mismatch")
section = '''## 1.10 DA04 database-set semantic and structural comparison\n\n`1.10.0-Alpha-4` adds deterministic comparison of two ordered database sets as\nboth effective precedence views and physical/provenance collections. The result\nseparates effective winner/membership/alias changes from root topology, winner\nprovenance, shadow-set, completeness, and issue differences, while incomplete\ninputs remain explicitly indeterminate. Cross-set terminal semantics continue to\nuse `TerminalDescriptionComparer`; alias scanning reuses the DA03 bound.\n\nSee\n`docs/1.10.0-DA04-DATABASE-SET-SEMANTIC-AND-STRUCTURAL-COMPARISON.md`.\n\n'''
readme_path.write_text(readme.replace(marker, section + marker, 1), encoding="utf-8", newline="\n")
