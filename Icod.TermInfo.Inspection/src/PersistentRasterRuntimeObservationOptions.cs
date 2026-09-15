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
/// Defines deterministic resource bounds for persistent-raster runtime
/// observation snapshots.
/// </summary>
public sealed class PersistentRasterRuntimeObservationOptions {
	/// <summary>
	/// Gets the default maximum number of lifecycle and placement observations in
	/// one combined runtime-observation set.
	/// </summary>
	public const int DefaultMaximumObservationCount = 256;

	/// <summary>
	/// Gets the largest supported configured runtime-observation count.
	/// </summary>
	public const int MaximumSupportedObservationCount = 4096;

	/// <summary>
	/// Gets the maximum permitted runtime-observation source-label length in UTF-16
	/// code units.
	/// </summary>
	public const int MaximumSourceLabelLength = 256;

	/// <summary>
	/// Initializes options using the default observation-count bound.
	/// </summary>
	public PersistentRasterRuntimeObservationOptions()
		: this( DefaultMaximumObservationCount ) {
	}

	/// <summary>
	/// Initializes options using an explicit observation-count bound.
	/// </summary>
	/// <param name="maximumObservationCount">
	/// Maximum combined lifecycle and placement observation count.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="maximumObservationCount"/> is outside the supported range.
	/// </exception>
	public PersistentRasterRuntimeObservationOptions(
		int maximumObservationCount
	) {
		if (
			maximumObservationCount < 1
			|| maximumObservationCount > MaximumSupportedObservationCount
		) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumObservationCount ),
				maximumObservationCount,
				$"The maximum runtime-observation count must be between 1 and {MaximumSupportedObservationCount}."
			);
		}

		MaximumObservationCount = maximumObservationCount;
	}

	/// <summary>
	/// Gets the maximum combined lifecycle and placement observation count.
	/// </summary>
	public int MaximumObservationCount {
		get;
	}
}
