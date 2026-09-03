namespace Icod.TermInfo.Inspection;

/// <summary>
/// Classifies deterministic semantic evidence for repeated database-set
/// identities and alias collisions.
/// </summary>
public enum TermInfoDatabaseSetSemanticRelationship {
	/// <summary>
	/// The relevant effective terminal descriptions compare equal.
	/// </summary>
	SemanticallyEqual = 0,

	/// <summary>
	/// At least one relevant effective terminal description or canonical owner
	/// conflicts with the selected precedence evidence.
	/// </summary>
	SemanticallyDifferent = 1,

	/// <summary>
	/// Incomplete input prevents a conclusive semantic classification.
	/// </summary>
	Indeterminate = 2,
}
