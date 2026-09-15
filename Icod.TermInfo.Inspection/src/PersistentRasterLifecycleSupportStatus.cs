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
/// Describes the deterministic semantic support state for one persistent-raster
/// lifecycle capability.
/// </summary>
public enum PersistentRasterLifecycleSupportStatus {
	/// <summary>
	/// The available evidence does not establish either support or lack of support.
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// The available evidence establishes support for the semantic capability.
	/// </summary>
	Supported = 1,

	/// <summary>
	/// The available evidence establishes that the semantic capability is not
	/// supported.
	/// </summary>
	Unsupported = 2,

	/// <summary>
	/// Conclusive positive and negative evidence coexist and the contradiction is
	/// retained rather than silently resolved.
	/// </summary>
	Contradicted = 3,
}
