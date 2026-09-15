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
/// Configures deterministic resource bounds for advanced persistent-raster
/// placement evidence snapshots.
/// </summary>
public sealed class PersistentRasterPlacementEvidenceOptions {
	/// <summary>
	/// The default maximum number of evidence assertions accepted by one snapshot
	/// operation.
	/// </summary>
	public const int DefaultMaximumEvidenceCount = 256;

	/// <summary>
	/// The largest supported caller-selected evidence-count bound.
	/// </summary>
	public const int MaximumSupportedEvidenceCount = 4096;

	/// <summary>
	/// Initializes the canonical evidence snapshot resource policy.
	/// </summary>
	public PersistentRasterPlacementEvidenceOptions()
		: this( DefaultMaximumEvidenceCount ) {
	}

	/// <summary>
	/// Initializes an explicit deterministic evidence-count bound.
	/// </summary>
	/// <param name="maximumEvidenceCount">
	/// The maximum number of evidence assertions accepted by one snapshot.
	/// </param>
	public PersistentRasterPlacementEvidenceOptions(
		int maximumEvidenceCount
	) {
		if (
			maximumEvidenceCount < 1
			|| maximumEvidenceCount > MaximumSupportedEvidenceCount
		) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumEvidenceCount ),
				maximumEvidenceCount,
				$"The maximum evidence count must be between 1 and {MaximumSupportedEvidenceCount}."
			);
		}

		MaximumEvidenceCount = maximumEvidenceCount;
	}

	/// <summary>
	/// Gets the maximum number of assertions accepted by one evidence snapshot.
	/// </summary>
	public int MaximumEvidenceCount {
		get;
	}
}
