namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies one stable database-set comparison difference category.
/// </summary>
public enum TermInfoDatabaseSetDifferenceKind {
	/// <summary>
	/// The ordered constituent root topology differs.
	/// </summary>
	RootTopology = 0,

	/// <summary>
	/// Aggregate or constituent completeness differs.
	/// </summary>
	Completeness = 1,

	/// <summary>
	/// Frozen catalog issue evidence differs.
	/// </summary>
	Issue = 2,

	/// <summary>
	/// A conclusive canonical identity is present only in the left set.
	/// </summary>
	OnlyInLeft = 3,

	/// <summary>
	/// A conclusive canonical identity is present only in the right set.
	/// </summary>
	OnlyInRight = 4,

	/// <summary>
	/// The effective precedence winners are semantically different.
	/// </summary>
	EffectiveSemantic = 5,

	/// <summary>
	/// The effective winners are semantically equal but their physical provenance
	/// differs.
	/// </summary>
	EffectiveProvenance = 6,

	/// <summary>
	/// Effective alias ownership, owner semantics, or owner provenance differs.
	/// </summary>
	AliasOwnership = 7,

	/// <summary>
	/// The observed ordered shadow set differs semantically or structurally.
	/// </summary>
	ShadowSet = 8,

	/// <summary>
	/// Incomplete evidence prevents a complete comparison conclusion.
	/// </summary>
	Indeterminate = 9,
}
