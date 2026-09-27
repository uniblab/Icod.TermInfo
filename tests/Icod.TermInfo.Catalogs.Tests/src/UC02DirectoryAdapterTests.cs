using System.Globalization;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

public sealed class UC02DirectoryAdapterTests {
	[Fact]
	public void CanonicalDeclarationsDoNotManufactureAliases() {
		using DirectoryCatalogFixture fixture = new();
		string path = fixture.Write( "s", "sample" );
		TerminalCatalog result = fixture.Read();
		TerminalCatalogEntry entry = Assert.Single( result.Entries );
		Assert.Equal( "sample", entry.PublicationName );
		Assert.Equal( TerminalCatalogEntryKind.Canonical, entry.Kind );
		Assert.Equal( new[] { "a", "b" }, entry.Terminal.Aliases );
		Assert.Equal( path, entry.EntryPath );
		Assert.Equal( fixture.Root, entry.SourcePath );
		Assert.Equal( TerminalCatalogStatus.Complete, result.Status );
		Assert.Empty( result.Issues );
	}

	[Fact]
	public void SeparateAliasFilesArePublicationsNotDuplicates() {
		using DirectoryCatalogFixture fixture = new();
		fixture.Write( "s", "sample" );
		fixture.Write( "61", "a" );
		fixture.Write( "b", "b" );
		Assert.Equal( new[] { "sample" }, TermInfoDatabaseInspector.InspectDirectoryBounded( fixture.Root ).DuplicateCanonicalNames );
		TerminalCatalog result = fixture.Read();
		Assert.Equal( new[] { "a", "b", "sample" }, result.Entries.Select( entry => entry.PublicationName ) );
		Assert.Equal( new[] { TerminalCatalogEntryKind.Alias, TerminalCatalogEntryKind.Alias, TerminalCatalogEntryKind.Canonical }, result.Entries.Select( entry => entry.Kind ) );
		Assert.Equal( TerminalCatalogStatus.Complete, result.Status );
		Assert.Empty( result.DuplicatePublicationNames );
		Assert.Empty( result.Issues );
	}

	[Fact]
	public void LiteralAndHexCopiesRetainEveryOccurrence() {
		using DirectoryCatalogFixture fixture = new();
		string literal = fixture.Write( "s", "sample" );
		string hex = fixture.Write( "73", "sample" );
		TerminalCatalog result = fixture.Read();
		Assert.Equal( new[] { hex, literal }, result.Entries.Select( entry => entry.EntryPath ) );
		Assert.Equal( new[] { "sample" }, result.DuplicatePublicationNames );
		Assert.Equal( TerminalCatalogStatus.Partial, result.Status );
		TerminalCatalogIssue issue = Assert.Single( result.Issues );
		Assert.Equal( TerminalCatalogIssueKind.DuplicatePublication, issue.Kind );
		Assert.Equal( "sample", issue.PublicationName );
		Assert.Null( issue.EntryPath );
		Assert.Equal( fixture.Root, issue.SourcePath );
	}

	[Fact]
	public void RepeatedAliasesAndDifferentDescriptionsRetainIdentityWithoutChoosingWinner() {
		using DirectoryCatalogFixture fixture = new();
		TerminalDescription first = DirectoryCatalogFixture.Terminal();
		TerminalDescription second = DirectoryCatalogFixture.Terminal( description: "different" );
		TermInfoDatabaseCatalog physical = fixture.Physical( entries: [
			fixture.Entry( "3/a", first ), fixture.Entry( "1/a", second ), fixture.Entry( "2/a", first ),
			fixture.Entry( "s/sample", first ), fixture.Entry( "73/sample", second ),
		], duplicates: [ "irrelevant-canonical-name" ]
		);
		TerminalCatalog result = ConventionalTerminalCatalogAdapter.Normalize( fixture.Source, new(), physical, default );
		Assert.Equal( 5, result.Entries.Count );
		Assert.Equal( new[] { "a", "sample" }, result.DuplicatePublicationNames );
		Assert.Equal( 2, result.Issues.Count );
		Assert.All( result.Issues, issue => Assert.Equal( TerminalCatalogIssueKind.DuplicatePublication, issue.Kind ) );
		Assert.Same( second, result.Entries[ 0 ].Terminal );
		Assert.Same( first, result.Entries[ 1 ].Terminal );
		Assert.All( result.Entries.Take( 3 ), entry => Assert.Equal( TerminalCatalogEntryKind.Alias, entry.Kind ) );
	}

