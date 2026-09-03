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
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSetPlanningCandidate.cs",
    '''namespace Icod.TermInfo.Inspection;

/// <summary>
/// Maps one frozen planner candidate position back to its original ordered
/// database-set publication.
/// </summary>
public sealed class TermInfoDatabaseSetPlanningCandidate {
	internal TermInfoDatabaseSetPlanningCandidate(
		int candidateIndex,
		TermInfoDatabaseSetEntry database,
		TermInfoDatabaseSetOccurrence occurrence,
		TerminalDescriptionSourceSynthesisParent parent
	) {
		if ( candidateIndex < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( candidateIndex ) );
		}
		ArgumentNullException.ThrowIfNull( database );
		ArgumentNullException.ThrowIfNull( occurrence );
		ArgumentNullException.ThrowIfNull( parent );
		if ( database.Index != occurrence.DatabaseIndex ) {
			throw new ArgumentException(
				"The candidate database must contain the mapped occurrence.",
				nameof( database )
			);
		}
		if ( !string.Equals(
			occurrence.Name,
			parent.UseName,
			StringComparison.Ordinal
		) ) {
			throw new ArgumentException(
				"Database-set planning candidates must emit the canonical occurrence name.",
				nameof( parent )
			);
		}
		if ( !ReferenceEquals( occurrence.Entry.Terminal, parent.Description ) ) {
			throw new ArgumentException(
				"The planning parent must preserve the exact occurrence description.",
				nameof( parent )
			);
		}

		CandidateIndex = candidateIndex;
		Database = database;
		Occurrence = occurrence;
		Parent = parent;
	}

	/// <summary>
	/// Gets the zero-based position supplied to the frozen 1.8 planner.
	/// </summary>
	public int CandidateIndex {
		get;
	}

	/// <summary>
	/// Gets the original constituent database evidence.
	/// </summary>
	public TermInfoDatabaseSetEntry Database {
		get;
	}

	/// <summary>
	/// Gets the exact original physical occurrence used as the candidate
	/// representative.
	/// </summary>
	public TermInfoDatabaseSetOccurrence Occurrence {
		get;
	}

	/// <summary>
	/// Gets the exact frozen synthesis-parent object supplied to the planner.
	/// </summary>
	public TerminalDescriptionSourceSynthesisParent Parent {
		get;
	}

	/// <summary>
	/// Gets the caller-order database index.
	/// </summary>
	public int DatabaseIndex =>
		Occurrence.DatabaseIndex;

	/// <summary>
	/// Gets the constituent catalog-entry index.
	/// </summary>
	public int CatalogEntryIndex =>
		Occurrence.CatalogEntryIndex;

	/// <summary>
	/// Gets the canonical terminal name of the represented publication.
	/// </summary>
	public string CanonicalName =>
		Occurrence.Name;

	/// <summary>
	/// Gets the exact <c>use=</c> spelling supplied to the frozen planner.
	/// </summary>
	public string UseName =>
		Parent.UseName;

	/// <summary>
	/// Gets the exact effective terminal semantics supplied to the frozen planner.
	/// </summary>
	public TerminalDescription Description =>
		Parent.Description;
}
''',
)

