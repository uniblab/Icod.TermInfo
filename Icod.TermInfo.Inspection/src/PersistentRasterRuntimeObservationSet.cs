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
