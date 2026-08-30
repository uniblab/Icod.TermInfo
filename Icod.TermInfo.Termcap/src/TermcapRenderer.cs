using System.Globalization;
using System.Text;
using Icod.TermInfo;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Performs deterministic preflight analysis and lossless reverse rendering of
/// Runtime terminal descriptions into conventional termcap source.
/// </summary>
public static class TermcapRenderer
{
	/// <summary>
	/// Determines whether a Runtime terminal description can be represented by the
	/// adopted TC05 termcap subset without emitting text.
	/// </summary>
	public static TermcapRepresentabilityResult Analyze(
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( description );

		RenderPlan plan = CreatePlan( description );
		return new TermcapRepresentabilityResult(
			plan.Diagnostics
		);
	}

	/// <summary>
	/// Renders a Runtime terminal description as deterministic termcap source after
	/// completing representability preflight.
	/// </summary>
	public static TermcapRenderResult Render(
		TerminalDescription description,
		TermcapRenderOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( description );

		TermcapRenderOptions effectiveOptions =
			options ?? new TermcapRenderOptions();
		RenderPlan plan = CreatePlan( description );
		if ( plan.HasErrors ) {
			return new TermcapRenderResult(
				null,
				plan.Diagnostics
			);
		}

		return new TermcapRenderResult(
			RenderPlanText(
				plan,
				effectiveOptions.MaximumLineLength
			),
			plan.Diagnostics
		);
	}

	private static RenderPlan CreatePlan(
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( description );

		List<TermcapRenderDiagnostic> diagnostics = [];
		string? header =
			CreateHeader(
				description,
				diagnostics
			);
		List<RenderField> fields = [];

		foreach ( BooleanCapability capability in description.BooleanCapabilities ) {
			TermcapStandardCapabilityMapping? mapping =
				FindCanonicalMapping( capability );
			if (
				mapping is null
				|| !IsSelectedMapping( mapping )
			) {
				AddStandardMappingDiagnostic(
					diagnostics,
					capability.ToString(),
					TermInfoCapabilityValueKind.Boolean
				);
				continue;
			}

			fields.Add(
				new RenderField(
					mapping.CanonicalTermcapCode,
					mapping.CanonicalTermcapCode
				)
			);
		}

		foreach (
			KeyValuePair<NumericCapability, int> pair
			in description.NumericCapabilities
		) {
			TermcapStandardCapabilityMapping? mapping =
				FindCanonicalMapping( pair.Key );
			if (
				mapping is null
				|| !IsSelectedMapping( mapping )
			) {
				AddStandardMappingDiagnostic(
					diagnostics,
					pair.Key.ToString(),
					TermInfoCapabilityValueKind.Number
				);
				continue;
			}
			if ( pair.Value < 0 ) {
				AddDiagnostic(
					diagnostics,
					TermcapRenderDiagnosticCodes.NumericValueNotRepresentable,
					$"Runtime numeric capability '{mapping.TermInfoShortName}' has value {pair.Value}, but the adopted termcap numeric grammar is nonnegative.",
					mapping.TermInfoShortName,
					TermInfoCapabilityValueKind.Number
				);
				continue;
			}

			fields.Add(
				new RenderField(
					mapping.CanonicalTermcapCode,
					mapping.CanonicalTermcapCode
						+ "#"
						+ pair.Value.ToString( CultureInfo.InvariantCulture )
				)
			);
		}

		foreach (
			KeyValuePair<StringCapability, string> pair
			in description.StringCapabilities
		) {
			TermcapStandardCapabilityMapping? mapping =
				FindCanonicalMapping( pair.Key );
			if (
				mapping is null
				|| !IsSelectedMapping( mapping )
			) {
				AddStandardMappingDiagnostic(
					diagnostics,
					pair.Key.ToString(),
					TermInfoCapabilityValueKind.String
				);
				continue;
			}

			bool parameterized =
				TermcapStringConverter.IsParameterizedCapability(
					mapping.CanonicalTermcapCode
				);
			if (
				!parameterized
				&& TermcapStringConverter.ContainsParameterOperator( pair.Value )
			) {
				AddDiagnostic(
					diagnostics,
					TermcapRenderDiagnosticCodes.ParameterProgramNotRepresentable,
					$"Runtime string capability '{mapping.TermInfoShortName}' contains parameter operators but its canonical termcap code is outside TC04's adopted parameterized profile set.",
					mapping.TermInfoShortName,
					TermInfoCapabilityValueKind.String
				);
				continue;
			}
			if (
				!TermcapReverseStringConverter.TryConvert(
					pair.Value,
					parameterized,
					out string converted,
					out string? error
				)
			) {
				AddDiagnostic(
					diagnostics,
					parameterized
						? TermcapRenderDiagnosticCodes.ParameterProgramNotRepresentable
						: TermcapRenderDiagnosticCodes.StringValueNotRepresentable,
					error ?? $"Runtime string capability '{mapping.TermInfoShortName}' cannot be represented faithfully as termcap.",
					mapping.TermInfoShortName,
					TermInfoCapabilityValueKind.String
				);
				continue;
			}

			fields.Add(
				new RenderField(
					mapping.CanonicalTermcapCode,
					mapping.CanonicalTermcapCode + "=" + converted
				)
			);
		}

		foreach (
			KeyValuePair<string, TermInfoCapabilityValue> pair
			in description.ExtendedCapabilities.OrderBy(
				item => item.Key,
				StringComparer.Ordinal
			)
		) {
			AppendExtendedField(
				pair.Key,
				pair.Value,
				fields,
				diagnostics
			);
		}

		foreach (
			IGrouping<string, RenderField> collision
			in fields
				.GroupBy(
					field => field.Code,
					StringComparer.Ordinal
				)
				.Where( group => group.Count() > 1 )
				.OrderBy(
					group => group.Key,
					StringComparer.Ordinal
				)
		) {
			AddDiagnostic(
				diagnostics,
				TermcapRenderDiagnosticCodes.StandardCapabilityNotRepresentable,
				$"More than one Runtime capability would render as termcap code '{collision.Key}', but TC03 effective-field resolution can retain only one exact code.",
				collision.Key,
				null
			);
		}

		RenderField[] orderedFields =
			fields
				.OrderBy(
					field => field.Code,
					StringComparer.Ordinal
				)
				.ThenBy(
					field => field.Spelling,
					StringComparer.Ordinal
				)
				.ToArray();
		return new RenderPlan(
			header,
			orderedFields,
			diagnostics
		);
	}

