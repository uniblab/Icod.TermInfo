/*
	Icod.TermInfo.Tests.Shared
	Creates minimal Berkeley DB Hash-v9 terminfo fixtures.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

using System.Buffers.Binary;
using System.Text;

namespace Icod.TermInfo.Tests.Shared;

internal static class BerkeleyDbHashV9TestStore {
	internal static byte[] CreateCatalogStore(
		string canonical,
		string description,
		params string[] aliases
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( canonical );
		ArgumentException.ThrowIfNullOrWhiteSpace( description );
		ArgumentNullException.ThrowIfNull( aliases );

		var names = new List<string> { canonical };
		names.AddRange( aliases );
		byte[] storageKey = Encoding.UTF8.GetBytes(
			string.Join( "|", names ) + "|" + description
		);
		var records = new List<( byte[] Key, byte[] Value )>();
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
					CreateCompiledEntry( canonical, description, aliases )
				)
			)
		);
		return CreateDatabase( records.ToArray() );
	}

	internal static byte[] CreateMalformedStore() =>
		[ 0x01, 0x02, 0x03, 0x04 ];

	private static byte[] CreateCompiledEntry(
		string canonical,
		string description,
		string[] aliases
	) {
		string identity = ( aliases.Length == 0 )
			? canonical + "|" + description + "\0"
			: canonical + "|" + string.Join( "|", aliases )
				+ "|" + description + "\0";
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
			Span<byte> page = database.AsSpan( 512 * ( index + 1 ), 512 );
			BinaryPrimitives.WriteUInt32LittleEndian(
				page.Slice( 8, 4 ),
				checked( (uint)( index + 1 ) )
			);
			page[25] = 13;
			ushort keyOffset = checked( (ushort)( 511 - key.Length ) );
			ushort valueOffset = checked(
				(ushort)( keyOffset - value.Length - 1 )
			);
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
}
