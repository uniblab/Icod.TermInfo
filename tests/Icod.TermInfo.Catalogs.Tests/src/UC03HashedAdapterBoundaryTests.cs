using Icod.TermInfo.BerkeleyDb;
using System.Collections;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

public sealed class UC03HashedAdapterBoundaryTests {
	[Fact]
	public void FileDisappearingAfterInspectionReturnsMissing() {
		using HashedCatalogFixture fixture = new(); fixture.Write();
		var result = HashedTerminalCatalogAdapter.ReadCore( fixture.Source, new(), default,
			path => { var attributes = File.GetAttributes( path ); File.Delete( path ); return attributes; },
			( path, options, limits, token ) => new BerkeleyDbTerminalCatalogReader( path, options ).ReadBounded( limits, token )
		);
		Assert.Equal( TerminalCatalogStatus.Missing, result.Status );
		Assert.Equal( TerminalCatalogIssueKind.MissingSource, Assert.Single( result.Issues ).Kind );
	}

	[Fact]
	public void CancellationAfterFinalMappedRowPreventsResultReturn() {
		using HashedCatalogFixture fixture = new(); fixture.Write();
		using var cancellation = new CancellationTokenSource();
		var rows = new CompletingRows( fixture.Rows(), cancellation.Cancel );
		Assert.Throws<OperationCanceledException>( () => HashedTerminalCatalogAdapter.ReadCore( fixture.Source, new(), cancellation.Token,
			File.GetAttributes, ( p, o, l, c ) => rows
		)
		);
	}

	[Fact]
	public void NormalizationFailuresAreNotMisclassifiedAsAcquisitionIo() {
		using HashedCatalogFixture fixture = new(); fixture.Write();
		var failure = new IOException( "normalization failure" );
		var rows = new CompletingRows( fixture.Rows(), () => throw failure );
		Assert.Same( failure, Assert.Throws<IOException>( () => HashedTerminalCatalogAdapter.ReadCore( fixture.Source, new(), default,
			File.GetAttributes, ( p, o, l, c ) => rows
		)
		)
		);
	}