	private static string? CreateHeader(
		TerminalDescription description,
		ICollection<TermcapRenderDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( description );
		ArgumentNullException.ThrowIfNull( diagnostics );

		bool valid = true;
		if ( !IsRepresentableIdentity( description.Name ) ) {
			AddHeaderDiagnostic(
				diagnostics,
				$"Canonical terminal name '{description.Name}' cannot be represented in a conventional termcap header without changing its identity."
			);
			valid = false;
		}

		foreach ( string alias in description.Aliases ) {
			if ( IsRepresentableIdentity( alias ) ) {
				continue;
			}
			AddHeaderDiagnostic(
				diagnostics,
				$"Terminal alias '{alias}' cannot be represented in a conventional termcap header without changing its identity."
			);
			valid = false;
		}

		if (
			description.Description is not null
			&& !IsRepresentableDescription( description.Description )
		) {
			AddHeaderDiagnostic(
				diagnostics,
				"The terminal description cannot be represented as TC04's final whitespace-bearing verbose header component."
			);
			valid = false;
		}
		if ( !valid ) {
			return null;
		}

		StringBuilder builder =
			new( description.Name.Length + 32 );
		builder.Append( description.Name );
		foreach ( string alias in description.Aliases ) {
			builder.Append( '|' );
			builder.Append( alias );
		}
		if ( description.Description is not null ) {
			builder.Append( '|' );
			builder.Append( description.Description );
		}
		builder.Append( ':' );
		return builder.ToString();
	}

	private static bool IsRepresentableIdentity(
		string value
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( value );

		return value.AsSpan().Trim().SequenceEqual( value.AsSpan() )
			&& !value.Any( char.IsWhiteSpace )
			&& value.IndexOf( '|' ) < 0
			&& value.IndexOf( ':' ) < 0
			&& value.IndexOf( '\r' ) < 0
			&& value.IndexOf( '\n' ) < 0
		;
	}

	private static bool IsRepresentableDescription(
		string value
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( value );

		return value.AsSpan().Trim().SequenceEqual( value.AsSpan() )
			&& value.Any( char.IsWhiteSpace )
			&& value.IndexOf( '|' ) < 0
			&& value.IndexOf( ':' ) < 0
			&& value.IndexOf( '\r' ) < 0
			&& value.IndexOf( '\n' ) < 0
		;
	}

	private static void AppendExtendedField(
		string name,
		TermInfoCapabilityValue value,
		ICollection<RenderField> fields,
		ICollection<TermcapRenderDiagnostic> diagnostics
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );
		ArgumentNullException.ThrowIfNull( fields );
		ArgumentNullException.ThrowIfNull( diagnostics );