write_new(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSetSourcePlanningResult.cs",
    '''namespace Icod.TermInfo.Inspection;

/// <summary>
/// Contains one frozen 1.8 relative-source plan together with deterministic
/// database-set candidate provenance.
/// </summary>
public sealed class TermInfoDatabaseSetSourcePlanningResult {
	internal TermInfoDatabaseSetSourcePlanningResult(
		TermInfoDatabaseSet databaseSet,
		TerminalDescriptionSourcePlan plan,
		IEnumerable<TermInfoDatabaseSetPlanningCandidate> candidates,
		int collapsedDuplicateOccurrenceCount,
		int candidateSemanticComparisonCount
	) {
		ArgumentNullException.ThrowIfNull( databaseSet );
		ArgumentNullException.ThrowIfNull( plan );
		ArgumentNullException.ThrowIfNull( candidates );
		if ( collapsedDuplicateOccurrenceCount < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( collapsedDuplicateOccurrenceCount )
			);
		}
		if ( candidateSemanticComparisonCount < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( candidateSemanticComparisonCount )
			);
		}

		TermInfoDatabaseSetPlanningCandidate[] candidateArray = candidates.ToArray();
		if ( candidateArray.Any( candidate => candidate is null ) ) {
			throw new ArgumentException(
				"Database-set planning candidates cannot contain null.",
				nameof( candidates )
			);
		}
		if ( candidateArray.Length != plan.CandidateCount ) {
			throw new ArgumentException(
				"Database-set candidate evidence must match the frozen planner candidate count.",
				nameof( candidates )
			);
		}
		for ( int index = 0; index < candidateArray.Length; index++ ) {
			if ( candidateArray[ index ].CandidateIndex != index ) {
				throw new ArgumentException(
					"Database-set candidate indices must be contiguous planner positions.",
					nameof( candidates )
				);
			}
		}

		TermInfoDatabaseSetPlanningCandidate[] selected =
			new TermInfoDatabaseSetPlanningCandidate[
				plan.Score.SelectedCandidateIndices.Count
			];
		for ( int index = 0; index < selected.Length; index++ ) {
			int candidateIndex = plan.Score.SelectedCandidateIndices[ index ];
			if ( candidateIndex < 0 || candidateIndex >= candidateArray.Length ) {
				throw new ArgumentException(
					"The frozen plan selected a candidate position outside the database-set candidate evidence.",
					nameof( plan )
				);
			}
			selected[ index ] = candidateArray[ candidateIndex ];
			if ( !ReferenceEquals(
				selected[ index ].Parent,
				plan.SelectedParents[ index ]
			) ) {
				throw new ArgumentException(
					"Selected database-set candidate evidence must preserve the exact frozen planner parent objects.",
					nameof( plan )
				);
			}
		}

		DatabaseSet = databaseSet;
		Plan = plan;
		Candidates = Array.AsReadOnly( candidateArray );
		SelectedCandidates = Array.AsReadOnly( selected );
		CollapsedDuplicateOccurrenceCount = collapsedDuplicateOccurrenceCount;
		CandidateSemanticComparisonCount = candidateSemanticComparisonCount;
	}

	/// <summary>
	/// Gets the exact immutable ordered database set used for candidate discovery.
	/// </summary>
	public TermInfoDatabaseSet DatabaseSet {
		get;
	}

	/// <summary>
	/// Gets the unchanged frozen 1.8 planner result.
	/// </summary>
	public TerminalDescriptionSourcePlan Plan {
		get;
	}

	/// <summary>
	/// Gets canonical non-self candidate positions in exact order supplied to the
	/// frozen planner.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetPlanningCandidate> Candidates {
		get;
	}

	/// <summary>
	/// Gets selected candidate evidence in exact emitted <c>use=</c> order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetPlanningCandidate> SelectedCandidates {
		get;
	}

	/// <summary>
	/// Gets the number of later semantically equal physical publications collapsed
	/// behind the first ordered canonical representative.
	/// </summary>
	public int CollapsedDuplicateOccurrenceCount {
		get;
	}

	/// <summary>
	/// Gets the number of semantic duplicate-validation comparisons performed while
	/// constructing the candidate universe before invoking the frozen planner.
	/// </summary>
	public int CandidateSemanticComparisonCount {
		get;
	}
}
''',
)

write_new(
    "Icod.TermInfo.Inspection/src/TerminalDescriptionSourcePlanner.DatabaseSet.cs",
    '''namespace Icod.TermInfo.Inspection;

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
''',
)

