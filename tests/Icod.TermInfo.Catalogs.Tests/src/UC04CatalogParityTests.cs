using Icod.TermInfo.BerkeleyDb;
using Icod.TermInfo.Compiler;
using System.Globalization;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

[Collection( "UC01 process state" )]
public sealed class UC04CatalogParityTests {
	private static TerminalDescription Terminal() => new TerminalDescriptionBuilder( "sample" )
		.SetDescription( "paired fixture" ).AddAlias( "a" ).AddAlias( "b" )
		.SetBoolean( BooleanCapability.AutoRightMargin )
		.SetNumber( NumericCapability.Columns, 93 )
		.SetString( StringCapability.ClearScreen, "\u001b[2J" ).Build();

	[Fact]
	public void UnpublishedDeclarationsAreNotPublicationsInEitherSource() {
		using DirectoryCatalogFixture directory = new();
		using HashedCatalogFixture hashed = new();
		byte[] bytes = CompiledTermInfoWriter.Write( Terminal() );
		directory.Write( "s", "sample", bytes );
		hashed.WritePayload( bytes, aliases: false );
		TerminalCatalog first = new TerminalCatalogReader( directory.Source ).Read();
		TerminalCatalog second = new TerminalCatalogReader( hashed.Source ).Read();
		Assert.Equal( TerminalCatalogStatus.Complete, first.Status );
		Assert.Equal( TerminalCatalogStatus.Complete, second.Status );
		Assert.Empty( first.Issues ); Assert.Empty( second.Issues );
		Assert.Equal( "sample", Assert.Single( first.Entries ).PublicationName );
		Assert.Equal( "sample", Assert.Single( second.Entries ).PublicationName );
		Assert.Equal( new[] { "a", "b" }, first.Entries[0].Terminal.Aliases );
		Assert.Equal( first.Entries[0].Terminal.Name, second.Entries[0].Terminal.Name );
		Assert.Equal( first.Entries[0].Terminal.Description, second.Entries[0].Terminal.Description );
		Assert.Equal( first.Entries[0].Terminal.Aliases, second.Entries[0].Terminal.Aliases );
		Assert.True( second.Entries[0].Terminal.GetBoolean( BooleanCapability.AutoRightMargin ) );
		Assert.Equal( 93, second.Entries[0].Terminal.GetNumber( NumericCapability.Columns ) );
		Assert.Equal( "\u001b[2J", second.Entries[0].Terminal.GetString( StringCapability.ClearScreen ) );
		Assert.Equal( first.Entries[0].Terminal.GetString( StringCapability.ClearScreen ), second.Entries[0].Terminal.GetString( StringCapability.ClearScreen ) );
		Assert.Equal( directory.Root, first.Entries[0].SourcePath );
		Assert.NotNull( first.Entries[0].EntryPath );
		Assert.Equal( hashed.PathName, second.Entries[0].SourcePath );
		Assert.Null( second.Entries[0].EntryPath );
	}

	[Fact]
	public void PublishedAliasesMatchAcrossBothSourcesWithDistinctProvenance() {
		using DirectoryCatalogFixture directory = new();
		using HashedCatalogFixture hashed = new();
		byte[] bytes = CompiledTermInfoWriter.Write( Terminal() );
		string canonical = directory.Write( "s", "sample", bytes );
		string aliasA = directory.Write( "61", "a", bytes );
		string aliasB = directory.Write( "b", "b", bytes );
		BerkeleyDbTerminalDatabaseWriter.Write( hashed.PathName, [new( "sample", [ "a", "b" ], bytes )] );
		TerminalCatalog first = new TerminalCatalogReader( directory.Source ).Read();
		TerminalCatalog second = new TerminalCatalogReader( hashed.Source ).Read();
		Assert.Equal( TerminalCatalogStatus.Complete, first.Status );
		Assert.Equal( TerminalCatalogStatus.Complete, second.Status );
		Assert.Equal( new[] { "a", "b", "sample" }, first.Entries.Select( item => item.PublicationName ) );
		Assert.Equal( first.Entries.Select( item => item.PublicationName ), second.Entries.Select( item => item.PublicationName ) );
		Assert.Equal( new[] { TerminalCatalogEntryKind.Alias, TerminalCatalogEntryKind.Alias, TerminalCatalogEntryKind.Canonical }, first.Entries.Select( item => item.Kind ) );
		Assert.Equal( first.Entries.Select( item => item.Kind ), second.Entries.Select( item => item.Kind ) );
		Assert.Equal( new[] { aliasA, aliasB, canonical }, first.Entries.Select( item => item.EntryPath ) );
		Assert.All( second.Entries, item => {
			Assert.Equal( hashed.PathName, item.SourcePath );
			Assert.Null( item.EntryPath );
			Assert.Equal( "sample", item.Terminal.Name );
			Assert.Equal( 93, item.Terminal.GetNumber( NumericCapability.Columns ) );
		}
		);
		Assert.Empty( first.Issues ); Assert.Empty( second.Issues );
		Assert.Empty( first.DuplicatePublicationNames ); Assert.Empty( second.DuplicatePublicationNames );
	}

