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
/// Identifies one protocol-neutral advanced persistent-raster placement semantic
/// about which static or caller-supplied evidence may make a support assertion.
/// </summary>
/// <remarks>
/// These values describe support for placement semantics only. They do not carry
/// concrete source-rectangle coordinates, signed z-order values, terminal-side
/// identifiers, or wire-protocol fields.
/// </remarks>
public enum PersistentRasterPlacementSubject {
	/// <summary>
	/// Placement of a selected pixel-space source rectangle from an uploaded raster
	/// resource.
	/// </summary>
	SourceRectangle = 0,

	/// <summary>
	/// Placement using an explicit signed stacking or z-order value.
	/// </summary>
	SignedZOrder = 1,
}