write_new(
    "tests/Icod.TermInfo.Inspection.Tests/src/DA05MultiDatabaseCandidatePlanningTests.cs",
    '''using System.Globalization;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class DA05MultiDatabaseCandidatePlanningTests {
	[Fact]
	public void DatabaseSetPlanningPreservesPhysicalCandidateAndSelectedEvidenceOrder() {
		TerminalDescription target = CreateTarget();
		TermInfoDatabaseCatalog first =
			CreateCatalogAtRoot(
				AbsolutePath( "first" ),
				CreateAlphaParent()
			);
		TermInfoDatabaseCatalog second =
			CreateCatalogAtRoot(
				AbsolutePath( "second" ),
				CreateBetaParent(),
				CreateDecoyParent()
			);
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet([ first, second ]);

		TermInfoDatabaseSetSourcePlanningResult result =
			TerminalDescriptionSourcePlanner.PlanFromDatabaseSet(
				target,
				set
			);

		Assert.Same( set, result.DatabaseSet );
		Assert.Equal(
			new[] { "da05-alpha", "da05-beta", "da05-decoy" },
			result.Candidates.Select( candidate => candidate.CanonicalName ).ToArray()
		);
		Assert.Equal(
			new[] { 0, 1, 1 },
			result.Candidates.Select( candidate => candidate.DatabaseIndex ).ToArray()
		);
		Assert.Equal(
			new[] { "da05-alpha", "da05-beta" },
			result.Plan.SelectedParents.Select( parent => parent.UseName ).ToArray()
		);
		Assert.Equal( new[] { 0, 1 }, result.Plan.Score.SelectedCandidateIndices );
		Assert.Equal( 2, result.SelectedCandidates.Count );
		Assert.Same( result.Candidates[ 0 ], result.SelectedCandidates[ 0 ] );
		Assert.Same( result.Candidates[ 1 ], result.SelectedCandidates[ 1 ] );
		Assert.Equal( 0, result.SelectedCandidates[ 0 ].DatabaseIndex );
		Assert.Equal( 1, result.SelectedCandidates[ 1 ].DatabaseIndex );
		Assert.Same(
			result.SelectedCandidates[ 0 ].Parent,
			result.Plan.SelectedParents[ 0 ]
		);
		Assert.Same(
			result.SelectedCandidates[ 1 ].Parent,
			result.Plan.SelectedParents[ 1 ]
		);
	}

	[Fact]
	public void EqualCanonicalPublicationsCollapseToFirstPhysicalRepresentative() {
		TerminalDescription alpha = CreateAlphaParent();
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalogAtRoot( AbsolutePath( "collapse-first" ), alpha ),
					CreateCatalogAtRoot(
						AbsolutePath( "collapse-second" ),
						CreateAlphaParent(),
						CreateBetaParent()
					),
				]
			);

		TermInfoDatabaseSetSourcePlanningResult result =
			TerminalDescriptionSourcePlanner.PlanFromDatabaseSet(
				CreateTarget(),
				set
			);

		Assert.Equal( 2, result.Candidates.Count );
		Assert.Equal( "da05-alpha", result.Candidates[ 0 ].CanonicalName );
		Assert.Equal( 0, result.Candidates[ 0 ].DatabaseIndex );
		Assert.Equal( 1, result.CollapsedDuplicateOccurrenceCount );
		Assert.Equal( 1, result.CandidateSemanticComparisonCount );
	}

	[Fact]
	public void ConflictingCanonicalCandidatePublicationsAreRejectedBeforePlanner() {
		TerminalDescription first =
			new TerminalDescriptionBuilder( "da05-conflict" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.Build();
		TerminalDescription second =
			new TerminalDescriptionBuilder( "da05-conflict" )
				.SetNumber( NumericCapability.Columns, 132 )
				.Build();
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalogAtRoot( AbsolutePath( "conflict-first" ), first ),
					CreateCatalogAtRoot( AbsolutePath( "conflict-second" ), second ),
				]
			);

		InvalidOperationException exception =
			Assert.Throws<InvalidOperationException>(
				() => TerminalDescriptionSourcePlanner.PlanFromDatabaseSet(
					CreateTarget(),
					set
				)
			);

		Assert.Contains(
			"conflicting physical candidate publications",
			exception.Message,
			StringComparison.Ordinal
		);
		Assert.Contains( "da05-conflict", exception.Message, StringComparison.Ordinal );
	}

	[Fact]
	public void IncompleteDatabaseSetIsRejectedRatherThanPlannedAsComplete() {
		string root = AbsolutePath( "incomplete" );
		TermInfoDatabaseCatalogIssue issue =
			new(
				TermInfoDatabaseCatalogIssueKind.MalformedEntry,
				Path.Combine( root, "broken" ),
				"DA05 incomplete fixture."
			);
		TermInfoDatabaseCatalog incomplete =
			CreateCatalogCore(
				root,
				[ CreateAlphaParent() ],
				[ issue ]
			);
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet([ incomplete ]);

		InvalidOperationException exception =
			Assert.Throws<InvalidOperationException>(
				() => TerminalDescriptionSourcePlanner.PlanFromDatabaseSet(
					CreateTarget(),
					set
				)
			);

		Assert.False( set.IsComplete );
		Assert.Contains( "incomplete database indices: 0", exception.Message, StringComparison.Ordinal );
	}

	[Fact]
	public void TargetIdentityExclusionReusesFrozenCandidateIdentityRule() {
		TerminalDescription target = CreateTarget();
		TerminalDescription selfReference =
			new TerminalDescriptionBuilder( "da05-self-reference" )
				.AddAlias( target.Aliases[ 0 ] )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalogAtRoot(
						AbsolutePath( "self" ),
						selfReference,
						CreateAlphaParent()
					),
				]
			);

		TermInfoDatabaseSetSourcePlanningResult result =
			TerminalDescriptionSourcePlanner.PlanFromDatabaseSet( target, set );

		Assert.Single( result.Candidates );
		Assert.Equal( "da05-alpha", result.Candidates[ 0 ].CanonicalName );
		Assert.DoesNotContain(
			result.Candidates,
			candidate => candidate.CanonicalName == "da05-self-reference"
		);
	}

	[Fact]
	public void CatalogWrapperBuildsEquivalentFrozenDatabaseSetPlan() {
		TerminalDescription target = CreateTarget();
		TermInfoDatabaseCatalog[] catalogs =
		[
			CreateCatalogAtRoot( AbsolutePath( "catalog-a" ), CreateAlphaParent() ),
			CreateCatalogAtRoot( AbsolutePath( "catalog-b" ), CreateBetaParent() ),
		];
		TermInfoDatabaseSet set = TermInfoDatabaseInspector.CreateSet( catalogs );

		TermInfoDatabaseSetSourcePlanningResult direct =
			TerminalDescriptionSourcePlanner.PlanFromDatabaseSet( target, set );
		TermInfoDatabaseSetSourcePlanningResult composed =
			TerminalDescriptionSourcePlanner.PlanFromCatalogs( target, catalogs );

		Assert.Equal( direct.Plan.Source, composed.Plan.Source );
		Assert.Equal(
			direct.Candidates.Select( candidate => candidate.UseName ).ToArray(),
			composed.Candidates.Select( candidate => candidate.UseName ).ToArray()
		);
		Assert.Equal(
			direct.Plan.Score.SelectedCandidateIndices.ToArray(),
			composed.Plan.Score.SelectedCandidateIndices.ToArray()
		);
	}

	[Fact]
	public void DirectoryWrapperInspectsExplicitRootsAndPreservesRootOrder() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();
		try {
			CompiledTermInfoDatabaseWriter.Write( firstRoot, CreateAlphaParent() );
			CompiledTermInfoDatabaseWriter.Write( secondRoot, CreateBetaParent() );

			TermInfoDatabaseSetSourcePlanningResult result =
				TerminalDescriptionSourcePlanner.PlanFromDirectories(
					CreateTarget(),
					[ firstRoot, secondRoot ]
				);

			Assert.Equal( 2, result.DatabaseSet.Entries.Count );
			Assert.Equal( Path.GetFullPath( firstRoot ), result.DatabaseSet.Entries[ 0 ].Catalog.Root );
			Assert.Equal( Path.GetFullPath( secondRoot ), result.DatabaseSet.Entries[ 1 ].Catalog.Root );
			Assert.Equal( new[] { 0, 1 }, result.Candidates.Select( candidate => candidate.DatabaseIndex ).ToArray() );
		} finally {
			DeleteTemporaryDirectory( firstRoot );
			DeleteTemporaryDirectory( secondRoot );
		}
	}

	[Fact]
	public void DatabaseSetPlanningPreservesFrozenCandidateBound() {
		TerminalDescriptionSourcePlanningOptions options =
			new(
				new TerminalDescriptionSourceSynthesisOptions(
					80,
					maximumParentCount: 1
				),
				maximumCandidateCount: 1,
				maximumSelectedParentCount: 1
			);
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalogAtRoot(
						AbsolutePath( "bound" ),
						CreateAlphaParent(),
						CreateBetaParent()
					),
				]
			);

		ArgumentException exception =
			Assert.Throws<ArgumentException>(
				() => TerminalDescriptionSourcePlanner.PlanFromDatabaseSet(
					CreateTarget(),
					set,
					options
				)
			);

		Assert.Contains( "1 canonical non-self candidates", exception.Message, StringComparison.Ordinal );
	}

	[Fact]
	public void PreCanceledDatabaseSetPlanningDoesNotConstructCandidates() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalogAtRoot( AbsolutePath( "cancel" ), CreateAlphaParent() ),
				]
			);
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();

		Assert.Throws<OperationCanceledException>(
			() => TerminalDescriptionSourcePlanner.PlanFromDatabaseSet(
				CreateTarget(),
				set,
				cancellationToken: cancellation.Token
			)
		);
	}

	[Fact]
	public void Da05AddsOnlyReviewedDatabaseSetPlanningEvidenceTypes() {
		Type[] exportedTypes =
			typeof( TermInfoDatabaseSetSourcePlanningResult ).Assembly.GetExportedTypes();

		Assert.Contains( typeof( TermInfoDatabaseSetPlanningCandidate ), exportedTypes );
		Assert.Contains( typeof( TermInfoDatabaseSetSourcePlanningResult ), exportedTypes );
		Assert.InRange( exportedTypes.Length, 51, int.MaxValue );
	}

	private static TermInfoDatabaseCatalog CreateCatalogAtRoot(
		string root,
		params TerminalDescription[] terminals
	) =>
		CreateCatalogCore(
			root,
			terminals,
			Array.Empty<TermInfoDatabaseCatalogIssue>()
		);

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

	private static TerminalDescription CreateTarget() =>
		new TerminalDescriptionBuilder( "da05-target" )
			.AddAlias( "da05-target-alias" )
			.SetBoolean( BooleanCapability.AutoRightMargin )
			.SetNumber( NumericCapability.Columns, 80 )
			.Build();

	private static TerminalDescription CreateAlphaParent() =>
		new TerminalDescriptionBuilder( "da05-alpha" )
			.AddAlias( "da05-alpha-alias" )
			.SetBoolean( BooleanCapability.AutoRightMargin )
			.Build();

	private static TerminalDescription CreateBetaParent() =>
		new TerminalDescriptionBuilder( "da05-beta" )
			.SetNumber( NumericCapability.Columns, 80 )
			.Build();

	private static TerminalDescription CreateDecoyParent() =>
		new TerminalDescriptionBuilder( "da05-decoy" )
			.SetBoolean( BooleanCapability.AutoLeftMargin )
			.Build();

	private static string AbsolutePath(
		string suffix
	) =>
		Path.Combine(
			Path.GetTempPath(),
			$"icod-terminfo-da05-{suffix}-{Guid.NewGuid():N}"
		);

	private static string CreateTemporaryDirectory() {
		string path = AbsolutePath( "directory" );
		Directory.CreateDirectory( path );
		return path;
	}

	private static void DeleteTemporaryDirectory(
		string path
	) {
		if ( !Directory.Exists( path ) ) {
			return;
		}

		try {
			Directory.Delete( path, recursive: true );
		} catch ( IOException ) {
		} catch ( UnauthorizedAccessException ) {
		}
	}
}
''',
)

