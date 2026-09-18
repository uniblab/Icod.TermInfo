/*
	Icod.TermInfo.BerkeleyDb.Interop.Tests
	Defines the HW00 managed Hash-v9 writer proof boundary.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

using System.Buffers.Binary;
using System.Text;
using Icod.TermInfo.Hw00.ManagedWriterProbe;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Interop.Tests;

public sealed class Hw00ManagedWriterProofTests {
	[Fact]
	public void WritesCanonicalLittleEndianHashV9Profile() {
		byte[] database = WriteCatalog(
			CreateCompiledEntry(
				"hw00-primary-0",
				"HW00 proof 0",
				"hw00-alias-0"
			)
		);

		Assert.Equal( 0, database.Length % Hw00HashV9Writer.PageSize );
		Assert.Equal(
			0x00061561U,
			BinaryPrimitives.ReadUInt32LittleEndian( database.AsSpan( 12, 4 ) )
		);
		Assert.Equal(
			9U,
			BinaryPrimitives.ReadUInt32LittleEndian( database.AsSpan( 16, 4 ) )
		);
		Assert.Equal(
			4096U,
			BinaryPrimitives.ReadUInt32LittleEndian( database.AsSpan( 20, 4 ) )
		);
		Assert.Equal( (byte)8, database[25] );
		Assert.Equal(
			0x5E688DD1U,
			BinaryPrimitives.ReadUInt32LittleEndian( database.AsSpan( 92, 4 ) )
		);
	}

	[Fact]
	public void IsDeterministicAcrossCompiledEntryOrder() {
		byte[] first = CreateCompiledEntry(
			"hw00-first",
			"HW00 deterministic first"
		);
		byte[] second = CreateCompiledEntry(
			"hw00-second",
			"HW00 deterministic second",
			"hw00-second-alias"
		);

		byte[] forward = WriteCatalog( first, second );
		byte[] reverse = WriteCatalog( second, first );

		Assert.Equal( forward, reverse );
	}

	[Fact]
	public void OrdersExactPrefixesForNativeHashBinarySearch() {
		byte[] database = WriteCatalog(
			CreateCompiledEntry(
				"hw00-primary",
				"HW00 managed writer proof",
				"hw00-alias"
			)
		);
		ReadOnlySpan<byte> page = database.AsSpan(
			2 * Hw00HashV9Writer.PageSize,
			Hw00HashV9Writer.PageSize
		);

		Assert.Equal(
			new[] {
				"hw00-alias-0",
				"hw00-primary-0|hw00-alias-0|HW00 proof 0",
				"hw00-primary-0",
			},
			ReadInlineKeys( page )
		);
	}

	[Fact]
	public void DistributesRecordsAcrossBothBucketsAndPreservesCollisions() {
		byte[][] entries = Enumerable.Range( 0, 8 )
			.Select(
				index => CreateCompiledEntry(
					$"hw00-bucket-{index:D2}",
					$"HW00 bucket fixture {index:D2}"
				)
			)
			.ToArray()
		;

		byte[] database = WriteCatalog( entries );
		ushort firstBucketItems = BinaryPrimitives.ReadUInt16LittleEndian(
			database.AsSpan( Hw00HashV9Writer.PageSize + 20, 2 )
		);
		ushort secondBucketItems = BinaryPrimitives.ReadUInt16LittleEndian(
			database.AsSpan( ( 2 * Hw00HashV9Writer.PageSize ) + 20, 2 )
		);

		Assert.NotEqual( 0, firstBucketItems );
		Assert.NotEqual( 0, secondBucketItems );
		Assert.Equal( 32, firstBucketItems + secondBucketItems );
		Assert.True(
			firstBucketItems >= 4 || secondBucketItems >= 4,
			"At least one canonical bucket must preserve multiple colliding records."
		);
	}

	[Fact]
	public void WritesLargeCompiledValueThroughOverflowEnvelope() {
		byte[] compiled = CreateCompiledEntry(
			"hw00-overflow",
			"HW00 overflow fixture"
		);
		Array.Resize( ref compiled, 3000 );

		byte[] database = WriteCatalog( compiled );
		uint lastPage = BinaryPrimitives.ReadUInt32LittleEndian(
			database.AsSpan( 32, 4 )
		);
		Assert.True( lastPage > 2 );

		bool foundOffPageValue = false;
		for ( int pageNumber = 1; pageNumber <= 2; pageNumber++ ) {
			ReadOnlySpan<byte> page = database.AsSpan(
				pageNumber * Hw00HashV9Writer.PageSize,
				Hw00HashV9Writer.PageSize
			);
			ushort itemCount = BinaryPrimitives.ReadUInt16LittleEndian(
				page[20..22]
			);
			for ( int index = 1; index < itemCount; index += 2 ) {
				ushort itemOffset = BinaryPrimitives.ReadUInt16LittleEndian(
					page.Slice( 26 + ( index * 2 ), 2 )
				);
				if ( page[itemOffset] != 3 ) {
					continue;
				}

				uint overflowPage = BinaryPrimitives.ReadUInt32LittleEndian(
					page.Slice( itemOffset + 4, 4 )
				);
				uint declaredLength = BinaryPrimitives.ReadUInt32LittleEndian(
					page.Slice( itemOffset + 8, 4 )
				);
				Assert.Equal( checked( (uint)( compiled.Length + 1 ) ), declaredLength );
				Assert.Equal(
					(byte)7,
					database[checked( (int)overflowPage * Hw00HashV9Writer.PageSize ) + 25]
				);
				foundOffPageValue = true;
			}
		}

		Assert.True( foundOffPageValue, "Expected one off-page marker-0 record." );
	}

	[Fact]
	public void ExistingManagedReaderResolvesCanonicalAliasAndOverflowRecords() {
		byte[] inline = CreateCompiledEntry(
			"hw00-primary",
			"HW00 reader fixture",
			"hw00-alias"
		);
		byte[] overflow = CreateCompiledEntry(
			"hw00-overflow",
			"HW00 reader overflow fixture"
		);
		Array.Resize( ref overflow, 3000 );
		string path = Path.Combine(
			Path.GetTempPath(),
			$"icod-terminfo-hw00-{Guid.NewGuid():N}.db"
		);

		try {
			File.WriteAllBytes( path, WriteCatalog( inline, overflow ) );
			Assert.True(
				NcursesRecordReader.TryReadCompiledEntry(
					path,
					Encoding.ASCII.GetBytes( "hw00-primary" ),
					out byte[] canonical
				)
			);
			Assert.True(
				NcursesRecordReader.TryReadCompiledEntry(
					path,
					Encoding.ASCII.GetBytes( "hw00-alias" ),
					out byte[] alias
				)
			);
			Assert.True(
				NcursesRecordReader.TryReadCompiledEntry(
					path,
					Encoding.ASCII.GetBytes( "hw00-overflow" ),
					out byte[] actualOverflow
				)
			);
			Assert.Equal( inline, canonical );
			Assert.Equal( inline, alias );
			Assert.Equal( overflow, actualOverflow );
		} finally {
			File.Delete( path );
		}
	}

	private static byte[] WriteCatalog( params byte[][] compiledEntries ) {
		using var destination = new MemoryStream();
		Hw00HashV9Writer.WriteNcursesCatalog(
			destination,
			compiledEntries.Select(
				static entry => (ReadOnlyMemory<byte>)entry
			)
		);
		return destination.ToArray();
	}

	private static string[] ReadInlineKeys( ReadOnlySpan<byte> page ) {
		ushort itemCount = BinaryPrimitives.ReadUInt16LittleEndian(
			page[20..22]
		);
		var keys = new List<string>( itemCount / 2 );
		for ( int index = 0; index < itemCount; index += 2 ) {
			ushort offset = BinaryPrimitives.ReadUInt16LittleEndian(
				page.Slice( 26 + ( index * 2 ), 2 )
			);
			int previousOffset = ( index == 0 )
				? Hw00HashV9Writer.PageSize
				: BinaryPrimitives.ReadUInt16LittleEndian(
					page.Slice( 26 + ( ( index - 1 ) * 2 ), 2 )
				)
			;
			Assert.Equal( (byte)1, page[offset] );
			keys.Add(
				Encoding.Latin1.GetString(
					page.Slice( offset + 1, previousOffset - offset - 1 )
				)
			);
		}
		return keys.ToArray();
	}

	private static byte[] CreateCompiledEntry(
		string canonical,
		string description,
		params string[] aliases
	) {
		string identity = ( aliases.Length == 0 )
			? canonical + "|" + description + "\0"
			: canonical + "|" + string.Join( "|", aliases )
				+ "|" + description + "\0"
		;
		byte[] names = Encoding.Latin1.GetBytes( identity );
		int length = checked( 12 + names.Length );
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
}
