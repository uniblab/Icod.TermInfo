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
/// Identifies a semantic lexical unit in terminfo source.
/// </summary>
/// <remarks>
/// Field-separator commas and inter-field whitespace are recognized as lexical
/// structure but are not emitted as tokens. Capability value decoding is
/// intentionally deferred to the value-semantics layer.
/// </remarks>
public enum TermInfoSourceTokenKind {
	/// <summary>
	/// The canonical terminal name in an entry header.
	/// </summary>
	TerminalName = 0,

	/// <summary>
	/// An alternate terminal name in an entry header.
	/// </summary>
	Alias = 1,

	/// <summary>
	/// The descriptive final component of an entry header.
	/// </summary>
	Description = 2,

	/// <summary>
	/// A Boolean capability field with no value operator.
	/// </summary>
	BooleanCapability = 3,

	/// <summary>
	/// A numeric capability field using the <c>#</c> operator.
	/// </summary>
	NumericCapability = 4,

	/// <summary>
	/// A string capability field using the <c>=</c> operator.
	/// </summary>
	StringCapability = 5,

	/// <summary>
	/// A capability cancellation field using the <c>@</c> operator.
	/// </summary>
	CancelledCapability = 6,

	/// <summary>
	/// A <c>use=</c> inheritance reference.
	/// </summary>
	UseReference = 7,

	/// <summary>
	/// An ncurses-compatible field disabled by a leading period.
	/// </summary>
	DisabledCapability = 8,

	/// <summary>
	/// A full-line source comment.
	/// </summary>
	Comment = 9,

	/// <summary>
	/// A field retained for diagnostics but not classifiable as valid lexical
	/// input.
	/// </summary>
	Invalid = 10,
}
