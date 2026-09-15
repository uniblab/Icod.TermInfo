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
/// Identifies the deterministic outcome of advanced persistent-raster placement
/// planning.
/// </summary>
public enum PersistentRasterPlacementPlanStatus {
	/// <summary>
	/// All requested advanced placement semantics and required lifecycle operations
	/// are statically admissible.
	/// </summary>
	Satisfied = 0,

	/// <summary>
	/// Lifecycle planning is admissible, but one or more requested advanced placement
	/// semantics require runtime verification.
	/// </summary>
	RequiresRuntimeVerification = 1,

	/// <summary>
	/// The placement semantics are not conclusively impossible, but the prerequisite
	/// lifecycle plan is indeterminate.
	/// </summary>
	Indeterminate = 2,

	/// <summary>
	/// The prerequisite lifecycle plan is impossible or at least one requested
	/// advanced placement semantic is unsupported or contradicted.
	/// </summary>
	Impossible = 3,
}
