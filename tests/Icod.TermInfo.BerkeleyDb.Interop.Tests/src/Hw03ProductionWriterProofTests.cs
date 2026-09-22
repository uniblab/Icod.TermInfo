/*
	Icod.TermInfo.BerkeleyDb.Interop.Tests
	Proves production HW03 overflow images against the accepted HW00 byte oracle.
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

public sealed class Hw03ProductionWriterProofTests {
	[Fact]
	public void TwoBucketOverflowProductionImageExactlyMatchesHw00Oracle() {
		const string canonical = "hw03-overflow";
		const string description = "HW03 overflow byte proof";
		byte[] compact = CreateCompiledEntry(
			canonical,
			description
		);
		int descriptionPaddingLength = checked( 3000 - compact.Length );
		Assert.True( descriptionPaddingLength > 0 );
		Assert.Equal( 0, descriptionPaddingLength & 1 );
		byte[] compiled = CreateCompiledEntry(
			canonical,
			description + new string( 'x', descriptionPaddingLength )
		);
		Assert.Equal( 3000, compiled.Length );

		Assert.Equal(
			WriteOracle( compiled ),
			WriteProduction(
				new BerkeleyDbTerminalDatabaseEntry(
					canonical,
					Array.Empty<string>(),
					compiled
				)
			)
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
