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
/// Represents malformed terminfo padding syntax.
/// </summary>
public sealed class TermInfoPaddingFormatException : FormatException {
	/// <summary>
	/// Initializes an exception with the default message.
	/// </summary>
	public TermInfoPaddingFormatException() {
		Position = -1;
	}

	/// <summary>
	/// Initializes an exception with the specified message.
	/// </summary>
	public TermInfoPaddingFormatException( string? message )
		: base( message ) {
		Position = -1;
	}

	/// <summary>
	/// Initializes an exception with the specified message and inner exception.
	/// </summary>
	public TermInfoPaddingFormatException(
		string? message,
		Exception? innerException
	) : base( message, innerException ) {
		Position = -1;
	}

	internal TermInfoPaddingFormatException(
		string message,
		int position
	) : base( message ) {
		ArgumentNullException.ThrowIfNull( message );

		if ( position < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( position ) );
		}

		Position = position;
	}

	/// <summary>
	/// Gets the zero-based position of the malformed padding directive, or
	/// <c>-1</c> when no source position was supplied.
	/// </summary>
	public int Position { get; }
}