	private sealed class CompletingRows( IReadOnlyList<BerkeleyDbTerminalCatalogEntry> rows, Action complete ) : IReadOnlyList<BerkeleyDbTerminalCatalogEntry> {
		public int Count => rows.Count;
		public BerkeleyDbTerminalCatalogEntry this[int index] => rows[index];
		public IEnumerator<BerkeleyDbTerminalCatalogEntry> GetEnumerator() {
			foreach ( var row in rows ) { yield return row; }
			complete();
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	[Fact]
	public void MissingFileAndExplicitDirectoryAreDistinct() {
		using HashedCatalogFixture fixture = new();
		Assert.Equal( TerminalCatalogStatus.Missing, fixture.Read().Status );
		var result = HashedTerminalCatalogAdapter.Read( new( fixture.Root, TerminalCatalogSourceKind.BerkeleyDbHash ), new(), default );
		Assert.Equal( TerminalCatalogStatus.UnsupportedSource, result.Status );
		Assert.Equal( TerminalCatalogIssueKind.UnsupportedSource, Assert.Single( result.Issues ).Kind );
	}

	[Theory]
	[InlineData( "missing", false, TerminalCatalogStatus.Missing, TerminalCatalogIssueKind.MissingSource )]
	[InlineData( "directory-missing", false, TerminalCatalogStatus.Missing, TerminalCatalogIssueKind.MissingSource )]
	[InlineData( "permission", false, TerminalCatalogStatus.Unavailable, TerminalCatalogIssueKind.PermissionFailure )]
	[InlineData( "io", false, TerminalCatalogStatus.Unavailable, TerminalCatalogIssueKind.IoFailure )]
	[InlineData( "missing", true, TerminalCatalogStatus.Missing, TerminalCatalogIssueKind.MissingSource )]
	[InlineData( "directory-missing", true, TerminalCatalogStatus.Missing, TerminalCatalogIssueKind.MissingSource )]
	[InlineData( "permission", true, TerminalCatalogStatus.Unavailable, TerminalCatalogIssueKind.PermissionFailure )]
	[InlineData( "io", true, TerminalCatalogStatus.Unavailable, TerminalCatalogIssueKind.IoFailure )]
	[InlineData( "format", true, TerminalCatalogStatus.InvalidStore, TerminalCatalogIssueKind.InvalidHashedStore )]
	[InlineData( "invalid-data", true, TerminalCatalogStatus.InvalidStore, TerminalCatalogIssueKind.InvalidHashedStore )]
	public void KnownFailuresHaveScopedSourceOutcomes( string failure, bool acquisition, TerminalCatalogStatus status, TerminalCatalogIssueKind kind ) {
		using HashedCatalogFixture fixture = new();
		Exception exception = failure switch {
			"missing" => new FileNotFoundException(), "directory-missing" => new DirectoryNotFoundException(),
			"permission" => new UnauthorizedAccessException(), "io" => new IOException(),
			"format" => new BerkeleyDbDatabaseFormatException( "bad" ), _ => new InvalidDataException(),
		};
		var result = HashedTerminalCatalogAdapter.ReadCore( fixture.Source, new(), default,
			p => acquisition ? FileAttributes.Normal : throw exception, ( p, o, l, c ) => throw exception
		);
		Assert.Equal( status, result.Status ); Assert.Empty( result.Entries );
		var issue = Assert.Single( result.Issues ); Assert.Equal( kind, issue.Kind );
		Assert.Null( issue.EntryPath ); Assert.Null( issue.PublicationName );
	}

	[Theory]
	[InlineData( "MaximumDatabaseSize" )]
	[InlineData( "MaximumRecordCount" )]
	[InlineData( "MaximumIndexHops" )]
	[InlineData( "MaximumDecodedBytes" )]
	[InlineData( "MaximumParsedBytes" )]
	[InlineData( "MaximumEntrySize" )]
	[InlineData( "MaximumStoredItemSize" )]
	[InlineData( "MaximumPublicationCount" )]
	public void TypedLimitsPreserveCauseSourceAndValue( string name ) {
		using HashedCatalogFixture fixture = new(); var source = fixture.Source;
		var lower = new BerkeleyDbCatalogLimitException( source.Path, name, name == "MaximumIndexHops" ? 0 : 7 );
		var error = Assert.Throws<TerminalCatalogLimitException>( () => HashedTerminalCatalogAdapter.ReadCore( source, new(), default,
			p => FileAttributes.Normal, ( p, o, l, c ) => throw lower
		)
		);
		Assert.Same( source, error.Source ); Assert.Same( lower, error.InnerException ); Assert.Equal( lower.Limit, error.Limit );
		Assert.Equal( name == "MaximumPublicationCount" ? "MaximumEntryCount" : name, error.LimitName );
	}

	[Fact]
	public void ForwardsAllAcquisitionOptionsAndToken() {
		using HashedCatalogFixture fixture = new(); fixture.Write();
		using var cancel = new CancellationTokenSource(); var rows = fixture.Rows();
		var result = HashedTerminalCatalogAdapter.ReadCore( fixture.Source,
			new( new( 1234 ), maximumEntryCount: 7, maximumParsedBytes: 2345, maximumDatabaseSize: 3456,
				maximumRecordCount: 8, maximumIndexHops: 9, maximumDecodedBytes: 4567
			), cancel.Token,
			File.GetAttributes, ( path, options, limits, token ) => {
				Assert.Equal( fixture.PathName, path ); Assert.Equal( cancel.Token, token );
				Assert.Equal( 1234, options.ParserOptions.MaximumEntrySize ); Assert.Equal( 3456, options.MaximumDatabaseSize );
				Assert.Equal( 8, options.MaximumRecordCount ); Assert.Equal( 9, options.MaximumIndexHops );
				Assert.Equal( 7, limits.MaximumPublicationCount ); Assert.Equal( 2345, limits.MaximumParsedBytes ); Assert.Equal( 4567, limits.MaximumDecodedBytes );
				return rows;
			}
		); Assert.Equal( 3, result.Entries.Count );
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 1 )]
	[InlineData( 2 )]
	public void CancellationStopsInspectionAcquisitionOrNormalization( int stage ) {
		using HashedCatalogFixture fixture = new(); fixture.Write(); var rows = fixture.Rows();
		using var cancel = new CancellationTokenSource(); int inspections = 0, acquisitions = 0;
		if ( stage == 0 ) { cancel.Cancel(); }
		Assert.Throws<OperationCanceledException>( () => HashedTerminalCatalogAdapter.ReadCore( fixture.Source, new(), cancel.Token,
			p => { inspections++; if ( stage == 1 ) { cancel.Cancel(); } return FileAttributes.Normal; },
			( p, o, l, c ) => { acquisitions++; cancel.Cancel(); return rows; }
		)
		);
		Assert.Equal( stage == 0 ? 0 : 1, inspections ); Assert.Equal( stage == 2 ? 1 : 0, acquisitions );
	}

	[Fact]
	public void UnexpectedAndInspectionFormatExceptionsPropagate() {
		using HashedCatalogFixture fixture = new();
		var format = new BerkeleyDbDatabaseFormatException( "inspection failure" );
		Assert.Same( format, Assert.Throws<BerkeleyDbDatabaseFormatException>( () => HashedTerminalCatalogAdapter.ReadCore(
			fixture.Source, new(), default, p => throw format, ( p, o, l, c ) => []
		)
		)
		);
		foreach ( Exception unexpected in new Exception[] { new ArgumentException(), new InvalidOperationException() } ) {
			Assert.Same( unexpected, Assert.ThrowsAny<Exception>( () => HashedTerminalCatalogAdapter.ReadCore( fixture.Source, new(), default,
				p => FileAttributes.Normal, ( p, o, l, c ) => throw unexpected
			)
			)
			);
		}
	}
}
