/*
	Icod.TermInfo.Inspection
	Provides terminfo inspection, comparison, planning, and machine-readable automation.
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

namespace Icod.TermInfo.Inspection;

/// <summary>
/// Selects the physical layout used when rendering an effective terminal
/// description as terminfo source.
/// </summary>
public enum TerminalDescriptionSourceLayout {
	/// <summary>
	/// Uses canonical multi-line source formatting with deterministic wrapping.
	/// </summary>
	Canonical = 0,

	/// <summary>
	/// Emits the complete entry on one logical source line without wrapping.
	/// </summary>
	SingleLine = 1,

	/// <summary>
	/// Emits the header on one line and each capability on exactly one following
	/// line without continuation wrapping.
	/// </summary>
	OneCapabilityPerLine = 2,
}
