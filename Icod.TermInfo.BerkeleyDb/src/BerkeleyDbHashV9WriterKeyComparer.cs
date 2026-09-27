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

internal sealed class BerkeleyDbHashV9WriterKeyComparer
	: IComparer<byte[]> {
	internal static BerkeleyDbHashV9WriterKeyComparer Instance { get; } =
		new BerkeleyDbHashV9WriterKeyComparer();

	private BerkeleyDbHashV9WriterKeyComparer() {
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

		return Compare( left.AsSpan(), right.AsSpan() );
	}

	internal int Compare(
		ReadOnlySpan<byte> left,
		ReadOnlySpan<byte> right
	) {
		int sharedLength = Math.Min( left.Length, right.Length );
		for ( int index = 0; index < sharedLength; index++ ) {
			int comparison = left[index].CompareTo( right[index] );
			if ( comparison != 0 ) {
				return comparison;
			}
		}

		return right.Length.CompareTo( left.Length );
	}
}
