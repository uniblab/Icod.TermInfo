/*
	Icod.TermInfo.BerkeleyDb
	Managed read-only support for ncurses-compatible Berkeley DB Hash-v9 terminfo stores.
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

internal sealed class ByteArrayComparer
	: IComparer<byte[]>, IEqualityComparer<byte[]> {
	internal static ByteArrayComparer Instance { get; } =
		new ByteArrayComparer();

	private ByteArrayComparer() {
	}

	public int Compare(
		byte[]? left,
		byte[]? right
	) {
		if ( ReferenceEquals( left, right ) ) {
			return 0;
		}
		if ( left is null ) {
			return -1;
		}
		if ( right is null ) {
			return 1;
		}

		int sharedLength = Math.Min(
			left.Length,
			right.Length
		);
		for ( int index = 0; index < sharedLength; index++ ) {
			int comparison = left[index].CompareTo( right[index] );
			if ( comparison != 0 ) {
				return comparison;
			}
		}

		return left.Length.CompareTo( right.Length );
	}

	internal int Compare(
		ReadOnlySpan<byte> left,
		ReadOnlySpan<byte> right
	) {
		int sharedLength = Math.Min(
			left.Length,
			right.Length
		);
		for ( int index = 0; index < sharedLength; index++ ) {
			int comparison = left[index].CompareTo( right[index] );
			if ( comparison != 0 ) {
				return comparison;
			}
		}

		return left.Length.CompareTo( right.Length );
	}

	public bool Equals(
		byte[]? left,
		byte[]? right
	) {
		if ( ReferenceEquals( left, right ) ) {
			return true;
		}
		if ( left is null || right is null ) {
			return false;
		}

		return left.AsSpan().SequenceEqual( right );
	}

	public int GetHashCode( byte[] value ) {
		ArgumentNullException.ThrowIfNull( value );

		var hash = new HashCode();
		foreach ( byte item in value ) {
			hash.Add( item );
		}
		return hash.ToHashCode();
	}
}
