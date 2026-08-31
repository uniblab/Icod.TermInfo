using Icod.TermInfo;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Describes one mapping from a two-character termcap code to one canonical
/// Runtime standard capability identity.
/// </summary>
public sealed class TermcapStandardCapabilityMapping
{
	private TermcapStandardCapabilityMapping(
		string termcapCode,
		string canonicalTermcapCode,
		TermInfoCapabilityValueKind valueKind,
		int binaryIndex,
		string termInfoShortName,
		string termInfoLongName,
		bool isObsoleteStandard,
		bool isObsoleteAlias,
		string? aliasOrigin,
		BooleanCapability? booleanCapability,
		NumericCapability? numericCapability,
		StringCapability? stringCapability
	) {
		ValidateCode(
			termcapCode,
			nameof( termcapCode )
		);
		ValidateCode(
			canonicalTermcapCode,
			nameof( canonicalTermcapCode )
		);
		ArgumentOutOfRangeException.ThrowIfNegative( binaryIndex );
		ArgumentException.ThrowIfNullOrWhiteSpace( termInfoShortName );
		ArgumentException.ThrowIfNullOrWhiteSpace( termInfoLongName );

		int identities = 0;
		if ( booleanCapability.HasValue ) {
			identities++;
		}
		if ( numericCapability.HasValue ) {
			identities++;
		}
		if ( stringCapability.HasValue ) {
			identities++;
		}
		if ( identities != 1 ) {
			throw new ArgumentException(
				"Exactly one managed standard-capability identity must be supplied."
			);
		}
		if ( isObsoleteAlias && string.IsNullOrWhiteSpace( aliasOrigin ) ) {
			throw new ArgumentException(
				"An obsolete alias must identify its compatibility origin.",
				nameof( aliasOrigin )
			);
		}
		if ( !isObsoleteAlias && aliasOrigin is not null ) {
			throw new ArgumentException(
				"Only obsolete aliases may identify an alias origin.",
				nameof( aliasOrigin )
			);
		}

		TermcapCode = termcapCode;
		CanonicalTermcapCode = canonicalTermcapCode;
		ValueKind = valueKind;
		BinaryIndex = binaryIndex;
		TermInfoShortName = termInfoShortName;
		TermInfoLongName = termInfoLongName;
		IsObsoleteStandard = isObsoleteStandard;
		IsObsoleteAlias = isObsoleteAlias;
		AliasOrigin = aliasOrigin;
		BooleanCapability = booleanCapability;
		NumericCapability = numericCapability;
		StringCapability = stringCapability;
	}

	/// <summary>
	/// Gets the two-character code accepted from termcap source.
	/// </summary>
	public string TermcapCode { get; }

	/// <summary>
	/// Gets the canonical termcap code recorded by the Runtime standard
	/// capability metadata.
	/// </summary>
	public string CanonicalTermcapCode { get; }

	/// <summary>
	/// Gets the Runtime value kind expected for this capability.
	/// </summary>
	public TermInfoCapabilityValueKind ValueKind { get; }

	/// <summary>
	/// Gets the capability's compiled-table index for its Runtime value kind.
	/// </summary>
	public int BinaryIndex { get; }

	/// <summary>
	/// Gets the canonical terminfo short name from the Runtime metadata.
	/// </summary>
	public string TermInfoShortName { get; }

	/// <summary>
	/// Gets the canonical terminfo long name from the Runtime metadata.
	/// </summary>
	public string TermInfoLongName { get; }

	/// <summary>
	/// Gets whether the target is an obsolete termcap compatibility capability
	/// retained in the Runtime standard catalog.
	/// </summary>
	public bool IsObsoleteStandard { get; }

	/// <summary>
	/// Gets whether <see cref="TermcapCode"/> is an obsolete non-standard alias
	/// rather than the Runtime metadata's canonical termcap code.
	/// </summary>
	public bool IsObsoleteAlias { get; }

	/// <summary>
	/// Gets the historical source family for an obsolete alias, or
	/// <see langword="null"/> for a canonical mapping.
	/// </summary>
	public string? AliasOrigin { get; }

	/// <summary>
	/// Gets the mapped Boolean capability identity when
	/// <see cref="ValueKind"/> is <see cref="TermInfoCapabilityValueKind.Boolean"/>.
	/// </summary>
	public BooleanCapability? BooleanCapability { get; }

	/// <summary>
	/// Gets the mapped numeric capability identity when
	/// <see cref="ValueKind"/> is <see cref="TermInfoCapabilityValueKind.Number"/>.
	/// </summary>
	public NumericCapability? NumericCapability { get; }

	/// <summary>
	/// Gets the mapped string capability identity when
	/// <see cref="ValueKind"/> is <see cref="TermInfoCapabilityValueKind.String"/>.
	/// </summary>
	public StringCapability? StringCapability { get; }

	internal static TermcapStandardCapabilityMapping Create(
		StandardCapabilityMetadata<BooleanCapability> metadata
	) {
		ArgumentNullException.ThrowIfNull( metadata );

		return new TermcapStandardCapabilityMapping(
			metadata.TermcapCode,
			metadata.TermcapCode,
			metadata.Kind,
			metadata.BinaryIndex,
			metadata.ShortName,
			metadata.LongName,
			IsObsoleteStandardName( metadata.ShortName ),
			false,
			null,
			metadata.Capability,
			null,
			null
		);
	}

	internal static TermcapStandardCapabilityMapping Create(
		StandardCapabilityMetadata<NumericCapability> metadata
	) {
		ArgumentNullException.ThrowIfNull( metadata );

		return new TermcapStandardCapabilityMapping(
			metadata.TermcapCode,
			metadata.TermcapCode,
			metadata.Kind,
			metadata.BinaryIndex,
			metadata.ShortName,
			metadata.LongName,
			IsObsoleteStandardName( metadata.ShortName ),
			false,
			null,
			null,
			metadata.Capability,
			null
		);
	}

	internal static TermcapStandardCapabilityMapping Create(
		StandardCapabilityMetadata<StringCapability> metadata
	) {
		ArgumentNullException.ThrowIfNull( metadata );

		return new TermcapStandardCapabilityMapping(
			metadata.TermcapCode,
			metadata.TermcapCode,
			metadata.Kind,
			metadata.BinaryIndex,
			metadata.ShortName,
			metadata.LongName,
			IsObsoleteStandardName( metadata.ShortName ),
			false,
			null,
			null,
			null,
			metadata.Capability
		);
	}

	internal TermcapStandardCapabilityMapping CreateAlias(
		string aliasCode,
		string origin
	) {
		ValidateCode(
			aliasCode,
			nameof( aliasCode )
		);
		ArgumentException.ThrowIfNullOrWhiteSpace( origin );

		return new TermcapStandardCapabilityMapping(
			aliasCode,
			CanonicalTermcapCode,
			ValueKind,
			BinaryIndex,
			TermInfoShortName,
			TermInfoLongName,
			IsObsoleteStandard,
			true,
			origin,
			BooleanCapability,
			NumericCapability,
			StringCapability
		);
	}

	private static bool IsObsoleteStandardName(
		string shortName
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( shortName );

		return shortName.StartsWith(
			"OT",
			StringComparison.Ordinal
		);
	}

	private static void ValidateCode(
		string code,
		string parameterName
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( code );
		ArgumentException.ThrowIfNullOrWhiteSpace( parameterName );

		if ( code.Length != 2 ) {
			throw new ArgumentException(
				"A termcap capability code must contain exactly two characters.",
				parameterName
			);
		}
	}
}
