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
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSetSemanticRelationship.cs",
    '''namespace Icod.TermInfo.Inspection;

/// <summary>
/// Classifies deterministic semantic evidence for repeated database-set
/// identities and alias collisions.
/// </summary>
public enum TermInfoDatabaseSetSemanticRelationship {
	/// <summary>
	/// The relevant effective terminal descriptions compare equal.
	/// </summary>
	SemanticallyEqual = 0,

	/// <summary>
	/// At least one relevant effective terminal description or canonical owner
	/// conflicts with the selected precedence evidence.
	/// </summary>
	SemanticallyDifferent = 1,

	/// <summary>
	/// Incomplete input prevents a conclusive semantic classification.
	/// </summary>
	Indeterminate = 2,
}
''',
)

write_new(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSetSemanticAnalysisOptions.cs",
    '''namespace Icod.TermInfo.Inspection;

/// <summary>
/// Configures deterministic resource bounds for database-set semantic analysis.
/// </summary>
public sealed class TermInfoDatabaseSetSemanticAnalysisOptions {
	/// <summary>
	/// The default maximum number of alias declarations scanned across all
	/// physical occurrences.
	/// </summary>
	public const int DefaultMaximumAliasOccurrenceCount = 1_048_576;

	/// <summary>
	/// The largest supported caller-selected alias declaration bound.
	/// </summary>
	public const int MaximumSupportedAliasOccurrenceCount = 4_194_304;

	/// <summary>
	/// Initializes the canonical semantic-analysis resource policy.
	/// </summary>
	public TermInfoDatabaseSetSemanticAnalysisOptions()
		: this(
			DefaultMaximumAliasOccurrenceCount
		) {
	}

	/// <summary>
	/// Initializes an explicit deterministic alias-scan bound.
	/// </summary>
	/// <param name="maximumAliasOccurrenceCount">
	/// The maximum number of alias declarations scanned across all physical
	/// occurrences.
	/// </param>
	public TermInfoDatabaseSetSemanticAnalysisOptions(
		int maximumAliasOccurrenceCount
	) {
		if ( maximumAliasOccurrenceCount < 1
			|| maximumAliasOccurrenceCount > MaximumSupportedAliasOccurrenceCount ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumAliasOccurrenceCount ),
				maximumAliasOccurrenceCount,
				$"The maximum alias occurrence count must be between 1 and {MaximumSupportedAliasOccurrenceCount}."
			);
		}

		MaximumAliasOccurrenceCount = maximumAliasOccurrenceCount;
	}

	/// <summary>
	/// Gets the maximum number of alias declarations scanned during one analysis.
	/// </summary>
	public int MaximumAliasOccurrenceCount {
		get;
	}
}
''',
)

write_new(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSetShadowAnalysis.cs",
    '''namespace Icod.TermInfo.Inspection;

/// <summary>
/// Describes one observed later occurrence relative to canonical precedence
/// evidence.
/// </summary>
public sealed class TermInfoDatabaseSetShadowAnalysis {
	internal TermInfoDatabaseSetShadowAnalysis(
		TermInfoDatabaseSetOccurrence occurrence,
		TermInfoDatabaseSetSemanticRelationship relationship,
		TermInfoComparisonResult? comparison
	) {
		ArgumentNullException.ThrowIfNull( occurrence );
		if ( relationship == TermInfoDatabaseSetSemanticRelationship.Indeterminate ) {
			if ( comparison is not null ) {
				throw new ArgumentException(
					"Indeterminate shadow evidence cannot contain a semantic comparison.",
					nameof( comparison )
				);
			}
		} else {
			ArgumentNullException.ThrowIfNull( comparison );
			bool expectedEqual =
				relationship == TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual;
			if ( comparison.AreEqual != expectedEqual ) {
				throw new ArgumentException(
					"The comparison result does not match the requested semantic relationship.",
					nameof( comparison )
				);
			}
		}

		Occurrence = occurrence;
		Relationship = relationship;
		Comparison = comparison;
	}

	/// <summary>
	/// Gets the observed later physical occurrence.
	/// </summary>
	public TermInfoDatabaseSetOccurrence Occurrence {
		get;
	}

	/// <summary>
	/// Gets the semantic relationship to the precedence winner, or indeterminate
	/// when no winner can be established.
	/// </summary>
	public TermInfoDatabaseSetSemanticRelationship Relationship {
		get;
	}

	/// <summary>
	/// Gets the frozen structured comparison when a precedence winner is known.
	/// </summary>
	public TermInfoComparisonResult? Comparison {
		get;
	}
}
''',
)

