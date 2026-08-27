namespace Icod.TermInfo.Inspection;

/// <summary>
/// Contains the deterministic structured result of one terminfo comparison.
/// </summary>
public sealed class TermInfoComparisonResult {
	private readonly IReadOnlyList<TermInfoDifference> _differences;

	internal TermInfoComparisonResult(
		IEnumerable<TermInfoDifference> differences
	) {
		ArgumentNullException.ThrowIfNull( differences );

		_differences =
			Array.AsReadOnly(
				differences.ToArray()
			);
	}

	/// <summary>
	/// Gets whether the compared effective descriptions are semantically equal.
	/// </summary>
	public bool AreEqual =>
		_differences.Count == 0;

	/// <summary>
	/// Gets the differences in deterministic comparison order.
	/// </summary>
	public IReadOnlyList<TermInfoDifference> Differences =>
		_differences;
}
