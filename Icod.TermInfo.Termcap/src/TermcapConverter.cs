using Icod.TermInfo;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Converts resolved termcap source semantics into the canonical immutable
/// Runtime terminal-description model.
/// </summary>
public static class TermcapConverter
{
	/// <summary>
	/// Converts one TC03-resolved termcap entry into a Runtime description.
	/// </summary>
	/// <remarks>
	/// Conversion never performs source acquisition or inheritance resolution.
	/// Non-exact decisions and conversion failures are returned as structured
	/// diagnostics rather than being silently hidden.
	/// </remarks>
	public static TermcapConversionResult Convert(
		TermcapSourceResolvedEntry entry
	) {
		ArgumentNullException.ThrowIfNull( entry );

		List<TermcapConversionDiagnostic> diagnostics = [];
		TerminalDescriptionBuilder builder =
			CreateBuilder(
				entry.SourceEntry,
				diagnostics
			);
		HashSet<BooleanCapability> booleanCapabilities = new();
		HashSet<NumericCapability> numericCapabilities = new();
		HashSet<StringCapability> stringCapabilities = new();

		foreach ( TermcapSourceResolvedField resolvedField in entry.Fields ) {
			ConvertField(
				builder,
				resolvedField,
				booleanCapabilities,
				numericCapabilities,
				stringCapabilities,
				diagnostics
			);
		}

		bool hasErrors =
			diagnostics.Any(
				diagnostic =>
					diagnostic.Severity == TermcapConversionDiagnosticSeverity.Error
			);
		TerminalDescription? description =
			hasErrors
				? null
				: builder.Build()
		;

		return new TermcapConversionResult(
			description,
			diagnostics
		);
	}

	private static TerminalDescriptionBuilder CreateBuilder(
		TermcapSourceEntry entry,
		ICollection<TermcapConversionDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( entry );
		ArgumentNullException.ThrowIfNull( diagnostics );

		string canonicalName = entry.Names[0];
		TerminalDescriptionBuilder builder =
			new( canonicalName );
		HashSet<string> aliases =
			new( StringComparer.Ordinal );

		for ( int index = 1; index < entry.Names.Count; index++ ) {
			string name = entry.Names[index];
			bool isFinal = index == entry.Names.Count - 1;
			if ( isFinal && name.Any( char.IsWhiteSpace ) ) {
				builder.SetDescription( name );
				continue;
			}
			if (
				string.Equals(
					canonicalName,
					name,
					StringComparison.Ordinal
				)
				|| !aliases.Add( name )
			) {
				AddDiagnostic(
					diagnostics,
					TermcapConversionDiagnosticCodes.DuplicateTerminalName,
					TermcapConversionDiagnosticSeverity.Warning,
					TermcapConversionDecision.Approximation,
					$"Duplicate terminal header identity '{name}' was ignored.",
					entry,
					null
				);
				continue;
			}

			builder.AddAlias( name );
		}

		return builder;
	}

