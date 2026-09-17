/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates the HDB07C stable Berkeley DB acquisition contract.
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

public sealed class Hdb07cAcquisitionStabilityTests {
	[Theory]
	[InlineData( 1 )]
	[InlineData( 7 )]
	[InlineData( 512 )]
	public void StableAcquisitionAcceptsTwoIdenticalPartialReadObservations(
		int chunkSize
	) {
		byte[] expected = CreateDatabaseImage();
		using ObservationStream stream = new ObservationStream(
			expected,
			expected.ToArray(),
			expected.Length,
			expected.Length,
			chunkSize
		);

		byte[] actual = BerkeleyDbHashReader.ReadStableDatabase( stream, 512 );

		Assert.Equal( expected, actual );
		Assert.Equal( 2, stream.ObservationCount );
		Assert.True( stream.CanRead );
	}

	[Fact]
	public void StableAcquisitionRejectsSameLengthContentChangeWithoutRetry() {
		byte[] first = CreateDatabaseImage();
		byte[] second = first.ToArray();
		second[257] ^= 0xff;
		using ObservationStream stream = new ObservationStream(
			first,
			second,
			first.Length,
			second.Length,
			7
		);

		IOException error = Assert.Throws<IOException>(
			() => BerkeleyDbHashReader.ReadStableDatabase( stream, 512 )
		);

		Assert.Equal(
			"The Berkeley DB file changed while it was being read.",
			error.Message
		);
		Assert.Equal( 2, stream.ObservationCount );
		Assert.True( stream.CanRead );
	}

	[Theory]
	[InlineData( 511 )]
	[InlineData( 513 )]
	public void StableAcquisitionRejectsSecondObservationLengthChange(
		int secondLength
	) {
		byte[] first = CreateDatabaseImage();
		using ObservationStream stream = new ObservationStream(
			first,
			new byte[secondLength],
			first.Length,
			secondLength,
			7
		);

		IOException error = Assert.Throws<IOException>(
			() => BerkeleyDbHashReader.ReadStableDatabase( stream, 512 )
		);

		Assert.Equal(
			"The Berkeley DB file changed while it was being read.",
			error.Message
		);
		Assert.Equal( 2, stream.ObservationCount );
		Assert.True( stream.CanRead );
	}

	[Fact]
	public void StableAcquisitionPropagatesSecondObservationIoFailure() {
		byte[] expectedBytes = CreateDatabaseImage();
		IOException expected = new IOException( "Injected second-observation failure." );
		using ObservationStream stream = new ObservationStream(
			expectedBytes,
			expectedBytes.ToArray(),
			expectedBytes.Length,
			expectedBytes.Length,
			7,
			127,
			expected
		);

		IOException actual = Assert.Throws<IOException>(
			() => BerkeleyDbHashReader.ReadStableDatabase( stream, 512 )
		);

		Assert.Same( expected, actual );
		Assert.Equal( 2, stream.ObservationCount );
		Assert.True( stream.CanRead );
	}

	[Fact]
	public void BorrowedSinglePassAcquisitionStillAcceptsNonSeekableStream() {
		byte[] expected = CreateDatabaseImage();
		using ObservationStream stream = new ObservationStream(
			expected,
			expected.ToArray(),
			expected.Length,
			expected.Length,
			7,
			canSeek: false
		);

		byte[] actual = BerkeleyDbHashReader.ReadDatabase( stream, 512 );

		Assert.Equal( expected, actual );
		Assert.Equal( 1, stream.ObservationCount );
		Assert.True( stream.CanRead );
	}

	private static byte[] CreateDatabaseImage() {
		return Enumerable.Range( 0, 512 ).Select( index => (byte)index ).ToArray();
	}

	private sealed class ObservationStream : Stream {
		private readonly MemoryStream[] observations;
		private readonly long[] reportedLengths;
		private readonly int chunkSize;
		private readonly int secondFailureOffset;
		private readonly IOException? secondFailure;
		private readonly bool canSeek;
		private int activeObservation;
		private bool disposed;

		internal ObservationStream(
			byte[] first,
			byte[] second,
			long firstReportedLength,
			long secondReportedLength,
			int chunkSize,
			int secondFailureOffset = int.MaxValue,
			IOException? secondFailure = null,
			bool canSeek = true
		) {
			observations = [
				new MemoryStream( first, writable: false ),
				new MemoryStream( second, writable: false )
			];
			reportedLengths = [ firstReportedLength, secondReportedLength ];
			this.chunkSize = chunkSize;
			this.secondFailureOffset = secondFailureOffset;
			this.secondFailure = secondFailure;
			this.canSeek = canSeek;
			ObservationCount = 1;
		}

		internal int ObservationCount { get; private set; }

		public override bool CanRead => !disposed;
		public override bool CanSeek => !disposed && canSeek;
		public override bool CanWrite => false;
		public override long Length {
			get {
				ObjectDisposedException.ThrowIf( disposed, this );
				return reportedLengths[activeObservation];
			}
		}
		public override long Position {
			get {
				ObjectDisposedException.ThrowIf( disposed, this );
				return observations[activeObservation].Position;
			}
			set {
				ObjectDisposedException.ThrowIf( disposed, this );
				if ( !canSeek ) {
					throw new NotSupportedException();
				}
				if (
					value != 0
					|| activeObservation != 0
					|| observations[0].Position < observations[0].Length
				) {
					throw new InvalidOperationException(
						"Only one rewind after a complete first observation is supported."
					);
				}

				activeObservation = 1;
				observations[1].Position = 0;
				ObservationCount++;
			}
		}

		public override int Read( Span<byte> buffer ) {
			ObjectDisposedException.ThrowIf( disposed, this );
			if (
				activeObservation == 1
				&& Position >= secondFailureOffset
				&& secondFailure is not null
			) {
				throw secondFailure;
			}

			int count = Math.Min( buffer.Length, chunkSize );
			if ( activeObservation == 1 ) {
				count = Math.Min(
					count,
					(int)Math.Max( 0, secondFailureOffset - Position )
				);
			}
			return observations[activeObservation].Read( buffer[..count] );
		}

		public override int Read( byte[] buffer, int offset, int count ) {
			return Read( buffer.AsSpan( offset, count ) );
		}

		public override int ReadByte() {
			Span<byte> buffer = stackalloc byte[1];
			if ( Read( buffer ) == 0 ) {
				return -1;
			}
			return buffer[0];
		}

		public override void Flush() {
			throw new NotSupportedException();
		}

		public override long Seek( long offset, SeekOrigin origin ) {
			if ( origin == SeekOrigin.Begin && offset == 0 ) {
				Position = 0;
				return Position;
			}
			throw new NotSupportedException();
		}

		public override void SetLength( long value ) {
			throw new NotSupportedException();
		}

		public override void Write( byte[] buffer, int offset, int count ) {
			throw new NotSupportedException();
		}

		protected override void Dispose( bool disposing ) {
			if ( disposing && !disposed ) {
				foreach ( MemoryStream observation in observations ) {
					observation.Dispose();
				}
				disposed = true;
			}
			base.Dispose( disposing );
		}
	}
}
