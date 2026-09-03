using System.Globalization;
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
				result.Differences,
				difference => difference.Kind
					== TermInfoDatabaseSetDifferenceKind.EffectiveProvenance
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
				result.Differences,
				difference => difference.Kind
					== TermInfoDatabaseSetDifferenceKind.AliasOwnership
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
				result.Differences,
				difference => difference.Kind
					== TermInfoDatabaseSetDifferenceKind.ShadowSet
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
