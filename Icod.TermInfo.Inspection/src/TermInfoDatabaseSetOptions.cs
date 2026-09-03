namespace Icod.TermInfo.Inspection;

/// <summary>
/// Configures deterministic resource bounds for ordered terminfo database-set
/// construction.
/// </summary>
public sealed class TermInfoDatabaseSetOptions {
	/// <summary>
	/// The default maximum number of constituent database catalogs.
	/// </summary>
	public const int DefaultMaximumDatabaseCount = 64;

	/// <summary>
	/// The largest supported caller-selected constituent database count.
	/// </summary>
	public const int MaximumSupportedDatabaseCount = 4096;

	/// <summary>
	/// The default maximum number of physical catalog entries aggregated across
	/// the complete database set.
	/// </summary>
	public const int DefaultMaximumTotalEntryCount = 262_144;

	/// <summary>
	/// The largest supported caller-selected aggregate physical entry count.
	/// </summary>
	public const int MaximumSupportedTotalEntryCount = 1_048_576;

	/// <summary>
	/// Initializes the canonical database-set resource policy.
	/// </summary>
	public TermInfoDatabaseSetOptions()
		: this(
			DefaultMaximumDatabaseCount,
			DefaultMaximumTotalEntryCount
		) {
	}

	/// <summary>
	/// Initializes explicit deterministic database-set resource bounds.
	/// </summary>
	/// <param name="maximumDatabaseCount">
	/// The maximum number of constituent database catalogs.
	/// </param>
	/// <param name="maximumTotalEntryCount">
	/// The maximum number of physical catalog entries aggregated across all
	/// constituent catalogs.
	/// </param>
	public TermInfoDatabaseSetOptions(
		int maximumDatabaseCount,
		int maximumTotalEntryCount
	) {
		if (
			maximumDatabaseCount < 1
			|| maximumDatabaseCount > MaximumSupportedDatabaseCount
		) {
			throw new ArgumentOutOfRangeException(
				nameof(maximumDatabaseCount),
				maximumDatabaseCount,
				$"The maximum database count must be between 1 and {MaximumSupportedDatabaseCount}."
			);
		}
		if (
			maximumTotalEntryCount < 1
			|| maximumTotalEntryCount > MaximumSupportedTotalEntryCount
		) {
			throw new ArgumentOutOfRangeException(
				nameof(maximumTotalEntryCount),
				maximumTotalEntryCount,
				$"The maximum total entry count must be between 1 and {MaximumSupportedTotalEntryCount}."
			);
		}

		MaximumDatabaseCount = maximumDatabaseCount;
		MaximumTotalEntryCount = maximumTotalEntryCount;
	}

	/// <summary>
	/// Gets the maximum number of constituent database catalogs.
	/// </summary>
	public int MaximumDatabaseCount {
		get;
	}

	/// <summary>
	/// Gets the maximum aggregate number of physical catalog entries.
	/// </summary>
	public int MaximumTotalEntryCount {
		get;
	}
}
