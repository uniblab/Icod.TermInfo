using System.Collections.ObjectModel;
using Icod.TermInfo;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Exposes the adopted mapping from two-character termcap codes to the existing
/// Runtime standard-capability identities.
/// </summary>
public static class TermcapCapabilityCatalog
{
	private static readonly ObsoleteAliasDefinition[] ObsoleteAliases =
	[
		new( "BO", "mr", "AT&T" ),
		new( "CI", "vi", "AT&T" ),
		new( "CV", "ve", "AT&T" ),
		new( "DS", "mh", "AT&T" ),
		new( "EE", "me", "AT&T" ),
		new( "FE", "LF", "AT&T" ),
		new( "FL", "LO", "AT&T" ),
		new( "XS", "mk", "AT&T" ),
		new( "EN", "@7", "XENIX" ),
		new( "GE", "ae", "XENIX" ),
		new( "GS", "as", "XENIX" ),
		new( "HM", "kh", "XENIX" ),
		new( "LD", "kL", "XENIX" ),
		new( "PD", "kN", "XENIX" ),
		new( "PN", "po", "XENIX" ),
		new( "PS", "pf", "XENIX" ),
		new( "PU", "kP", "XENIX" ),
		new( "RT", "@8", "XENIX" ),
		new( "UP", "ku", "XENIX" ),
		new( "KA", "k;", "Tektronix" ),
		new( "KB", "F1", "Tektronix" ),
		new( "KC", "F2", "Tektronix" ),
		new( "KD", "F3", "Tektronix" ),
		new( "KE", "F4", "Tektronix" ),
		new( "KF", "F5", "Tektronix" ),
		new( "BC", "Sb", "Tektronix" ),
		new( "FC", "Sf", "Tektronix" ),
		new( "HS", "mh", "IRIX" ),
	];

	private static readonly TermcapStandardCapabilityMapping[] CanonicalMappingArray =
		CreateCanonicalMappings();
	private static readonly TermcapStandardCapabilityMapping[] MappingArray =
		CreateMappings(
			CanonicalMappingArray
		);
	private static readonly IReadOnlyList<TermcapStandardCapabilityMapping> MappingList =
		Array.AsReadOnly(
			MappingArray
		);
	private static readonly IReadOnlyDictionary<string, IReadOnlyList<TermcapStandardCapabilityMapping>> ByCode =
		CreateByCode(
			MappingArray
		);
	private static readonly IReadOnlyList<TermcapStandardCapabilityMapping> EmptyMappings =
		Array.Empty<TermcapStandardCapabilityMapping>();

	/// <summary>
	/// Gets all adopted canonical and obsolete-alias mappings in deterministic
	/// termcap-code order.
	/// </summary>
	public static IReadOnlyList<TermcapStandardCapabilityMapping> Mappings =>
		MappingList;

	/// <summary>
	/// Gets every adopted Runtime mapping for a two-character termcap code.
	/// </summary>
	/// <param name="termcapCode">The exact two-character termcap code.</param>
	/// <returns>
	/// A deterministic read-only list. The list is empty when the code is
	/// syntactically valid but unmapped.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="termcapCode"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="termcapCode"/> is empty, whitespace, or is not exactly
	/// two characters long.
	/// </exception>
	public static IReadOnlyList<TermcapStandardCapabilityMapping> GetMappings(
		string termcapCode
	) {
		ValidateTermcapCode( termcapCode );

		if (
			ByCode.TryGetValue(
				termcapCode,
				out IReadOnlyList<TermcapStandardCapabilityMapping>? mappings
			)
		) {
			return mappings;
		}

		return EmptyMappings;
	}

	private static TermcapStandardCapabilityMapping[] CreateCanonicalMappings() {
		List<TermcapStandardCapabilityMapping> mappings = [];

		foreach (
			StandardCapabilityMetadata<BooleanCapability> metadata
			in StandardCapabilityCatalog.BooleanCapabilities
		) {
			mappings.Add(
				TermcapStandardCapabilityMapping.Create( metadata )
			);
		}
		foreach (
			StandardCapabilityMetadata<NumericCapability> metadata
			in StandardCapabilityCatalog.NumericCapabilities
		) {
			mappings.Add(
				TermcapStandardCapabilityMapping.Create( metadata )
			);
		}
		foreach (
			StandardCapabilityMetadata<StringCapability> metadata
			in StandardCapabilityCatalog.StringCapabilities
		) {
			mappings.Add(
				TermcapStandardCapabilityMapping.Create( metadata )
			);
		}

		return mappings.ToArray();
	}

