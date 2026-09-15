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
/// Represents one immutable deterministic advanced persistent-raster placement plan.
/// </summary>
public sealed class PersistentRasterPlacementPlan {
	internal PersistentRasterPlacementPlan(
		PersistentRasterPlacementPlanStatus status,
		PersistentRasterLifecyclePlanStatus lifecycleStatus,
		IReadOnlyList<PersistentRasterPlacementPlanRequirement> requirements,
		IReadOnlyList<PersistentRasterPlacementPlanIssue> issues
	) {
		if ( !Enum.IsDefined( status ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( status ),
				status,
				"The placement plan status must be a defined value."
			);
		}
		if ( !Enum.IsDefined( lifecycleStatus ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( lifecycleStatus ),
				lifecycleStatus,
				"The lifecycle plan status must be a defined value."
			);
		}
		ArgumentNullException.ThrowIfNull( requirements );
		ArgumentNullException.ThrowIfNull( issues );
		if ( requirements.Count < 1 || requirements.Count > 2 ) {
			throw new ArgumentException(
				"A placement plan must contain between one and two requirements.",
				nameof( requirements )
			);
		}
		if ( issues.Count > requirements.Count ) {
			throw new ArgumentException(
				"A placement plan cannot contain more issues than requirements.",
				nameof( issues )
			);
		}
		if ( requirements.Any( item => item is null ) ) {
			throw new ArgumentException(
				"A placement plan cannot contain a null requirement.",
				nameof( requirements )
			);
		}
		if ( issues.Any( item => item is null ) ) {
			throw new ArgumentException(
				"A placement plan cannot contain a null issue.",
				nameof( issues )
			);
		}

		Status = status;
		LifecycleStatus = lifecycleStatus;
		Requirements =
			Array.AsReadOnly(
				requirements.ToArray()
			);
		Issues =
			Array.AsReadOnly(
				issues.ToArray()
			);
	}

	/// <summary>Gets the combined deterministic placement-planning outcome.</summary>
	public PersistentRasterPlacementPlanStatus Status {
		get;
	}

	/// <summary>
	/// Gets the frozen 1.11 lifecycle-plan outcome supplied to placement planning.
	/// </summary>
	public PersistentRasterLifecyclePlanStatus LifecycleStatus {
		get;
	}

	/// <summary>
	/// Gets the canonical immutable advanced-placement requirements and their support
	/// states.
	/// </summary>
	public IReadOnlyList<PersistentRasterPlacementPlanRequirement> Requirements {
		get;
	}

	/// <summary>
	/// Gets the canonical immutable non-supported advanced-placement issues.
	/// </summary>
	public IReadOnlyList<PersistentRasterPlacementPlanIssue> Issues {
		get;
	}
}
