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
/// Identifies the severity of a terminfo source diagnostic.
/// </summary>
public enum TermInfoSourceDiagnosticSeverity {
	/// <summary>
	/// The condition does not prevent a source consumer from continuing.
	/// </summary>
	Warning = 0,

	/// <summary>
	/// The source is malformed or cannot be processed as requested.
	/// </summary>
	Error = 1,
}
