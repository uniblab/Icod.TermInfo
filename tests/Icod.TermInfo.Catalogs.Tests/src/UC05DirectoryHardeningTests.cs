using Icod.TermInfo.Compiler;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

public sealed class UC05DirectoryHardeningTests {
	[Theory]
	[InlineData( 0 )]
	[InlineData( 37 )]
	public void EveryRootAndChildCandidateConsumesOneInclusiveSlot( int ignoredCount ) {
		using DirectoryCatalogFixture fixture = new();
		fixture.Write( "s", "sample" );
		Directory.CreateDirectory( Path.Combine( fixture.Root, "s", "nested" ) );
		for ( int i = 0; i < ignoredCount; i++ ) {
			File.WriteAllText( Path.Combine( fixture.Root, $"ignored-{i:D3}" ), "x" );
		}
		// One eligible root directory, one compiled file, one nested directory,
		// and all ignored root children are yielded by the two enumerators.
		int exact = ignoredCount + 3;
		TerminalCatalogSource source = fixture.Source;
		TerminalCatalogReader reader = new( source, new( maximumCandidateCount: exact ) );
		Assert.Single( reader.Read().Entries );
		Assert.Single( reader.Read().Entries );
		TerminalCatalogLimitException error = Assert.Throws<TerminalCatalogLimitException>( () =>
			new TerminalCatalogReader( source, new( maximumCandidateCount: exact - 1 ) ).Read()
		);
		Assert.Same( source, error.Source );
		Assert.Equal( "MaximumCandidateCount", error.LimitName );
		Assert.Equal( exact - 1, error.Limit );
		Assert.IsType<TermInfoDatabaseCatalogLimitException>( error.InnerException );
	}

	[Fact]
	public void PhysicalCopiesMalformedBytesAndMisplacementConsumeAggregateBytes() {
		using DirectoryCatalogFixture fixture = new();
		byte[] large = CompiledTermInfoWriter.Write( DirectoryCatalogFixture.Terminal( description: new string( 'x', 8192 ) ) );
		fixture.Write( "s", "sample", large );
		fixture.Write( "73", "sample", large );
		fixture.Write( "z", "sample", large );
		fixture.Write( "b", "broken", [ 1, 2, 3 ] );
		long total = 3L * large.Length + 3;
		TerminalCatalogSource source = fixture.Source;
		TerminalCatalogReader reader = new( source, new( new( large.Length ), maximumParsedBytes: total ) );
		TerminalCatalog result = reader.Read();
		Assert.Equal( TerminalCatalogStatus.Partial, result.Status );
		Assert.Equal( 2, result.Entries.Count );
		Assert.Equal( 3, result.Issues.Count );
		Assert.Equal( 2, reader.Read().Entries.Count );
		AssertLimit( source, new( maximumParsedBytes: total - 1 ), "MaximumParsedBytes", total - 1 );
		AssertLimit( source, new( new( large.Length - 1 ) ), "MaximumEntrySize", large.Length - 1 );
		TermInfoDatabaseCatalog physical = TermInfoDatabaseInspector.InspectDirectory( fixture.Root );
		Assert.Equal( 3, physical.Entries.Count );
	}

	[Fact]
	public void DuplicateGroupsAndAcquisitionIssuesShareIssueBudgetWithoutPartialResult() {
		using DirectoryCatalogFixture fixture = new();
		byte[] bytes = DirectoryCatalogFixture.Bytes();
		fixture.Write( "s", "sample", bytes );
		fixture.Write( "73", "sample", bytes );
		fixture.Write( "a", "a", bytes );
		fixture.Write( "61", "a", bytes );
		fixture.Write( "b", "broken", [ 1, 2, 3 ] );
		TerminalCatalogSource source = fixture.Source;
		TerminalCatalog result = new TerminalCatalogReader( source, new( maximumIssueCount: 3, maximumEntryCount: 4 ) ).Read();
		Assert.Equal( TerminalCatalogStatus.Partial, result.Status );
		Assert.Equal( 4, result.Entries.Count );
		Assert.Equal( 3, result.Issues.Count );
		Assert.Equal( new[] { "a", "sample" }, result.DuplicatePublicationNames );
		AssertLimit( source, new( maximumIssueCount: 2 ), "MaximumIssueCount", 2, lower: false );
		AssertLimit( source, new( maximumEntryCount: 3 ), "MaximumEntryCount", 3 );
		fixture.Write( "z", "sample", bytes );
		AssertLimit( source, new( maximumEntryCount: 4 ), "MaximumEntryCount", 4 );
	}

	private static void AssertLimit( TerminalCatalogSource source, TerminalCatalogReadOptions options, string name, long limit, bool lower = true ) {
		TerminalCatalogLimitException error = Assert.Throws<TerminalCatalogLimitException>( () => new TerminalCatalogReader( source, options ).Read() );
		Assert.Same( source, error.Source );
		Assert.Equal( name, error.LimitName );
		Assert.Equal( limit, error.Limit );
		if ( lower ) {
			Assert.IsType<TermInfoDatabaseCatalogLimitException>( error.InnerException );
		} else {
			Assert.Null( error.InnerException );
		}
	}
}
