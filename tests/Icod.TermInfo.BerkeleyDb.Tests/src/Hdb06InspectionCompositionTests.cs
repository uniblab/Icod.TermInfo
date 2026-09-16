/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates HDB06 provider-neutral Inspection composition.
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
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hdb06InspectionCompositionTests {
	[Theory]
	[InlineData( "hdb06-main" )]
	[InlineData( "hdb06-alias" )]
	public void InspectionAcquiresCanonicalAndAliasThroughProvider(
		string requestedName
	) {
		WithDatabase(
			CreateCatalogStore(
				"hdb06-main",
				"HDB06 terminal",
				"hdb06-alias"
			),
			path => {
				var provider =
					new BerkeleyDbTerminalDescriptionProvider( path );
				var target =
					new TermInfoInspectionTarget(
						provider,
						requestedName,
						provider.DatabasePath
					);
				TermInfoInspectionResult result =
					TermInfoInspectionEngine.Inspect( target );

				Assert.Equal( "hdb06-main", result.Terminal.Name );
				Assert.Contains(
					"hdb06-alias",
					result.Terminal.Aliases
				);
				Assert.Equal(
					TerminalDescriptionSourceRenderer.Render(
						result.Terminal
					),
					TermInfoInspectionEngine.Render( result )
				);
			}
		);
	}

	[Fact]
	public void InspectionPreservesCleanMiss() {
		WithDatabase(
			CreateCatalogStore(
				"hdb06-main",
				"HDB06 terminal",
				"hdb06-alias"
			),
			path => {
				var provider =
					new BerkeleyDbTerminalDescriptionProvider( path );
				var target =
					new TermInfoInspectionTarget(
						provider,
						"hdb06-missing",
						provider.DatabasePath
					);

				Assert.False(
					TermInfoInspectionEngine.TryInspect(
						target,
						out TermInfoInspectionResult? result
					)
				);
				Assert.Null( result );
			}
		);
	}

	[Fact]
	public void InspectionPreservesDatabaseFormatFailure() {
		WithDatabase(
			new byte[] { 1, 2, 3, 4 },
			path => {
				var provider =
					new BerkeleyDbTerminalDescriptionProvider( path );
				var target =
					new TermInfoInspectionTarget(
						provider,
						"hdb06-main",
						provider.DatabasePath
					);

				Assert.Throws<BerkeleyDbDatabaseFormatException>(
					() => TermInfoInspectionEngine.Inspect( target )
				);
			}
		);
	}

	private static byte[] CreateCatalogStore(
		string canonical,
		string description,
		params string[] aliases
	) {
		var names = new List<string> { canonical };
		names.AddRange( aliases );
		byte[] storageKey =
			Encoding.UTF8.GetBytes(
				string.Join( "|", names )
					+ "|"
					+ description
			);
		var records =
			new List<( byte[] Key, byte[] Value )>();

		foreach ( string name in names ) {
			records.Add(
				(
					Encoding.UTF8.GetBytes( name ),
					PrependMarker( storageKey, 2 )
				)
			);
		}
		records.Add(
			(
				storageKey,
				PrependMarker(
					CreateCompiledEntry(
						canonical,
						aliases,
						description
					)
				)
			)
		);
		return CreateDatabase( records.ToArray() );
	}

	private static byte[] CreateCompiledEntry(
		string canonical,
		string[] aliases,
		string description
	) {
		string identity = ( aliases.Length == 0 )
			? canonical + "|" + description + "\0"
			: canonical
				+ "|"
				+ string.Join( "|", aliases )
				+ "|"
				+ description
				+ "\0"
		;
		byte[] names = Encoding.Latin1.GetBytes( identity );
		int length = 12 + names.Length;
		if ( ( length & 1 ) != 0 ) {
			length++;
		}

		byte[] entry = new byte[length];
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan( 0, 2 ),
			0x011A
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan( 2, 2 ),
			checked( (ushort)names.Length )
		);
		names.CopyTo( entry.AsSpan( 12 ) );
		return entry;
	}

	private static byte[] PrependMarker(
		byte[] bytes,
		byte marker = 0
	) {
		byte[] value = new byte[bytes.Length + 1];
		value[0] = marker;
		bytes.CopyTo( value.AsSpan( 1 ) );
		return value;
	}

	private static byte[] CreateDatabase(
		params ( byte[] Key, byte[] Value )[] records
	) {
		byte[] database = new byte[512 * ( records.Length + 1 )];
		BinaryPrimitives.WriteUInt32LittleEndian(
			database.AsSpan( 12, 4 ),
			0x00061561
		);
		BinaryPrimitives.WriteUInt32LittleEndian(
			database.AsSpan( 16, 4 ),
			9
		);
		BinaryPrimitives.WriteUInt32LittleEndian(
			database.AsSpan( 20, 4 ),
			512
		);
		database[25] = 8;
		BinaryPrimitives.WriteUInt32LittleEndian(
			database.AsSpan( 32, 4 ),
			checked( (uint)records.Length )
		);

		for ( int index = 0; index < records.Length; index++ ) {
			( byte[] key, byte[] value ) = records[index];
			Span<byte> page =
				database.AsSpan( 512 * ( index + 1 ), 512 );
			BinaryPrimitives.WriteUInt32LittleEndian(
				page.Slice( 8, 4 ),
				checked( (uint)( index + 1 ) )
			);
			page[25] = 13;
			ushort keyOffset =
				checked( (ushort)( 511 - key.Length ) );
			ushort valueOffset =
				checked( (ushort)( keyOffset - value.Length - 1 ) );
			BinaryPrimitives.WriteUInt16LittleEndian(
				page.Slice( 20, 2 ),
				2
			);
			BinaryPrimitives.WriteUInt16LittleEndian(
				page.Slice( 22, 2 ),
				valueOffset
			);
			BinaryPrimitives.WriteUInt16LittleEndian(
				page.Slice( 26, 2 ),
				keyOffset
			);
			BinaryPrimitives.WriteUInt16LittleEndian(
				page.Slice( 28, 2 ),
				valueOffset
			);
			page[keyOffset] = 1;
			key.CopyTo( page[( keyOffset + 1 )..] );
			page[valueOffset] = 1;
			value.CopyTo( page[( valueOffset + 1 )..] );
		}
		return database;
	}

	private static void WithDatabase(
		byte[] database,
		Action<string> assertion
	) {
		string path = Path.GetTempFileName();
		try {
			File.WriteAllBytes( path, database );
			assertion( path );
		} finally {
			File.Delete( path );
		}
	}
}
