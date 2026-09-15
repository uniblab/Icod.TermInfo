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
/// Represents one immutable bounded canonical snapshot of caller-owned
/// persistent-raster runtime observations.
/// </summary>
public sealed class PersistentRasterRuntimeObservationSet {
	/// <summary>
	/// Initializes an immutable canonical runtime-observation snapshot.
	/// </summary>
	/// <param name="lifecycleObservations">
	/// Lifecycle runtime observations to snapshot.
	/// </param>
	/// <param name="placementObservations">
	/// Placement runtime observations to snapshot.
	/// </param>
	/// <param name="options">Optional deterministic resource bounds.</param>
	public PersistentRasterRuntimeObservationSet(
		IEnumerable<PersistentRasterRuntimeLifecycleObservation> lifecycleObservations,
		IEnumerable<PersistentRasterRuntimePlacementObservation> placementObservations,
		PersistentRasterRuntimeObservationOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( lifecycleObservations );
		ArgumentNullException.ThrowIfNull( placementObservations );

		PersistentRasterRuntimeObservationOptions effectiveOptions =
			options ?? new PersistentRasterRuntimeObservationOptions();
		List<PersistentRasterRuntimeLifecycleObservation> lifecycleItems = [];
		List<PersistentRasterRuntimePlacementObservation> placementItems = [];
		int count = 0;

		foreach (
			PersistentRasterRuntimeLifecycleObservation item
				in lifecycleObservations
		) {
			if ( item is null ) {
				throw new ArgumentException(
					"A lifecycle runtime-observation collection cannot contain null.",
					nameof( lifecycleObservations )
				);
			}
			if ( count >= effectiveOptions.MaximumObservationCount ) {
				throw new ArgumentException(
					$"The runtime-observation set exceeds the configured maximum of {effectiveOptions.MaximumObservationCount} observations.",
					nameof( lifecycleObservations )
				);
			}

			lifecycleItems.Add( item );
			count++;
		}

		foreach (
			PersistentRasterRuntimePlacementObservation item
				in placementObservations
		) {
			if ( item is null ) {
				throw new ArgumentException(
					"A placement runtime-observation collection cannot contain null.",
					nameof( placementObservations )
				);
			}
			if ( count >= effectiveOptions.MaximumObservationCount ) {
				throw new ArgumentException(
					$"The runtime-observation set exceeds the configured maximum of {effectiveOptions.MaximumObservationCount} observations.",
					nameof( placementObservations )
				);
			}

			placementItems.Add( item );
			count++;
		}

		PersistentRasterRuntimeLifecycleObservation[] orderedLifecycle =
			lifecycleItems
				.OrderBy( item => (int)item.Subject )
				.ThenBy( item => item.SourceLabel, StringComparer.Ordinal )
				.ThenBy( item => item.SourceOrdinal )
				.ThenBy( item => (int)item.Outcome )
				.ToArray();
		PersistentRasterRuntimePlacementObservation[] orderedPlacement =
			placementItems
				.OrderBy( item => (int)item.Subject )
				.ThenBy( item => item.SourceLabel, StringComparer.Ordinal )
				.ThenBy( item => item.SourceOrdinal )
				.ThenBy( item => (int)item.Outcome )
				.ToArray();

		LifecycleObservations = Array.AsReadOnly( orderedLifecycle );
		PlacementObservations = Array.AsReadOnly( orderedPlacement );
		Count = count;
	}

	/// <summary>Gets the canonical immutable lifecycle observation snapshot.</summary>
	public IReadOnlyList<PersistentRasterRuntimeLifecycleObservation>
		LifecycleObservations {
		get;
	}

	/// <summary>Gets the canonical immutable placement observation snapshot.</summary>
	public IReadOnlyList<PersistentRasterRuntimePlacementObservation>
		PlacementObservations {
		get;
	}

	/// <summary>
	/// Gets the combined number of lifecycle and placement observations.
	/// </summary>
	public int Count {
		get;
	}
}
