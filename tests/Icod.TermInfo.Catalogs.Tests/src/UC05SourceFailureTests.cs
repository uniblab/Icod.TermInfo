using Icod.TermInfo.BerkeleyDb;
using Icod.TermInfo.Inspection;
using Xunit;
using Xunit.Abstractions;

namespace Icod.TermInfo.Catalogs.Tests;

public sealed class UC05SourceFailureTests( ITestOutputHelper output ) {
	[Theory]
	[InlineData( false, TerminalCatalogStatus.Unavailable )]
	[InlineData( true, TerminalCatalogStatus.Partial )]
	public void EnumerationFailureKeepsOnlyObservedChildrenAndDisposesIterator( bool progress, TerminalCatalogStatus status ) {
		using DirectoryCatalogFixture fixture = new();
		fixture.Write( "s", "sample" );
		bool disposed = false;
		IEnumerable<string> Enumerate( string path ) {
			if ( path != fixture.Root ) {
				foreach ( string child in Directory.EnumerateFileSystemEntries( path ) ) { yield return child; }
				yield break;
			}
			try {
				if ( progress ) { yield return Path.Combine( fixture.Root, "s" ); }
				throw new UnauthorizedAccessException( "injected root denial" );
			} finally { disposed = true; }
		}
		TerminalCatalogSource source = fixture.Source;
		TerminalCatalog result = ConventionalTerminalCatalogAdapter.ReadCore( source, new(), default,
			( root, options, token ) => TermInfoDatabaseInspector.InspectDirectoryBoundedCore( root, options, token, Enumerate )
		);
		Assert.True( disposed );
		Assert.Equal( status, result.Status );
		Assert.Equal( progress ? 1 : 0, result.Entries.Count );
		Assert.Equal( TerminalCatalogIssueKind.PermissionFailure, Assert.Single( result.Issues ).Kind );
		Assert.Single( new TerminalCatalogReader( source ).Read().Entries );
	}

	[Fact]
	public void RootDisappearingBeforeObservationIsMissingAndChildAfterDiscoveryIsPartial() {
		using DirectoryCatalogFixture fixture = new();
		string first = fixture.Write( "s", "sample" );
		string second = fixture.Write( "s", "a" );
		TerminalCatalogSource source = fixture.Source;
		IEnumerable<string> Missing( string path ) {
			throw new DirectoryNotFoundException( "injected root removal" );
#pragma warning disable CS0162
			yield break;
#pragma warning restore CS0162
		}
		TerminalCatalog missing = ConventionalTerminalCatalogAdapter.ReadCore( source, new(), default,
			( root, options, token ) => TermInfoDatabaseInspector.InspectDirectoryBoundedCore( root, options, token, Missing )
		);
		Assert.Equal( TerminalCatalogStatus.Missing, missing.Status );
		Assert.Empty( missing.Entries );
		Assert.Equal( TerminalCatalogIssueKind.MissingSource, Assert.Single( missing.Issues ).Kind );
		IEnumerable<string> DeleteSecond( string path ) {
			if ( path == fixture.Root ) { yield return Path.Combine( fixture.Root, "s" ); yield break; }
			yield return first;
			yield return second;
			File.Delete( second );
		}
		TerminalCatalog partial = ConventionalTerminalCatalogAdapter.ReadCore( source, new(), default,
			( root, options, token ) => TermInfoDatabaseInspector.InspectDirectoryBoundedCore( root, options, token, DeleteSecond )
		);
		Assert.Equal( TerminalCatalogStatus.Partial, partial.Status );
		Assert.Single( partial.Entries );
		Assert.Equal( TerminalCatalogIssueKind.IoFailure, Assert.Single( partial.Issues ).Kind );
		Assert.Single( new TerminalCatalogReader( source ).Read().Entries );
	}

	[Fact]
	public void CancellationDuringDirectoryDiscoveryDisposesEnumeratorAndRetryStartsFresh() {
		using DirectoryCatalogFixture fixture = new();
		fixture.Write( "s", "sample" );
		using CancellationTokenSource cancellation = new();
		bool disposed = false;
		IEnumerable<string> Enumerate( string path ) {
			try {
				cancellation.Cancel();
				yield return Path.Combine( path, "s" );
			} finally { disposed = true; }
		}
		TerminalCatalogSource source = fixture.Source;
		Assert.Throws<OperationCanceledException>( () => ConventionalTerminalCatalogAdapter.ReadCore( source, new(), cancellation.Token,
			( root, options, token ) => TermInfoDatabaseInspector.InspectDirectoryBoundedCore( root, options, token, Enumerate )
		)
		);
		Assert.True( disposed );
		Assert.Throws<OperationCanceledException>( () => new TerminalCatalogReader( source ).Read( cancellation.Token ) );
		Assert.Single( new TerminalCatalogReader( source ).Read().Entries );
	}