write_new(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSetIdentityAnalysis.cs",
    '''namespace Icod.TermInfo.Inspection;

/// <summary>
/// Describes deterministic semantic evidence for one repeated canonical identity.
/// </summary>
public sealed class TermInfoDatabaseSetIdentityAnalysis {
	private readonly IReadOnlyList<TermInfoDatabaseSetShadowAnalysis> _equalShadows;
	private readonly IReadOnlyList<TermInfoDatabaseSetShadowAnalysis> _conflictingShadows;
	private readonly IReadOnlyList<TermInfoDatabaseSetShadowAnalysis> _indeterminateShadows;

	internal TermInfoDatabaseSetIdentityAnalysis(
		TermInfoDatabaseSetIdentity identity,
		TermInfoDatabaseSetLookupResult lookup,
		TermInfoDatabaseSetSemanticRelationship relationship,
		IEnumerable<TermInfoDatabaseSetShadowAnalysis> shadows
	) {
		ArgumentNullException.ThrowIfNull( identity );
		ArgumentNullException.ThrowIfNull( lookup );
		ArgumentNullException.ThrowIfNull( shadows );
		if ( identity.Occurrences.Count < 2 ) {
			throw new ArgumentException(
				"Identity semantic analysis requires a repeated canonical identity.",
				nameof( identity )
			);
		}
		if ( !string.Equals(
			identity.Name,
			lookup.Name,
			StringComparison.Ordinal
		) ) {
			throw new ArgumentException(
				"Lookup evidence must identify the analyzed canonical identity.",
				nameof( lookup )
			);
		}

		TermInfoDatabaseSetShadowAnalysis[] shadowArray = shadows.ToArray();
		if ( shadowArray.Any( shadow => shadow is null ) ) {
			throw new ArgumentException(
				"Identity shadow analysis cannot contain null.",
				nameof( shadows )
			);
		}

		TermInfoDatabaseSetShadowAnalysis[] equalShadows =
			shadowArray
				.Where(
					shadow => shadow.Relationship
						== TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual
				)
				.ToArray();
		TermInfoDatabaseSetShadowAnalysis[] conflictingShadows =
			shadowArray
				.Where(
					shadow => shadow.Relationship
						== TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent
				)
				.ToArray();
		TermInfoDatabaseSetShadowAnalysis[] indeterminateShadows =
			shadowArray
				.Where(
					shadow => shadow.Relationship
						== TermInfoDatabaseSetSemanticRelationship.Indeterminate
				)
				.ToArray();

		if ( relationship == TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual ) {
			if ( lookup.Status != TermInfoDatabaseSetLookupStatus.WinnerKnown
				|| !lookup.IsObservationComplete
				|| conflictingShadows.Length != 0
				|| indeterminateShadows.Length != 0 ) {
				throw new ArgumentException(
					"Semantically equal repeated identities require complete known-winner evidence.",
					nameof( relationship )
				);
			}
		} else if ( relationship == TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent ) {
			if ( conflictingShadows.Length == 0 ) {
				throw new ArgumentException(
					"A semantic difference requires at least one conflicting observed shadow.",
					nameof( relationship )
				);
			}
		} else if ( relationship == TermInfoDatabaseSetSemanticRelationship.Indeterminate ) {
			if ( lookup.Status != TermInfoDatabaseSetLookupStatus.Indeterminate
				&& lookup.IsObservationComplete ) {
				throw new ArgumentException(
					"Indeterminate identity evidence requires incomplete or indeterminate lookup evidence.",
					nameof( relationship )
				);
			}
		} else {
			throw new ArgumentOutOfRangeException( nameof( relationship ) );
		}

		Identity = identity;
		Lookup = lookup;
		Relationship = relationship;
		Shadows = Array.AsReadOnly( shadowArray );
		_equalShadows = Array.AsReadOnly( equalShadows );
		_conflictingShadows = Array.AsReadOnly( conflictingShadows );
		_indeterminateShadows = Array.AsReadOnly( indeterminateShadows );
	}

	/// <summary>
	/// Gets the repeated canonical identity.
	/// </summary>
	public TermInfoDatabaseSetIdentity Identity {
		get;
	}

	/// <summary>
	/// Gets the frozen DA02 precedence evidence.
	/// </summary>
	public TermInfoDatabaseSetLookupResult Lookup {
		get;
	}

	/// <summary>
	/// Gets the aggregate semantic relationship for the repeated identity.
	/// </summary>
	public TermInfoDatabaseSetSemanticRelationship Relationship {
		get;
	}

	/// <summary>
	/// Gets observed later-occurrence analyses in deterministic occurrence order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetShadowAnalysis> Shadows {
		get;
	}

	/// <summary>
	/// Gets observed shadows semantically equal to the precedence winner.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetShadowAnalysis> EqualShadows =>
		_equalShadows;

	/// <summary>
	/// Gets observed shadows semantically different from the precedence winner.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetShadowAnalysis> ConflictingShadows =>
		_conflictingShadows;

	/// <summary>
	/// Gets later observed occurrences which cannot be compared to a known winner.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetShadowAnalysis> IndeterminateShadows =>
		_indeterminateShadows;

	/// <summary>
	/// Gets whether the complete occurrence universe was observed.
	/// </summary>
	public bool IsComplete =>
		Lookup.IsObservationComplete;
}
''',
)

