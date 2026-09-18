/*
	Icod.TermInfo.BerkeleyDb.Interop.Tests
	Defines the HW00 managed Hash-v9 writer proof boundary.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

using System.Buffers.Binary;
using System.Text;
using Icod.TermInfo.Hw00.ManagedWriterProbe;

namespace Icod.TermInfo.BerkeleyDb.Interop.Tests;

public sealed class Hw00ManagedWriterProofTests {
	[Fact]
	public void WritesCanonicalLittleEndianHashV9Profile() {
		byte[] database = WriteCatalog(
			CreateCompiledEntry(
				"hw00-primary",
				"HW00 managed writer proof",
				"hw00-alias"
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