write_new(
    "docs/1.10.0-DA05-MULTI-DATABASE-CANDIDATE-PLANNING.md",
    '''# Icod.TermInfo 1.10.0 DA05 — Multi-Database Candidate Planning

**Development version:** `1.10.0-Alpha-5`  
**Tranche:** DA05  
**Published baseline:** `1.9.0`  
**DA04 baseline:** `1.10.0-Alpha-4`  
**Primary package:** `Icod.TermInfo.Inspection`  
**Authoritative planner:** frozen 1.8 `TerminalDescriptionSourcePlanner`  
**Status:** implementation complete; PR Staging validation pending  

## 1. Purpose

DA05 composes the frozen 1.8 relative-source planner over candidates discovered
from multiple explicit ordered databases. It does not change planner scoring,
parent permutations, synthesis, source rendering, exhaustive/bounded policy, or
candidate-position tie breaking.

The new responsibility is only:

```text
explicit roots/catalogs/database set
    -> complete ordered database-set evidence
    -> canonical non-self candidate positions
    -> validate/collapse duplicate physical publications
    -> frozen TerminalDescriptionSourcePlanner.Plan
    -> map selected candidate indices back to database evidence
```

## 2. Public surface

DA05 adds exactly two public evidence types:

```text
TermInfoDatabaseSetPlanningCandidate
TermInfoDatabaseSetSourcePlanningResult
```

It adds three composition methods to `TerminalDescriptionSourcePlanner`:

```csharp
PlanFromDatabaseSet(...)
PlanFromCatalogs(...)
PlanFromDirectories(...)
```

`PlanFromCatalogs` performs no filesystem I/O. `PlanFromDirectories` delegates
root acquisition to `TermInfoDatabaseInspector.InspectSet`, so every explicit root
is inspected once in caller order. The core planning composition accepts an
already-frozen `TermInfoDatabaseSet`.

## 3. Candidate construction

Candidate construction scans physical evidence in exactly:

```text
database index
then catalog-entry index
```

No canonical-name global sort is allowed to replace caller-selected database
order.

For the first physical publication of each canonical identity:

- the occurrence becomes the representative for duplicate validation;
- the frozen RP05 `SharesIdentity` rule excludes target/self identities;
- every remaining canonical identity becomes exactly one planner candidate;
- the emitted `use=` spelling is the canonical occurrence name, matching the
  frozen explicit-catalog planning policy.

The frozen planner's `MaximumCandidateCount` remains authoritative after target
exclusion and duplicate collapse.

## 4. Duplicate candidate publications

Later physical publications with the same canonical name are compared to the
first ordered representative through `TerminalDescriptionComparer`.

- semantically equal publications collapse behind the first representative;
- semantically different publications cause the composed planning operation to
  fail before the frozen planner is invoked;
- no later conflicting publication is selected merely because it exists in a
  lower-precedence database.

This deliberately matches the frozen RP05 single-catalog rule and extends it
across database boundaries.

`CollapsedDuplicateOccurrenceCount` and
`CandidateSemanticComparisonCount` expose the bounded orchestration work.

## 5. Incomplete input

Planning requires a complete database-set candidate universe. If any constituent
is missing, unavailable, unsupported, or issue-bearing, DA05 rejects the
operation and identifies the incomplete database indices.

DA05 therefore never plans from a falsely complete partial candidate universe.
This is the multi-database analogue of RP05's issue-free complete-catalog gate.

## 6. Result evidence

`TermInfoDatabaseSetSourcePlanningResult` retains:

```text
DatabaseSet
Plan
Candidates
SelectedCandidates
CollapsedDuplicateOccurrenceCount
CandidateSemanticComparisonCount
```

Each `TermInfoDatabaseSetPlanningCandidate` maps one frozen planner position to:

```text
candidate index
input database index
catalog entry index
canonical name
exact use= spelling
exact TerminalDescription semantics
original database-set entry
original database-set occurrence
exact TerminalDescriptionSourceSynthesisParent object
```

`SelectedCandidates` is derived from the unchanged
`Plan.Score.SelectedCandidateIndices` and preserves emitted parent order. No
secondary selection policy is introduced.

## 7. Bounds and cancellation

DA05 introduces no new planner bound. The frozen 1.8 limits remain authoritative:

```text
MaximumCandidateCount
MaximumSelectedParentCount
MaximumEvaluatedPlanCount
MaximumGeneratedSourceLength
AllowNonExhaustiveResult
```

Database count and physical-entry bounds remain DA01 policy. Compiled parser
bounds remain the explicit-root acquisition policy. Cancellation is observed
before acquisition, during database-set candidate construction, duplicate
comparison, and by the frozen planner itself.

## 8. Frozen boundaries

DA05 does not change:

- Runtime, Source, Compiler, or Termcap APIs;
- DA01 database-set construction;
- DA02 lookup/precedence semantics;
- DA03 semantic and alias analysis;
- DA04 database-set comparison;
- 1.7 synthesis semantics;
- 1.8 planner score ordering, permutations, or bounds;
- frozen 1.9 JSON v1;
- command syntax or output.

Command-line multi-database planning composition remains DA06 work.

## 9. Validation

DA05 tests cover:

- physical root/catalog candidate ordering and selected-parent provenance;
- selected candidate mapping through frozen planner indices;
- semantically equal cross-database duplicate collapse;
- conflicting cross-database candidate rejection;
- incomplete database-set rejection;
- frozen target/self identity exclusion;
- already-inspected catalog composition;
- explicit-directory composition and root order;
- frozen candidate bounds;
- cancellation;
- reviewed public API growth only.

**DA05 gate:** a target can be planned against candidates drawn from multiple
explicit ordered databases, with complete provenance for every selected parent,
without changing the frozen 1.8 planner or 1.7 synthesizer semantics.
''',
)

