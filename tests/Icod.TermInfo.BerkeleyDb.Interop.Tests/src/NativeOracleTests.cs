/*
	Icod.TermInfo.BerkeleyDb.Interop.Tests
	Compares the production reader with native Berkeley DB dump records.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Text;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Interop.Tests;

public sealed class NativeOracleTests {
	private const string Latin1Canonical = "hdb07c-caf\u00E9";
	private const string Latin1Alias = "hdb07c-ali\u00E9";

	[Theory]
	[InlineData( "hashed-db", 3 )]
	[InlineData( "big-endian-hashed-db", 3 )]
	[InlineData( "latin1-hashed-db", 3 )]
	[InlineData( "overflow-hashed-db", 2 )]
	[InlineData( "multi-hashed-db", 192 )]
	[InlineData( "hw00-managed-writer", 5 )]
	[InlineData( "hw03-overflow", 1 )]
	[InlineData( "hw03-growth", 4 )]
	[InlineData( "hw03-collision", 2 )]
	public void ProductionReaderMatchesEveryNativeRecord(
		string fixtureName,
		int expectedRecordCount
	) {
		List<( byte[] Key, byte[] Value )> records = ReadNativeRecords( fixtureName );
		Assert.Equal( expectedRecordCount, records.Count );
		string databasePath = FixturePath( fixtureName + ".db" );

		foreach ( ( byte[] key, byte[] expected ) in records ) {
			Assert.True(
				BerkeleyDbHashReader.TryReadValue( databasePath, key, out byte[] actual ),
				$"Native key {Convert.ToHexString( key )} was not found."
			);
			Assert.Equal( expected, actual );
		}
	}

	[Fact]
	public void NativeLatin1DumpContainsExactPublicationKeyBytes() {
		List<( byte[] Key, byte[] Value )> records =
			ReadNativeRecords( "latin1-hashed-db" );

		Assert.Contains(
			records,
			record => record.Key.AsSpan().SequenceEqual(
				Convert.FromHexString( "6864623037632D636166E9" )
			)
		);
		Assert.Contains(
			records,
			record => record.Key.AsSpan().SequenceEqual(
				Convert.FromHexString( "6864623037632D616C69E9" )
			)
		);
	}

	[Theory]
	[InlineData( "hashed-db" )]
	[InlineData( "big-endian-hashed-db" )]
	[InlineData( "overflow-hashed-db" )]
	[InlineData( "hw00-managed-writer" )]
	[InlineData( "hw03-overflow" )]
	[InlineData( "hw03-growth" )]
	[InlineData( "hw03-collision" )]
	public void ProductionReaderReturnsCleanMissForAbsentNativeKey( string fixtureName ) {
		byte[] key = Encoding.UTF8.GetBytes( "hdb00-missing" );
		List<( byte[] Key, byte[] Value )> records = ReadNativeRecords( fixtureName );
		Assert.NotEmpty( records );
		Assert.DoesNotContain(
			records,
			record => record.Key.AsSpan().SequenceEqual( key )
		);

		Assert.False(
			BerkeleyDbHashReader.TryReadValue(
				FixturePath( fixtureName + ".db" ),
				key,
				out byte[] actual
			)
		);
		Assert.Empty( actual );
	}

	[Theory]
	[InlineData( "hw03-overflow", "hw03-overflow.key", "hw03-overflow.value" )]
	[InlineData(
		"hw03-collision",
		"hw03-collision-first.key",
		"hw03-collision-first.value"
	)]
	[InlineData(
		"hw03-collision",
		"hw03-collision-second.key",
		"hw03-collision-second.value"
	)]
	public void ProductionReaderRecoversExactHw03RawValue(
		string database,
		string keyFile,
		string valueFile
	) {
		byte[] key = File.ReadAllBytes( FixturePath( keyFile ) );
		byte[] expected = File.ReadAllBytes( FixturePath( valueFile ) );
		Assert.True(
			BerkeleyDbHashReader.TryReadValue(
				FixturePath( database + ".db" ),
				key,
				out byte[] actual
			)
		);
		Assert.Equal( expected, actual );
	}

	[Theory]
	[InlineData( "not-hash.db" )]
	[InlineData( "random.db" )]
	public void ProductionReaderRejectsUnsupportedNativeInput( string fixtureName ) {
		Assert.Throws<InvalidDataException>(
			() => BerkeleyDbHashReader.TryReadValue(
				FixturePath( fixtureName ),
				new byte[] { 0x6B, 0x65, 0x79 },
				out _
			)
		);
	}

	[Theory]
	[InlineData( "hashed-db", "hdb00-primary", "hdb00-primary.bin" )]
	[InlineData( "hashed-db", "hdb00-alias", "hdb00-primary.bin" )]
	[InlineData( "big-endian-hashed-db", "hdb00-primary", "hdb00-primary.bin" )]
	[InlineData( "big-endian-hashed-db", "hdb00-alias", "hdb00-primary.bin" )]
	[InlineData( "overflow-hashed-db", "hdb00-overflow", "hdb00-overflow.bin" )]
	[InlineData( "hw00-managed-writer", "hdb00-primary", "hdb00-primary.bin" )]
	[InlineData( "hw00-managed-writer", "hdb00-alias", "hdb00-primary.bin" )]
	[InlineData( "hw00-managed-writer", "hdb00-overflow", "hdb00-overflow.bin" )]
	public void ProductionRecordResolverMatchesNativeCompiledEntry(
		string fixtureName,
		string terminalName,
		string expectedFile
	) {
		byte[] expected = File.ReadAllBytes( FixturePath( expectedFile ) );
		Assert.NotEmpty( expected );

		Assert.True( NcursesRecordReader.TryReadCompiledEntry(
			FixturePath( fixtureName + ".db" ),
			Encoding.UTF8.GetBytes( terminalName ),
			out byte[] actual
		)
		);

		Assert.Equal( expected, actual );
	}

	[Theory]
	[InlineData( "hashed-db", "hdb00-primary", "hdb00-primary", "hdb00-alias" )]
	[InlineData( "hashed-db", "hdb00-alias", "hdb00-primary", "hdb00-alias" )]
	[InlineData( "big-endian-hashed-db", "hdb00-primary", "hdb00-primary", "hdb00-alias" )]
	[InlineData( "big-endian-hashed-db", "hdb00-alias", "hdb00-primary", "hdb00-alias" )]
	[InlineData( "latin1-hashed-db", Latin1Canonical, Latin1Canonical, Latin1Alias )]
	[InlineData( "latin1-hashed-db", Latin1Alias, Latin1Canonical, Latin1Alias )]
	[InlineData( "overflow-hashed-db", "hdb00-overflow", "hdb00-overflow", null )]
	[InlineData( "hw00-managed-writer", "hdb00-primary", "hdb00-primary", "hdb00-alias" )]
	[InlineData( "hw00-managed-writer", "hdb00-alias", "hdb00-primary", "hdb00-alias" )]
	[InlineData( "hw00-managed-writer", "hdb00-overflow", "hdb00-overflow", null )]
	public void PublicProviderParsesNativeStore(
		string fixtureName,
		string requestedName,
		string expectedCanonicalName,
		string? expectedAlias
	) {
		BerkeleyDbTerminalDescriptionProvider provider =
			new( FixturePath( fixtureName + ".db" ) );

		Assert.True(
			provider.TryLoad(
				requestedName,
				out TerminalDescription? terminal
			)
		);
		Assert.Equal( expectedCanonicalName, terminal.Name );

		if ( expectedAlias is null ) {
			Assert.Empty( terminal.Aliases );
		} else {
			Assert.Contains( expectedAlias, terminal.Aliases );
		}
	}

	[Fact]
	public void PublicProviderReturnsCleanMissForAbsentNativeKey() {
		BerkeleyDbTerminalDescriptionProvider provider =
			new( FixturePath( "hashed-db.db" ) );

		Assert.False(
			provider.TryLoad(
				"hdb00-missing",
				out TerminalDescription? terminal
			)
		);
		Assert.Null( terminal );
	}

	[Theory]
	[InlineData( "not-hash.db" )]
	[InlineData( "random.db" )]
	public void PublicProviderMapsUnsupportedNativeInput( string fixtureName ) {
		BerkeleyDbTerminalDescriptionProvider provider =
			new( FixturePath( fixtureName ) );

		BerkeleyDbDatabaseFormatException exception =
			Assert.Throws<BerkeleyDbDatabaseFormatException>(
				() => provider.TryLoad( "key", out _ )
			);
		Assert.IsType<InvalidDataException>( exception.InnerException );
	}

	[Theory]
	[InlineData( "hashed-db", "hdb00-primary", "hdb00-primary" )]
	[InlineData( "hashed-db", "hdb00-alias", "hdb00-primary" )]
	[InlineData( "big-endian-hashed-db", "hdb00-primary", "hdb00-primary" )]
	[InlineData( "big-endian-hashed-db", "hdb00-alias", "hdb00-primary" )]
	[InlineData( "latin1-hashed-db", Latin1Canonical, Latin1Canonical )]
	[InlineData( "latin1-hashed-db", Latin1Alias, Latin1Canonical )]
	[InlineData( "overflow-hashed-db", "hdb00-overflow", "hdb00-overflow" )]
	public void PublicSystemProviderParsesNativeStore(
		string fixtureName,
		string requestedName,
		string expectedCanonicalName
	) {
		string? previousTermInfo =
			Environment.GetEnvironmentVariable( "TERMINFO" );
		string? previousTermInfoDirs =
			Environment.GetEnvironmentVariable( "TERMINFO_DIRS" );

		try {
			Environment.SetEnvironmentVariable(
				"TERMINFO",
				FixturePath(
					fixtureName + ".db"
				)
			);
			Environment.SetEnvironmentVariable(
				"TERMINFO_DIRS",
				null
			);
			BerkeleyDbSystemTerminalDescriptionProvider provider =
				new(
					new BerkeleyDbSystemTerminalDescriptionProviderOptions(
						useEnvironment: true,
						useUserDatabase: false,
						useSystemDatabases: false
					)
				);

			Assert.True(
				provider.TryLoad(
					requestedName,
					out TerminalDescription? terminal
				)
			);
			Assert.Equal(
				expectedCanonicalName,
				terminal.Name
			);
		} finally {
			Environment.SetEnvironmentVariable(
				"TERMINFO",
				previousTermInfo
			);
			Environment.SetEnvironmentVariable(
				"TERMINFO_DIRS",
				previousTermInfoDirs
			);
		}
	}


	[Theory]
	[InlineData( "hashed-db" )]
	[InlineData( "big-endian-hashed-db" )]
	public void PublicCatalogEnumeratesNativeCanonicalAndAliasPublications(
		string fixtureName
	) {
		BerkeleyDbTerminalCatalogReader reader =
			new( FixturePath( fixtureName + ".db" ) );

		IReadOnlyList<BerkeleyDbTerminalCatalogEntry> entries =
			reader.Read();

		Assert.Collection(
			entries,
			entry => {
				Assert.Equal( "hdb00-alias", entry.Name );
				Assert.Equal(
					BerkeleyDbTerminalCatalogEntryKind.Alias,
					entry.Kind
				);
			},
			entry => {
				Assert.Equal( "hdb00-primary", entry.Name );
				Assert.Equal(
					BerkeleyDbTerminalCatalogEntryKind.Canonical,
					entry.Kind
				);
			}
		);
		Assert.Same( entries[0].Terminal, entries[1].Terminal );
		Assert.Equal( "hdb00-primary", entries[0].Terminal.Name );
		Assert.Contains(
			"hdb00-alias",
			entries[0].Terminal.Aliases
		);
	}

	[Fact]
	public void PublicCatalogEnumeratesNativeLatin1Publications() {
		BerkeleyDbTerminalCatalogReader reader =
			new( FixturePath( "latin1-hashed-db.db" ) );

		IReadOnlyList<BerkeleyDbTerminalCatalogEntry> entries =
			reader.Read();

		Assert.Collection(
			entries,
			entry => {
				Assert.Equal( Latin1Alias, entry.Name );
				Assert.Equal(
					BerkeleyDbTerminalCatalogEntryKind.Alias,
					entry.Kind
				);
			},
			entry => {
				Assert.Equal( Latin1Canonical, entry.Name );
				Assert.Equal(
					BerkeleyDbTerminalCatalogEntryKind.Canonical,
					entry.Kind
				);
			}
		);
		Assert.Same( entries[0].Terminal, entries[1].Terminal );
		Assert.Equal( Latin1Canonical, entries[0].Terminal.Name );
		Assert.Contains( Latin1Alias, entries[0].Terminal.Aliases );
	}

	[Fact]
	public void PublicCatalogParsesNativeOverflowPublication() {
		BerkeleyDbTerminalCatalogReader reader =
			new( FixturePath( "overflow-hashed-db.db" ) );

		BerkeleyDbTerminalCatalogEntry entry =
			Assert.Single( reader.Read() );

		Assert.Equal( "hdb00-overflow", entry.Name );
		Assert.Equal(
			BerkeleyDbTerminalCatalogEntryKind.Canonical,
			entry.Kind
		);
		Assert.Equal( "hdb00-overflow", entry.Terminal.Name );
	}

	[Theory]
	[InlineData( "hdb07-multi-000", "hdb07-multi-000" )]
	[InlineData( "hdb07-multi-000-alias", "hdb07-multi-000" )]
	[InlineData( "hdb07-multi-032", "hdb07-multi-032" )]
	[InlineData( "hdb07-multi-032-alias", "hdb07-multi-032" )]
	[InlineData( "hdb07-multi-063", "hdb07-multi-063" )]
	[InlineData( "hdb07-multi-063-alias", "hdb07-multi-063" )]
	public void MultiRecordNativeStoreLoadsThroughDirectAndExplicitRouter(
		string requestedName,
		string expectedCanonicalName
	) {
		BerkeleyDbTerminalDescriptionProvider provider =
			new( FixturePath( "multi-hashed-db.db" ) );
		TerminalDatabase router = new( [ provider ] );

		Assert.True( provider.TryLoad( requestedName, out TerminalDescription? direct ) );
		Assert.Equal( expectedCanonicalName, direct.Name );
		Assert.Contains( expectedCanonicalName + "-alias", direct.Aliases );

		Assert.True( router.TryLoad( requestedName, out TerminalDescription? routed ) );
		Assert.Equal( expectedCanonicalName, routed.Name );
		Assert.Contains( expectedCanonicalName + "-alias", routed.Aliases );
	}

	[Theory]
	[InlineData( "hdb07-multi-000", "hdb07-multi-000" )]
	[InlineData( "hdb07-multi-000-alias", "hdb07-multi-000" )]
	[InlineData( "hdb07-multi-032", "hdb07-multi-032" )]
	[InlineData( "hdb07-multi-032-alias", "hdb07-multi-032" )]
	[InlineData( "hdb07-multi-063", "hdb07-multi-063" )]
	[InlineData( "hdb07-multi-063-alias", "hdb07-multi-063" )]
	public void MultiRecordNativeStoreLoadsThroughSystemRouter(
		string requestedName,
		string expectedCanonicalName
	) {
		string? previousTermInfo =
			Environment.GetEnvironmentVariable( "TERMINFO" );
		string? previousTermInfoDirs =
			Environment.GetEnvironmentVariable( "TERMINFO_DIRS" );

		try {
			Environment.SetEnvironmentVariable(
				"TERMINFO",
				FixturePath( "multi-hashed-db.db" )
			);
			Environment.SetEnvironmentVariable( "TERMINFO_DIRS", null );
			BerkeleyDbSystemTerminalDescriptionProvider provider =
				new(
					new BerkeleyDbSystemTerminalDescriptionProviderOptions(
						useEnvironment: true,
						useUserDatabase: false,
						useSystemDatabases: false
					)
				);

			Assert.True(
				provider.TryLoad(
					requestedName,
					out TerminalDescription? terminal
				)
			);
			Assert.Equal( expectedCanonicalName, terminal.Name );
		} finally {
			Environment.SetEnvironmentVariable( "TERMINFO", previousTermInfo );
			Environment.SetEnvironmentVariable(
				"TERMINFO_DIRS",
				previousTermInfoDirs
			);
		}
	}

	[Fact]
	public void MultiRecordNativeCatalogHasExactPublicationsAndEdgeEntries() {
		BerkeleyDbTerminalCatalogReader reader =
			new( FixturePath( "multi-hashed-db.db" ) );

		IReadOnlyList<BerkeleyDbTerminalCatalogEntry> entries = reader.Read();

		Assert.Equal( 128, entries.Count );
		foreach ( int index in new[] { 0, 32, 63 } ) {
			string canonical = $"hdb07-multi-{index:D3}";
			BerkeleyDbTerminalCatalogEntry primary = Assert.Single(
				entries,
				entry => entry.Name == canonical
			);
			BerkeleyDbTerminalCatalogEntry alias = Assert.Single(
				entries,
				entry => entry.Name == canonical + "-alias"
			);

			Assert.Equal(
				BerkeleyDbTerminalCatalogEntryKind.Canonical,
				primary.Kind
			);
			Assert.Equal(
				BerkeleyDbTerminalCatalogEntryKind.Alias,
				alias.Kind
			);
			Assert.Same( primary.Terminal, alias.Terminal );
			Assert.Equal( canonical, primary.Terminal.Name );
		}
	}

	private static List<( byte[] Key, byte[] Value )> ReadNativeRecords( string fixtureName ) {
		string[] lines = File.ReadAllLines( FixturePath( fixtureName + ".dump" ) );
		int headerEnd = Array.IndexOf( lines, "HEADER=END" );
		Assert.True( headerEnd >= 3, "The native dump header is missing." );
		string[] header = lines[..headerEnd];
		Assert.Contains( "VERSION=3", header );
		Assert.Contains( "format=bytevalue", header );
		Assert.Contains( "type=hash", header );
		Assert.Equal( "DATA=END", lines[^1] );
		int dataLineCount = lines.Length - headerEnd - 2;
		Assert.True( dataLineCount > 0 && ( dataLineCount % 2 ) == 0 );

		List<( byte[] Key, byte[] Value )> records = [];
		for ( int index = headerEnd + 1; index < lines.Length - 1; index += 2 ) {
			Assert.StartsWith( " ", lines[index] );
			Assert.StartsWith( " ", lines[index + 1] );
			records.Add(
				(
					Convert.FromHexString( lines[index][1..] ),
					Convert.FromHexString( lines[index + 1][1..] )
				)
			);
		}
		return records;
	}

	private static string FixturePath( string name ) {
		string? root = Environment.GetEnvironmentVariable( "ICOD_HDB02_FIXTURE_ROOT" );
		if ( string.IsNullOrWhiteSpace( root ) ) {
			throw new InvalidOperationException(
				"ICOD_HDB02_FIXTURE_ROOT must point to native-generated HDB00 fixtures."
			);
		}
		return Path.Combine( root, name );
	}
}
