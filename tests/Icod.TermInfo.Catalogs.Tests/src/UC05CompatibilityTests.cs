using Icod.TermInfo.BerkeleyDb;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

[Collection( "UC01 process state" )]
public sealed class UC05CompatibilityTests {
	[Fact]
	public void OldDirectoryPhysicalRowsAndSetPlanningRetainMisplacedEntry() {
		using DirectoryCatalogFixture first = new();
		using DirectoryCatalogFixture second = new();
		byte[] bytes = DirectoryCatalogFixture.Bytes();
		first.Write( "s", "sample", bytes );
		first.Write( "z", "sample", bytes );
		second.Write( "s", "sample", bytes );
		TermInfoDatabaseCatalog physical = TermInfoDatabaseInspector.InspectDirectory( first.Root );
		Assert.Equal( TermInfoDatabaseCatalogKind.ConventionalDirectory, physical.Kind );
		Assert.Equal( 2, physical.Entries.Count );
		Assert.Contains( physical.Issues, issue => issue.Kind == TermInfoDatabaseCatalogIssueKind.InvalidPlacement );
		// Legacy source planning deliberately refuses an issue-bearing catalog.
		Assert.Throws<InvalidOperationException>( () =>
			TerminalDescriptionSourcePlanner.PlanFromDirectories( DirectoryCatalogFixture.Terminal(), [first.Root, second.Root] )
		);
		TermInfoDatabaseSetSourcePlanningResult plan = TerminalDescriptionSourcePlanner.PlanFromDirectories(
			DirectoryCatalogFixture.Terminal(), [second.Root]
		);
		Assert.Equal( second.Root, Assert.Single( plan.DatabaseSet.Entries ).Catalog.Root );
		TerminalCatalog unified = new TerminalCatalogReader( first.Source ).Read();
		Assert.Equal( TerminalCatalogStatus.Partial, unified.Status );
		Assert.Equal( "sample", Assert.Single( unified.Entries ).PublicationName );
		Assert.Contains( unified.Issues, issue => issue.Kind == TerminalCatalogIssueKind.InvalidPlacement );
	}

	[Fact]
	public void LegacyHashReadRetainsPublicationFamilyForSameImageAsUnified() {
		using HashedCatalogFixture fixture = new();
		fixture.Write();
		BerkeleyDbTerminalCatalogReader legacy = new( fixture.PathName );
		IReadOnlyList<BerkeleyDbTerminalCatalogEntry> oldRows = legacy.Read();
		Assert.Equal( new[] { "a", "b", "sample" }, oldRows.Select( row => row.Name ) );
		Assert.Equal( new[] { BerkeleyDbTerminalCatalogEntryKind.Alias,
			BerkeleyDbTerminalCatalogEntryKind.Alias, BerkeleyDbTerminalCatalogEntryKind.Canonical }, oldRows.Select( row => row.Kind )
		);
		TerminalCatalog unified = new TerminalCatalogReader( fixture.Source ).Read();
		Assert.Equal( TerminalCatalogStatus.Complete, unified.Status );
		Assert.Equal( oldRows.Select( row => row.Name ), unified.Entries.Select( row => row.PublicationName ) );
		Assert.All( unified.Entries, row => Assert.Null( row.EntryPath ) );
	}
}