write_new(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSetAliasAnalysis.cs",
    '''namespace Icod.TermInfo.Inspection;

/// <summary>
/// Describes one repeated or canonical-name-colliding alias across an ordered
/// database set.
/// </summary>
public sealed class TermInfoDatabaseSetAliasAnalysis {
	internal TermInfoDatabaseSetAliasAnalysis(
		string alias,
		IEnumerable<TermInfoDatabaseSetOccurrence> occurrences,
		IEnumerable<string> canonicalNames,
		TermInfoDatabaseSetOccurrence? precedenceOwner,
		TermInfoDatabaseSetIdentity? matchingCanonicalIdentity,
		TermInfoDatabaseSetSemanticRelationship relationship,
		bool isComplete,
		IEnumerable<int> blockingDatabaseIndices
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( alias );
		ArgumentNullException.ThrowIfNull( occurrences );
		ArgumentNullException.ThrowIfNull( canonicalNames );
		ArgumentNullException.ThrowIfNull( blockingDatabaseIndices );

		TermInfoDatabaseSetOccurrence[] occurrenceArray = occurrences.ToArray();
		string[] canonicalNameArray = canonicalNames.ToArray();
		int[] blockingArray = blockingDatabaseIndices.ToArray();
		if ( occurrenceArray.Length == 0
			|| occurrenceArray.Any( occurrence => occurrence is null ) ) {
			throw new ArgumentException(
				"Alias analysis requires at least one non-null physical occurrence.",
				nameof( occurrences )
			);
		}
		if ( canonicalNameArray.Length == 0
			|| canonicalNameArray.Any( string.IsNullOrWhiteSpace )
			|| !canonicalNameArray.SequenceEqual(
				canonicalNameArray.OrderBy( name => name, StringComparer.Ordinal ),
				StringComparer.Ordinal
			) ) {
			throw new ArgumentException(
				"Canonical alias owners must be non-empty and ordinally ordered.",
				nameof( canonicalNames )
			);
		}
		if ( precedenceOwner is not null
			&& !ReferenceEquals( precedenceOwner, occurrenceArray[ 0 ] ) ) {
			throw new ArgumentException(
				"Alias precedence ownership must select the first observed alias occurrence.",
				nameof( precedenceOwner )
			);
		}
		if ( matchingCanonicalIdentity is not null
			&& !string.Equals(
				matchingCanonicalIdentity.Name,
				alias,
				StringComparison.Ordinal
			) ) {
			throw new ArgumentException(
				"The matching canonical identity must exactly equal the alias.",
				nameof( matchingCanonicalIdentity )
			);
		}
		if ( blockingArray.Any( index => index < 0 )
			|| blockingArray.Distinct().Count() != blockingArray.Length ) {
			throw new ArgumentException(
				"Alias blocking database indices must be unique and non-negative.",
				nameof( blockingDatabaseIndices )
			);
		}
		if ( relationship == TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual
			&& !isComplete ) {
			throw new ArgumentException(
				"Semantically equal alias evidence requires complete database-set evidence.",
				nameof( relationship )
			);
		}
		if ( relationship == TermInfoDatabaseSetSemanticRelationship.Indeterminate
			&& isComplete ) {
			throw new ArgumentException(
				"Indeterminate alias evidence requires incomplete database-set evidence.",
				nameof( relationship )
			);
		}

		Alias = alias;
		Occurrences = Array.AsReadOnly( occurrenceArray );
		CanonicalNames = Array.AsReadOnly( canonicalNameArray );
		PrecedenceOwner = precedenceOwner;
		MatchingCanonicalIdentity = matchingCanonicalIdentity;
		Relationship = relationship;
		IsComplete = isComplete;
		BlockingDatabaseIndices = Array.AsReadOnly( blockingArray );
	}

	/// <summary>
	/// Gets the exact ordinal alias string.
	/// </summary>
	public string Alias {
		get;
	}

	/// <summary>
	/// Gets physical declarations in database and catalog-entry order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetOccurrence> Occurrences {
		get;
	}

	/// <summary>
	/// Gets distinct canonical owner names in ordinal order.
	/// </summary>
	public IReadOnlyList<string> CanonicalNames {
		get;
	}

	/// <summary>
	/// Gets the first declared alias owner when earlier incomplete databases do not
	/// block precedence evidence.
	/// </summary>
	public TermInfoDatabaseSetOccurrence? PrecedenceOwner {
		get;
	}

	/// <summary>
	/// Gets the canonical database-set identity whose name exactly matches this
	/// alias, when such an identity is observed.
	/// </summary>
	public TermInfoDatabaseSetIdentity? MatchingCanonicalIdentity {
		get;
	}

	/// <summary>
	/// Gets the aggregate semantic or ownership relationship.
	/// </summary>
	public TermInfoDatabaseSetSemanticRelationship Relationship {
		get;
	}

	/// <summary>
	/// Gets whether all constituent databases were inspected completely.
	/// </summary>
	public bool IsComplete {
		get;
	}

	/// <summary>
	/// Gets incomplete database indices which prevent a conclusive precedence owner.
	/// </summary>
	public IReadOnlyList<int> BlockingDatabaseIndices {
		get;
	}

	/// <summary>
	/// Gets whether more than one canonical name declares the alias.
	/// </summary>
	public bool HasMultipleCanonicalOwners =>
		CanonicalNames.Count > 1;

	/// <summary>
	/// Gets whether the alias also exactly names an observed canonical identity.
	/// </summary>
	public bool MatchesCanonicalName =>
		MatchingCanonicalIdentity is not null;
}
''',
)

