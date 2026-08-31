using Icod.TermInfo;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Classifies unresolved termcap fields against the canonical Runtime standard
/// capability metadata without performing conversion or inheritance resolution.
/// </summary>
public static class TermcapCapabilityClassifier
{
	/// <summary>
	/// Classifies one unresolved termcap source field.
	/// </summary>
	/// <param name="field">The parsed termcap source field.</param>
	/// <returns>The semantic mapping and source-value-kind assessment.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="field"/> is <see langword="null"/>.
	/// </exception>
	public static TermcapCapabilityClassificationResult Classify(
		TermcapSourceField field
	) {
		ArgumentNullException.ThrowIfNull( field );

		if ( field.Kind == TermcapSourceFieldKind.Reference ) {
			return new TermcapCapabilityClassificationResult(
				field,
				TermcapCapabilityClassification.Reference,
				Array.Empty<TermcapStandardCapabilityMapping>(),
				null,
				null
			);
		}

		IReadOnlyList<TermcapStandardCapabilityMapping> mappings =
			TermcapCapabilityCatalog.GetMappings(
				field.CapabilityName
			);
		TermInfoCapabilityValueKind? sourceValueKind =
			GetSourceValueKind(
				field.Kind
			);

		if ( mappings.Count == 0 ) {
			return new TermcapCapabilityClassificationResult(
				field,
				TermcapCapabilityClassification.Unmapped,
				mappings,
				null,
				sourceValueKind
			);
		}

		TermcapStandardCapabilityMapping? mapping =
			SelectMapping(
				mappings,
				sourceValueKind
			);
		if ( mapping is null ) {
			return new TermcapCapabilityClassificationResult(
				field,
				TermcapCapabilityClassification.Ambiguous,
				mappings,
				null,
				sourceValueKind
			);
		}

		TermcapCapabilityClassification classification;
		if ( mapping.IsObsoleteAlias ) {
			classification =
				TermcapCapabilityClassification.ObsoleteAlias;
		}
		else if ( mapping.IsObsoleteStandard ) {
			classification =
				TermcapCapabilityClassification.ObsoleteStandard;
		}
		else {
			classification =
				TermcapCapabilityClassification.Standard;
		}

		return new TermcapCapabilityClassificationResult(
			field,
			classification,
			mappings,
			mapping,
			sourceValueKind
		);
	}

	private static TermcapStandardCapabilityMapping? SelectMapping(
		IReadOnlyList<TermcapStandardCapabilityMapping> mappings,
		TermInfoCapabilityValueKind? sourceValueKind
	) {
		ArgumentNullException.ThrowIfNull( mappings );

		if ( mappings.Count == 1 ) {
			return mappings[0];
		}
		if ( !sourceValueKind.HasValue ) {
			return null;
		}

		TermcapStandardCapabilityMapping? selected = null;
		foreach ( TermcapStandardCapabilityMapping mapping in mappings ) {
			if ( mapping.ValueKind != sourceValueKind.Value ) {
				continue;
			}
			if ( selected is not null ) {
				return null;
			}
			selected = mapping;
		}

		return selected;
	}

	private static TermInfoCapabilityValueKind? GetSourceValueKind(
		TermcapSourceFieldKind fieldKind
	) {
		switch ( fieldKind ) {
			case TermcapSourceFieldKind.BooleanCapability:
				return TermInfoCapabilityValueKind.Boolean;

			case TermcapSourceFieldKind.NumericCapability:
				return TermInfoCapabilityValueKind.Number;

			case TermcapSourceFieldKind.StringCapability:
				return TermInfoCapabilityValueKind.String;

			case TermcapSourceFieldKind.CancelledCapability:
			case TermcapSourceFieldKind.DisabledCapability:
			case TermcapSourceFieldKind.Reference:
				return null;

			default:
				throw new ArgumentOutOfRangeException(
					nameof( fieldKind ),
					fieldKind,
					"The termcap source field kind is not supported."
				);
		}
	}
}
