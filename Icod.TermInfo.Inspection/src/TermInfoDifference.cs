namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one machine-readable difference between two effective terminal
/// descriptions.
/// </summary>
/// <remarks>
/// Identity differences use <see cref="LeftText"/>, <see cref="RightText"/>,
/// <see cref="LeftAliases"/>, and <see cref="RightAliases"/> as appropriate.
/// Capability differences use <see cref="CapabilityName"/>,
/// <see cref="IsExtendedCapability"/>, <see cref="LeftCapabilityValue"/>, and
/// <see cref="RightCapabilityValue"/>.
/// </remarks>
public sealed class TermInfoDifference {
	private readonly IReadOnlyList<string>? _leftAliases;
	private readonly IReadOnlyList<string>? _rightAliases;

	internal TermInfoDifference(
		TermInfoDifferenceKind kind,
		string? capabilityName,
		bool? isExtendedCapability,
		string? leftText,
		string? rightText,
		IEnumerable<string>? leftAliases,
		IEnumerable<string>? rightAliases,
		TermInfoCapabilityValue? leftCapabilityValue,
		TermInfoCapabilityValue? rightCapabilityValue
	) {
		Kind = kind;
		CapabilityName = capabilityName;
		IsExtendedCapability = isExtendedCapability;
		LeftText = leftText;
		RightText = rightText;
		_leftAliases =
			leftAliases is null
				? null
				: Array.AsReadOnly(
					leftAliases.ToArray()
				);
		_rightAliases =
			rightAliases is null
				? null
				: Array.AsReadOnly(
					rightAliases.ToArray()
				);
		LeftCapabilityValue = leftCapabilityValue;
		RightCapabilityValue = rightCapabilityValue;
	}

	/// <summary>
	/// Gets the semantic category of the difference.
	/// </summary>
	public TermInfoDifferenceKind Kind { get; }

	/// <summary>
	/// Gets whether this is a capability difference rather than identity metadata.
	/// </summary>
	public bool IsCapabilityDifference =>
		CapabilityName is not null;

	/// <summary>
	/// Gets the canonical standard short name or exact extended capability name.
	/// </summary>
	/// <remarks>
	/// This is <see langword="null"/> for identity-metadata differences.
	/// </remarks>
	public string? CapabilityName { get; }

	/// <summary>
	/// Gets whether a capability difference refers to an extended capability.
	/// </summary>
	/// <remarks>
	/// This is <see langword="null"/> for identity-metadata differences.
	/// </remarks>
	public bool? IsExtendedCapability { get; }

	/// <summary>
	/// Gets the left canonical name or description for the matching identity
	/// difference kind.
	/// </summary>
	public string? LeftText { get; }

	/// <summary>
	/// Gets the right canonical name or description for the matching identity
	/// difference kind.
	/// </summary>
	public string? RightText { get; }

	/// <summary>
	/// Gets the left ordered alias list for an alias identity difference.
	/// </summary>
	public IReadOnlyList<string>? LeftAliases =>
		_leftAliases;

	/// <summary>
	/// Gets the right ordered alias list for an alias identity difference.
	/// </summary>
	public IReadOnlyList<string>? RightAliases =>
		_rightAliases;

	/// <summary>
	/// Gets the effective capability value on the left when one is present.
	/// </summary>
	public TermInfoCapabilityValue? LeftCapabilityValue { get; }

	/// <summary>
	/// Gets the effective capability value on the right when one is present.
	/// </summary>
	public TermInfoCapabilityValue? RightCapabilityValue { get; }
}
