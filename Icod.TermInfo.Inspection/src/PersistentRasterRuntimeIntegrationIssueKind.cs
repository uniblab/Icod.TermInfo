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
/// Describes one represented limitation encountered while importing persistent-
/// raster runtime observations into frozen lifecycle or placement evidence.
/// </summary>
public enum PersistentRasterRuntimeIntegrationIssueKind {
	/// <summary>
	/// Conclusive lifecycle observations cannot fit within the frozen lifecycle
	/// evidence-count bound.
	/// </summary>
	LifecycleEvidenceCapacityExhausted = 0,

	/// <summary>
	/// Conclusive placement observations cannot fit within the frozen placement
	/// evidence-count bound.
	/// </summary>
	PlacementEvidenceCapacityExhausted = 1,

	/// <summary>
	/// Consecutive final lifecycle evidence ordinals cannot be assigned without
	/// overflowing <see cref="int"/>.
	/// </summary>
	LifecycleOrdinalSpaceExhausted = 2,

	/// <summary>
	/// Consecutive final placement evidence ordinals cannot be assigned without
	/// overflowing <see cref="int"/>.
	/// </summary>
	PlacementOrdinalSpaceExhausted = 3,
}
