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
/// Couples one successfully acquired terminal description with the explicit
/// inspection target that produced it.
/// </summary>
public sealed class TermInfoInspectionResult {
	internal TermInfoInspectionResult(
		TermInfoInspectionTarget target,
		TerminalDescription terminal
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( terminal );

		Target = target;
		Terminal = terminal;
	}

	/// <summary>
	/// Gets the explicit provider/name target used for acquisition.
	/// </summary>
	public TermInfoInspectionTarget Target { get; }

	/// <summary>
	/// Gets the acquired immutable effective terminal description.
	/// </summary>
	public TerminalDescription Terminal { get; }
}
