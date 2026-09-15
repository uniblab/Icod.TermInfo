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
/// Represents the immutable derived evaluation of one raster-backend candidate.
/// </summary>
public sealed class RasterBackendCandidateEvaluation {
	internal RasterBackendCandidateEvaluation(
		RasterBackendCandidate candidate,
		PersistentRasterLifecyclePlan lifecyclePlan,
		PersistentRasterPlacementPlan? placementPlan,
		RasterBackendCandidateStatus status
	) {
		ArgumentNullException.ThrowIfNull( candidate );
		ArgumentNullException.ThrowIfNull( lifecyclePlan );
		if ( !Enum.IsDefined( status ) ) {
			throw new ArgumentOutOfRangeException( nameof( status ) );
		}

		Candidate = candidate;
		LifecyclePlan = lifecyclePlan;
		PlacementPlan = placementPlan;
		Status = status;
	}

	/// <summary>Gets the candidate that was evaluated.</summary>
	public RasterBackendCandidate Candidate {
		get;
	}

	/// <summary>Gets the frozen lifecycle planner result.</summary>
	public PersistentRasterLifecyclePlan LifecyclePlan {
		get;
	}

	/// <summary>
	/// Gets the frozen placement planner result, or <see langword="null"/> when no
	/// advanced placement semantics were requested.
	/// </summary>
	public PersistentRasterPlacementPlan? PlacementPlan {
		get;
	}

	/// <summary>Gets the derived candidate status.</summary>
	public RasterBackendCandidateStatus Status {
		get;
	}
}
