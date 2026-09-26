/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates HW05 safe Hash-v9 filesystem publication.
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

using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hw05PublicationVerificationTests {
	internal static (byte[] Image, IReadOnlyList<BerkeleyDbHashRecord> Records, BerkeleyDbTerminalDatabaseWriter.PreparedPublication[] Publications) Prepare() {
		byte[] compiled = Hdb07HashV9FixtureBuilder.CreateCompiledEntry( "hw05-primary", "HW05 verification", "hw05-alias" );
		var publications = BerkeleyDbTerminalDatabaseWriter.PreparePublications(
			[ new BerkeleyDbTerminalDatabaseEntry( "hw05-primary", [ "hw05-alias" ], compiled ) ],
			new(), CancellationToken.None
		);
		var records = BerkeleyDbNcursesRecordPlanner.CreateRecords( publications, CancellationToken.None );
		return ( BerkeleyDbHashV9ImageBuilder.Build( records, 1_048_576, CancellationToken.None ), records, publications );
	}

	[Fact]
	public void ExactImageAndCatalogVerify() {
		var data = Prepare();
		BerkeleyDbDatabasePublicationVerifier.Verify( data.Image.ToArray(), data.Image, data.Records, data.Publications, new(), default );
	}

	[Fact]
	public void ChangedImageIsRejected() {
		var data = Prepare();
		byte[] changed = data.Image.ToArray();
		changed[^1] ^= 1;
		Assert.Throws<InvalidDataException>( () => BerkeleyDbDatabasePublicationVerifier.Verify(
			changed, data.Image, data.Records, data.Publications, new(), default
		) );
	}

	[Fact]
	public void MissingPhysicalRecordIsRejected() {
		var data = Prepare();
		Assert.Throws<InvalidDataException>( () => BerkeleyDbDatabasePublicationVerifier.Verify(
			data.Image, data.Image, data.Records.Skip( 1 ).ToArray(), data.Publications, new(), default
		) );
	}

	[Fact]
	public void WrongPreparedAliasIsRejected() {
		var data = Prepare();
		var changed = data.Publications[0] with {
			Aliases = [ new( "wrong-alias", [ 0x78 ] ) ]
		};
		Assert.Throws<InvalidDataException>( () => BerkeleyDbDatabasePublicationVerifier.Verify(
			data.Image, data.Image, data.Records, [ changed ], new(), default
		) );
	}

	[Theory]
	[InlineData( 1 )]
	[InlineData( 2 )]
	public void CancellationInterruptsBothStableReadPasses( int pass ) {
		var data = Prepare();
		using var cancellation = new CancellationTokenSource();
		using var source = new CancellingStream( data.Image, cancellation, pass );
		using var adapter = new BerkeleyDbCancellationReadStream( source, cancellation.Token );
		Assert.Throws<OperationCanceledException>( () => BerkeleyDbHashReader.ReadStableDatabase( adapter, 1_048_576 ) );
		Assert.True( source.CanRead );
	}

	private sealed class CancellingStream( byte[] image, CancellationTokenSource cancellation, int cancelPass ) : MemoryStream( image ) {
		private int _pass = 1;
		public override long Position {
			get => base.Position;
			set {
				if ( value == 0 && base.Position != 0 ) {
					_pass++;
				}
				base.Position = value;
			}
		}
		public override int Read( byte[] buffer, int offset, int count ) {
			if ( _pass == cancelPass ) {
				cancellation.Cancel();
			}
			return base.Read( buffer, offset, count );
		}
		public override int Read( Span<byte> buffer ) {
			if ( _pass == cancelPass ) {
				cancellation.Cancel();
			}
			return base.Read( buffer );
		}
	}
}
