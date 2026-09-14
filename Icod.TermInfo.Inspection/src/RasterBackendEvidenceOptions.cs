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