write_new(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSetSemanticAnalysis.cs",
    '''namespace Icod.TermInfo.Inspection;

/// <summary>
/// Contains deterministic semantic analysis of repeated canonical identities and
/// alias collisions in one ordered database set.
/// </summary>
public sealed class TermInfoDatabaseSetSemanticAnalysis {
	internal TermInfoDatabaseSetSemanticAnalysis(
		IEnumerable<TermInfoDatabaseSetIdentityAnalysis> repeatedIdentities,
		IEnumerable<TermInfoDatabaseSetAliasAnalysis> aliases,
		int semanticComparisonCount,
		int aliasOccurrenceCount,
		bool isComplete
	) {
		ArgumentNullException.ThrowIfNull( repeatedIdentities );
		ArgumentNullException.ThrowIfNull( aliases );
		if ( semanticComparisonCount < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( semanticComparisonCount ) );
		}
		if ( aliasOccurrenceCount < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( aliasOccurrenceCount ) );
		}

		TermInfoDatabaseSetIdentityAnalysis[] identityArray =
			repeatedIdentities.ToArray();
		TermInfoDatabaseSetAliasAnalysis[] aliasArray = aliases.ToArray();
		if ( identityArray.Any( identity => identity is null ) ) {
			throw new ArgumentException(
				"Repeated identity analyses cannot contain null.",
				nameof( repeatedIdentities )
			);
		}
		if ( aliasArray.Any( alias => alias is null ) ) {
			throw new ArgumentException(
				"Alias analyses cannot contain null.",
				nameof( aliases )
			);
		}

		RepeatedIdentities = Array.AsReadOnly( identityArray );
		Aliases = Array.AsReadOnly( aliasArray );
		SemanticComparisonCount = semanticComparisonCount;
		AliasOccurrenceCount = aliasOccurrenceCount;
		IsComplete = isComplete;
	}

	/// <summary>
	/// Gets repeated canonical identities in ordinal canonical-name order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetIdentityAnalysis> RepeatedIdentities {
		get;
	}

	/// <summary>
	/// Gets repeated or canonical-name-colliding aliases in ordinal alias order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetAliasAnalysis> Aliases {
		get;
	}

	/// <summary>
	/// Gets the number of winner-versus-shadow calls made to the frozen semantic
	/// comparer.
	/// </summary>
	public int SemanticComparisonCount {
		get;
	}

	/// <summary>
	/// Gets the number of alias declarations scanned while constructing the alias
	/// collision index.
	/// </summary>
	public int AliasOccurrenceCount {
		get;
	}

	/// <summary>
	/// Gets whether every constituent database was inspected completely.
	/// </summary>
	public bool IsComplete {
		get;
	}

	/// <summary>
	/// Gets whether any repeated identity or alias collision is definitively
	/// semantically different.
	/// </summary>
	public bool HasSemanticDifferences =>
		RepeatedIdentities.Any(
			identity => identity.Relationship
				== TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent
		)
		|| Aliases.Any(
			alias => alias.Relationship
				== TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent
		);

	/// <summary>
	/// Gets whether incomplete input leaves any analysis conclusion indeterminate.
	/// </summary>
	public bool HasIndeterminateEvidence =>
		!IsComplete
		|| RepeatedIdentities.Any(
			identity => identity.Relationship
				== TermInfoDatabaseSetSemanticRelationship.Indeterminate
		)
		|| Aliases.Any(
			alias => alias.Relationship
				== TermInfoDatabaseSetSemanticRelationship.Indeterminate
		);
}
''',
)

replace_exact(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSet.cs",
    "public sealed class TermInfoDatabaseSet {",
    "public sealed partial class TermInfoDatabaseSet {",
)

write_new(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSet.SemanticAnalysis.cs",
    '''namespace Icod.TermInfo.Inspection;

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
		foreach ( TermInfoDatabaseSetIdentity identity in Identities ) {
			foreach ( TermInfoDatabaseSetOccurrence occurrence in identity.Occurrences ) {
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
				&& matchingCanonicalIdentity is null ) {
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
''',
)