	private static void ConvertField(
		TerminalDescriptionBuilder builder,
		TermcapSourceResolvedField resolvedField,
		ISet<BooleanCapability> booleanCapabilities,
		ISet<NumericCapability> numericCapabilities,
		ISet<StringCapability> stringCapabilities,
		ICollection<TermcapConversionDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentNullException.ThrowIfNull( resolvedField );
		ArgumentNullException.ThrowIfNull( booleanCapabilities );
		ArgumentNullException.ThrowIfNull( numericCapabilities );
		ArgumentNullException.ThrowIfNull( stringCapabilities );
		ArgumentNullException.ThrowIfNull( diagnostics );

		TermcapSourceField field = resolvedField.SourceField;
		TermcapCapabilityClassificationResult classification =
			TermcapCapabilityClassifier.Classify( field );
		if ( classification.Classification == TermcapCapabilityClassification.Reference ) {
			AddDiagnostic(
				diagnostics,
				TermcapConversionDiagnosticCodes.InvalidResolvedFieldKind,
				TermcapConversionDiagnosticSeverity.Error,
				TermcapConversionDecision.Unsupported,
				"A resolved effective field set cannot contain a tc= reference.",
				resolvedField.SourceEntry,
				field
			);
			return;
		}
		if ( classification.Classification == TermcapCapabilityClassification.Ambiguous ) {
			AddDiagnostic(
				diagnostics,
				TermcapConversionDiagnosticCodes.AmbiguousCapability,
				TermcapConversionDiagnosticSeverity.Error,
				TermcapConversionDecision.Unsupported,
				$"Termcap capability '{field.CapabilityName}' has multiple adopted semantic mappings.",
				resolvedField.SourceEntry,
				field
			);
			return;
		}
		if ( classification.Classification == TermcapCapabilityClassification.Unmapped ) {
			ConvertExtendedField(
				builder,
				resolvedField,
				diagnostics
			);
			return;
		}
		if ( classification.HasValueKindMismatch ) {
			AddDiagnostic(
				diagnostics,
				TermcapConversionDiagnosticCodes.ValueKindMismatch,
				TermcapConversionDiagnosticSeverity.Error,
				TermcapConversionDecision.Unrepresentable,
				$"Termcap capability '{field.CapabilityName}' uses "
				+ $"{classification.SourceValueKind} syntax but maps to "
				+ $"{classification.ExpectedValueKind} Runtime data.",
				resolvedField.SourceEntry,
				field
			);
			return;
		}

		TermcapStandardCapabilityMapping? mapping = classification.Mapping;
		if ( mapping is null ) {
			AddDiagnostic(
				diagnostics,
				TermcapConversionDiagnosticCodes.AmbiguousCapability,
				TermcapConversionDiagnosticSeverity.Error,
				TermcapConversionDecision.Unsupported,
				$"Termcap capability '{field.CapabilityName}' does not have one selected Runtime mapping.",
				resolvedField.SourceEntry,
				field
			);
			return;
		}

		if ( mapping.IsObsoleteAlias ) {
			AddDiagnostic(
				diagnostics,
				TermcapConversionDiagnosticCodes.HistoricalAlias,
				TermcapConversionDiagnosticSeverity.Information,
				TermcapConversionDecision.HistoricalAlias,
				$"Historical {mapping.AliasOrigin} termcap alias '{mapping.TermcapCode}' maps to canonical code '{mapping.CanonicalTermcapCode}'.",
				resolvedField.SourceEntry,
				field
			);
		}

		switch ( mapping.ValueKind ) {
			case TermInfoCapabilityValueKind.Boolean:
				ApplyBoolean(
					builder,
					resolvedField,
					mapping,
					booleanCapabilities,
					diagnostics
				);
				break;

			case TermInfoCapabilityValueKind.Number:
				ApplyNumber(
					builder,
					resolvedField,
					mapping,
					numericCapabilities,
					diagnostics
				);
				break;

			case TermInfoCapabilityValueKind.String:
				ApplyString(
					builder,
					resolvedField,
					mapping,
					stringCapabilities,
					diagnostics
				);
				break;

			default:
				throw new ArgumentOutOfRangeException(
					nameof( mapping.ValueKind ),
					mapping.ValueKind,
					"The mapped Runtime capability value kind is not supported."
				);
		}
	}

	private static void ApplyBoolean(
		TerminalDescriptionBuilder builder,
		TermcapSourceResolvedField resolvedField,
		TermcapStandardCapabilityMapping mapping,
		ISet<BooleanCapability> claimed,
		ICollection<TermcapConversionDiagnostic> diagnostics
	) {
		BooleanCapability capability =
			mapping.BooleanCapability
			?? throw new InvalidOperationException(
				"A Boolean mapping does not identify a Boolean Runtime capability."
			);
		if (
			!TryClaim(
				claimed,
				capability,
				mapping,
				resolvedField,
				diagnostics
			)
		) {
			return;
		}

		builder.SetBoolean( capability );
	}

	private static void ApplyNumber(
		TerminalDescriptionBuilder builder,
		TermcapSourceResolvedField resolvedField,
		TermcapStandardCapabilityMapping mapping,
		ISet<NumericCapability> claimed,
		ICollection<TermcapConversionDiagnostic> diagnostics
	) {
		NumericCapability capability =
			mapping.NumericCapability
			?? throw new InvalidOperationException(
				"A numeric mapping does not identify a numeric Runtime capability."
			);
		if (
			!TryClaim(
				claimed,
				capability,
				mapping,
				resolvedField,
				diagnostics
			)
		) {
			return;
		}

		int value =
			resolvedField.SourceField.NumericValue
			?? throw new InvalidOperationException(
				"A resolved numeric field does not contain a numeric value."
			);
		builder.SetNumber(
			capability,
			value
		);
	}