	[Fact]
	public void MisplacedFilesRemainOnlyAsIssues() {
		using DirectoryCatalogFixture fixture = new();
		string bad = fixture.Write( "z", "sample" );
		string good = fixture.Write( "s", "sample" );
		Assert.Equal( 2, TermInfoDatabaseInspector.InspectDirectory( fixture.Root ).Entries.Count );
		TermInfoDatabaseCatalog physical = TermInfoDatabaseInspector.InspectDirectoryBounded( fixture.Root );
		Assert.Equal( 2, physical.Entries.Count );
		TerminalCatalog result = fixture.Read();
		Assert.Equal( good, Assert.Single( result.Entries ).EntryPath );
		TerminalCatalogIssue issue = Assert.Single( result.Issues );
		Assert.Equal( TerminalCatalogIssueKind.InvalidPlacement, issue.Kind );
		Assert.Equal( bad, issue.EntryPath );
		Assert.Equal( Assert.Single( physical.Issues ).Message, issue.Message );
		Assert.Null( issue.PublicationName );
		Assert.Equal( TerminalCatalogStatus.Partial, result.Status );
	}

	[Fact]
	public void InvalidPlacementPathMatchingIsOrdinal() {
		using DirectoryCatalogFixture fixture = new();
		TermInfoDatabaseCatalog physical = fixture.Physical(
			entries: [ fixture.Entry( "s/sample" ), fixture.Entry( "S/sample" ) ],
			issues: [ fixture.Issue( TermInfoDatabaseCatalogIssueKind.InvalidPlacement, "S/sample" ) ]
		);
		TerminalCatalog result = ConventionalTerminalCatalogAdapter.Normalize( fixture.Source, new(), physical, default );
		Assert.Equal( Path.Combine( fixture.Root, "s/sample" ), Assert.Single( result.Entries ).EntryPath );
		Assert.Empty( result.DuplicatePublicationNames );
	}

	[Theory]
	[InlineData( TermInfoDatabaseCatalogKind.Missing, TerminalCatalogStatus.Missing, TerminalCatalogIssueKind.MissingSource )]
	[InlineData( TermInfoDatabaseCatalogKind.UnsupportedStore, TerminalCatalogStatus.UnsupportedSource, TerminalCatalogIssueKind.UnsupportedSource )]
	public void EmptyFailureStatesGetOneSourceIssue( TermInfoDatabaseCatalogKind kind, TerminalCatalogStatus status, TerminalCatalogIssueKind issueKind ) {
		using DirectoryCatalogFixture fixture = new();
		TerminalCatalog result = ConventionalTerminalCatalogAdapter.Normalize( fixture.Source, new( maximumIssueCount: 1 ), fixture.Physical( kind ), default );
		Assert.Equal( status, result.Status );
		Assert.Empty( result.Entries );
		TerminalCatalogIssue issue = Assert.Single( result.Issues );
		Assert.Equal( issueKind, issue.Kind );
		Assert.Null( issue.EntryPath );
		Assert.Null( issue.PublicationName );
		Assert.Equal( fixture.Root, issue.SourcePath );
	}

	[Theory]
	[InlineData( TermInfoDatabaseCatalogIssueKind.MalformedEntry, TerminalCatalogIssueKind.MalformedEntry )]
	[InlineData( TermInfoDatabaseCatalogIssueKind.InvalidPlacement, TerminalCatalogIssueKind.InvalidPlacement )]
	[InlineData( TermInfoDatabaseCatalogIssueKind.PermissionFailure, TerminalCatalogIssueKind.PermissionFailure )]
	[InlineData( TermInfoDatabaseCatalogIssueKind.IoFailure, TerminalCatalogIssueKind.IoFailure )]
	[InlineData( TermInfoDatabaseCatalogIssueKind.LinkSkipped, TerminalCatalogIssueKind.LinkSkipped )]
	public void AcquisitionIssuesPreserveKindPathAndMessage( TermInfoDatabaseCatalogIssueKind lower, TerminalCatalogIssueKind expected ) {
		using DirectoryCatalogFixture fixture = new();
		TermInfoDatabaseCatalog physical = fixture.Physical( issues: [ fixture.Issue( lower, "s", "diagnostic" ) ] );
		TerminalCatalog result = ConventionalTerminalCatalogAdapter.Normalize( fixture.Source, new(), physical, default );
		Assert.Equal( TerminalCatalogStatus.Partial, result.Status );
		Assert.Empty( result.Entries );
		TerminalCatalogIssue issue = Assert.Single( result.Issues );
		Assert.Equal( expected, issue.Kind );
		Assert.Equal( Path.Combine( fixture.Root, "s" ), issue.EntryPath );
		Assert.Equal( "diagnostic", issue.Message );
		Assert.Null( issue.PublicationName );
	}

