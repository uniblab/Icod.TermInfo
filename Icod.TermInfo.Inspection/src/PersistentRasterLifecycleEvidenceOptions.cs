namespace Icod.TermInfo.Inspection;

/// <summary>
/// Configures deterministic resource bounds for persistent-raster lifecycle
/// evidence snapshots.
/// </summary>
public sealed class PersistentRasterLifecycleEvidenceOptions {
	/// <summary>
	/// The default maximum number of evidence assertions accepted by one
	/// snapshot operation.
	/// </summary>
	public const int DefaultMaximumEvidenceCount = 256;

	/// <summary>
	/// The largest supported caller-selected evidence-count bound.
	/// </summary>
	public const int MaximumSupportedEvidenceCount = 4096;

	/// <summary>
	/// Initializes the canonical evidence snapshot resource policy.
	/// </summary>
	public PersistentRasterLifecycleEvidenceOptions()
		: this( DefaultMaximumEvidenceCount ) {
	}

	/// <summary>
	/// Initializes an explicit deterministic evidence-count bound.
	/// </summary>
	/// <param name="maximumEvidenceCount">
	/// The maximum number of evidence assertions accepted by one snapshot.
	/// </param>
	public PersistentRasterLifecycleEvidenceOptions(
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
