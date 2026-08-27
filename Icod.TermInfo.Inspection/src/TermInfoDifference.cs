using Icod.TermInfo.Source;

namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one machine-readable difference from an effective or source-aware
/// terminfo comparison.
/// </summary>
/// <remarks>
/// <para>
/// Identity differences use <see cref="LeftText"/>,
/// <see cref="RightText"/>, <see cref="LeftAliases"/>, and
/// <see cref="RightAliases"/> as appropriate. Effective capability differences
/// use <see cref="CapabilityName"/>, <see cref="IsExtendedCapability"/>,
/// <see cref="LeftCapabilityValue"/>, and <see cref="RightCapabilityValue"/>.
/// </para>
/// <para>
/// Source-aware differences expose the immutable Source 1.1 entries and fields
/// directly through the source-context properties. Source spans are supplied
/// only from the retained parsed model; no provenance is reconstructed.
/// </para>
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
		TermInfoCapabilityValue? rightCapabilityValue,
		TermInfoSourceEntry? leftSourceEntry = null,
		TermInfoSourceEntry? rightSourceEntry = null,
		int? leftSourceEntryIndex = null,
		int? rightSourceEntryIndex = null,
		TermInfoSourceField? leftSourceField = null,
		TermInfoSourceField? rightSourceField = null,
		int? leftSourceFieldIndex = null,
		int? rightSourceFieldIndex = null
	) {
		if ( !Enum.IsDefined( typeof( TermInfoDifferenceKind ), kind ) ) {
			throw new ArgumentOutOfRangeException( nameof( kind ) );
		}

		ValidateSourceIndex(
			leftSourceEntryIndex,
			nameof( leftSourceEntryIndex )
		);
		ValidateSourceIndex(
			rightSourceEntryIndex,
			nameof( rightSourceEntryIndex )
		);
		ValidateSourceIndex(
			leftSourceFieldIndex,
			nameof( leftSourceFieldIndex )
		);
		ValidateSourceIndex(
			rightSourceFieldIndex,
			nameof( rightSourceFieldIndex )
		);

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
		LeftSourceEntry = leftSourceEntry;
		RightSourceEntry = rightSourceEntry;
		LeftSourceEntryIndex = leftSourceEntryIndex;
		RightSourceEntryIndex = rightSourceEntryIndex;
		LeftSourceField = leftSourceField;
		RightSourceField = rightSourceField;
		LeftSourceFieldIndex = leftSourceFieldIndex;
		RightSourceFieldIndex = rightSourceFieldIndex;
	}

	/// <summary>
	/// Gets the semantic category of the difference.
	/// </summary>
	public TermInfoDifferenceKind Kind { get; }

	/// <summary>
	/// Gets whether this is an effective capability difference rather than
	/// effective identity metadata or source-aware structure.
	/// </summary>
	public bool IsCapabilityDifference =>
		CapabilityName is not null;

	/// <summary>
	/// Gets whether this difference came from source-aware comparison.
	/// </summary>
	public bool IsSourceDifference =>
		LeftSourceEntry is not null
			|| RightSourceEntry is not null
			|| LeftSourceField is not null
			|| RightSourceField is not null;

	/// <summary>
	/// Gets the canonical standard short name or exact extended capability name for
	/// an effective capability difference.
	/// </summary>
	/// <remarks>
	/// This is <see langword="null"/> for identity-metadata and source-aware
	/// differences.
	/// </remarks>
	public string? CapabilityName { get; }

	/// <summary>
	/// Gets whether an effective capability difference refers to an extended
	/// capability.
	/// </summary>
	/// <remarks>
	/// This is <see langword="null"/> for identity-metadata and source-aware
	/// differences.
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

	/// <summary>
	/// Gets the unresolved source entry on the left when source-aware context is
	/// available.
	/// </summary>
	public TermInfoSourceEntry? LeftSourceEntry { get; }

	/// <summary>
	/// Gets the unresolved source entry on the right when source-aware context is
	/// available.
	/// </summary>
	public TermInfoSourceEntry? RightSourceEntry { get; }

	/// <summary>
	/// Gets the zero-based left document-entry index when comparison originated from
	/// a source document.
	/// </summary>
	public int? LeftSourceEntryIndex { get; }

	/// <summary>
	/// Gets the zero-based right document-entry index when comparison originated
	/// from a source document.
	/// </summary>
	public int? RightSourceEntryIndex { get; }

	/// <summary>
	/// Gets the unresolved source field on the left when this is a source-field
	/// difference.
	/// </summary>
	public TermInfoSourceField? LeftSourceField { get; }

	/// <summary>
	/// Gets the unresolved source field on the right when this is a source-field
	/// difference.
	/// </summary>
	public TermInfoSourceField? RightSourceField { get; }

	/// <summary>
	/// Gets the zero-based left source-field index when one is available.
	/// </summary>
	public int? LeftSourceFieldIndex { get; }

	/// <summary>
	/// Gets the zero-based right source-field index when one is available.
	/// </summary>
	public int? RightSourceFieldIndex { get; }

	/// <summary>
	/// Gets the most specific retained source span on the left.
	/// </summary>
	public TermInfoSourceSpan? LeftSourceSpan =>
		LeftSourceField is not null
			|| RightSourceField is not null
			? LeftSourceField?.Span
			: LeftSourceEntry?.Span;

	/// <summary>
	/// Gets the most specific retained source span on the right.
	/// </summary>
	public TermInfoSourceSpan? RightSourceSpan =>
		LeftSourceField is not null
			|| RightSourceField is not null
			? RightSourceField?.Span
			: RightSourceEntry?.Span;

	private static void ValidateSourceIndex(
		int? value,
		string parameterName
	) {
		if ( value.HasValue && value.Value < 0 ) {
			throw new ArgumentOutOfRangeException( parameterName );
		}
	}
}
