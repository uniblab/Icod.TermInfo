using Icod.TermInfo;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Reports the semantic classification of one unresolved termcap source field.
/// </summary>
public sealed class TermcapCapabilityClassificationResult
{
	internal TermcapCapabilityClassificationResult(
		TermcapSourceField field,
		TermcapCapabilityClassification classification,
		IReadOnlyList<TermcapStandardCapabilityMapping> mappings,
		TermcapStandardCapabilityMapping? mapping,
		TermInfoCapabilityValueKind? sourceValueKind
	) {
		ArgumentNullException.ThrowIfNull( field );
		ArgumentNullException.ThrowIfNull( mappings );

		Field = field;
		Classification = classification;
		Mappings = mappings;
		Mapping = mapping;
		SourceValueKind = sourceValueKind;
	}

	/// <summary>
	/// Gets the unresolved source field which was classified.
	/// </summary>
	public TermcapSourceField Field { get; }

	/// <summary>
	/// Gets the mapping classification.
	/// </summary>
	public TermcapCapabilityClassification Classification { get; }

	/// <summary>
	/// Gets all Runtime mappings associated with the source code. More than one
	/// mapping indicates that code-level ambiguity exists even when active source
	/// syntax is sufficient to select one value kind.
	/// </summary>
	public IReadOnlyList<TermcapStandardCapabilityMapping> Mappings { get; }

	/// <summary>
	/// Gets the uniquely selected Runtime mapping, or <see langword="null"/> when
	/// the field is unmapped, remains ambiguous, or is a <c>tc=</c> reference.
	/// </summary>
	public TermcapStandardCapabilityMapping? Mapping { get; }

	/// <summary>
	/// Gets the value kind implied by the source field syntax. Cancellation,
	/// disabled fields, and inheritance references do not carry a source value
	/// kind and return <see langword="null"/>.
	/// </summary>
	public TermInfoCapabilityValueKind? SourceValueKind { get; }

	/// <summary>
	/// Gets the value kind expected by the selected Runtime mapping, or
	/// <see langword="null"/> when there is no unique selected mapping.
	/// </summary>
	public TermInfoCapabilityValueKind? ExpectedValueKind {
		get {
			return Mapping?.ValueKind;
		}
	}

	/// <summary>
	/// Gets whether the active source field syntax conflicts with the value kind
	/// required by its selected Runtime mapping.
	/// </summary>
	public bool HasValueKindMismatch {
		get {
			return SourceValueKind.HasValue
				&& ExpectedValueKind.HasValue
				&& SourceValueKind.Value != ExpectedValueKind.Value;
		}
	}
}
