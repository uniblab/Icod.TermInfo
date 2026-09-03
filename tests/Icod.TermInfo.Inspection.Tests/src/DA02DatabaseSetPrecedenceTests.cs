using System.Globalization;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class DA02DatabaseSetPrecedenceTests {
	[Fact]
	public void CompleteAbsenceIsConclusiveAndAliasDoesNotBecomeCanonicalLookup() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog(
						"absence",
						CreateTerminal( "canonical", "alias" )
					),
				]
			);

		TermInfoDatabaseSetLookupResult missing =
			set.LookupCanonicalName( "missing" );
		Assert.Equal( TermInfoDatabaseSetLookupStatus.NotObserved, missing.Status );
		Assert.False( missing.IsObserved );
		Assert.True( missing.IsObservationComplete );
		Assert.Null( missing.Winner );
		Assert.Empty( missing.Occurrences );
		Assert.Empty( missing.ShadowedOccurrences );
		Assert.Empty( missing.BlockingDatabaseIndices );

		TermInfoDatabaseSetLookupResult alias =
			set.LookupCanonicalName( "alias" );
		Assert.Equal( TermInfoDatabaseSetLookupStatus.NotObserved, alias.Status );
	}

	[Fact]
	public void CompleteSingleOccurrenceHasKnownWinnerWithoutShadows() {
		TermInfoDatabaseCatalog catalog =
			CreateCatalog( "single", CreateTerminal( "target", "target-alias" ) );
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet([ catalog ]);

		TermInfoDatabaseSetLookupResult result =
			set.LookupCanonicalName( "target" );

		Assert.Equal( TermInfoDatabaseSetLookupStatus.WinnerKnown, result.Status );
		Assert.True( result.IsObserved );
		Assert.False( result.HasMultipleOccurrences );
		Assert.True( result.IsObservationComplete );
		TermInfoDatabaseSetOccurrence winner = Assert.IsType<TermInfoDatabaseSetOccurrence>( result.Winner );
		Assert.Same( result.Occurrences[ 0 ], winner );
		Assert.Equal( 0, winner.DatabaseIndex );
		Assert.Equal( 0, winner.CatalogEntryIndex );
		Assert.Contains( "target-alias", winner.Aliases );
		Assert.Empty( result.ShadowedOccurrences );
		Assert.Empty( result.IncompleteDatabaseIndices );
	}

	[Fact]
	public void CompleteRepeatedIdentityUsesFirstOccurrenceAndPreservesLaterShadowOrder() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog(
						"first",
						CreateTerminal( "same", "first-a" ),
						CreateTerminal( "same", "first-b" )
					),
					CreateCatalog(
						"second",
						CreateTerminal( "same", "second" )
					),
				]
			);

		TermInfoDatabaseSetLookupResult result =
			set.LookupCanonicalName( "same" );

		Assert.Equal( TermInfoDatabaseSetLookupStatus.WinnerKnown, result.Status );
		Assert.True( result.HasMultipleOccurrences );
		Assert.Equal( 3, result.Occurrences.Count );
		Assert.Same( result.Occurrences[ 0 ], result.Winner );
		Assert.Equal( 2, result.ShadowedOccurrences.Count );
		Assert.Same( result.Occurrences[ 1 ], result.ShadowedOccurrences[ 0 ] );
		Assert.Same( result.Occurrences[ 2 ], result.ShadowedOccurrences[ 1 ] );
		Assert.Equal(
			new[] { 0, 0, 1 },
			result.Occurrences.Select( occurrence => occurrence.DatabaseIndex ).ToArray()
		);
	}

	[Fact]
	public void EarlierIncompleteDatabaseMakesLaterObservedWinnerIndeterminate() {
		TermInfoDatabaseCatalog incomplete =
			CreateIncompleteCatalog( "blocking", Array.Empty<TerminalDescription>() );
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					incomplete,
					CreateCatalog( "later", CreateTerminal( "target" ) ),
				]
			);

		TermInfoDatabaseSetLookupResult result =
			set.LookupCanonicalName( "target" );

		Assert.Equal( TermInfoDatabaseSetLookupStatus.Indeterminate, result.Status );
		Assert.True( result.IsObserved );
		Assert.False( result.IsObservationComplete );
		Assert.Null( result.Winner );
		Assert.Empty( result.ShadowedOccurrences );
		Assert.Equal( new[] { 0 }, result.IncompleteDatabaseIndices );
		Assert.Equal( new[] { 0 }, result.BlockingDatabaseIndices );
	}

	[Fact]
	public void ObservedOccurrenceInsideIncompleteDatabaseCannotClaimWinner() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateIncompleteCatalog(
						"same-root",
						[ CreateTerminal( "target" ) ]
					),
				]
			);

		TermInfoDatabaseSetLookupResult result =
			set.LookupCanonicalName( "target" );

		Assert.Equal( TermInfoDatabaseSetLookupStatus.Indeterminate, result.Status );
		Assert.Single( result.Occurrences );
		Assert.Null( result.Winner );
		Assert.Equal( new[] { 0 }, result.BlockingDatabaseIndices );
	}

	[Fact]
	public void LaterIncompleteDatabaseDoesNotInvalidateEarlierKnownWinnerButMarksObservationIncomplete() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog( "winner", CreateTerminal( "target" ) ),
					CreateIncompleteCatalog( "later-incomplete", Array.Empty<TerminalDescription>() ),
				]
			);

		TermInfoDatabaseSetLookupResult result =
			set.LookupCanonicalName( "target" );

		Assert.Equal( TermInfoDatabaseSetLookupStatus.WinnerKnown, result.Status );
		Assert.NotNull( result.Winner );
		Assert.Equal( 0, result.Winner!.DatabaseIndex );
		Assert.False( result.IsObservationComplete );
		Assert.Equal( new[] { 1 }, result.IncompleteDatabaseIndices );
		Assert.Empty( result.BlockingDatabaseIndices );
	}

	[Fact]
	public void IncompleteSetCannotClaimCanonicalAbsence() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog( "complete", CreateTerminal( "other" ) ),
					CreateIncompleteCatalog( "unknown", Array.Empty<TerminalDescription>() ),
				]
			);

		TermInfoDatabaseSetLookupResult result =
			set.LookupCanonicalName( "target" );

		Assert.Equal( TermInfoDatabaseSetLookupStatus.Indeterminate, result.Status );
		Assert.False( result.IsObserved );
		Assert.Null( result.Winner );
		Assert.Equal( new[] { 1 }, result.BlockingDatabaseIndices );
	}

	[Fact]
	public void LookupIsOrdinalCultureIndependentAndDoesNotReorderPathEvidence() {
		TermInfoDatabaseCatalog catalog =
			CreateCatalog(
				"culture",
				CreateTerminal( "I-terminal" ),
				CreateTerminal( "ı-terminal" )
			);
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		try {
			CultureInfo.CurrentCulture = new CultureInfo( "tr-TR" );
			TermInfoDatabaseSet set =
				TermInfoDatabaseInspector.CreateSet([ catalog ]);

			TermInfoDatabaseSetLookupResult latin =
				set.LookupCanonicalName( "I-terminal" );
			TermInfoDatabaseSetLookupResult dotless =
				set.LookupCanonicalName( "ı-terminal" );

			Assert.Equal( TermInfoDatabaseSetLookupStatus.WinnerKnown, latin.Status );
			Assert.Equal( TermInfoDatabaseSetLookupStatus.WinnerKnown, dotless.Status );
			Assert.NotEqual( latin.Winner!.Entry.Path, dotless.Winner!.Entry.Path );
			Assert.Same( catalog.Entries[ 0 ], latin.Winner.Entry );
			Assert.Same( catalog.Entries[ 1 ], dotless.Winner.Entry );
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
		}
	}

	[Fact]
	public void Da02AddsOnlyStructuredLookupStatusAndResultConcepts() {
		Type[] exportedTypes =
			typeof( TermInfoDatabaseSetLookupResult ).Assembly.GetExportedTypes();

		Assert.Contains( typeof( TermInfoDatabaseSetLookupResult ), exportedTypes );
		Assert.Contains( typeof( TermInfoDatabaseSetLookupStatus ), exportedTypes );
		Assert.InRange( exportedTypes.Length, 39, int.MaxValue );
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
				"DA02 incomplete fixture."
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
		params string[] aliases
	) {
		TerminalDescriptionBuilder builder =
			new TerminalDescriptionBuilder( name );
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
			$"icod-terminfo-da02-{suffix}-{Guid.NewGuid():N}"
		);
}
