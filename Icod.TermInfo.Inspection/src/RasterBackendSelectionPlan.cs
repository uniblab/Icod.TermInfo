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
/// Represents the immutable advisory result of raster-backend selection
/// planning.
/// </summary>
public sealed class RasterBackendSelectionPlan {
	internal RasterBackendSelectionPlan(
		RasterBackendSelectionRequest request,
		RasterBackendSelectionOptions options,
		IReadOnlyList<RasterBackendCandidateEvaluation> candidateEvaluations,
		RasterBackendSelectionStatus status,
		RasterBackendKind? selectedBackend
	) {
		ArgumentNullException.ThrowIfNull( request );
		ArgumentNullException.ThrowIfNull( options );
		ArgumentNullException.ThrowIfNull( candidateEvaluations );
		if ( !Enum.IsDefined( status ) ) {
			throw new ArgumentOutOfRangeException( nameof( status ) );
		}
		if ( selectedBackend.HasValue && !Enum.IsDefined( selectedBackend.Value ) ) {
			throw new ArgumentOutOfRangeException( nameof( selectedBackend ) );
		}
		if (
			(status == RasterBackendSelectionStatus.Selected) != selectedBackend.HasValue
		) {
			throw new ArgumentException(
				"A selected backend is required exactly when selection status is Selected.",
				nameof( selectedBackend )
			);
		}

		RasterBackendCandidateEvaluation[] snapshot = candidateEvaluations.ToArray();
		if ( snapshot.Length > RasterBackendSelectionOptions.MaximumSupportedPreferenceCount ) {
			throw new ArgumentException(
				$"Raster-backend selection cannot evaluate more than {RasterBackendSelectionOptions.MaximumSupportedPreferenceCount} candidates.",
				nameof( candidateEvaluations )
			);
		}
		HashSet<RasterBackendKind> seen = [];
		foreach ( RasterBackendCandidateEvaluation? evaluation in snapshot ) {
			if ( evaluation is null ) {
				throw new ArgumentException(
					"Raster-backend candidate evaluations cannot contain null elements.",
					nameof( candidateEvaluations )
				);
			}
			if ( !seen.Add( evaluation.Candidate.BackendProfile.Backend ) ) {
				throw new ArgumentException(
					"Raster-backend selection cannot contain duplicate backend candidates.",
					nameof( candidateEvaluations )
				);
			}
		}

		Request = request;
		Options = options;
		CandidateEvaluations = Array.AsReadOnly( snapshot );
		Status = status;
		SelectedBackend = selectedBackend;
	}

	/// <summary>Gets the semantic request evaluated by this plan.</summary>
	public RasterBackendSelectionRequest Request {
		get;
	}

	/// <summary>Gets the explicit caller selection policy.</summary>
	public RasterBackendSelectionOptions Options {
		get;
	}

	/// <summary>Gets the immutable per-backend candidate evaluations.</summary>
	public IReadOnlyList<RasterBackendCandidateEvaluation> CandidateEvaluations {
		get;
	}

	/// <summary>Gets the overall selection status.</summary>
	public RasterBackendSelectionStatus Status {
		get;
	}

	/// <summary>Gets the selected backend when <see cref="Status"/> is Selected.</summary>
	public RasterBackendKind? SelectedBackend {
		get;
	}
}
