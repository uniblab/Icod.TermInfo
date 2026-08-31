namespace Icod.TermInfo.Termcap;

/// <summary>
/// Represents one effective termcap field after <c>tc=</c> inheritance has been
/// resolved.
/// </summary>
public sealed class TermcapSourceResolvedField
{
	internal TermcapSourceResolvedField(
		TermcapSourceEntry sourceEntry,
		TermcapSourceField sourceField,
		int inheritanceDepth
	) {
		ArgumentNullException.ThrowIfNull( sourceEntry );
		ArgumentNullException.ThrowIfNull( sourceField );
		ArgumentOutOfRangeException.ThrowIfNegative( inheritanceDepth );

		SourceEntry = sourceEntry;
		SourceField = sourceField;
		InheritanceDepth = inheritanceDepth;
	}

	/// <summary>
	/// Gets the unresolved source entry which supplied this effective field.
	/// </summary>
	public TermcapSourceEntry SourceEntry { get; }

	/// <summary>
	/// Gets the original unresolved source field, including its source span.
	/// </summary>
	public TermcapSourceField SourceField { get; }

	/// <summary>
	/// Gets the exact two-character termcap capability name.
	/// </summary>
	public string CapabilityName => SourceField.CapabilityName;

	/// <summary>
	/// Gets the number of <c>tc=</c> edges between the requested root and the
	/// entry which supplied this field.
	/// </summary>
	public int InheritanceDepth { get; }

	/// <summary>
	/// Gets whether this field came from an inherited entry.
	/// </summary>
	public bool IsInherited => InheritanceDepth != 0;
}
