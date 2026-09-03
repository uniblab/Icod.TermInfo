namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one deterministic typed difference between two ordered terminfo
/// database sets.
/// </summary>
public sealed class TermInfoDatabaseSetDifference {
	internal TermInfoDatabaseSetDifference(
		TermInfoDatabaseSetDifferenceKind kind,
		string? name = null,
		TermInfoDatabaseSetEntry? leftDatabase = null,
		TermInfoDatabaseSetEntry? rightDatabase = null,
		TermInfoDatabaseSetOccurrence? leftOccurrence = null,
		TermInfoDatabaseSetOccurrence? rightOccurrence = null,
		TermInfoDatabaseSetIssue? leftIssue = null,
		TermInfoDatabaseSetIssue? rightIssue = null,
		TermInfoDatabaseSetLookupResult? leftLookup = null,
		TermInfoDatabaseSetLookupResult? rightLookup = null,
		TermInfoComparisonResult? semanticComparison = null
	) {
		if ( name is not null && string.IsNullOrWhiteSpace( name ) ) {
			throw new ArgumentException(
				"A comparison difference name cannot be empty or whitespace.",
				nameof( name )
			);
		}

		Kind = kind;
		Name = name;
		LeftDatabase = leftDatabase;
		RightDatabase = rightDatabase;
		LeftOccurrence = leftOccurrence;
		RightOccurrence = rightOccurrence;
		LeftIssue = leftIssue;
		RightIssue = rightIssue;
		LeftLookup = leftLookup;
		RightLookup = rightLookup;
		SemanticComparison = semanticComparison;
	}

	/// <summary>
	/// Gets the stable difference category.
	/// </summary>
	public TermInfoDatabaseSetDifferenceKind Kind {
		get;
	}

	/// <summary>
	/// Gets the canonical identity or alias associated with the difference, when
	/// the category is name-scoped.
	/// </summary>
	public string? Name {
		get;
	}

	/// <summary>
	/// Gets relevant left constituent database evidence, when applicable.
	/// </summary>
	public TermInfoDatabaseSetEntry? LeftDatabase {
		get;
	}

	/// <summary>
	/// Gets relevant right constituent database evidence, when applicable.
	/// </summary>
	public TermInfoDatabaseSetEntry? RightDatabase {
		get;
	}

	/// <summary>
	/// Gets relevant left physical occurrence evidence, when applicable.
	/// </summary>
	public TermInfoDatabaseSetOccurrence? LeftOccurrence {
		get;
	}

	/// <summary>
	/// Gets relevant right physical occurrence evidence, when applicable.
	/// </summary>
	public TermInfoDatabaseSetOccurrence? RightOccurrence {
		get;
	}

	/// <summary>
	/// Gets relevant left catalog issue evidence, when applicable.
	/// </summary>
	public TermInfoDatabaseSetIssue? LeftIssue {
		get;
	}

	/// <summary>
	/// Gets relevant right catalog issue evidence, when applicable.
	/// </summary>
	public TermInfoDatabaseSetIssue? RightIssue {
		get;
	}

	/// <summary>
	/// Gets relevant left precedence lookup evidence, when applicable.
	/// </summary>
	public TermInfoDatabaseSetLookupResult? LeftLookup {
		get;
	}

	/// <summary>
	/// Gets relevant right precedence lookup evidence, when applicable.
	/// </summary>
	public TermInfoDatabaseSetLookupResult? RightLookup {
		get;
	}

	/// <summary>
	/// Gets the retained cross-set semantic comparison when the difference compared
	/// two observed terminal descriptions.
	/// </summary>
	public TermInfoComparisonResult? SemanticComparison {
		get;
	}
}
