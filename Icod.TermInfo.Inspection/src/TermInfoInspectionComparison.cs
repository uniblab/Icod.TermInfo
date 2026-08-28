namespace Icod.TermInfo.Inspection;

/// <summary>
/// Couples an effective semantic comparison with the two explicitly acquired
/// inspection results that produced it.
/// </summary>
public sealed class TermInfoInspectionComparison {
	internal TermInfoInspectionComparison(
		TermInfoInspectionResult left,
		TermInfoInspectionResult right,
		TermInfoComparisonResult comparison
	) {
		ArgumentNullException.ThrowIfNull( left );
		ArgumentNullException.ThrowIfNull( right );
		ArgumentNullException.ThrowIfNull( comparison );

		Left = left;
		Right = right;
		Comparison = comparison;
	}

	/// <summary>
	/// Gets the acquired left target and terminal.
	/// </summary>
	public TermInfoInspectionResult Left { get; }

	/// <summary>
	/// Gets the acquired right target and terminal.
	/// </summary>
	public TermInfoInspectionResult Right { get; }

	/// <summary>
	/// Gets the deterministic effective semantic comparison.
	/// </summary>
	public TermInfoComparisonResult Comparison { get; }

	/// <summary>
	/// Gets whether the acquired effective descriptions are semantically equal.
	/// </summary>
	public bool AreEqual =>
		Comparison.AreEqual;
}