	[Fact]
	public void DirectoryDuplicateAndMalformedSiblingDifferFromHashedFailures() {
		using DirectoryCatalogFixture directory = new();
		using HashedCatalogFixture hashed = new();
		byte[] bytes = CompiledTermInfoWriter.Write( Terminal() );
		directory.Write( "s", "sample", bytes );
		directory.Write( "73", "sample", bytes );
		directory.Write( "z", "sample", bytes );
		directory.Write( "s", "bad", [ 1, 2, 3 ] );
		BerkeleyDbTerminalDatabaseWriter.Write( hashed.PathName, [new( "sample", [ "a", "b" ], bytes )] );
		TerminalCatalog partial = new TerminalCatalogReader( directory.Source ).Read();
		TerminalCatalog complete = new TerminalCatalogReader( hashed.Source ).Read();
		Assert.Equal( TerminalCatalogStatus.Partial, partial.Status );
		Assert.Equal( new[] { "sample", "sample" }, partial.Entries.Select( item => item.PublicationName ) );
		Assert.Equal( new[] { "sample" }, partial.DuplicatePublicationNames );
		Assert.Contains( partial.Issues, item => item.Kind == TerminalCatalogIssueKind.InvalidPlacement );
		Assert.Contains( partial.Issues, item => item.Kind == TerminalCatalogIssueKind.MalformedEntry );
		Assert.Equal( 1, partial.Issues.Count( item => item.Kind == TerminalCatalogIssueKind.DuplicatePublication ) );
		Assert.Equal( TerminalCatalogStatus.Complete, complete.Status );
		Assert.Equal( 3, complete.Entries.Count );
		Assert.Empty( complete.DuplicatePublicationNames );
		using HashedCatalogFixture corrupt = new(); corrupt.Write( fault: "orphan" );
		TerminalCatalog invalid = new TerminalCatalogReader( corrupt.Source ).Read();
		Assert.Equal( TerminalCatalogStatus.InvalidStore, invalid.Status );
		Assert.Empty( invalid.Entries );
		Assert.Equal( TerminalCatalogIssueKind.InvalidHashedStore, Assert.Single( invalid.Issues ).Kind );
	}

	[Fact]
	public void WriterAndDirectoryInputOrderDoNotChangePublicOrdering() {
		using DirectoryCatalogFixture directory = new();
		using HashedCatalogFixture hashed = new();
		byte[] first = CompiledTermInfoWriter.Write( Terminal() );
		byte[] last = CompiledTermInfoWriter.Write( new TerminalDescriptionBuilder( "z" ).SetDescription( "other fixture" ).Build() );
		directory.Write( "z", "z", last );
		directory.Write( "s", "sample", first );
		BerkeleyDbTerminalDatabaseWriter.Write( hashed.PathName, [
			new( "z", [], last ), new( "sample", [ "a", "b" ], first ),
		]
		);
		Assert.Equal( new[] { "sample", "z" }, new TerminalCatalogReader( directory.Source ).Read().Entries.Select( item => item.PublicationName ) );
		Assert.Equal( new[] { "a", "b", "sample", "z" }, new TerminalCatalogReader( hashed.Source ).Read().Entries.Select( item => item.PublicationName ) );
	}

	[Fact]
	public void LinkedDirectoryChildProducesOnlyDirectoryDiagnostic() {
		using DirectoryCatalogFixture directory = new();
		using HashedCatalogFixture hashed = new();
		byte[] bytes = CompiledTermInfoWriter.Write( Terminal() );
		directory.Write( "s", "sample", bytes );
		BerkeleyDbTerminalDatabaseWriter.Write( hashed.PathName, [new( "sample", [ "a", "b" ], bytes )] );
		try {
			File.CreateSymbolicLink( Path.Combine( directory.Root, "s", "a" ), Path.Combine( directory.Root, "s", "sample" ) );
		} catch ( Exception ex ) when ( ex is UnauthorizedAccessException or PlatformNotSupportedException or IOException ) {
			return;
		}
		TerminalCatalog partial = new TerminalCatalogReader( directory.Source ).Read();
		Assert.Equal( TerminalCatalogStatus.Partial, partial.Status );
		Assert.Single( partial.Entries );
		Assert.Equal( TerminalCatalogIssueKind.LinkSkipped, Assert.Single( partial.Issues ).Kind );
		Assert.Equal( TerminalCatalogStatus.Complete, new TerminalCatalogReader( hashed.Source ).Read().Status );
	}

	[Theory]
	[InlineData( "en-US" )]
	[InlineData( "tr-TR" )]
	public void PublicReadsRetainOrdinalOrderingAcrossCultures( string culture ) {
		using DirectoryCatalogFixture directory = new();
		using HashedCatalogFixture hashed = new();
		byte[] bytes = CompiledTermInfoWriter.Write( new TerminalDescriptionBuilder( "I" ).AddAlias( "j" ).SetDescription( "culture fixture" ).Build() );
		directory.Write( "I", "I", bytes );
		directory.Write( "j", "j", bytes );
		BerkeleyDbTerminalDatabaseWriter.Write( hashed.PathName, [new( "I", [ "j" ], bytes )] );
		CultureInfo old = CultureInfo.CurrentCulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( culture );
			foreach ( var reader in new[] { new TerminalCatalogReader( directory.Source ), new TerminalCatalogReader( hashed.Source ) } ) {
				Assert.Equal( new[] { "I", "j" }, reader.Read().Entries.Select( item => item.PublicationName ) );
				Assert.Equal( new[] { "I", "j" }, reader.Read().Entries.Select( item => item.PublicationName ) );
			}
		} finally { CultureInfo.CurrentCulture = old; }
	}
}
