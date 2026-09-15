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
/// Couples an effective semantic comparison with the two explicitly acquired
/// inspection results that produced it.
/// </summary>
public sealed class TermInfoInspectionComparison {
	internal TermInfoInspectionComparison(
		TermInfoInspectionResult left,
		TermInfoInspectionResult right,
		TermInfoComparisonResult comparison
	) {
		ArgumentNullException.ThrowIfNull( left );
		ArgumentNullException.ThrowIfNull( right );
		ArgumentNullException.ThrowIfNull( comparison );

		Left = left;
		Right = right;
		Comparison = comparison;
	}

	/// <summary>
	/// Gets the acquired left target and terminal.
	/// </summary>
	public TermInfoInspectionResult Left { get; }

	/// <summary>
	/// Gets the acquired right target and terminal.
	/// </summary>
	public TermInfoInspectionResult Right { get; }

	/// <summary>
	/// Gets the deterministic effective semantic comparison.
	/// </summary>
	public TermInfoComparisonResult Comparison { get; }

	/// <summary>
	/// Gets whether the acquired effective descriptions are semantically equal.
	/// </summary>
	public bool AreEqual =>
		Comparison.AreEqual;
}
