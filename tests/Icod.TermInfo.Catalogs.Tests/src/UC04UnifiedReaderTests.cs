using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

public sealed class UC04UnifiedReaderTests {
	[Fact]
	public void ReaderRejectsNullSourceAndPreservesTheExplicitSource() {
		Assert.Equal( "source", Assert.Throws<ArgumentNullException>( () => new TerminalCatalogReader( null! ) ).ParamName );
		using DirectoryCatalogFixture fixture = new();
		TerminalCatalogSource source = fixture.Source;
		var reader = new TerminalCatalogReader( source, new TerminalCatalogReadOptions() );
		Assert.Same( source, reader.Source );
		Assert.Same( source, reader.Read().Source );
	}

	[Fact]
	public void ExplicitSourcesDispatchToTheSelectedStore() {
		using DirectoryCatalogFixture directory = new();
		using HashedCatalogFixture hashed = new();
		File.WriteAllBytes( hashed.PathName, Hdb07HashV9FixtureBuilder.CreateDatabase( Hdb07ByteOrder.LittleEndian, 512 ) );
		var directoryReader = new TerminalCatalogReader( directory.Source );
		var hashedReader = new TerminalCatalogReader( hashed.Source );
		Assert.Same( directoryReader.Source, directoryReader.Read().Source );
		Assert.Equal( TerminalCatalogStatus.Complete, directoryReader.Read().Status );
		Assert.Equal( TerminalCatalogStatus.Complete, hashedReader.Read( default ).Status );
		Assert.Empty( hashedReader.Read().Entries );
	}

	[Fact]
	public void SourceKindMismatchAndMissingSourcesKeepDistinctOutcomes() {
		using DirectoryCatalogFixture directory = new();
		using HashedCatalogFixture hashed = new();
		string file = directory.Write( "s", "sample" );
		var directoryAsHash = new TerminalCatalogReader( new( directory.Root, TerminalCatalogSourceKind.BerkeleyDbHash ) ).Read();
		var fileAsDirectory = new TerminalCatalogReader( new( file, TerminalCatalogSourceKind.ConventionalDirectory ) ).Read();
		Assert.Equal( TerminalCatalogStatus.UnsupportedSource, directoryAsHash.Status );
		Assert.Equal( TerminalCatalogStatus.UnsupportedSource, fileAsDirectory.Status );
		Assert.Equal( TerminalCatalogIssueKind.UnsupportedSource, Assert.Single( directoryAsHash.Issues ).Kind );
		Assert.Equal( TerminalCatalogIssueKind.UnsupportedSource, Assert.Single( fileAsDirectory.Issues ).Kind );
		Assert.Empty( directoryAsHash.Entries );
		Assert.Empty( fileAsDirectory.Entries );
		var missingHash = new TerminalCatalogReader( hashed.Source ).Read();
		Assert.Equal( TerminalCatalogStatus.Missing, missingHash.Status );
		Assert.Equal( TerminalCatalogIssueKind.MissingSource, Assert.Single( missingHash.Issues ).Kind );
	}

	[Fact]
	public void MalformedSourcesUseTheirStorageSpecificFailureSemantics() {
		using DirectoryCatalogFixture directory = new();
		using HashedCatalogFixture hashed = new();
		directory.Write( "s", "sample" );
		directory.Write( "s", "bad", [ 1, 2, 3 ] );
		hashed.Write( fault: "orphan" );
		TerminalCatalog partial = new TerminalCatalogReader( directory.Source ).Read();
		TerminalCatalog invalid = new TerminalCatalogReader( hashed.Source ).Read();
		Assert.Equal( TerminalCatalogStatus.Partial, partial.Status );
		Assert.Equal( "sample", Assert.Single( partial.Entries ).PublicationName );
		Assert.Equal( TerminalCatalogIssueKind.MalformedEntry, Assert.Single( partial.Issues ).Kind );
		Assert.Equal( TerminalCatalogStatus.InvalidStore, invalid.Status );
		Assert.Empty( invalid.Entries );
		Assert.Equal( TerminalCatalogIssueKind.InvalidHashedStore, Assert.Single( invalid.Issues ).Kind );
	}

	[Fact]
	public void PreCancellationAndLimitsThrowWithoutReturningAResult() {
		using DirectoryCatalogFixture directory = new();
		using HashedCatalogFixture hashed = new(); hashed.Write();
		var reader = new TerminalCatalogReader( directory.Source );
		using CancellationTokenSource canceled = new(); canceled.Cancel();
		Assert.ThrowsAny<OperationCanceledException>( () => reader.Read( canceled.Token ) );
		var hashedReader = new TerminalCatalogReader( hashed.Source, new( maximumEntryCount: 2 ) );
		TerminalCatalogLimitException exception = Assert.Throws<TerminalCatalogLimitException>( () => hashedReader.Read() );
		Assert.Equal( nameof( TerminalCatalogReadOptions.MaximumEntryCount ), exception.LimitName );
		Assert.Equal( 2, exception.Limit );
		Assert.Same( hashedReader.Source, exception.Source );
	}

	[Fact]
	public void RepeatedReadsAcquireFreshObservationsAndIndependentBudgets() {
		using DirectoryCatalogFixture directory = new();
		var reader = new TerminalCatalogReader( directory.Source, new( maximumEntryCount: 1 ) );
		Assert.Empty( reader.Read().Entries );
		directory.Write( "s", "sample" );
		Assert.Single( reader.Read().Entries );
		Assert.Single( reader.Read().Entries );
		directory.Write( "61", "a" );
		var exception = Assert.Throws<TerminalCatalogLimitException>( () => reader.Read() );
		Assert.Equal( "MaximumEntryCount", exception.LimitName );
	}
}