	private static void ApplyString(
		TerminalDescriptionBuilder builder,
		TermcapSourceResolvedField resolvedField,
		TermcapStandardCapabilityMapping mapping,
		ISet<StringCapability> claimed,
		ICollection<TermcapConversionDiagnostic> diagnostics
	) {
		StringCapability capability =
			mapping.StringCapability
			?? throw new InvalidOperationException(
				"A string mapping does not identify a string Runtime capability."
			);
		if (
			!TryClaim(
				claimed,
				capability,
				mapping,
				resolvedField,
				diagnostics
			)
		) {
			return;
		}

		string sourceValue =
			resolvedField.SourceField.StringValue
			?? throw new InvalidOperationException(
				"A resolved string field does not contain a string value."
			);
		bool parameterized =
			TermcapStringConverter.IsParameterizedCapability(
				mapping.CanonicalTermcapCode
			);
		if (
			!parameterized
			&& TermcapStringConverter.ContainsParameterOperator( sourceValue )
		) {
			AddDiagnostic(
				diagnostics,
				TermcapConversionDiagnosticCodes.UnsupportedParameterizedCapability,
				TermcapConversionDiagnosticSeverity.Error,
				TermcapConversionDecision.Unsupported,
				$"Termcap capability '{mapping.CanonicalTermcapCode}' contains parameter operators "
				+ "but is outside TC04's adopted one- and two-numeric-parameter conversion profiles.",
				resolvedField.SourceEntry,
				resolvedField.SourceField
			);
			return;
		}
		if (
			!TermcapStringConverter.TryConvert(
				sourceValue,
				parameterized,
				out string converted,
				out string? error
			)
		) {
			AddDiagnostic(
				diagnostics,
				TermcapConversionDiagnosticCodes.UnsupportedParameterOperator,
				TermcapConversionDiagnosticSeverity.Error,
				TermcapConversionDecision.Unrepresentable,
				error ?? "The termcap string cannot be represented as a Runtime terminfo string.",
				resolvedField.SourceEntry,
				resolvedField.SourceField
			);
			return;
		}

		builder.SetString(
			capability,
			converted
		);
	}

