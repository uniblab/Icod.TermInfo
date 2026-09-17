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

using System.Text;

namespace Icod.TermInfo.BerkeleyDb;

internal static class TerminalNameEncoding {
	private static readonly UTF8Encoding StrictUtf8 =
		new(
			encoderShouldEmitUTF8Identifier: false,
			throwOnInvalidBytes: true
		);

	internal static byte[] EncodeUtf8( string name ) {
		ArgumentNullException.ThrowIfNull( name );
		return StrictUtf8.GetBytes( name );
	}

	internal static string DecodePublicationName(
		ReadOnlySpan<byte> bytes
	) {
		try {
			return StrictUtf8.GetString( bytes );
		} catch ( DecoderFallbackException ) {
			return Encoding.Latin1.GetString( bytes );
		}
	}

	internal static bool TryEncodeDistinctLatin1(
		string name,
		ReadOnlySpan<byte> utf8,
		out byte[] latin1
	) {
		ArgumentNullException.ThrowIfNull( name );
		latin1 = new byte[name.Length];
		for ( int index = 0; index < name.Length; index++ ) {
			if ( name[index] > '\u00FF' ) {
				latin1 = [];
				return false;
			}
			latin1[index] = (byte)name[index];
		}

		if ( utf8.SequenceEqual( latin1 ) ) {
			latin1 = [];
			return false;
		}
		return true;
	}
}
