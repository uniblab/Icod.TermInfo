/*
	Icod.TermInfo.BerkeleyDb
	Managed read and write support for ncurses-compatible Berkeley DB Hash-v9 terminfo stores.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This library is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public License
	along with this library.  If not, see <https://www.gnu.org/licenses/>.
*/


namespace Icod.TermInfo.BerkeleyDb;

internal sealed class BerkeleyDbCancellationReadStream( Stream inner, CancellationToken token ) : Stream {
	public override bool CanRead => inner.CanRead;
	public override bool CanSeek => inner.CanSeek;
	public override bool CanWrite => false;
	public override long Length => inner.Length;
	public override long Position { get => inner.Position; set => inner.Position = value; }
	public override int Read( byte[] buffer, int offset, int count ) {
		token.ThrowIfCancellationRequested();
		int read = inner.Read( buffer, offset, Math.Min( count, 81_920 ) );
		token.ThrowIfCancellationRequested();
		return read;
	}
	public override int Read( Span<byte> buffer ) {
		token.ThrowIfCancellationRequested();
		int read = inner.Read( buffer[..Math.Min( buffer.Length, 81_920 )] );
		token.ThrowIfCancellationRequested();
		return read;
	}
	public override int ReadByte() {
		token.ThrowIfCancellationRequested();
		int value = inner.ReadByte();
		token.ThrowIfCancellationRequested();
		return value;
	}
	public override long Seek( long offset, SeekOrigin origin ) => inner.Seek( offset, origin );
	public override void Flush() => throw new NotSupportedException();
	public override void SetLength( long value ) => throw new NotSupportedException();
	public override void Write( byte[] buffer, int offset, int count ) => throw new NotSupportedException();
}
