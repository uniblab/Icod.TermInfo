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
/// Selects whether termcap acquisition may append a conventional implicit
/// database path set after explicitly supplied sources.
/// </summary>
public enum TermcapDefaultPathPolicy {
	/// <summary>Do not add any implicit termcap database paths.</summary>
	None = 0,

	/// <summary>
	/// Append the conventional ncurses-compatible path order
	/// <c>/etc/termcap</c>, <c>/usr/share/misc/termcap</c>, and, when a home
	/// directory was supplied, <c>$HOME/.termcap</c>.
	/// </summary>
	Ncurses = 1,
}
