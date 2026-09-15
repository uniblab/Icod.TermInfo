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
/// Defines deterministic resource bounds for raster-backend evidence snapshots.
/// </summary>
public sealed class RasterBackendEvidenceOptions {
	/// <summary>Gets the default maximum number of evidence assertions.</summary>
	public const int DefaultMaximumEvidenceCount = 256;

	/// <summary>Gets the largest supported configured evidence count.</summary>
	public const int MaximumSupportedEvidenceCount = 4096;

	/// <summary>Gets the maximum source-label length in UTF-16 code units.</summary>
	public const int MaximumSourceLabelLength = 256;

	/// <summary>Initializes options using the default evidence-count bound.</summary>
	public RasterBackendEvidenceOptions()
		: this( DefaultMaximumEvidenceCount ) {
	}

	/// <summary>Initializes options using an explicit evidence-count bound.</summary>
	/// <param name="maximumEvidenceCount">Maximum evidence count.</param>
	public RasterBackendEvidenceOptions(
		int maximumEvidenceCount
	) {
		if (
			maximumEvidenceCount < 1
			|| maximumEvidenceCount > MaximumSupportedEvidenceCount
		) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumEvidenceCount ),
				maximumEvidenceCount,
				$"The maximum raster-backend evidence count must be between 1 and {MaximumSupportedEvidenceCount}."
			);
		}

		MaximumEvidenceCount = maximumEvidenceCount;
	}

	/// <summary>Gets the maximum evidence count.</summary>
	public int MaximumEvidenceCount {
		get;
	}
}
