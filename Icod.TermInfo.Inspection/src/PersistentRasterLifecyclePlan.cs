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
/// Contains one immutable deterministic persistent-raster lifecycle planning
/// result.
/// </summary>
public sealed class PersistentRasterLifecyclePlan {
	internal PersistentRasterLifecyclePlan(
		PersistentRasterLifecyclePlanStatus status,
		IEnumerable<PersistentRasterLifecyclePlanStep> steps,
		IEnumerable<PersistentRasterLifecyclePlanIssue> issues
	) {
		ArgumentNullException.ThrowIfNull( steps );
		ArgumentNullException.ThrowIfNull( issues );
		if ( !Enum.IsDefined( status ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( status ),
				status,
				"The lifecycle plan status must be a defined value."
			);
		}

		PersistentRasterLifecyclePlanStep[] stepArray =
			steps.ToArray();
		PersistentRasterLifecyclePlanIssue[] issueArray =
			issues.ToArray();
		if ( stepArray.Any( step => step is null ) ) {
			throw new ArgumentException(
				"The lifecycle plan step sequence cannot contain null.",
				nameof( steps )
			);
		}
		if ( issueArray.Any( issue => issue is null ) ) {
			throw new ArgumentException(
				"The lifecycle plan issue sequence cannot contain null.",
				nameof( issues )
			);
		}
		for ( int index = 0; index < stepArray.Length; index++ ) {
			if ( stepArray[ index ].SequenceIndex != index ) {
				throw new ArgumentException(
					"Lifecycle plan step indices must be contiguous and zero-based.",
					nameof( steps )
				);
			}
		}

		switch ( status ) {
			case PersistentRasterLifecyclePlanStatus.Success:
				if ( issueArray.Length != 0 ) {
					throw new ArgumentException(
						"A successful lifecycle plan cannot contain issues.",
						nameof( issues )
					);
				}
				break;
			case PersistentRasterLifecyclePlanStatus.Indeterminate:
				if (
					stepArray.Length == 0
					|| !issueArray.Any(
						issue => issue.RequiresRuntimeVerification
					)
				) {
					throw new ArgumentException(
						"An indeterminate lifecycle plan requires executable steps and at least one runtime-verification issue."
					);
				}
				break;
			case PersistentRasterLifecyclePlanStatus.Impossible:
				if (
					stepArray.Length != 0
					|| !issueArray.Any(
						issue =>
							issue.SupportStatus
								== PersistentRasterLifecycleSupportStatus.Unsupported
					)
				) {
					throw new ArgumentException(
						"An impossible lifecycle plan must contain no executable steps and at least one unsupported requirement."
					);
				}
				break;
		}

		Status = status;
		Steps = Array.AsReadOnly( stepArray );
		Issues = Array.AsReadOnly( issueArray );
		RequiresRuntimeVerification =
			status == PersistentRasterLifecyclePlanStatus.Indeterminate;
	}

	/// <summary>Gets the deterministic planning outcome.</summary>
	public PersistentRasterLifecyclePlanStatus Status {
		get;
	}

	/// <summary>Gets the ordered semantic operations when execution remains admissible.</summary>
	public IReadOnlyList<PersistentRasterLifecyclePlanStep> Steps {
		get;
	}

	/// <summary>Gets the structured non-supported requirements.</summary>
	public IReadOnlyList<PersistentRasterLifecyclePlanIssue> Issues {
		get;
	}

	/// <summary>
	/// Gets whether the plan is admissible only after consumer-owned runtime
	/// verification strengthens one or more uncertain requirements.
	/// </summary>
	public bool RequiresRuntimeVerification {
		get;
	}
}
