/*
	Icod.TermInfo
	Provides managed terminfo runtime parsing, discovery, capabilities, and terminal profiles.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Text;

namespace Icod.TermInfo;

public static partial class CompiledTermInfoParser {
	private static int GetExtendedNameCount(
		ExtendedHeader header
	) {
		try {
			return checked(
				header.BooleanCount
				+ header.NumericCount
				+ header.StringCount
			);
		} catch ( OverflowException exception ) {
			throw CreateFormatException(
				"Extended capability-name count arithmetic overflowed.",
				-1,
				"extended-header",
				exception
			);
		}
	}

	private static int GetExtendedStringTableItemCount(
		ExtendedHeader header,
		int nameCount
	) {
		try {
			return checked(
				nameCount
				+ header.StringCount
			);
		} catch ( OverflowException exception ) {
			throw CreateFormatException(
				"Extended string-table item-count arithmetic overflowed.",
				-1,
				"extended-header",
				exception
			);
		}
	}

	private static int[] BuildStringTerminatorIndex(
		ReadOnlySpan<byte> stringTable
	) {
		int[] terminators =
			new int[stringTable.Length];
		int nextTerminator = -1;

		for (
			int index = stringTable.Length - 1;
			index >= 0;
			index--
		) {
			if ( stringTable[index] == 0 ) {
				nextTerminator = index;
			}

			terminators[index] = nextTerminator;
		}

		return terminators;
	}

	private readonly ref struct ExtendedStringTableReader {
		private readonly ReadOnlySpan<byte> _stringTable;
		private readonly ReadOnlySpan<int> _terminatorIndex;

		public ExtendedStringTableReader(
			ReadOnlySpan<byte> stringTable,
			ReadOnlySpan<int> terminatorIndex
		) {
			if ( stringTable.Length != terminatorIndex.Length ) {
				throw new ArgumentException(
					"The terminator index must match the string-table length.",
					nameof( terminatorIndex )
				);
			}

			_stringTable = stringTable;
			_terminatorIndex = terminatorIndex;
		}

		public int GetTerminatedStringEnd(
			int relativeOffset,
			int sourceOffset,
			string offsetSection,
			string dataSection
		) {
			if (
				(uint)relativeOffset
				>= (uint)_stringTable.Length
			) {
				throw CreateFormatException(
					$"String offset {relativeOffset} lies outside the declared extended string table.",
					sourceOffset,
					offsetSection
				);
			}

			int terminator =
				_terminatorIndex[relativeOffset];

			if ( terminator < 0 ) {
				throw CreateFormatException(
					"An extended string is not NUL-terminated inside the declared extended string table.",
					sourceOffset,
					dataSection
				);
			}

			return terminator + 1;
		}

		public string ReadLatin1String(
			int relativeOffset,
			int sourceOffset,
			string offsetSection,
			string dataSection
		) {
			int stringEnd =
				GetTerminatedStringEnd(
					relativeOffset,
					sourceOffset,
					offsetSection,
					dataSection
				);
			int length =
				stringEnd
				- relativeOffset
				- 1;

			return Encoding.Latin1.GetString(
				_stringTable.Slice(
					relativeOffset,
					length
				)
			);
		}
	}
}