	private static TermcapStandardCapabilityMapping[] CreateMappings(
		IReadOnlyList<TermcapStandardCapabilityMapping> canonicalMappings
	) {
		ArgumentNullException.ThrowIfNull( canonicalMappings );

		Dictionary<string, TermcapStandardCapabilityMapping[]> canonicalByCode =
			canonicalMappings
				.GroupBy(
					mapping => mapping.TermcapCode,
					StringComparer.Ordinal
				)
				.ToDictionary(
					group => group.Key,
					group => group.ToArray(),
					StringComparer.Ordinal
				);
		List<TermcapStandardCapabilityMapping> mappings =
			new( canonicalMappings );
		HashSet<string> aliasCodes =
			new( StringComparer.Ordinal );

		foreach ( ObsoleteAliasDefinition alias in ObsoleteAliases ) {
			if ( !aliasCodes.Add( alias.AliasCode ) ) {
				throw new InvalidOperationException(
					$"The obsolete termcap alias '{alias.AliasCode}' is declared more than once."
				);
			}
			if (
				!canonicalByCode.TryGetValue(
					alias.CanonicalCode,
					out TermcapStandardCapabilityMapping[]? targets
				)
			) {
				throw new InvalidOperationException(
					$"The obsolete termcap alias '{alias.AliasCode}' targets unknown canonical code '{alias.CanonicalCode}'."
				);
			}

			foreach ( TermcapStandardCapabilityMapping target in targets ) {
				mappings.Add(
					target.CreateAlias(
						alias.AliasCode,
						alias.Origin
					)
				);
			}
		}

		return mappings
			.OrderBy(
				mapping => mapping.TermcapCode,
				StringComparer.Ordinal
			)
			.ThenBy(
				mapping => mapping.IsObsoleteAlias
			)
			.ThenBy(
				mapping => mapping.ValueKind
			)
			.ThenBy(
				mapping => mapping.BinaryIndex
			)
			.ThenBy(
				mapping => mapping.TermInfoShortName,
				StringComparer.Ordinal
			)
			.ToArray();
	}

	private static IReadOnlyDictionary<string, IReadOnlyList<TermcapStandardCapabilityMapping>> CreateByCode(
		IEnumerable<TermcapStandardCapabilityMapping> mappings
	) {
		ArgumentNullException.ThrowIfNull( mappings );

		Dictionary<string, IReadOnlyList<TermcapStandardCapabilityMapping>> dictionary =
			mappings
				.GroupBy(
					mapping => mapping.TermcapCode,
					StringComparer.Ordinal
				)
				.ToDictionary(
					group => group.Key,
					group =>
						( IReadOnlyList<TermcapStandardCapabilityMapping> )Array.AsReadOnly(
							group.ToArray()
						),
					StringComparer.Ordinal
				);

		return new ReadOnlyDictionary<string, IReadOnlyList<TermcapStandardCapabilityMapping>>(
			dictionary
		);
	}

	private static void ValidateTermcapCode(
		string termcapCode
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( termcapCode );

		if ( termcapCode.Length != 2 ) {
			throw new ArgumentException(
				"A termcap capability code must contain exactly two characters.",
				nameof( termcapCode )
			);
		}
	}

	private readonly struct ObsoleteAliasDefinition
	{
		internal ObsoleteAliasDefinition(
			string aliasCode,
			string canonicalCode,
			string origin
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( aliasCode );
			ArgumentException.ThrowIfNullOrWhiteSpace( canonicalCode );
			ArgumentException.ThrowIfNullOrWhiteSpace( origin );
			if ( aliasCode.Length != 2 ) {
				throw new ArgumentException(
					"An obsolete termcap alias code must contain exactly two characters.",
					nameof( aliasCode )
				);
			}
			if ( canonicalCode.Length != 2 ) {
				throw new ArgumentException(
					"An obsolete termcap alias target must contain exactly two characters.",
					nameof( canonicalCode )
				);
			}

			AliasCode = aliasCode;
			CanonicalCode = canonicalCode;
			Origin = origin;
		}

		internal string AliasCode { get; }
		internal string CanonicalCode { get; }
		internal string Origin { get; }
	}
}
