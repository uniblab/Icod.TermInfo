using Icod.TermInfo.Compiler;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class UC01BoundedCatalogTests {
	[Fact]
	public void OptionsSnapshotDefaultsAndSupportIndependentBudgets() {
		CompiledTermInfoParserOptions parser = new();
		TermInfoDatabaseCatalogReadOptions options = new( parser );
		Assert.NotSame( parser, options.ParserOptions );
		Assert.Equal( 1_048_576, options.ParserOptions.MaximumEntrySize );
		Assert.Equal( 131_072, options.MaximumCandidateCount );
		Assert.Equal( 65_536, options.MaximumEntryCount );
		Assert.Equal( 4_096, options.MaximumIssueCount );
		Assert.Equal( 67_108_864L, options.MaximumParsedBytes );
		Assert.Equal( 1, new TermInfoDatabaseCatalogReadOptions( maximumParsedBytes: 1 ).MaximumParsedBytes );
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( -1 )]
	public void OptionsRejectNonpositiveBudgets( int value ) {
		Assert.Throws<ArgumentOutOfRangeException>( () => new TermInfoDatabaseCatalogReadOptions( maximumCandidateCount: value ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new TermInfoDatabaseCatalogReadOptions( maximumEntryCount: value ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new TermInfoDatabaseCatalogReadOptions( maximumIssueCount: value ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new TermInfoDatabaseCatalogReadOptions( maximumParsedBytes: value ) );
	}

	[Fact]
	public void BudgetsAcceptExactMaximumAndFailWithoutOverflow() {
		using Fixture fixture = new();
		CatalogReadBudget budget = new( fixture.Root, new( maximumCandidateCount: 2, maximumEntryCount: 1, maximumIssueCount: 1, maximumParsedBytes: long.MaxValue ) );
		budget.ReserveCandidate();
		budget.ReserveCandidate();
		AssertLimit( "MaximumCandidateCount", 2, () => budget.ReserveCandidate() );
		budget.EnsureEntryCapacity();
		budget.ReserveEntry();
		AssertLimit( "MaximumEntryCount", 1, () => budget.EnsureEntryCapacity() );
		AssertLimit( "MaximumEntryCount", 1, () => budget.ReserveEntry() );
		budget.ReserveIssue();
		AssertLimit( "MaximumIssueCount", 1, () => budget.ReserveIssue() );
		budget.ReserveParsedBytes( long.MaxValue - 1 );
		budget.ReserveParsedBytes( 1 );
		AssertLimit( "MaximumParsedBytes", long.MaxValue, () => budget.ReserveParsedBytes( 1 ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => budget.ReserveParsedBytes( -1 ) );
	}

	[Fact]
	public void LimitExceptionValidatesMachineReadableFields() {
		using Fixture fixture = new();
		IOException cause = new( "cause" );
		TermInfoDatabaseCatalogLimitException error = new( fixture.Root, "MaximumParsedBytes", 7, cause );
		Assert.Equal( fixture.Root, error.SourcePath );
		Assert.Equal( "MaximumParsedBytes", error.LimitName );
		Assert.Equal( 7, error.Limit );
		Assert.Same( cause, error.InnerException );
		Assert.Throws<ArgumentException>( () => new TermInfoDatabaseCatalogLimitException( "relative", "MaximumParsedBytes", 1 ) );
		Assert.Throws<ArgumentException>( () => new TermInfoDatabaseCatalogLimitException( fixture.Root, "Unknown", 1 ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new TermInfoDatabaseCatalogLimitException( fixture.Root, "MaximumEntrySize", 0 ) );
	}

	[Fact]
	public void DistinctMethodPreservesLegacyNullCallAndRootStates() {
		using Fixture fixture = new();
		Assert.Empty( TermInfoDatabaseInspector.InspectDirectory( fixture.Root, null ).Entries );
		Assert.Empty( TermInfoDatabaseInspector.InspectDirectory( fixture.Root, null, default ).Entries );
		Assert.Equal( TermInfoDatabaseCatalogKind.ConventionalDirectory, Read( fixture ).Kind );
		Assert.Equal( TermInfoDatabaseCatalogKind.Missing, TermInfoDatabaseInspector.InspectDirectoryBounded( Path.Combine( fixture.Root, "missing" ) ).Kind );
		string file = Path.Combine( fixture.Root, "file" );
		File.WriteAllText( file, "anything" );
		Assert.Equal( TermInfoDatabaseCatalogKind.UnsupportedStore, TermInfoDatabaseInspector.InspectDirectoryBounded( file ).Kind );
		Assert.Throws<ArgumentNullException>( () => TermInfoDatabaseInspector.InspectDirectoryBounded( null! ) );
		Assert.Throws<ArgumentException>( () => TermInfoDatabaseInspector.InspectDirectoryBounded( " " ) );
	}

	[Fact]
	public void IgnoredRootChildrenAndNestedDirectoriesConsumeCandidates() {
		using Fixture fixture = new();
		File.WriteAllText( Path.Combine( fixture.Root, "ignored" ), "" );
		Directory.CreateDirectory( Path.Combine( fixture.Root, "not-a-layout" ) );
		Directory.CreateDirectory( Path.Combine( fixture.Root, "x", "nested" ) );
		File.WriteAllText( Path.Combine( fixture.Root, "x", "nested", "never-read" ), "bad" );
		Assert.Empty( Read( fixture, new( maximumCandidateCount: 4 ) ).Issues );
		AssertLimit( "MaximumCandidateCount", 3, () => Read( fixture, new( maximumCandidateCount: 3 ) ) );
	}

	[Fact]
	public void EntryAndParsedByteLimitsApplyToPhysicalCopies() {
		using Fixture fixture = new();
		byte[] bytes = Fixture.Entry();
		fixture.Write( "x", "x-sample", bytes );
		fixture.Write( "78", "x-sample", bytes );
		Assert.Equal( 2, Read( fixture, new( maximumEntryCount: 2, maximumParsedBytes: bytes.Length * 2 ) ).Entries.Count );
		AssertLimit( "MaximumEntryCount", 1, () => Read( fixture, new( maximumEntryCount: 1 ) ) );
		AssertLimit( "MaximumParsedBytes", bytes.Length * 2 - 1, () => Read( fixture, new( maximumParsedBytes: bytes.Length * 2 - 1 ) ) );
	}

	[Fact]
	public void MalformedCandidatesConsumeBytesAndIssueBudget() {
		using Fixture fixture = new();
		fixture.Write( "x", "x-a", [ 1, 2 ] );
		fixture.Write( "x", "x-b", [ 3, 4 ] );
		Assert.Equal( 2, Read( fixture, new( maximumIssueCount: 2, maximumParsedBytes: 4 ) ).Issues.Count );
		AssertLimit( "MaximumParsedBytes", 3, () => Read( fixture, new( maximumParsedBytes: 3 ) ) );
		fixture.Write( "x", "x-c", [ 5, 6 ] );
		AssertLimit( "MaximumIssueCount", 2, () => Read( fixture, new( maximumIssueCount: 2 ) ) );
	}

	[Fact]
	public void BoundedOversizeIsTypedWhileLegacyKeepsMalformedIssue() {
		using Fixture fixture = new();
		byte[] bytes = Fixture.Entry();
		fixture.Write( "x", "x-sample", bytes );
		CompiledTermInfoParserOptions parser = new( bytes.Length - 1 );
		AssertLimit( "MaximumEntrySize", bytes.Length - 1, () => Read( fixture, new( parser ) ) );
		Assert.Equal( TermInfoDatabaseCatalogIssueKind.MalformedEntry, Assert.Single( TermInfoDatabaseInspector.InspectDirectory( fixture.Root, parser ).Issues ).Kind );
	}

	[Fact]
	public void ChildLinksAreSkippedAndExplicitLinkedRootIsAllowed() {
		using Fixture fixture = new();
		fixture.Write( "x", "x-sample", Fixture.Entry() );
		string linkedRoot = fixture.Root + "-link";
		try {
			try {
				Directory.CreateSymbolicLink( linkedRoot, fixture.Root );
				File.CreateSymbolicLink( Path.Combine( fixture.Root, "x", "alias" ), Path.Combine( fixture.Root, "x", "x-sample" ) );
			} catch ( UnauthorizedAccessException ) { return; }
			TermInfoDatabaseCatalog result = TermInfoDatabaseInspector.InspectDirectoryBounded( linkedRoot );
			Assert.Single( result.Entries );
			Assert.Equal( TermInfoDatabaseCatalogIssueKind.LinkSkipped, Assert.Single( result.Issues ).Kind );
		} finally {
			if ( Directory.Exists( linkedRoot ) ) {
				Directory.Delete( linkedRoot );
			}
		}
	}

	[Fact]
	public void BoundedResultsPreserveLegacyPlacementAliasesAndDuplicates() {
		using Fixture fixture = new();
		byte[] bytes = Fixture.Entry();
		fixture.Write( "x", "x-sample", bytes );
		fixture.Write( "78", "x-sample", bytes );
		fixture.Write( "a", "alias", bytes );
		fixture.Write( "z", "wrong", bytes );
		fixture.Write( "x", "x-malformed", [ 1 ] );
		TermInfoDatabaseCatalog legacy = TermInfoDatabaseInspector.InspectDirectory( fixture.Root );
		TermInfoDatabaseCatalog bounded = Read( fixture );
		Assert.Equal( legacy.Kind, bounded.Kind );
		Assert.Equal( legacy.Entries.Select( x => (x.Path, x.Name, string.Join( "|", x.Aliases )) ), bounded.Entries.Select( x => (x.Path, x.Name, string.Join( "|", x.Aliases )) ) );
		Assert.Equal( legacy.Issues.Select( x => (x.Path, x.Kind, x.Message) ), bounded.Issues.Select( x => (x.Path, x.Kind, x.Message) ) );
		Assert.Equal( legacy.DuplicateCanonicalNames, bounded.DuplicateCanonicalNames );
		Assert.Equal( 4, bounded.Entries.Count );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EnumerationFailureIsUnavailableBeforeProgressAndPartialAfterProgress( bool yieldFirst ) {
		using Fixture fixture = new();
		fixture.Write( "x", "x-sample", Fixture.Entry() );
		bool disposed = false;
		IEnumerable<string> Enumerate( string path ) {
			if ( path != fixture.Root ) {
				foreach ( string child in Directory.EnumerateFileSystemEntries( path ) ) {
					yield return child;
				}
				yield break;
			}
			try {
				if ( yieldFirst ) {
					yield return Path.Combine( fixture.Root, "x" );
				}
				throw new IOException( "injected failure" );
			} finally { disposed = true; }
		}
		TermInfoDatabaseCatalog result = TermInfoDatabaseInspector.InspectDirectoryBoundedCore( fixture.Root, new(), default, Enumerate );
		Assert.True( disposed );
		Assert.Equal( yieldFirst ? TermInfoDatabaseCatalogKind.ConventionalDirectory : TermInfoDatabaseCatalogKind.Unavailable, result.Kind );
		Assert.Equal( yieldFirst ? 1 : 0, result.Entries.Count );
		Assert.Equal( TermInfoDatabaseCatalogIssueKind.IoFailure, Assert.Single( result.Issues ).Kind );
	}

	[Fact]
	public void CancellationDuringEnumerationDisposesEnumerator() {
		using Fixture fixture = new();
		using CancellationTokenSource cancellation = new();
		bool disposed = false;
		IEnumerable<string> Enumerate( string path ) {
			try {
				cancellation.Cancel();
				yield return Path.Combine( path, "ignored" );
			} finally { disposed = true; }
		}
		Assert.Throws<OperationCanceledException>( () => TermInfoDatabaseInspector.InspectDirectoryBoundedCore( fixture.Root, new(), cancellation.Token, Enumerate ) );
		Assert.True( disposed );
		Assert.Throws<OperationCanceledException>( () => TermInfoDatabaseInspector.InspectDirectoryBounded( fixture.Root, cancellationToken: cancellation.Token ) );
	}

	[Theory]
	[InlineData( true )]
	[InlineData( false )]
	public void CancellationIsObservedBeforeAllocationAndAfterRead( bool cancelOnLength ) {
		using Fixture fixture = new();
		using CancellationTokenSource cancellation = new();
		using CancelingStream stream = new( Fixture.Entry(), cancellation, cancelOnLength );
		TermInfoDatabaseCatalogReadOptions options = new();
		CatalogReadBudget budget = new( fixture.Root, options );
		Assert.Throws<OperationCanceledException>( () => TermInfoDatabaseInspector.ReadCatalogTerminalStream( stream, fixture.Root, options.ParserOptions, cancellation.Token, budget ) );
		Assert.Equal( cancelOnLength ? 0 : 1, stream.ReadCalls );
		if ( cancelOnLength ) {
			budget.ReserveParsedBytes( options.MaximumParsedBytes );
		}
	}

	[Fact]
	public void LegacyStreamKeepsSizeFailurePriorityWhenCancellationRacesLengthRead() {
		using Fixture fixture = new();
		using CancellationTokenSource cancellation = new();
		using CancelingStream stream = new( Fixture.Entry(), cancellation, true );
		Assert.Throws<CompiledTermInfoFormatException>( () => TermInfoDatabaseInspector.ReadCatalogTerminalStream(
			stream, fixture.Root, new( 1 ), cancellation.Token, null
		)
		);
	}

	[Fact]
	public void RootFileLinkWithLayoutNameIsCountedButIgnored() {
		using Fixture fixture = new();
		string target = Path.Combine( fixture.Root, "ordinary-file" );
		File.WriteAllText( target, "bad" );
		try { File.CreateSymbolicLink( Path.Combine( fixture.Root, "x" ), target ); }
		catch ( UnauthorizedAccessException ) { return; }
		Assert.Empty( Read( fixture, new( maximumCandidateCount: 2 ) ).Issues );
		AssertLimit( "MaximumCandidateCount", 1, () => Read( fixture, new( maximumCandidateCount: 1 ) ) );
	}

	private static TermInfoDatabaseCatalog Read( Fixture fixture, TermInfoDatabaseCatalogReadOptions? options = null ) =>
		TermInfoDatabaseInspector.InspectDirectoryBounded( fixture.Root, options );

	private static void AssertLimit( string name, long limit, Action action ) {
		TermInfoDatabaseCatalogLimitException error = Assert.Throws<TermInfoDatabaseCatalogLimitException>( action );
		Assert.Equal( name, error.LimitName );
		Assert.Equal( limit, error.Limit );
		Assert.True( Path.IsPathFullyQualified( error.SourcePath ) );
	}

	private sealed class CancelingStream : MemoryStream {
		private readonly CancellationTokenSource _cancellation;
		private readonly bool _cancelOnLength;
		internal int ReadCalls { get; private set; }
		internal CancelingStream( byte[] bytes, CancellationTokenSource cancellation, bool cancelOnLength ) : base( bytes ) {
			_cancellation = cancellation;
			_cancelOnLength = cancelOnLength;
		}
		public override long Length {
			get { if ( _cancelOnLength ) {
				_cancellation.Cancel();
			} return base.Length; }
		}
		public override int Read( byte[] buffer, int offset, int count ) {
			ReadCalls++;
			int result = base.Read( buffer, offset, count );
			_cancellation.Cancel();
			return result;
		}
	}

	private sealed class Fixture : IDisposable {
		internal string Root { get; } = Path.Combine( Path.GetTempPath(), "icod-uc01-" + Guid.NewGuid().ToString( "N" ) );
		internal Fixture() => Directory.CreateDirectory( Root );
		internal void Write( string directory, string name, byte[] bytes ) {
			Directory.CreateDirectory( Path.Combine( Root, directory ) );
			File.WriteAllBytes( Path.Combine( Root, directory, name ), bytes );
		}
		internal static byte[] Entry() => CompiledTermInfoWriter.Write( new TerminalDescriptionBuilder( "x-sample" ).SetDescription( "UC01 fixture" ).AddAlias( "alias" ).Build() );
		public void Dispose() => Directory.Delete( Root, true );
	}
}
