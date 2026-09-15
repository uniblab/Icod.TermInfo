/*
	Icod.TermInfo.Source
	Parses, resolves, renders, and plans terminfo source descriptions.
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

namespace Icod.TermInfo.Source;

/// <summary>
/// Identifies one unresolved field in a parsed terminfo source entry.
/// </summary>
/// <remarks>
/// S04 deliberately preserves the source-language field kind without deciding
/// whether a capability name is standard or extended. Capability catalog
/// classification begins in S05.
/// </remarks>
public enum TermInfoSourceFieldKind {
	/// <summary>
	/// A Boolean capability declaration.
	/// </summary>
	BooleanCapability = 0,

	/// <summary>
	/// A numeric capability declaration.
	/// </summary>
	NumericCapability = 1,

	/// <summary>
	/// A string capability declaration.
	/// </summary>
	StringCapability = 2,

	/// <summary>
	/// A capability cancellation declaration.
	/// </summary>
	CancelledCapability = 3,

	/// <summary>
	/// A <c>use=</c> inheritance reference.
	/// </summary>
	UseReference = 4,

	/// <summary>
	/// An ncurses-compatible field disabled by a leading period.
	/// </summary>
	DisabledCapability = 5,
}
