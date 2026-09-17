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

/// <summary>Indicates a malformed or unsupported Berkeley DB container or ncurses record envelope.</summary>
public sealed class BerkeleyDbDatabaseFormatException : FormatException {
	/// <summary>Initializes an exception with the default message.</summary>
	public BerkeleyDbDatabaseFormatException() {
	}

	/// <summary>Initializes an exception with the specified message.</summary>
	public BerkeleyDbDatabaseFormatException( string? message )
		: base( message ) {
	}

	/// <summary>Initializes an exception with the specified message and inner exception.</summary>
	public BerkeleyDbDatabaseFormatException(
		string? message,
		Exception? innerException
	) : base( message, innerException ) {
	}
}
