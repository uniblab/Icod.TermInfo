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

namespace Icod.TermInfo;

/// <summary>
/// The terminfo parameter program is malformed.
/// </summary>
public sealed class TermInfoFormatException : FormatException {
	/// <summary>
	/// Initializes an exception with the default message.
	/// </summary>
	public TermInfoFormatException() {
		Position = -1;
	}

	/// <summary>
	/// Initializes an exception with the specified message.
	/// </summary>
	public TermInfoFormatException( string? message )
		: base( message ) {
		Position = -1;
	}

	/// <summary>
	/// Initializes an exception with the specified message and inner exception.
	/// </summary>
	public TermInfoFormatException(
		string? message,
		Exception? innerException
	)
		: base( message, innerException ) {
		Position = -1;
	}

	internal TermInfoFormatException(
		string message,
		int position
	)
		: base( CreateMessage( message, position ) ) {
		Position = position;
	}

	/// <summary>
	/// Gets the zero-based source position associated with the error, or
	/// <c>-1</c> when no source position was supplied.
	/// </summary>
	public int Position { get; }

	private static string CreateMessage(
		string message,
		int position
	) {
		ArgumentNullException.ThrowIfNull( message );

		if ( position < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( position ) );
		}

		return $"{message} (position {position}).";
	}
}