write_new(
    "tests/Icod.TermInfo.Inspection.Tests/src/DA03SemanticDuplicateAliasShadowTests.cs",
    '''using System.Globalization;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class DA03SemanticDuplicateAliasShadowTests {
	[Fact]
	public void EqualCanonicalShadowsUseFrozenSemanticComparer() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog(
						"equal-a",
						CreateTerminal( "same", 80, "shared" )
					),
					CreateCatalog(
						"equal-b",
						CreateTerminal( "same", 80, "shared" )
					),
				]
			);

		TermInfoDatabaseSetSemanticAnalysis analysis =
			set.AnalyzeSemantics(
				cancellationToken: TestContext.Current.CancellationToken
			);
		TermInfoDatabaseSetIdentityAnalysis identity =
			Assert.Single( analysis.RepeatedIdentities );
		TermInfoDatabaseSetShadowAnalysis shadow =
			Assert.Single( identity.Shadows );

		Assert.Equal(
			TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual,
			identity.Relationship
		);
		Assert.Equal(
			TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual,
			shadow.Relationship
		);
		Assert.NotNull( shadow.Comparison );
		Assert.True( shadow.Comparison!.AreEqual );
		Assert.Single( identity.EqualShadows );
		Assert.Empty( identity.ConflictingShadows );
		Assert.Equal( 1, analysis.SemanticComparisonCount );
	}

	[Fact]
	public void DifferentCanonicalShadowRetainsStructuredComparison() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog(
						"different-a",
						CreateTerminal( "same", 80 )
					),
					CreateCatalog(
						"different-b",
						CreateTerminal( "same", 132 )
					),
				]
			);

		TermInfoDatabaseSetSemanticAnalysis analysis =
			set.AnalyzeSemantics(
				cancellationToken: TestContext.Current.CancellationToken
			);
		TermInfoDatabaseSetIdentityAnalysis identity =
			Assert.Single( analysis.RepeatedIdentities );
		TermInfoDatabaseSetShadowAnalysis conflict =
			Assert.Single( identity.ConflictingShadows );

		Assert.Equal(
			TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent,
			identity.Relationship
		);
		Assert.NotNull( conflict.Comparison );
		Assert.False( conflict.Comparison!.AreEqual );
		Assert.NotEmpty( conflict.Comparison.Differences );
		Assert.True( analysis.HasSemanticDifferences );
	}

	[Fact]
	public void IndeterminateWinnerProducesIndeterminateShadowEvidenceWithoutComparison() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateIncompleteCatalog(
						"blocking",
						[ CreateTerminal( "same", 80 ) ]
					),
					CreateCatalog(
						"later",
						CreateTerminal( "same", 80 )
					),
				]
			);

		TermInfoDatabaseSetSemanticAnalysis analysis =
			set.AnalyzeSemantics(
				cancellationToken: TestContext.Current.CancellationToken
			);
		TermInfoDatabaseSetIdentityAnalysis identity =
			Assert.Single( analysis.RepeatedIdentities );
		TermInfoDatabaseSetShadowAnalysis shadow =
			Assert.Single( identity.IndeterminateShadows );

		Assert.Equal(
			TermInfoDatabaseSetSemanticRelationship.Indeterminate,
			identity.Relationship
		);
		Assert.Equal(
			TermInfoDatabaseSetLookupStatus.Indeterminate,
			identity.Lookup.Status
		);
		Assert.Null( shadow.Comparison );
		Assert.Equal( 0, analysis.SemanticComparisonCount );
		Assert.True( analysis.HasIndeterminateEvidence );
	}

	[Fact]
	public void DefiniteConflictRemainsDifferentEvenWhenLaterDatabaseIsIncomplete() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog(
						"winner",
						CreateTerminal( "same", 80 )
					),
					CreateIncompleteCatalog(
						"later-incomplete",
						[ CreateTerminal( "same", 132 ) ]
					),
				]
			);

		TermInfoDatabaseSetSemanticAnalysis analysis =
			set.AnalyzeSemantics(
				cancellationToken: TestContext.Current.CancellationToken
			);
		TermInfoDatabaseSetIdentityAnalysis identity =
			Assert.Single( analysis.RepeatedIdentities );

		Assert.Equal(
			TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent,
			identity.Relationship
		);
		Assert.False( identity.IsComplete );
		Assert.True( analysis.HasSemanticDifferences );
		Assert.True( analysis.HasIndeterminateEvidence );
	}

	[Fact]
	public void RepeatedAliasUnderEqualCanonicalIdentityIsEqualAndOrdered() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog(
						"alias-a",
						CreateTerminal( "canonical", 80, "shared" )
					),
					CreateCatalog(
						"alias-b",
						CreateTerminal( "canonical", 80, "shared" )
					),
				]
			);

		TermInfoDatabaseSetSemanticAnalysis analysis =
			set.AnalyzeSemantics(
				cancellationToken: TestContext.Current.CancellationToken
			);
		TermInfoDatabaseSetAliasAnalysis alias = Assert.Single( analysis.Aliases );
		TermInfoDatabaseSetOccurrence owner =
			Assert.IsType<TermInfoDatabaseSetOccurrence>( alias.PrecedenceOwner );

		Assert.Equal( "shared", alias.Alias );
		Assert.Equal(
			TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual,
			alias.Relationship
		);
		Assert.Equal( new[] { "canonical" }, alias.CanonicalNames );
		Assert.False( alias.HasMultipleCanonicalOwners );
		Assert.Equal( 0, owner.DatabaseIndex );
		Assert.Equal( new[] { 0, 1 }, alias.Occurrences.Select( occurrence => occurrence.DatabaseIndex ).ToArray() );
	}

	[Fact]
	public void SameAliasOwnedByDifferentCanonicalNamesIsExplicitConflict() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog(
						"owner-a",
						CreateTerminal( "alpha", 80, "shared" )
					),
					CreateCatalog(
						"owner-b",
						CreateTerminal( "beta", 80, "shared" )
					),
				]
			);

		TermInfoDatabaseSetAliasAnalysis alias =
			Assert.Single(
				set.AnalyzeSemantics(
					cancellationToken: TestContext.Current.CancellationToken
				).Aliases
			);

		Assert.Equal(
			TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent,
			alias.Relationship
		);
		Assert.Equal( new[] { "alpha", "beta" }, alias.CanonicalNames );
		Assert.True( alias.HasMultipleCanonicalOwners );
	}

	[Fact]
	public void AliasMatchingAnotherCanonicalNameIsDistinctCollisionEvidence() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog(
						"canonical-collision",
						CreateTerminal( "alpha", 80, "beta" ),
						CreateTerminal( "beta", 80 )
					),
				]
			);

		TermInfoDatabaseSetAliasAnalysis alias =
			Assert.Single(
				set.AnalyzeSemantics(
					cancellationToken: TestContext.Current.CancellationToken
				).Aliases
			);
		TermInfoDatabaseSetIdentity matching =
			Assert.IsType<TermInfoDatabaseSetIdentity>( alias.MatchingCanonicalIdentity );

		Assert.Equal( "beta", alias.Alias );
		Assert.True( alias.MatchesCanonicalName );
		Assert.Equal( "beta", matching.Name );
		Assert.Equal(
			TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent,
			alias.Relationship
		);
	}

	[Fact]
	public void IncompleteDatabaseSetMakesOtherwiseUncontestedAliasIndeterminate() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog(
						"alias-winner",
						CreateTerminal( "canonical", 80, "shared" )
					),
					CreateIncompleteCatalog(
						"unknown-later",
						Array.Empty<TerminalDescription>()
					),
				]
			);

		TermInfoDatabaseSetSemanticAnalysis analysis =
			set.AnalyzeSemantics(
				cancellationToken: TestContext.Current.CancellationToken
			);
		TermInfoDatabaseSetAliasAnalysis alias = Assert.Single( analysis.Aliases );

		Assert.Equal(
			TermInfoDatabaseSetSemanticRelationship.Indeterminate,
			alias.Relationship
		);
		Assert.False( alias.IsComplete );
		Assert.NotNull( alias.PrecedenceOwner );
		Assert.Empty( alias.BlockingDatabaseIndices );
	}

	[Fact]
	public void AliasScanBoundAndCancellationPreventPartialAnalysis() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog(
						"bounds",
						CreateTerminal( "one", 80, "a", "b" )
					),
				]
			);

		Assert.Throws<InvalidOperationException>(
			() => set.AnalyzeSemantics(
				new TermInfoDatabaseSetSemanticAnalysisOptions(
					maximumAliasOccurrenceCount: 1
				),
				TestContext.Current.CancellationToken
			)
		);

		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>(
			() => set.AnalyzeSemantics(
				cancellationToken: cancellation.Token
			)
		);
	}

	[Fact]
	public void Da03AddsOnlyReviewedSemanticAnalysisConceptFamily() {
		Type[] exportedTypes =
			typeof( TermInfoDatabaseSetSemanticAnalysis ).Assembly.GetExportedTypes();

		foreach (
			Type expected
			in new[] {
				typeof( TermInfoDatabaseSetSemanticRelationship ),
				typeof( TermInfoDatabaseSetSemanticAnalysisOptions ),
				typeof( TermInfoDatabaseSetSemanticAnalysis ),
				typeof( TermInfoDatabaseSetIdentityAnalysis ),
				typeof( TermInfoDatabaseSetShadowAnalysis ),
				typeof( TermInfoDatabaseSetAliasAnalysis ),
			}
		) {
			Assert.Contains( expected, exportedTypes );
		}
		Assert.InRange( exportedTypes.Length, 45, int.MaxValue );
	}

	private static TermInfoDatabaseCatalog CreateCatalog(
		string rootName,
		params TerminalDescription[] terminals
	) =>
		CreateCatalogCore(
			rootName,
			terminals,
			Array.Empty<TermInfoDatabaseCatalogIssue>()
		);

	private static TermInfoDatabaseCatalog CreateIncompleteCatalog(
		string rootName,
		IReadOnlyList<TerminalDescription> terminals
	) {
		string root = AbsolutePath( rootName );
		TermInfoDatabaseCatalogIssue issue =
			new(
				TermInfoDatabaseCatalogIssueKind.MalformedEntry,
				Path.Combine( root, "entries", "malformed" ),
				"DA03 incomplete fixture."
			);
		return CreateCatalogCore( rootName, terminals, [ issue ] );
	}

	private static TermInfoDatabaseCatalog CreateCatalogCore(
		string rootName,
		IEnumerable<TerminalDescription> terminals,
		IEnumerable<TermInfoDatabaseCatalogIssue> issues
	) {
		string root = AbsolutePath( rootName );
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
			$"icod-terminfo-da03-{suffix}-{Guid.NewGuid():N}"
		);
}
''',
)

