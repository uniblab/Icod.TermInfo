namespace Icod.TermInfo.Inspection;

/// <summary>
/// Describes one observed later occurrence relative to canonical precedence
/// evidence.
/// </summary>
public sealed class TermInfoDatabaseSetShadowAnalysis {
	internal TermInfoDatabaseSetShadowAnalysis(
		TermInfoDatabaseSetOccurrence occurrence,
		TermInfoDatabaseSetSemanticRelationship relationship,
		TermInfoComparisonResult? comparison
	) {
		ArgumentNullException.ThrowIfNull( occurrence );
		if ( relationship == TermInfoDatabaseSetSemanticRelationship.Indeterminate ) {
			if ( comparison is not null ) {
				throw new ArgumentException(
					"Indeterminate shadow evidence cannot contain a semantic comparison.",
					nameof( comparison )
				);
			}
		} else {
			ArgumentNullException.ThrowIfNull( comparison );
			bool expectedEqual =
				relationship == TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual;
			if ( comparison.AreEqual != expectedEqual ) {
				throw new ArgumentException(
					"The comparison result does not match the requested semantic relationship.",
					nameof( comparison )
				);
			}
		}

		Occurrence = occurrence;
		Relationship = relationship;
		Comparison = comparison;
	}

	/// <summary>
	/// Gets the observed later physical occurrence.
	/// </summary>
	public TermInfoDatabaseSetOccurrence Occurrence {
		get;
	}

	/// <summary>
	/// Gets the semantic relationship to the precedence winner, or indeterminate
	/// when no winner can be established.
	/// </summary>
	public TermInfoDatabaseSetSemanticRelationship Relationship {
		get;
	}

	/// <summary>
	/// Gets the frozen structured comparison when a precedence winner is known.
	/// </summary>
	public TermInfoComparisonResult? Comparison {
		get;
	}
}