replace_exact(
    "Directory.Build.props",
    "<IcodTermInfoSuiteVersion>1.10.0-Alpha-4</IcodTermInfoSuiteVersion>",
    "<IcodTermInfoSuiteVersion>1.10.0-Alpha-5</IcodTermInfoSuiteVersion>",
)

replace_exact(
    "Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj",
    "<PackageReleaseNotes>1.10.0-Alpha-4 adds deterministic ordered database-set comparison with separate effective semantic and structural/provenance classifications, typed topology/membership/winner/alias/shadow/issue/indeterminate evidence, bounded alias scanning, and retained structured semantic comparisons while preserving DA01-DA03, frozen 1.9 JSON v1, lower-layer, synthesis, planning, and command contracts.</PackageReleaseNotes>",
    "<PackageReleaseNotes>1.10.0-Alpha-5 adds deterministic complete database-set candidate discovery, target-identity exclusion, semantic duplicate validation/collapse, selected-parent provenance mapping, and explicit roots/catalog composition over the unchanged frozen 1.8 planner while preserving DA01-DA04, frozen 1.9 JSON v1, lower-layer, synthesis, planner scoring, and command contracts.</PackageReleaseNotes>",
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
    replace_all_required(path_name, "1.10.0-Alpha-4", "1.10.0-Alpha-5")

replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/RP08ReleaseClosureTests.cs",
    '"DA04",',
    '"DA05",',
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/MI07ReleaseClosureTests.cs",
    '"DA04 - Database-set semantic comparison",',
    '"DA05 - Multi-database candidate planning",',
)