write_new(
    "docs/1.10.0-DA03-SEMANTIC-DUPLICATE-CONFLICT-ALIAS-AND-SHADOW-ANALYSIS.md",
    '''# Icod.TermInfo 1.10.0 DA03 — Semantic Duplicate, Conflict, Alias, and Shadow Analysis

**Development version:** `1.10.0-Alpha-3`  
**Tranche:** DA03  
**Published baseline:** `1.9.0`  
**DA02 baseline:** `1.10.0-Alpha-2`  
**Primary package:** `Icod.TermInfo.Inspection`  
**Status:** implementation complete; PR Staging validation pending  

## 1. Purpose

DA03 layers semantic explanation onto the frozen DA02 ordered-precedence model.
It classifies repeated canonical identities and alias collisions without changing
lookup precedence, reparsing files, comparing compiled bytes, or inventing a
second semantic comparer.

The authoritative comparison remains:

```csharp
TerminalDescriptionComparer.Compare( winner, observedShadow )
```

No all-pairs comparison is performed.

## 2. Public surface

DA03 adds:

```text
TermInfoDatabaseSetSemanticRelationship
TermInfoDatabaseSetSemanticAnalysisOptions
TermInfoDatabaseSetSemanticAnalysis
TermInfoDatabaseSetIdentityAnalysis
TermInfoDatabaseSetShadowAnalysis
TermInfoDatabaseSetAliasAnalysis
```

`TermInfoDatabaseSet` gains:

```csharp
TermInfoDatabaseSetSemanticAnalysis AnalyzeSemantics(
    TermInfoDatabaseSetSemanticAnalysisOptions? options = null,
    CancellationToken cancellationToken = default
)
```

The relationship domain is exactly:

```text
SemanticallyEqual
SemanticallyDifferent
Indeterminate
```

## 3. Canonical duplicate and shadow semantics

Only repeated canonical identities receive identity-analysis records.

When DA02 has a known winner, every observed later occurrence is compared exactly
once to that winner. The structured `TermInfoComparisonResult` is retained on the
shadow record. Therefore callers can inspect the exact capability or identity
metadata differences without reacquiring or reparsing either entry.

The aggregate identity relationship is:

- `SemanticallyDifferent` when any observed shadow differs from the winner;
- `SemanticallyEqual` only when all observed shadows compare equal and the full
  occurrence universe is complete;
- `Indeterminate` when no winner is known, or when all observed shadows are equal
  but incomplete later evidence could hide another occurrence.

A definite observed difference remains a definite conflict even if a later
constituent database is incomplete.

## 4. Alias collision semantics

Aliases remain distinct from canonical identities. DA03 builds an ordinal alias
index from the immutable DA01 occurrence evidence and emits analysis only for an
alias which:

- is declared by more than one physical occurrence; or
- exactly matches an observed canonical identity name.

Each alias analysis retains ordered physical declarations, distinct canonical
owner names, the first declared precedence owner when earlier incomplete roots do
not block it, an exact canonical-name collision when present, completeness, and
blocking database indices.

Alias classification is conservative:

- different canonical owner names are a definite conflict;
- an alias which names another canonical identity is a definite ownership
  conflict;
- repeated aliases under one canonical identity inherit that canonical identity's
  semantic duplicate classification;
- incomplete set evidence makes an otherwise non-conflicting alias result
  `Indeterminate`, because an unseen declaration could change the collision set.

DA03 does not invent a capabilities-only comparer for different canonical names.
The frozen comparer includes effective identity metadata, and different canonical
ownership is itself material collision evidence.

## 5. Work bounds

Canonical comparison work is winner-versus-shadow only. Across the whole set,
the number of semantic comparisons cannot exceed the number of non-winning
physical occurrences and is therefore bounded by DA01's supported aggregate
physical-entry maximum.

Alias indexing has its own explicit bound because a single terminal description
may declare multiple aliases:

| Bound | Default | Supported maximum |
| --- | ---: | ---: |
| alias declarations scanned | 1,048,576 | 4,194,304 |

The analysis exposes both `SemanticComparisonCount` and `AliasOccurrenceCount` as
machine-readable work evidence. Cancellation is observed at deterministic
identity, shadow, occurrence, and alias boundaries.

## 6. Frozen boundaries

DA03 does not change:

- Runtime, Source, Compiler, or Termcap contracts;
- the frozen 1.9 JSON v1 schema or document kinds;
- DA01 catalog-set construction and completeness semantics;
- DA02 canonical lookup status, winner, or shadow order;
- path normalization;
- 1.7 source synthesis or 1.8 planning;
- any command syntax or output contract.

Command presentation and JSON for this analysis remain assigned to DA06.

## 7. Validation

DA03 tests cover:

- semantically equal canonical shadows;
- semantically different canonical shadows and retained structured differences;
- indeterminate winner evidence with no invented comparison;
- definite conflicts in partially incomplete sets;
- repeated equal alias ownership;
- different canonical alias owners;
- alias-to-canonical-name collision;
- incomplete alias universes;
- alias-scan bounds;
- cancellation;
- reviewed public API growth only.

**DA03 gate:** callers can explain every observed repeated canonical identity and
every observed alias collision as semantically equal, semantically different, or
indeterminate, with deterministic winner/shadow/owner evidence and without
compiled-byte shortcuts or all-pairs comparison.
''',
)

