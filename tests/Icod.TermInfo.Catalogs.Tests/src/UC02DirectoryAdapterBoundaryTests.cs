using Icod.TermInfo.Inspection;
using Xunit;
using Xunit.Abstractions;

namespace Icod.TermInfo.Catalogs.Tests;

public sealed class UC02DirectoryAdapterBoundaryTests( ITestOutputHelper output ) {
	[Fact]
	public void AcquisitionReceivesAllDirectoryBudgetsAndCancellation() {
		using DirectoryCatalogFixture fixture = new();
		using CancellationTokenSource cancellation = new();
		TerminalCatalogReadOptions options = new( new( 100 ), 2, 3, 4, 5L );
		bool acquired = false;
		TerminalCatalog result = ConventionalTerminalCatalogAdapter.ReadCore( fixture.Source, options, cancellation.Token, ( root, actual, token ) => {
			Assert.Equal( fixture.Root, root );
			Assert.Equal( cancellation.Token, token );
			Assert.Equal( 100, actual.ParserOptions.MaximumEntrySize );
			Assert.Equal( 2, actual.MaximumCandidateCount );
			Assert.Equal( 3, actual.MaximumEntryCount );
			Assert.Equal( 4, actual.MaximumIssueCount );
			Assert.Equal( 5L, actual.MaximumParsedBytes );
			acquired = true;
			return fixture.Physical();
		}
		);
		Assert.True( acquired );
		Assert.Equal( TerminalCatalogStatus.Complete, result.Status );
	}

	[Theory]
	[InlineData( "MaximumCandidateCount" )]
	[InlineData( "MaximumEntryCount" )]
	[InlineData( "MaximumIssueCount" )]
	[InlineData( "MaximumParsedBytes" )]
	[InlineData( "MaximumEntrySize" )]
	public void LowerLimitsKeepSourceNameValueAndInnerException( string name ) {
		using DirectoryCatalogFixture fixture = new();
		TerminalCatalogSource source = fixture.Source;
		TermInfoDatabaseCatalogLimitException lower = new( fixture.Root, name, 7 );
		TerminalCatalogLimitException error = Assert.Throws<TerminalCatalogLimitException>( () =>
			ConventionalTerminalCatalogAdapter.ReadCore( source, new(), default, ( _, _, _ ) => throw lower )
		);
		Assert.Same( source, error.Source );
		Assert.Same( lower, error.InnerException );
		Assert.Equal( name, error.LimitName );
		Assert.Equal( 7L, error.Limit );
	}

	[Fact]
	public void UnexpectedExceptionsAndCancellationPropagateUnchanged() {
		using DirectoryCatalogFixture fixture = new();
		Exception[] exceptions = [ new InvalidOperationException(), new ArgumentException(), new OperationCanceledException(), new IOException() ];
		foreach ( Exception expected in exceptions ) {
			Exception? actual = Record.Exception( () => ConventionalTerminalCatalogAdapter.ReadCore( fixture.Source, new(), default, ( _, _, _ ) => throw expected ) );
			Assert.Same( expected, actual );
		}
	}