	[Fact]
	public void ChildEnumerationFailureAfterValidSiblingRetainsSiblingAndIoIssue() {
		using DirectoryCatalogFixture fixture = new();
		string canonical = fixture.Write( "s", "sample" );
		bool disposed = false;
		IEnumerable<string> Enumerate( string path ) {
			if ( path == fixture.Root ) { yield return Path.Combine( fixture.Root, "s" ); yield break; }
			try {
				yield return canonical;
				throw new IOException( "injected child enumeration failure" );
			} finally { disposed = true; }
		}
		TerminalCatalogSource source = fixture.Source;
		TerminalCatalog result = ConventionalTerminalCatalogAdapter.ReadCore( source, new(), default,
			( root, options, token ) => TermInfoDatabaseInspector.InspectDirectoryBoundedCore( root, options, token, Enumerate )
		);
		Assert.True( disposed );
		Assert.Equal( TerminalCatalogStatus.Partial, result.Status );
		Assert.Equal( "sample", Assert.Single( result.Entries ).PublicationName );
		Assert.Equal( TerminalCatalogIssueKind.IoFailure, Assert.Single( result.Issues ).Kind );
	}

	[Fact]
	public void HashedFileReplacementBetweenInspectionAndAcquisitionDoesNotReturnOldRows() {
		using HashedCatalogFixture fixture = new();
		fixture.Write();
		TerminalCatalogSource source = fixture.Source;
		TerminalCatalog missing = HashedTerminalCatalogAdapter.ReadCore( source, new(), default,
			path => { FileAttributes attributes = File.GetAttributes( path ); File.Delete( path ); return attributes; },
			( path, options, limits, token ) => new BerkeleyDbTerminalCatalogReader( path, options ).ReadBounded( limits, token )
		);
		Assert.Equal( TerminalCatalogStatus.Missing, missing.Status );
		Assert.Empty( missing.Entries );
		fixture.Write();
		Assert.Equal( 3, new TerminalCatalogReader( source ).Read().Entries.Count );
	}

	[Fact]
	public void LinkedRootCanBeReadButLinkedChildrenProduceOnlyIssues() {
		using DirectoryCatalogFixture fixture = new();
		string canonical = fixture.Write( "s", "sample" );
		string linkedRoot = fixture.Root + "-uc05-link";
		try {
			try {
				Directory.CreateSymbolicLink( linkedRoot, fixture.Root );
				File.CreateSymbolicLink( Path.Combine( fixture.Root, "s", "a" ), canonical );
				Directory.CreateSymbolicLink( Path.Combine( fixture.Root, "61" ), Path.Combine( fixture.Root, "s" ) );
			} catch ( UnauthorizedAccessException ) {
				output.WriteLine( "SKIP: this host does not grant permission to create symbolic links." );
				return;
			}
			TerminalCatalog result = new TerminalCatalogReader( new( linkedRoot, TerminalCatalogSourceKind.ConventionalDirectory ) ).Read();
			Assert.Equal( TerminalCatalogStatus.Partial, result.Status );
			Assert.Equal( "sample", Assert.Single( result.Entries ).PublicationName );
			Assert.Equal( 2, result.Issues.Count );
			Assert.All( result.Issues, issue => Assert.Equal( TerminalCatalogIssueKind.LinkSkipped, issue.Kind ) );
		} finally { if ( Directory.Exists( linkedRoot ) ) { Directory.Delete( linkedRoot ); } }
	}

	[Fact]
	public void RealPermissionDenialIsAssertedOnlyWhenHostEnforcesIt() {
		if ( OperatingSystem.IsWindows() ) {
			output.WriteLine( "SKIP: Unix permission probe is not applicable on Windows; injected permission classification is covered above." );
			return;
		}
		using DirectoryCatalogFixture fixture = new();
		fixture.Write( "s", "sample" );
		UnixFileMode original = File.GetUnixFileMode( fixture.Root );
		try {
			File.SetUnixFileMode( fixture.Root, UnixFileMode.None );
			try {
				using IEnumerator<string> probe = Directory.EnumerateFileSystemEntries( fixture.Root ).GetEnumerator();
				probe.MoveNext();
				output.WriteLine( "SKIP: this account can enumerate a mode-000 directory (for example, an elevated CI account)." );
				return;
			} catch ( UnauthorizedAccessException ) {
				TerminalCatalog result = new TerminalCatalogReader( fixture.Source ).Read();
				Assert.Equal( TerminalCatalogStatus.Unavailable, result.Status );
				Assert.Equal( TerminalCatalogIssueKind.PermissionFailure, Assert.Single( result.Issues ).Kind );
			}
		} finally { File.SetUnixFileMode( fixture.Root, original ); }
	}
}
