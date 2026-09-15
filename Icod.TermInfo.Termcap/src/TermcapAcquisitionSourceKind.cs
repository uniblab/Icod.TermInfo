/*
	Icod.TermInfo.Termcap
	Provides managed termcap parsing, conversion, rendering, and interoperability.
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

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Identifies which explicitly configured termcap source supplied an acquired
/// root entry.
/// </summary>
public enum TermcapAcquisitionSourceKind {
	/// <summary>An inline termcap description supplied directly to acquisition.</summary>
	InlineTermcap = 0,

	/// <summary>A database path supplied as the explicit TERMCAP path.</summary>
	TermcapDatabasePath = 1,

	/// <summary>A database path supplied through the ordered TERMPATH list.</summary>
	TermPathDatabase = 2,

	/// <summary>A database selected by an explicit conventional-default policy.</summary>
	ConventionalDefaultDatabase = 3,
}
