/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates the HDB02 managed Berkeley DB Hash-v9 reader.
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

using System.Buffers.Binary;
using System.Text;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hdb03ProviderTests {
	[Fact]
	public void ConstructorCanonicalizesDatabasePath() {
		string relative = "provider.db";
		BerkeleyDbTerminalDescriptionProvider provider = new( relative );
		Assert.Equal( Path.GetFullPath( relative ), provider.DatabasePath );
		Assert.IsAssignableFrom<ITerminalDescriptionProvider>( provider );
	}

	[Theory]
	[InlineData( null )]
	[InlineData( "" )]
	[InlineData( " " )]
	public void ConstructorRejectsInvalidDatabasePath( string? path ) {
		Assert.ThrowsAny<ArgumentException>(
			() => new BerkeleyDbTerminalDescriptionProvider( path! )
		);
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( -1 )]
	public void OptionsRejectNonpositiveDatabaseLimit( int value ) {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new BerkeleyDbTerminalDescriptionProviderOptions(
				maximumDatabaseSize: value
			)
		);
	}

	[Theory]
	[InlineData( -1 )]
	[InlineData( 1025 )]
	public void OptionsRejectUnsupportedHopLimit( int value ) {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new BerkeleyDbTerminalDescriptionProviderOptions(
				maximumIndexHops: value
			)
		);
	}

	[Fact]
	public void OptionsSnapshotParserLimits() {
		CompiledTermInfoParserOptions parserOptions = new( 1234 );
		BerkeleyDbTerminalDescriptionProviderOptions options =
			new( parserOptions, 4096, 7 );

		Assert.Equal( 1234, options.ParserOptions.MaximumEntrySize );
		Assert.NotSame( parserOptions, options.ParserOptions );
		Assert.Equal( 4096, options.MaximumDatabaseSize );
		Assert.Equal( 7, options.MaximumIndexHops );
	}

	[Fact]
	public void CanonicalAndAliasRequestsParseAndVerifyIdentity() {
		WithDatabase(
			CreateStore( "sample", "sample-alias" ),
			path => {
				BerkeleyDbTerminalDescriptionProvider provider = new( path );
				TerminalDescription canonical = Load( provider, "sample" );
				TerminalDescription alias = Load( provider, "sample-alias" );

				Assert.Equal( "sample", canonical.Name );
				Assert.Contains( "sample-alias", canonical.Aliases );
				Assert.Equal( "sample", alias.Name );
				Assert.Contains( "sample-alias", alias.Aliases );
			}
		);
	}

	[Fact]
	public void ExactNameLookupIsOrdinal() {
		WithDatabase(
			CreateStore( "sample" ),
			path => {
				BerkeleyDbTerminalDescriptionProvider provider = new( path );
				Assert.False( provider.TryLoad( "Sample", out TerminalDescription? terminal ) );
				Assert.Null( terminal );
			}
		);
	}

	[Fact]
	public void CleanMissIsRetryable() {
		WithDatabase(
			CreateStore( "other" ),
			path => {
				BerkeleyDbTerminalDescriptionProvider provider = new( path );
				Assert.False( provider.TryLoad( "sample", out TerminalDescription? missing ) );
				Assert.Null( missing );

				File.WriteAllBytes( path, CreateStore( "sample" ) );
				Assert.Equal( "sample", Load( provider, "sample" ).Name );
			}
		);
	}

	[Fact]
	public void DatabaseFailureIsMappedAndRetryable() {
		byte[] malformed = CreateStore( "sample" );
		malformed[12] = 0;
		WithDatabase(
			malformed,
			path => {
				BerkeleyDbTerminalDescriptionProvider provider = new( path );
				BerkeleyDbDatabaseFormatException error =
					Assert.Throws<BerkeleyDbDatabaseFormatException>(
						() => provider.TryLoad( "sample", out _ )
					);
				Assert.IsType<InvalidDataException>( error.InnerException );

				File.WriteAllBytes( path, CreateStore( "sample" ) );
				Assert.Equal( "sample", Load( provider, "sample" ).Name );
			}
		);
	}

	[Fact]
	public void EnvelopeFailureIsMappedToDatabaseFormat() {
		WithDatabase(
			CreateDatabase(
				( Encoding.UTF8.GetBytes( "sample" ), new byte[] { 7 } )
			),
			path => {
				BerkeleyDbTerminalDescriptionProvider provider = new( path );
				BerkeleyDbDatabaseFormatException error =
					Assert.Throws<BerkeleyDbDatabaseFormatException>(
						() => provider.TryLoad( "sample", out _ )
					);
				Assert.IsType<InvalidDataException>( error.InnerException );
			}
		);
	}

	[Fact]
	public void CompiledFailureRemainsCompiledFormatAndIsRetryable() {
		WithDatabase(
			CreateDatabase(
				( Encoding.UTF8.GetBytes( "sample" ), new byte[] { 0, 1, 2 } )
			),
			path => {
				BerkeleyDbTerminalDescriptionProvider provider = new( path );
				Assert.Throws<CompiledTermInfoFormatException>(
					() => provider.TryLoad( "sample", out _ )
				);

				File.WriteAllBytes( path, CreateStore( "sample" ) );
				Assert.Equal( "sample", Load( provider, "sample" ).Name );
			}
		);
	}

	[Fact]
	public void ParsedIdentityMustMatchRequestedName() {
		WithDatabase(
			CreateDatabase(
				(
					Encoding.UTF8.GetBytes( "requested" ),
					PrependMarker( CreateCompiledEntry( "other" ) )
				)
			),
			path => {
				BerkeleyDbTerminalDescriptionProvider provider = new( path );
				Assert.Throws<InvalidDataException>(
					() => provider.TryLoad( "requested", out _ )
				);
			}
		);
	}

	[Fact]
	public void ParserMaximumEntrySizeRemainsAuthoritative() {
		byte[] entry = CreateCompiledEntry( "sample" );
		BerkeleyDbTerminalDescriptionProviderOptions options =
			new( new CompiledTermInfoParserOptions( entry.Length - 1 ) );

		WithDatabase(
			CreateDatabase(
				( Encoding.UTF8.GetBytes( "sample" ), PrependMarker( entry ) )
			),
			path => {
				BerkeleyDbTerminalDescriptionProvider provider = new( path, options );
				Assert.Throws<CompiledTermInfoFormatException>(
					() => provider.TryLoad( "sample", out _ )
				);
			}
		);
	}

	[Fact]
	public void SuccessfulDescriptionIsCachedPerExactName() {
		WithDatabase(
			CreateStore( "sample" ),
			path => {
				BerkeleyDbTerminalDescriptionProvider provider = new( path );
				TerminalDescription first = Load( provider, "sample" );
				File.WriteAllBytes( path, new byte[1024] );
				TerminalDescription second = Load( provider, "sample" );
				Assert.Same( first, second );
			}
		);
	}

	[Fact]
	public void SeparateProviderRefreshesAChangedDatabase() {
		WithDatabase(
			CreateStore( "sample" ),
			path => {
				TerminalDescription first =
					Load( new BerkeleyDbTerminalDescriptionProvider( path ), "sample" );
				File.WriteAllBytes( path, CreateStore( "sample" ) );
				TerminalDescription second =
					Load( new BerkeleyDbTerminalDescriptionProvider( path ), "sample" );
				Assert.NotSame( first, second );
			}
		);
	}

	[Fact]
	public void ConcurrentLoadsPublishOneDescription() {
		WithDatabase(
			CreateStore( "sample" ),
			path => {
				BerkeleyDbTerminalDescriptionProvider provider = new( path );
				Task<TerminalDescription>[] tasks = Enumerable
					.Range( 0, 16 )
					.Select( _ => Task.Run( () => Load( provider, "sample" ) ) )
					.ToArray();
				TerminalDescription[] terminals = Task.WhenAll( tasks ).GetAwaiter().GetResult();
				Assert.All( terminals, terminal => Assert.Same( terminals[0], terminal ) );
			}
		);
	}

	[Fact]
	public void MissingDatabaseIsAnIoFailure() {
		string path = Path.Combine( Path.GetTempPath(), Guid.NewGuid().ToString( "N" ) + ".db" );
		BerkeleyDbTerminalDescriptionProvider provider = new( path );
		Assert.Throws<FileNotFoundException>(
			() => provider.TryLoad( "sample", out _ )
		);
	}

	[Theory]
	[InlineData( "" )]
	[InlineData( " " )]
	[InlineData( "." )]
	[InlineData( ".." )]
	[InlineData( "../sample" )]
	[InlineData( "..\\sample" )]
	[InlineData( "bad/name" )]
	[InlineData( "bad\\name" )]
	[InlineData( "bad\0name" )]
	[InlineData( "bad\nname" )]
	public void InvalidTerminalNamesFailBeforeOpening( string name ) {
		string path = Path.Combine( Path.GetTempPath(), Guid.NewGuid().ToString( "N" ) + ".db" );
		BerkeleyDbTerminalDescriptionProvider provider = new( path );
		Assert.Throws<ArgumentException>(
			() => provider.TryLoad( name, out _ )
		);
	}

	[Fact]
	public void NullTerminalNameFailsBeforeOpening() {
		string path = Path.Combine( Path.GetTempPath(), Guid.NewGuid().ToString( "N" ) + ".db" );
		BerkeleyDbTerminalDescriptionProvider provider = new( path );
		Assert.Throws<ArgumentNullException>(
			() => provider.TryLoad( null!, out _ )
		);
	}

	[Fact]
	public void VeryLongAbsentNameIsACleanMiss() {
		WithDatabase(
			CreateStore( "sample" ),
			path => {
				BerkeleyDbTerminalDescriptionProvider provider = new( path );
				Assert.False( provider.TryLoad( new string( 'x', 65_536 ), out _ ) );
			}
		);
	}

	[Fact]
	public void DatabaseLimitFailureIsMapped() {
		BerkeleyDbTerminalDescriptionProviderOptions options =
			new( maximumDatabaseSize: 1023 );
		WithDatabase(
			CreateStore( "sample" ),
			path => Assert.Throws<BerkeleyDbDatabaseFormatException>(
				() => new BerkeleyDbTerminalDescriptionProvider( path, options )
					.TryLoad( "sample", out _ )
			)
		);
	}

	[Fact]
	public void HopLimitFailureIsMapped() {
		BerkeleyDbTerminalDescriptionProviderOptions options =
			new( maximumIndexHops: 0 );
		WithDatabase(
			CreateStore( "sample", "sample-alias" ),
			path => Assert.Throws<BerkeleyDbDatabaseFormatException>(
				() => new BerkeleyDbTerminalDescriptionProvider( path, options )
					.TryLoad( "sample-alias", out _ )
			)
		);
	}

	private static TerminalDescription Load(
		BerkeleyDbTerminalDescriptionProvider provider,
		string name
	) {
		Assert.True( provider.TryLoad( name, out TerminalDescription? terminal ) );
		return Assert.IsType<TerminalDescription>( terminal );
	}

	private static byte[] CreateStore( string canonical, string? alias = null ) {
		byte[] entry = CreateCompiledEntry( canonical, alias );
		if ( alias is null ) {
			return CreateDatabase(
				( Encoding.UTF8.GetBytes( canonical ), PrependMarker( entry ) )
			);
		}

		byte[] target = Encoding.UTF8.GetBytes( canonical + "|" + alias );
		return CreateDatabase(
			( Encoding.UTF8.GetBytes( canonical ), PrependMarker( target, 2 ) ),
			( Encoding.UTF8.GetBytes( alias ), PrependMarker( target, 2 ) ),
			( target, PrependMarker( entry ) )
		);
	}

	private static byte[] CreateCompiledEntry( string canonical, string? alias = null ) {
		string identity = ( alias is null )
			? canonical + "|test terminal\0"
			: canonical + "|" + alias + "|test terminal\0"
		;
		byte[] names = Encoding.Latin1.GetBytes( identity );
		int length = 12 + names.Length;
		if ( ( length & 1 ) != 0 ) {
			length++;
		}
		byte[] entry = new byte[length];
		BinaryPrimitives.WriteUInt16LittleEndian( entry.AsSpan( 0, 2 ), 0x011A );
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan( 2, 2 ),
			checked( (ushort)names.Length )
		);
		names.CopyTo( entry.AsSpan( 12 ) );
		return entry;
	}

	private static byte[] PrependMarker( byte[] bytes, byte marker = 0 ) {
		byte[] value = new byte[bytes.Length + 1];
		value[0] = marker;
		bytes.CopyTo( value.AsSpan( 1 ) );
		return value;
	}

	private static byte[] CreateDatabase(
		params ( byte[] Key, byte[] Value )[] records
	) {
		byte[] database = new byte[512 * ( records.Length + 1 )];
		BinaryPrimitives.WriteUInt32LittleEndian( database.AsSpan( 12, 4 ), 0x00061561 );
		BinaryPrimitives.WriteUInt32LittleEndian( database.AsSpan( 16, 4 ), 9 );
		BinaryPrimitives.WriteUInt32LittleEndian( database.AsSpan( 20, 4 ), 512 );
		database[25] = 8;
		BinaryPrimitives.WriteUInt32LittleEndian( database.AsSpan( 32, 4 ), (uint)records.Length );

		for ( int index = 0; index < records.Length; index++ ) {
			( byte[] key, byte[] value ) = records[index];
			Span<byte> page = database.AsSpan( 512 * ( index + 1 ), 512 );
			BinaryPrimitives.WriteUInt32LittleEndian( page.Slice( 8, 4 ), (uint)( index + 1 ) );
			page[25] = 13;
			ushort keyOffset = checked( (ushort)( 511 - key.Length ) );
			ushort valueOffset = checked( (ushort)( keyOffset - value.Length - 1 ) );
			BinaryPrimitives.WriteUInt16LittleEndian( page.Slice( 20, 2 ), 2 );
			BinaryPrimitives.WriteUInt16LittleEndian( page.Slice( 22, 2 ), valueOffset );
			BinaryPrimitives.WriteUInt16LittleEndian( page.Slice( 26, 2 ), keyOffset );
			BinaryPrimitives.WriteUInt16LittleEndian( page.Slice( 28, 2 ), valueOffset );
			page[keyOffset] = 1;
			key.CopyTo( page[( keyOffset + 1 )..] );
			page[valueOffset] = 1;
			value.CopyTo( page[( valueOffset + 1 )..] );
		}
		return database;
	}

	private static void WithDatabase( byte[] database, Action<string> assertion ) {
		string path = Path.GetTempFileName();
		try {
			File.WriteAllBytes( path, database );
			assertion( path );
		} finally {
			File.Delete( path );
		}
	}
}