	[Theory]
	[InlineData( TermInfoDatabaseCatalogIssueKind.PermissionFailure )]
	[InlineData( TermInfoDatabaseCatalogIssueKind.IoFailure )]
	public void RootFailureBeforeAndAfterObservationsHasDistinctStatus( TermInfoDatabaseCatalogIssueKind kind ) {
		using DirectoryCatalogFixture fixture = new();
		TermInfoDatabaseCatalogIssue issue = fixture.Issue( kind );
		TerminalCatalog unavailable = ConventionalTerminalCatalogAdapter.Normalize( fixture.Source, new(), fixture.Physical( TermInfoDatabaseCatalogKind.Unavailable, issues: [ issue ] ), default );
		Assert.Equal( TerminalCatalogStatus.Unavailable, unavailable.Status );
		Assert.Empty( unavailable.Entries );
		Assert.Null( Assert.Single( unavailable.Issues ).EntryPath );
		TerminalCatalog partial = ConventionalTerminalCatalogAdapter.Normalize( fixture.Source, new(), fixture.Physical( entries: [ fixture.Entry( "s/sample" ) ], issues: [ issue ] ), default );
		Assert.Equal( TerminalCatalogStatus.Partial, partial.Status );
		Assert.Single( partial.Entries );
		Assert.Null( Assert.Single( partial.Issues ).EntryPath );
	}

	[Fact]
	public void RealEmptyMissingFileAndMalformedRootsMapCorrectly() {
		using DirectoryCatalogFixture fixture = new();
		Assert.Equal( TerminalCatalogStatus.Complete, fixture.Read().Status );
		TerminalCatalogSource missing = new( Path.Combine( fixture.Root, "missing" ), TerminalCatalogSourceKind.ConventionalDirectory );
		Assert.Equal( TerminalCatalogStatus.Missing, ConventionalTerminalCatalogAdapter.Read( missing, new(), default ).Status );
		string file = fixture.Write( "s", "bad", [ 1, 2, 3 ] );
		Assert.Equal( TerminalCatalogStatus.UnsupportedSource, ConventionalTerminalCatalogAdapter.Read( new( file, TerminalCatalogSourceKind.ConventionalDirectory ), new(), default ).Status );
		TerminalCatalog malformed = fixture.Read();
		Assert.Equal( TerminalCatalogStatus.Partial, malformed.Status );
		Assert.Empty( malformed.Entries );
		Assert.Equal( TerminalCatalogIssueKind.MalformedEntry, Assert.Single( malformed.Issues ).Kind );
		fixture.Write( "s", "sample" );
		Assert.Single( fixture.Read().Entries );
	}

	[Fact]
	public void OrderingIsOrdinalAcrossCulturesAndInputOrder() {
		using DirectoryCatalogFixture fixture = new();
		TerminalDescription terminal = new TerminalDescriptionBuilder( "I" ).AddAlias( "i" ).Build();
		TermInfoDatabaseCatalogEntry[] entries = [ fixture.Entry( "z/i", terminal ), fixture.Entry( "a/I", terminal ), fixture.Entry( "b/I", terminal ) ];
		TermInfoDatabaseCatalogIssue[] issues = [ fixture.Issue( TermInfoDatabaseCatalogIssueKind.IoFailure, "z", "z" ), fixture.Issue( TermInfoDatabaseCatalogIssueKind.MalformedEntry, "z", "a" ) ];
		CultureInfo original = CultureInfo.CurrentCulture;
		try {
			foreach ( string culture in new[] { "en-US", "tr-TR" } ) {
				CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( culture );
				foreach ( bool reverse in new[] { false, true } ) {
					TerminalCatalog result = ConventionalTerminalCatalogAdapter.Normalize( fixture.Source, new(), fixture.Physical( entries: reverse ? entries.Reverse() : entries, issues: reverse ? issues.Reverse() : issues ), default );
					Assert.Equal( new[] { "I", "I", "i" }, result.Entries.Select( entry => entry.PublicationName ) );
					Assert.Equal( new[] { "a/I", "b/I", "z/i" }.Select( path => Path.Combine( fixture.Root, path ) ), result.Entries.Select( entry => entry.EntryPath ) );
					Assert.Equal( new[] { TerminalCatalogEntryKind.Canonical, TerminalCatalogEntryKind.Canonical, TerminalCatalogEntryKind.Alias }, result.Entries.Select( entry => entry.Kind ) );
					Assert.Equal( new[] { TerminalCatalogIssueKind.DuplicatePublication, TerminalCatalogIssueKind.MalformedEntry, TerminalCatalogIssueKind.IoFailure }, result.Issues.Select( issue => issue.Kind ) );
					Assert.Equal( new[] { "I" }, result.DuplicatePublicationNames );
				}
			}
		} finally {
			CultureInfo.CurrentCulture = original;
		}
	}
}
