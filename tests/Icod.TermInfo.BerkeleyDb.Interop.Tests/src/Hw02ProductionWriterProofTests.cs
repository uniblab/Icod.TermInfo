/*
	Icod.TermInfo.BerkeleyDb.Interop.Tests
	Proves production HW02 images against the accepted HW00 byte oracle.
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
using Icod.TermInfo.Hw00.ManagedWriterProbe;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Interop.Tests;

public sealed class Hw02ProductionWriterProofTests {
	[Fact]
	public void CompactProductionImageExactlyMatchesHw00Oracle() {
		byte[] compiled = CreateCompiledEntry(
			"hw00-primary-0",
			"HW00 proof 0",
			"hw00-alias-0"
		);
		var entry = new BerkeleyDbTerminalDatabaseEntry(
			"hw00-primary-0",
			[ "hw00-alias-0" ],
			compiled
		);

		Assert.Equal(
			WriteOracle( compiled ),
			WriteProduction( entry )
		);
	}

	[Fact]
	public void TwoBucketProductionImageExactlyMatchesHw00Oracle() {
		byte[][] compiledEntries = Enumerable.Range( 0, 8 )
			.Select(
				index => CreateCompiledEntry(
					$"hw00-bucket-{index:D2}",
					$"HW00 bucket fixture {index:D2}"
				)
			)
			.ToArray()
		;
		BerkeleyDbTerminalDatabaseEntry[] entries = compiledEntries
			.Select(
				( compiled, index ) =>
					new BerkeleyDbTerminalDatabaseEntry(
						$"hw00-bucket-{index:D2}",
						Array.Empty<string>(),
						compiled
					)
			)
			.ToArray()
		;

		byte[] production = WriteProduction( entries );
		ushort firstBucketItems =
			BinaryPrimitives.ReadUInt16LittleEndian(
				production.AsSpan( 4096 + 20, 2 )
			);
		ushort secondBucketItems =
			BinaryPrimitives.ReadUInt16LittleEndian(
				production.AsSpan( ( 2 * 4096 ) + 20, 2 )
			);

		Assert.NotEqual( 0, firstBucketItems );
		Assert.NotEqual( 0, secondBucketItems );
		Assert.Equal(
			WriteOracle( compiledEntries ),
			production
		);
	}

	private static byte[] WriteProduction(
		params BerkeleyDbTerminalDatabaseEntry[] entries
	) {
		var options = new BerkeleyDbTerminalDatabaseWriterOptions();
		BerkeleyDbTerminalDatabaseWriter.PreparedPublication[] prepared =
			BerkeleyDbTerminalDatabaseWriter.PreparePublications(
				entries,
				options,
				CancellationToken.None
			);
		IReadOnlyList<BerkeleyDbHashRecord> records =
			BerkeleyDbNcursesRecordPlanner.CreateRecords(
				prepared,
				CancellationToken.None
			);
		return BerkeleyDbHashV9ImageBuilder.Build(
			records,
			options.MaximumDatabaseSize,
			CancellationToken.None
		);
	}

	private static byte[] WriteOracle( params byte[][] compiledEntries ) {
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
