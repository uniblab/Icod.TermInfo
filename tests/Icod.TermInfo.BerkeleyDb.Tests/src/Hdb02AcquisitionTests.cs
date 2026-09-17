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

using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hdb02AcquisitionTests {
	[Theory]
	[InlineData( 1 )]
	[InlineData( 7 )]
	[InlineData( 511 )]
	[InlineData( 512 )]
	public void ReadDatabaseCombinesPartialReadsAndLeavesBorrowedStreamOpen( int chunkSize ) {
		byte[] expected = Enumerable.Range( 0, 512 ).Select( index => (byte)index ).ToArray();
		using AcquisitionStream stream = new AcquisitionStream( expected, 512, chunkSize );

		byte[] actual = BerkeleyDbHashReader.ReadDatabase( stream, 512 );

		Assert.Equal( expected, actual );
		Assert.True( stream.CanRead );
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 1 )]
	[InlineData( 511 )]
	public void ReadDatabasePropagatesTruncation( int availableBytes ) {
		using AcquisitionStream stream = new AcquisitionStream( new byte[availableBytes], 512, 7 );

		Assert.Throws<EndOfStreamException>(
			() => BerkeleyDbHashReader.ReadDatabase( stream, 512 )
		);
		Assert.True( stream.CanRead );
	}

	[Theory]
	[InlineData( 513 )]
	[InlineData( 1024 )]
	public void ReadDatabaseRejectsGrowthBeyondObservedLength( int availableBytes ) {
		using AcquisitionStream stream = new AcquisitionStream( new byte[availableBytes], 512, 7 );

		Assert.Throws<IOException>(
			() => BerkeleyDbHashReader.ReadDatabase( stream, 512 )
		);
		Assert.True( stream.CanRead );
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 127 )]
	[InlineData( 512 )]
	public void ReadDatabasePropagatesOriginalReadFailure( int failureOffset ) {
		IOException expected = new IOException( "Injected acquisition failure." );
		using AcquisitionStream stream = new AcquisitionStream(
			new byte[512], 512, 7, failureOffset, expected
		);

		IOException actual = Assert.Throws<IOException>(
			() => BerkeleyDbHashReader.ReadDatabase( stream, 512 )
		);

		Assert.Same( expected, actual );
		Assert.True( stream.CanRead );
	}

	[Theory]
	[InlineData( 0, 512 )]
	[InlineData( 511, 512 )]
	[InlineData( 512, 511 )]
	[InlineData( 2147483647, 2147483647 )]
	public void ReadDatabaseRejectsInvalidLengthBeforeReading( long reportedLength, int limit ) {
		using AcquisitionStream stream = new AcquisitionStream(
			[], reportedLength, 7, 0, new IOException( "Payload must not be read." )
		);

		Assert.Throws<InvalidDataException>(
			() => BerkeleyDbHashReader.ReadDatabase( stream, limit )
		);
		Assert.True( stream.CanRead );
	}

	[Fact]
	public void ReadDatabaseRejectsNullStream() {
		Assert.Throws<ArgumentNullException>(
			() => BerkeleyDbHashReader.ReadDatabase( (Stream)null!, 512 )
		);
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( -1 )]
	public void ReadDatabaseRejectsNonpositiveLimit( int limit ) {
		using AcquisitionStream stream = new AcquisitionStream( new byte[512], 512, 7 );

		Assert.Throws<ArgumentOutOfRangeException>(
			() => BerkeleyDbHashReader.ReadDatabase( stream, limit )
		);
		Assert.True( stream.CanRead );
	}

	// Models observed length and deterministic read outcomes without filesystem timing.
	// Both synchronous read paths and the trailing-byte probe use the same behavior.
	private sealed class AcquisitionStream : Stream {
		private readonly MemoryStream source;
		private readonly long reportedLength;
		private readonly int chunkSize;
		private readonly int failureOffset;
		private readonly IOException? failure;

		internal AcquisitionStream(
			byte[] bytes,
			long reportedLength,
			int chunkSize,
			int failureOffset = int.MaxValue,
			IOException? failure = null
		) {
			source = new MemoryStream( bytes, writable: false );
			this.reportedLength = reportedLength;
			this.chunkSize = chunkSize;
			this.failureOffset = failureOffset;
			this.failure = failure;
		}

		public override bool CanRead => source.CanRead;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => reportedLength;
		public override long Position {
			get => source.Position;
			set => throw new NotSupportedException();
		}

		public override int Read( Span<byte> buffer ) {
			if ( Position >= failureOffset && failure is not null ) {
				throw failure;
			}

			int count = Math.Min( buffer.Length, chunkSize );
			count = Math.Min( count, (int)Math.Max( 0, failureOffset - Position ) );
			return source.Read( buffer[..count] );
		}

		public override int Read( byte[] buffer, int offset, int count ) {
			return Read( buffer.AsSpan( offset, count ) );
		}

		public override void Flush() {
			throw new NotSupportedException();
		}

		public override long Seek( long offset, SeekOrigin origin ) {
			throw new NotSupportedException();
		}

		public override void SetLength( long value ) {
			throw new NotSupportedException();
		}

		public override void Write( byte[] buffer, int offset, int count ) {
			throw new NotSupportedException();
		}

		protected override void Dispose( bool disposing ) {
			if ( disposing ) {
				source.Dispose();
			}
			base.Dispose( disposing );
		}

		public override int ReadByte() {
			Span<byte> buffer = stackalloc byte[1];
			if ( Read( buffer ) == 0 ) {
				return -1;
			}
			return buffer[0];
		}
	}
}