		if ( !IsRepresentableCapabilityCode( name ) ) {
			AddDiagnostic(
				diagnostics,
				TermcapRenderDiagnosticCodes.ExtendedCapabilityNameNotRepresentable,
				$"Extended Runtime capability '{name}' is not an exact two-character termcap capability code.",
				name,
				value.Kind
			);
			return;
		}
		if (
			string.Equals( name, "tc", StringComparison.Ordinal )
			|| TermcapCapabilityCatalog.GetMappings( name ).Count != 0
		) {
			AddDiagnostic(
				diagnostics,
				TermcapRenderDiagnosticCodes.ExtendedCapabilityCollision,
				$"Extended Runtime capability '{name}' would be interpreted as reserved or standard termcap syntax rather than as an unmapped extended field.",
				name,
				value.Kind
			);
			return;
		}

		switch ( value.Kind ) {
			case TermInfoCapabilityValueKind.Boolean:
				if ( !value.BooleanValue ) {
					AddDiagnostic(
						diagnostics,
						TermcapRenderDiagnosticCodes.StandardCapabilityNotRepresentable,
						$"Extended Boolean Runtime capability '{name}' is false and therefore has no present-field termcap representation.",
						name,
						value.Kind
					);
					return;
				}
				fields.Add(
					new RenderField(
						name,
						name
					)
				);
				break;

			case TermInfoCapabilityValueKind.Number:
				if ( value.NumberValue < 0 ) {
					AddDiagnostic(
						diagnostics,
						TermcapRenderDiagnosticCodes.NumericValueNotRepresentable,
						$"Extended numeric Runtime capability '{name}' has value {value.NumberValue}, but the adopted termcap numeric grammar is nonnegative.",
						name,
						value.Kind
					);
					return;
				}
				fields.Add(
					new RenderField(
						name,
						name
							+ "#"
							+ value.NumberValue.ToString( CultureInfo.InvariantCulture )
					)
				);
				break;

			case TermInfoCapabilityValueKind.String:
				if ( TermcapStringConverter.ContainsParameterOperator( value.StringValue ) ) {
					AddDiagnostic(
						diagnostics,
						TermcapRenderDiagnosticCodes.ParameterProgramNotRepresentable,
						$"Extended Runtime string capability '{name}' contains a classic parameter operator but unmapped TC04 fields have no adopted parameter profile.",
						name,
						value.Kind
					);
					return;
				}
				if (
					!TermcapReverseStringConverter.TryConvert(
						value.StringValue,
						false,
						out string converted,
						out string? error
					)
				) {
					AddDiagnostic(
						diagnostics,
						TermcapRenderDiagnosticCodes.StringValueNotRepresentable,
						error ?? $"Extended Runtime string capability '{name}' cannot be represented faithfully as termcap.",
						name,
						value.Kind
					);
					return;
				}
				fields.Add(
					new RenderField(
						name,
						name + "=" + converted
					)
				);
				break;

			default:
				throw new ArgumentOutOfRangeException(
					nameof( value.Kind ),
					value.Kind,
					"The Runtime extended capability value kind is not supported."
				);
		}
	}

	private static bool IsRepresentableCapabilityCode(
		string name
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		return name.Length == 2
			&& name[0] != '.'
			&& IsTermcapCapabilityCharacter( name[0] )
			&& IsTermcapCapabilityCharacter( name[1] )
		;
	}

	private static bool IsTermcapCapabilityCharacter(
		char value
	) {
		return value >= '!'
			&& value <= '~'
			&& value != ':';
	}

	private static TermcapStandardCapabilityMapping? FindCanonicalMapping(
		BooleanCapability capability
	) {
		return TermcapCapabilityCatalog.Mappings.FirstOrDefault(
			mapping =>
				!mapping.IsObsoleteAlias
				&& mapping.BooleanCapability == capability
		);
	}

	private static TermcapStandardCapabilityMapping? FindCanonicalMapping(
		NumericCapability capability
	) {
		return TermcapCapabilityCatalog.Mappings.FirstOrDefault(
			mapping =>
				!mapping.IsObsoleteAlias
				&& mapping.NumericCapability == capability
		);
	}

	private static TermcapStandardCapabilityMapping? FindCanonicalMapping(
		StringCapability capability
	) {
		return TermcapCapabilityCatalog.Mappings.FirstOrDefault(
			mapping =>
				!mapping.IsObsoleteAlias
				&& mapping.StringCapability == capability
		);
	}

	private static bool IsSelectedMapping(
		TermcapStandardCapabilityMapping mapping
	) {
		ArgumentNullException.ThrowIfNull( mapping );

		if (
			!IsRepresentableCapabilityCode( mapping.CanonicalTermcapCode )
			|| (
				mapping.ValueKind == TermInfoCapabilityValueKind.String
				&& string.Equals(
					mapping.CanonicalTermcapCode,
					"tc",
					StringComparison.Ordinal
				)
			)
		) {
			return false;
		}

		TermcapStandardCapabilityMapping[] sameKind =
			TermcapCapabilityCatalog
				.GetMappings( mapping.CanonicalTermcapCode )
				.Where(
					candidate => candidate.ValueKind == mapping.ValueKind
				)
				.ToArray();
		return sameKind.Length == 1
			&& HasSameIdentity(
				sameKind[0],
				mapping
			);
	}

	private static bool HasSameIdentity(
		TermcapStandardCapabilityMapping left,
		TermcapStandardCapabilityMapping right
	) {
		ArgumentNullException.ThrowIfNull( left );
		ArgumentNullException.ThrowIfNull( right );

		return left.ValueKind == right.ValueKind
			&& left.BooleanCapability == right.BooleanCapability
			&& left.NumericCapability == right.NumericCapability
			&& left.StringCapability == right.StringCapability
		;
	}

	private static string RenderPlanText(
		RenderPlan plan,
		int maximumLineLength
	) {
		ArgumentNullException.ThrowIfNull( plan );
		if ( maximumLineLength < 1 ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumLineLength )
			);
		}
		string header =
			plan.Header
			?? throw new InvalidOperationException(
				"A successful termcap render plan must contain a header."
			);

		StringBuilder builder =
			new( header.Length + 128 );
		builder.Append( header );
		int lineLength = header.Length;
		foreach ( RenderField field in plan.Fields ) {
			string token = field.Spelling + ":";
			if (
				lineLength != 0
				&& lineLength + token.Length + 1 > maximumLineLength
			) {
				builder.Append( '\\' );
				builder.Append( '\n' );
				lineLength = 0;
			}

			builder.Append( token );
			lineLength += token.Length;
		}
		builder.Append( '\n' );
		return builder.ToString();
	}

	private static void AddStandardMappingDiagnostic(
		ICollection<TermcapRenderDiagnostic> diagnostics,
		string capabilityName,
		TermInfoCapabilityValueKind valueKind
	) {
		AddDiagnostic(
			diagnostics,
			TermcapRenderDiagnosticCodes.StandardCapabilityNotRepresentable,
			$"Runtime standard capability '{capabilityName}' has no unambiguous adopted canonical termcap representation.",
			capabilityName,
			valueKind
		);
	}

	private static void AddHeaderDiagnostic(
		ICollection<TermcapRenderDiagnostic> diagnostics,
		string message
	) {
		AddDiagnostic(
			diagnostics,
			TermcapRenderDiagnosticCodes.HeaderNotRepresentable,
			message,
			null,
			null
		);
	}

	private static void AddDiagnostic(
		ICollection<TermcapRenderDiagnostic> diagnostics,
		string code,
		string message,
		string? capabilityName,
		TermInfoCapabilityValueKind? valueKind
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );
		ArgumentException.ThrowIfNullOrWhiteSpace( code );
		ArgumentException.ThrowIfNullOrWhiteSpace( message );

		diagnostics.Add(
			new TermcapRenderDiagnostic(
				code,
				TermcapRenderDiagnosticSeverity.Error,
				message,
				capabilityName,
				valueKind
			)
		);
	}

	private sealed class RenderPlan
	{
		internal RenderPlan(
			string? header,
			IReadOnlyList<RenderField> fields,
			IEnumerable<TermcapRenderDiagnostic> diagnostics
		) {
			ArgumentNullException.ThrowIfNull( fields );
			ArgumentNullException.ThrowIfNull( diagnostics );

			Header = header;
			Fields = fields;
			Diagnostics = diagnostics.ToArray();
			HasErrors =
				Diagnostics.Any(
					diagnostic =>
						diagnostic.Severity == TermcapRenderDiagnosticSeverity.Error
				);
		}

		internal string? Header { get; }
		internal IReadOnlyList<RenderField> Fields { get; }
		internal IReadOnlyList<TermcapRenderDiagnostic> Diagnostics { get; }
		internal bool HasErrors { get; }
	}

	private readonly record struct RenderField(
		string Code,
		string Spelling
	);
}