replace_exact(
    "Directory.Build.props",
    "<IcodTermInfoSuiteVersion>1.10.0-Alpha-2</IcodTermInfoSuiteVersion>",
    "<IcodTermInfoSuiteVersion>1.10.0-Alpha-3</IcodTermInfoSuiteVersion>",
)

replace_exact(
    "Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj",
    "<PackageReleaseNotes>1.10.0-Alpha-2 adds deterministic exact canonical-name precedence lookup, known-winner and ordered-shadow evidence, and explicit indeterminate results for incomplete earlier catalogs while preserving the DA01 model, frozen 1.9 JSON v1, lower-layer, synthesis, planning, and command contracts.</PackageReleaseNotes>",
    "<PackageReleaseNotes>1.10.0-Alpha-3 adds bounded winner-versus-shadow semantic duplicate analysis, retained structured conflict comparisons, deterministic alias ownership/canonical-name collision analysis, and explicit indeterminate evidence while preserving DA01/DA02 precedence, frozen 1.9 JSON v1, lower-layer, synthesis, planning, and command contracts.</PackageReleaseNotes>",
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
    replace_all_required(path_name, "1.10.0-Alpha-2", "1.10.0-Alpha-3")

replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/RP08ReleaseClosureTests.cs",
    '"DA02",',
    '"DA03",',
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/MI07ReleaseClosureTests.cs",
    '"DA02 - Deterministic multi-catalog inspection and precedence",',
    '"DA03 - Semantic duplicate, conflict, alias, and shadow analysis",',
)