	private static void ConvertExtendedField(
		TerminalDescriptionBuilder builder,
		TermcapSourceResolvedField resolvedField,
		ICollection<TermcapConversionDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentNullException.ThrowIfNull( resolvedField );
		ArgumentNullException.ThrowIfNull( diagnostics );

		TermcapSourceField field = resolvedField.SourceField;
		if ( IsStandardShortName( field.CapabilityName ) ) {
			AddDiagnostic(
				diagnostics,
				TermcapConversionDiagnosticCodes.ExtendedNameCollision,
				TermcapConversionDiagnosticSeverity.Error,
				TermcapConversionDecision.Unsupported,
				$"Unmapped termcap code '{field.CapabilityName}' collides with a standard terminfo short name and cannot be stored as an extended capability.",
				resolvedField.SourceEntry,
				field
			);
			return;
		}

		switch ( field.Kind ) {
			case TermcapSourceFieldKind.BooleanCapability:
				builder.SetExtendedBoolean(
					field.CapabilityName
				);
				break;

			case TermcapSourceFieldKind.NumericCapability:
				builder.SetExtendedNumber(
					field.CapabilityName,
					field.NumericValue
						?? throw new InvalidOperationException(
							"A resolved numeric field does not contain a numeric value."
						)
				);
				break;

			case TermcapSourceFieldKind.StringCapability:
				string sourceValue =
					field.StringValue
					?? throw new InvalidOperationException(
						"A resolved string field does not contain a string value."
					);
				if ( TermcapStringConverter.ContainsParameterOperator( sourceValue ) ) {
					AddDiagnostic(
						diagnostics,
						TermcapConversionDiagnosticCodes.UnsupportedParameterizedCapability,
						TermcapConversionDiagnosticSeverity.Error,
						TermcapConversionDecision.Unsupported,
						$"Unmapped termcap capability '{field.CapabilityName}' contains parameter operators but has no adopted TC04 parameter profile.",
						resolvedField.SourceEntry,
						field
					);
					return;
				}
				if (
					!TermcapStringConverter.TryConvert(
						sourceValue,
						false,
						out string converted,
						out string? error
					)
				) {
					AddDiagnostic(
						diagnostics,
						TermcapConversionDiagnosticCodes.UnsupportedParameterOperator,
						TermcapConversionDiagnosticSeverity.Error,
						TermcapConversionDecision.Unrepresentable,
						error ?? "The extended termcap string cannot be represented as a Runtime terminfo string.",
						resolvedField.SourceEntry,
						field
					);
					return;
				}
				builder.SetExtendedString(
					field.CapabilityName,
					converted
				);
				break;

			case TermcapSourceFieldKind.CancelledCapability:
			case TermcapSourceFieldKind.DisabledCapability:
			case TermcapSourceFieldKind.Reference:
				AddDiagnostic(
					diagnostics,
					TermcapConversionDiagnosticCodes.InvalidResolvedFieldKind,
					TermcapConversionDiagnosticSeverity.Error,
					TermcapConversionDecision.Unsupported,
					$"Resolved field kind '{field.Kind}' cannot be materialized as a Runtime capability.",
					resolvedField.SourceEntry,
					field
				);
				return;

			default:
				throw new ArgumentOutOfRangeException(
					nameof( field.Kind ),
					field.Kind,
					"The termcap source field kind is not supported."
				);
		}

		AddDiagnostic(
			diagnostics,
			TermcapConversionDiagnosticCodes.UnmappedExtendedCapability,
			TermcapConversionDiagnosticSeverity.Information,
			TermcapConversionDecision.Extended,
			$"Unmapped termcap capability '{field.CapabilityName}' was preserved as a Runtime extended capability.",
			resolvedField.SourceEntry,
			field
		);
	}

	private static bool TryClaim<TCapability>(
		ISet<TCapability> claimed,
		TCapability capability,
		TermcapStandardCapabilityMapping mapping,
		TermcapSourceResolvedField resolvedField,
		ICollection<TermcapConversionDiagnostic> diagnostics
	) where TCapability : struct, Enum {
		ArgumentNullException.ThrowIfNull( claimed );
		ArgumentNullException.ThrowIfNull( mapping );
		ArgumentNullException.ThrowIfNull( resolvedField );
		ArgumentNullException.ThrowIfNull( diagnostics );

		if ( claimed.Add( capability ) ) {
			return true;
		}

		AddDiagnostic(
			diagnostics,
			TermcapConversionDiagnosticCodes.DuplicateSemanticCapability,
			TermcapConversionDiagnosticSeverity.Warning,
			TermcapConversionDecision.Approximation,
			$"Termcap capability '{resolvedField.CapabilityName}' maps to Runtime capability "
			+ $"'{mapping.TermInfoShortName}', which was already supplied by a higher-priority "
			+ "source field; the later field was ignored.",
			resolvedField.SourceEntry,
			resolvedField.SourceField
		);
		return false;
	}

	private static bool IsStandardShortName(
		string name
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		return StandardCapabilityCatalog.TryGetBoolean(
				name,
				out _
			)
			|| StandardCapabilityCatalog.TryGetNumeric(
				name,
				out _
			)
			|| StandardCapabilityCatalog.TryGetString(
				name,
				out _
			);
	}

	private static void AddDiagnostic(
		ICollection<TermcapConversionDiagnostic> diagnostics,
		string code,
		TermcapConversionDiagnosticSeverity severity,
		TermcapConversionDecision decision,
		string message,
		TermcapSourceEntry sourceEntry,
		TermcapSourceField? sourceField
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );
		ArgumentException.ThrowIfNullOrWhiteSpace( code );
		ArgumentException.ThrowIfNullOrWhiteSpace( message );
		ArgumentNullException.ThrowIfNull( sourceEntry );

		diagnostics.Add(
			new TermcapConversionDiagnostic(
				code,
				severity,
				decision,
				message,
				sourceEntry,
				sourceField
			)
		);
	}
}
