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
