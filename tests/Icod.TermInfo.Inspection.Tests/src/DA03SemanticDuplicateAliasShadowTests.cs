using System.Globalization;
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
				cancellationToken: CancellationToken.None
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
				cancellationToken: CancellationToken.None
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
				cancellationToken: CancellationToken.None
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
				cancellationToken: CancellationToken.None
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
				cancellationToken: CancellationToken.None
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
						"owner-first",
						CreateTerminal( "zeta", 80, "shared" )
					),
					CreateCatalog(
						"owner-second",
						CreateTerminal( "alpha", 80, "shared" )
					),
				]
			);

		TermInfoDatabaseSetAliasAnalysis alias =
			Assert.Single(
				set.AnalyzeSemantics(
					cancellationToken: CancellationToken.None
				).Aliases
			);
		TermInfoDatabaseSetOccurrence owner =
			Assert.IsType<TermInfoDatabaseSetOccurrence>( alias.PrecedenceOwner );

		Assert.Equal(
			TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent,
			alias.Relationship
		);
		Assert.Equal( new[] { "alpha", "zeta" }, alias.CanonicalNames );
		Assert.Equal( new[] { 0, 1 }, alias.Occurrences.Select( occurrence => occurrence.DatabaseIndex ).ToArray() );
		Assert.Equal( "zeta", owner.Name );
		Assert.Equal( 0, owner.DatabaseIndex );
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
					cancellationToken: CancellationToken.None
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
				cancellationToken: CancellationToken.None
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
				CancellationToken.None
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