	[Fact]
	public void CancellationBeforeAcquisitionAndAfterAcquisitionProducesNoResult() {
		using DirectoryCatalogFixture fixture = new();
		using CancellationTokenSource cancellation = new();
		Assert.Throws<OperationCanceledException>( () => ConventionalTerminalCatalogAdapter.ReadCore( fixture.Source, new(), cancellation.Token, ( _, _, _ ) => {
			cancellation.Cancel();
			return fixture.Physical();
		}
		)
		);
		Assert.Throws<OperationCanceledException>( () => ConventionalTerminalCatalogAdapter.ReadCore( fixture.Source, new(), cancellation.Token, ( _, _, _ ) => throw new InvalidOperationException( "Must not acquire" ) ) );
		Assert.Throws<OperationCanceledException>( () => ConventionalTerminalCatalogAdapter.Normalize( fixture.Source, new(), fixture.Physical(), cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => fixture.Read( token: cancellation.Token ) );
	}

	[Fact]
	public void AcquisitionAndDuplicateIssuesShareInclusiveCapacity() {
		using DirectoryCatalogFixture fixture = new();
		fixture.Write( "s", "sample" );
		fixture.Write( "73", "sample" );
		fixture.Write( "b", "bad", [ 1, 2, 3 ] );
		Assert.Equal( 2, fixture.Read( new( maximumIssueCount: 2 ) ).Issues.Count );
		TerminalCatalogLimitException error = Assert.Throws<TerminalCatalogLimitException>( () => fixture.Read( new( maximumIssueCount: 1 ) ) );
		Assert.Equal( "MaximumIssueCount", error.LimitName );
		Assert.Equal( 1L, error.Limit );
		Assert.Null( error.InnerException );
	}

	[Fact]
	public void DuplicateOnlyGroupsAndAcquisitionIssuesCannotExceedCapacity() {
		using DirectoryCatalogFixture fixture = new();
		fixture.Write( "s", "sample" );
		fixture.Write( "73", "sample" );
		Assert.Single( fixture.Read( new( maximumIssueCount: 1 ) ).Issues );
		fixture.Write( "a", "a" );
		fixture.Write( "61", "a" );
		Assert.Equal( 2, fixture.Read( new( maximumIssueCount: 2 ) ).Issues.Count );
		AssertLimit( () => fixture.Read( new( maximumIssueCount: 1 ) ), "MaximumIssueCount", 1, false );
		fixture.Write( "b", "bad1", [ 1 ] );
		fixture.Write( "b", "bad2", [ 2 ] );
		AssertLimit( () => fixture.Read( new( maximumIssueCount: 1 ) ), "MaximumIssueCount", 1 );
	}

	[Fact]
	public void CandidateEntryAndByteBoundsAcceptExactMaximum() {
		using DirectoryCatalogFixture fixture = new();
		byte[] bytes = DirectoryCatalogFixture.Bytes();
		fixture.Write( "s", "sample", bytes );
		TerminalCatalogReadOptions exact = new( new( bytes.Length ), 2, 1, 1, bytes.Length );
		Assert.Single( fixture.Read( exact ).Entries );
		AssertLimit( () => fixture.Read( new( maximumCandidateCount: 1 ) ), "MaximumCandidateCount", 1 );
		AssertLimit( () => fixture.Read( new( maximumParsedBytes: bytes.Length - 1 ) ), "MaximumParsedBytes", bytes.Length - 1 );
		AssertLimit( () => fixture.Read( new( new( bytes.Length - 1 ) ) ), "MaximumEntrySize", bytes.Length - 1 );
		fixture.Write( "a", "a", bytes );
		Assert.Equal( 2, fixture.Read( new( maximumEntryCount: 2, maximumParsedBytes: 2L * bytes.Length ) ).Entries.Count );
		AssertLimit( () => fixture.Read( new( maximumEntryCount: 1 ) ), "MaximumEntryCount", 1 );
		AssertLimit( () => fixture.Read( new( maximumParsedBytes: 2L * bytes.Length - 1 ) ), "MaximumParsedBytes", 2L * bytes.Length - 1 );
	}

	[Fact]
	public void IgnoredChildrenMalformedBytesAndMisplacedParsesAreNotRefunded() {
		using DirectoryCatalogFixture fixture = new();
		byte[] bytes = DirectoryCatalogFixture.Bytes();
		fixture.Write( "s", "sample", bytes );
		File.WriteAllText( Path.Combine( fixture.Root, "ignored" ), "ignored" );
		Directory.CreateDirectory( Path.Combine( fixture.Root, "s", "nested" ) );
		Assert.Single( fixture.Read( new( maximumCandidateCount: 4 ) ).Entries );
		AssertLimit( () => fixture.Read( new( maximumCandidateCount: 3 ) ), "MaximumCandidateCount", 3 );
		fixture.Write( "z", "sample", bytes );
		Assert.Single( fixture.Read( new( maximumEntryCount: 2 ) ).Entries );
		AssertLimit( () => fixture.Read( new( maximumEntryCount: 1 ) ), "MaximumEntryCount", 1 );
		fixture.Write( "b", "bad", [ 1, 2, 3 ] );
		Assert.Single( fixture.Read( new( maximumParsedBytes: 2L * bytes.Length + 3 ) ).Entries );
		AssertLimit( () => fixture.Read( new( maximumParsedBytes: 2L * bytes.Length + 2 ) ), "MaximumParsedBytes", 2L * bytes.Length + 2 );
	}

	[Fact]
	public void ChildLinksAreSkippedAndExplicitLinkedRootIsAllowed() {
		using DirectoryCatalogFixture fixture = new();
		string target = fixture.Write( "s", "sample" );
		string linkedRoot = fixture.Root + "-link";
		try {
			try {
				Directory.CreateSymbolicLink( linkedRoot, fixture.Root );
				File.CreateSymbolicLink( Path.Combine( fixture.Root, "s", "a" ), target );
				Directory.CreateSymbolicLink( Path.Combine( fixture.Root, "73" ), Path.Combine( fixture.Root, "s" ) );
			} catch ( UnauthorizedAccessException ) {
				output.WriteLine( "Link creation is unavailable on this host; link assertions were not exercised." );
				return;
			}
			TerminalCatalog result = ConventionalTerminalCatalogAdapter.Read( new( linkedRoot, TerminalCatalogSourceKind.ConventionalDirectory ), new(), default );
			Assert.Equal( TerminalCatalogStatus.Partial, result.Status );
			TerminalCatalogEntry entry = Assert.Single( result.Entries );
			Assert.Equal( "sample", entry.PublicationName );
			Assert.Equal( Path.Combine( linkedRoot, "s", "sample" ), entry.EntryPath );
			Assert.Equal( linkedRoot, entry.SourcePath );
			Assert.Equal( 2, result.Issues.Count );
			Assert.All( result.Issues, issue => Assert.Equal( TerminalCatalogIssueKind.LinkSkipped, issue.Kind ) );
		} finally {
			if ( Directory.Exists( linkedRoot ) ) {
				Directory.Delete( linkedRoot );
			}
		}
	}

	[Fact]
	public void InvalidInternalInputsFailInsteadOfInventingProvenance() {
		using DirectoryCatalogFixture fixture = new();
		TerminalCatalogSource hash = new( fixture.Root, TerminalCatalogSourceKind.BerkeleyDbHash );
		Assert.Throws<ArgumentNullException>( () => ConventionalTerminalCatalogAdapter.Read( null!, new(), default ) );
		Assert.Throws<ArgumentNullException>( () => ConventionalTerminalCatalogAdapter.Read( fixture.Source, null!, default ) );
		Assert.Throws<ArgumentException>( () => ConventionalTerminalCatalogAdapter.Read( hash, new(), default ) );
		Assert.Throws<ArgumentNullException>( () => ConventionalTerminalCatalogAdapter.ReadCore( fixture.Source, new(), default, null! ) );
		Assert.Throws<ArgumentNullException>( () => ConventionalTerminalCatalogAdapter.Normalize( fixture.Source, new(), null!, default ) );
		TerminalCatalogSource other = new( fixture.Root + "-other", TerminalCatalogSourceKind.ConventionalDirectory );
		Assert.Throws<ArgumentException>( () => ConventionalTerminalCatalogAdapter.Normalize( other, new(), fixture.Physical(), default ) );
	}

	private static void AssertLimit( Action action, string name, long value, bool lower = true ) {
		TerminalCatalogLimitException error = Assert.Throws<TerminalCatalogLimitException>( action );
		Assert.Equal( name, error.LimitName );
		Assert.Equal( value, error.Limit );
		if ( lower ) {
			Assert.IsType<TermInfoDatabaseCatalogLimitException>( error.InnerException );
		} else {
			Assert.Null( error.InnerException );
		}
	}
}