replace_exact(
    "tools/inspection-package-smoke/Program.cs",
    "exportedTypes.Length >= 39",
    "exportedTypes.Length >= 45",
)
replace_exact(
    "tools/inspection-package-smoke/Program.cs",
    "&& exportedTypes.Contains( typeof( TermInfoDatabaseSetLookupStatus ) )",
    '''&& exportedTypes.Contains( typeof( TermInfoDatabaseSetLookupStatus ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetSemanticRelationship ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetSemanticAnalysisOptions ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetSemanticAnalysis ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetIdentityAnalysis ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetShadowAnalysis ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetAliasAnalysis ) )''',
)

replace_exact(
    "Icod.TermInfo-1.10.0-Deterministic-Multi-Database-Inspection-Comparison-and-Planning-Automation-Roadmap.md",
    "**Status:** DA02 implementation complete; Staging validation pending",
    "**Status:** DA03 implementation complete; Staging validation pending",
)

replace_exact(
    "Icod.TermInfo-Post-1.0-Development-Roadmap.md",
    "**Current coordinated version:** `1.10.0-Alpha-2`",
    "**Current coordinated version:** `1.10.0-Alpha-3`",
)
replace_exact(
    "Icod.TermInfo-Post-1.0-Development-Roadmap.md",
    "**Current tranche:** DA02 - Deterministic multi-catalog inspection and precedence",
    "**Current tranche:** DA03 - Semantic duplicate, conflict, alias, and shadow analysis",
)

readme_path = Path("Icod.TermInfo.Inspection/README.md")
readme = readme_path.read_text(encoding="utf-8")
marker = "## 1.10 DA02 deterministic database-set precedence\n"
if readme.count(marker) != 1:
    raise RuntimeError("Inspection README DA02 heading marker mismatch")
section = '''## 1.10 DA03 semantic duplicate, alias, and shadow analysis\n\n`1.10.0-Alpha-3` adds bounded winner-versus-shadow semantic classification for\nrepeated canonical identities and deterministic alias collision analysis.\nObserved conflicts retain the frozen `TermInfoComparisonResult`; alias ownership\ncollisions distinguish multiple canonical owners and alias-to-canonical-name\ncollisions, while incomplete input remains explicitly indeterminate. No all-pairs\ncomparison, compiled-byte equality, command output, or new JSON document kind is\nintroduced.\n\nSee\n`docs/1.10.0-DA03-SEMANTIC-DUPLICATE-CONFLICT-ALIAS-AND-SHADOW-ANALYSIS.md`.\n\n'''
readme_path.write_text(readme.replace(marker, section + marker, 1), encoding="utf-8", newline="\n")
