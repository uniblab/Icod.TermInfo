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
/// Contains the immutable result and evidence of one relative-source planning
/// operation.
/// </summary>
public sealed class TerminalDescriptionSourcePlan {
	internal TerminalDescriptionSourcePlan(
		IEnumerable<TerminalDescriptionSourceSynthesisParent> selectedParents,
		string source,
		TerminalDescriptionSourcePlanningScore score,
		int evaluatedPlanCount,
		bool isExhaustive,
		int candidateCount
	) {
		ArgumentNullException.ThrowIfNull( selectedParents );
		ArgumentNullException.ThrowIfNull( source );
		ArgumentNullException.ThrowIfNull( score );
		if ( evaluatedPlanCount < 1 ) {
			throw new ArgumentOutOfRangeException(
				nameof( evaluatedPlanCount ),
				evaluatedPlanCount,
				"The evaluated plan count must be positive."
			);
		}
		if ( candidateCount < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( candidateCount ),
				candidateCount,
				"The considered candidate count cannot be negative."
			);
		}

		TerminalDescriptionSourceSynthesisParent[] parentArray =
			selectedParents.ToArray();
		if ( parentArray.Any( parent => parent is null ) ) {
			throw new ArgumentException(
				"The selected parent sequence cannot contain null.",
				nameof( selectedParents )
			);
		}
		if ( parentArray.Length != score.ParentCount ) {
			throw new ArgumentException(
				"The selected parent count must equal the planning score parent count.",
				nameof( selectedParents )
			);
		}
		if ( parentArray.Length > candidateCount ) {
			throw new ArgumentException(
				"The selected parent count cannot exceed the considered candidate count.",
				nameof( selectedParents )
			);
		}
		if (
			score.SelectedCandidateIndices.Any(
				candidateIndex => candidateIndex >= candidateCount
			)
		) {
			throw new ArgumentException(
				"Selected candidate indices must identify considered candidate positions.",
				nameof( score )
			);
		}

		SelectedParents = Array.AsReadOnly( parentArray );
		Source = source;
		Score = score;
		EvaluatedPlanCount = evaluatedPlanCount;
		IsExhaustive = isExhaustive;
		CandidateCount = candidateCount;
	}

	/// <summary>
	/// Gets the selected parents in exact emitted <c>use=</c> order.
	/// </summary>
	public IReadOnlyList<TerminalDescriptionSourceSynthesisParent> SelectedParents {
		get;
	}

	/// <summary>
	/// Gets the deterministic generated LF terminfo source.
	/// </summary>
	public string Source {
		get;
	}

	/// <summary>
	/// Gets the frozen lexicographic score of the selected plan.
	/// </summary>
	public TerminalDescriptionSourcePlanningScore Score {
		get;
	}

	/// <summary>
	/// Gets the number of valid or rejected candidate plans evaluated.
	/// </summary>
	public int EvaluatedPlanCount {
		get;
	}

	/// <summary>
	/// Gets whether every legal plan under the active limits was evaluated.
	/// </summary>
	public bool IsExhaustive {
		get;
	}

	/// <summary>
	/// Gets the number of non-self candidate positions considered.
	/// </summary>
	public int CandidateCount {
		get;
	}
}
