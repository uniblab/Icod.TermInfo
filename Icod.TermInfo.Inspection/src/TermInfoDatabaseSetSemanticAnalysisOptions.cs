namespace Icod.TermInfo.Inspection;

/// <summary>
/// Configures deterministic resource bounds for database-set semantic analysis.
/// </summary>
public sealed class TermInfoDatabaseSetSemanticAnalysisOptions {
	/// <summary>
	/// The default maximum number of alias declarations scanned across all
	/// physical occurrences.
	/// </summary>
	public const int DefaultMaximumAliasOccurrenceCount = 1_048_576;

	/// <summary>
	/// The largest supported caller-selected alias declaration bound.
	/// </summary>
	public const int MaximumSupportedAliasOccurrenceCount = 4_194_304;

	/// <summary>
	/// Initializes the canonical semantic-analysis resource policy.
	/// </summary>
	public TermInfoDatabaseSetSemanticAnalysisOptions()
		: this(
			DefaultMaximumAliasOccurrenceCount
		) {
	}

	/// <summary>
	/// Initializes an explicit deterministic alias-scan bound.
	/// </summary>
	/// <param name="maximumAliasOccurrenceCount">
	/// The maximum number of alias declarations scanned across all physical
	/// occurrences.
	/// </param>
	public TermInfoDatabaseSetSemanticAnalysisOptions(
		int maximumAliasOccurrenceCount
	) {
		if ( maximumAliasOccurrenceCount < 1
			|| maximumAliasOccurrenceCount > MaximumSupportedAliasOccurrenceCount ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumAliasOccurrenceCount ),
				maximumAliasOccurrenceCount,
				$"The maximum alias occurrence count must be between 1 and {MaximumSupportedAliasOccurrenceCount}."
			);
		}

		MaximumAliasOccurrenceCount = maximumAliasOccurrenceCount;
	}

	/// <summary>
	/// Gets the maximum number of alias declarations scanned during one analysis.
	/// </summary>
	public int MaximumAliasOccurrenceCount {
		get;
	}
}
