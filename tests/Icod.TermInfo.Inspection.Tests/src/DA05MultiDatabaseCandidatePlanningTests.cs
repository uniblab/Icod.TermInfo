using System.Globalization;
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
				.SetDescription( "DA05 conflicting boolean candidate" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.Build();
		TerminalDescription second =
			new TerminalDescriptionBuilder( "da05-conflict" )
				.SetDescription( "DA05 conflicting numeric candidate" )
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
				.SetDescription( "DA05 self-reference candidate" )
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
			.SetDescription( "DA05 target" )
			.AddAlias( "da05-target-alias" )
			.SetBoolean( BooleanCapability.AutoRightMargin )
			.SetNumber( NumericCapability.Columns, 80 )
			.Build();

	private static TerminalDescription CreateAlphaParent() =>
		new TerminalDescriptionBuilder( "da05-alpha" )
			.SetDescription( "DA05 alpha parent" )
			.AddAlias( "da05-alpha-alias" )
			.SetBoolean( BooleanCapability.AutoRightMargin )
			.Build();

	private static TerminalDescription CreateBetaParent() =>
		new TerminalDescriptionBuilder( "da05-beta" )
			.SetDescription( "DA05 beta parent" )
			.SetNumber( NumericCapability.Columns, 80 )
			.Build();

	private static TerminalDescription CreateDecoyParent() =>
		new TerminalDescriptionBuilder( "da05-decoy" )
			.SetDescription( "DA05 decoy parent" )
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