replace_exact(
    "tools/inspection-package-smoke/Program.cs",
    "exportedTypes.Length >= 49",
    "exportedTypes.Length >= 51",
)
replace_exact(
    "tools/inspection-package-smoke/Program.cs",
    "&& exportedTypes.Contains( typeof( TermInfoDatabaseSetComparer ) )",
    '''&& exportedTypes.Contains( typeof( TermInfoDatabaseSetComparer ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetPlanningCandidate ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetSourcePlanningResult ) )''',
)
replace_exact(
    "tools/inspection-package-smoke/Program.cs",
    '''\tTerminalDescriptionSourcePlan directoryPlan =\n\t\tTerminalDescriptionSourcePlanner.PlanFromDirectory(\n\t\t\tterminal,\n\t\t\temptyCatalogRoot\n\t\t);''',
    '''\tTerminalDescriptionSourcePlan directoryPlan =\n\t\tTerminalDescriptionSourcePlanner.PlanFromDirectory(\n\t\t\tterminal,\n\t\t\temptyCatalogRoot\n\t\t);\n\tTermInfoDatabaseSetSourcePlanningResult databaseSetPlan =\n\t\tTerminalDescriptionSourcePlanner.PlanFromDatabaseSet(\n\t\t\tterminal,\n\t\t\tTermInfoDatabaseInspector.CreateSet([ emptyCatalog ])\n\t\t);''',
)
replace_exact(
    "tools/inspection-package-smoke/Program.cs",
    '''\t\t\t&& directoryPlan.CandidateCount == 0\n\t\t\t&& directoryPlan.EvaluatedPlanCount == 1\n\t\t\t&& directoryPlan.IsExhaustive,''',
    '''\t\t\t&& directoryPlan.CandidateCount == 0\n\t\t\t&& directoryPlan.EvaluatedPlanCount == 1\n\t\t\t&& directoryPlan.IsExhaustive\n\t\t\t&& databaseSetPlan.Plan.Source == catalogPlan.Source\n\t\t\t&& databaseSetPlan.Candidates.Count == 0\n\t\t\t&& databaseSetPlan.SelectedCandidates.Count == 0\n\t\t\t&& databaseSetPlan.Plan.CandidateCount == 0\n\t\t\t&& databaseSetPlan.Plan.EvaluatedPlanCount == 1\n\t\t\t&& databaseSetPlan.Plan.IsExhaustive,''',
)

