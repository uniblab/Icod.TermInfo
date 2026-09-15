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
/// Contains the deterministic structured result of one terminfo comparison.
/// </summary>
public sealed class TermInfoComparisonResult {
	private readonly IReadOnlyList<TermInfoDifference> _differences;

	internal TermInfoComparisonResult(
		IEnumerable<TermInfoDifference> differences
	) {
		ArgumentNullException.ThrowIfNull( differences );

		_differences =
			Array.AsReadOnly(
				differences.ToArray()
			);
	}

	/// <summary>
	/// Gets whether the compared values are equal in the comparison domain selected
	/// by the comparer which produced this result.
	/// </summary>
	public bool AreEqual =>
		_differences.Count == 0;

	/// <summary>
	/// Gets the differences in deterministic comparison order.
	/// </summary>
	public IReadOnlyList<TermInfoDifference> Differences =>
		_differences;
}