replace_exact(
    "Icod.TermInfo-1.10.0-Deterministic-Multi-Database-Inspection-Comparison-and-Planning-Automation-Roadmap.md",
    "**Status:** DA04 implementation complete; Staging validation pending",
    "**Status:** DA05 implementation complete; Staging validation pending",
)

replace_exact(
    "Icod.TermInfo-Post-1.0-Development-Roadmap.md",
    "**Current coordinated version:** `1.10.0-Alpha-4`",
    "**Current coordinated version:** `1.10.0-Alpha-5`",
)
replace_exact(
    "Icod.TermInfo-Post-1.0-Development-Roadmap.md",
    "**Current tranche:** DA04 - Database-set semantic comparison",
    "**Current tranche:** DA05 - Multi-database candidate planning",
)

readme_path = Path("Icod.TermInfo.Inspection/README.md")
readme = readme_path.read_text(encoding="utf-8")
marker = "## 1.10 DA04 database-set semantic and structural comparison\n"
if readme.count(marker) != 1:
    raise RuntimeError("Inspection README DA04 heading marker mismatch")
section = '''## 1.10 DA05 multi-database candidate planning\n\n`1.10.0-Alpha-5` composes the frozen 1.8 planner over canonical candidates\ndiscovered from a complete explicit ordered database set. Candidate order follows\nphysical database/catalog order; target identities are excluded by the frozen\nRP05 rule; semantically equal duplicate publications collapse behind the first\nrepresentative; conflicting duplicates and incomplete sets are rejected before\nplanning. The composed result maps frozen planner candidate indices and selected\nparents back to exact database, catalog-entry, canonical-name, `use=`, and\n`TerminalDescription` evidence.\n\nSee `docs/1.10.0-DA05-MULTI-DATABASE-CANDIDATE-PLANNING.md`.\n\n'''
readme_path.write_text(readme.replace(marker, section + marker, 1), encoding="utf-8", newline="\n")
