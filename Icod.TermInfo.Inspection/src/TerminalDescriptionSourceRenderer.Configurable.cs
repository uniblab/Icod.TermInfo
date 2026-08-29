using System.Globalization;
using System.Text;
using Icod.TermInfo;

namespace Icod.TermInfo.Inspection;

public static partial class TerminalDescriptionSourceRenderer {
	/// <summary>
	/// Renders one effective terminal description using explicit deterministic
	/// presentation options.
	/// </summary>
	/// <param name="description">The effective terminal description.</param>
	/// <param name="options">The presentation policy.</param>
	/// <returns>The deterministic source representation.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="description"/> or <paramref name="options"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The effective description contains state which the frozen Source 1.1
	/// grammar cannot represent losslessly.
	/// </exception>
	public static string Render(
		TerminalDescription description,
		TerminalDescriptionSourceRendererOptions options
	) {
		ArgumentNullException.ThrowIfNull( description );
		ArgumentNullException.ThrowIfNull( options );

		if ( IsFrozenCanonicalPolicy( options ) ) {
			return RenderCore( description );
		}

		return RenderConfigured(
			description,
			options
		);
	}

	/// <summary>
	/// Writes one effective terminal description using explicit deterministic
	/// presentation options.
	/// </summary>
	/// <param name="writer">The destination writer.</param>
	/// <param name="description">The effective terminal description.</param>
	/// <param name="options">The presentation policy.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="writer"/>, <paramref name="description"/>, or
	/// <paramref name="options"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The effective description contains state which the frozen Source 1.1
	/// grammar cannot represent losslessly.
	/// </exception>
	public static void Write(
		TextWriter writer,
		TerminalDescription description,
		TerminalDescriptionSourceRendererOptions options
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentNullException.ThrowIfNull( description );
		ArgumentNullException.ThrowIfNull( options );

		writer.Write(
			Render(
				description,
				options
			)
		);
	}

	private static bool IsFrozenCanonicalPolicy(
		TerminalDescriptionSourceRendererOptions options
	) {
		ArgumentNullException.ThrowIfNull( options );

		return options.LineWidth == MaximumLineLength
			&& options.Layout == TerminalDescriptionSourceLayout.Canonical
			&& options.CapabilityOrder
				== TerminalDescriptionSourceCapabilityOrder.Database
			&& options.IncludeExtendedCapabilities;
	}

	private static string RenderConfigured(
		TerminalDescription description,
		TerminalDescriptionSourceRendererOptions options
	) {
		ArgumentNullException.ThrowIfNull( description );
		ArgumentNullException.ThrowIfNull( options );
		ValidateIdentity( description );

		StringBuilder builder = new();
		AppendConfiguredHeader(
			builder,
			description,
			options.Layout
		);

		foreach (
			StandardCapabilityMetadata<BooleanCapability> metadata
			in OrderStandardCapabilities(
				StandardCapabilityCatalog.BooleanCapabilities,
				options.CapabilityOrder
			)
		) {
			if ( description.GetBoolean( metadata.Capability ) ) {
				AppendConfiguredBooleanField(
					builder,
					metadata.ShortName,
					options
				);
			}
		}

		foreach (
			StandardCapabilityMetadata<NumericCapability> metadata
			in OrderStandardCapabilities(
				StandardCapabilityCatalog.NumericCapabilities,
				options.CapabilityOrder
			)
		) {
			int? value =
				description.GetNumber( metadata.Capability );
			if ( value.HasValue ) {
				AppendConfiguredNumericField(
					builder,
					metadata.ShortName,
					value.Value,
					options
				);
			}
		}

		foreach (
			StandardCapabilityMetadata<StringCapability> metadata
			in OrderStandardCapabilities(
				StandardCapabilityCatalog.StringCapabilities,
				options.CapabilityOrder
			)
		) {
			string? value =
				description.GetString( metadata.Capability );
			if ( value is not null ) {
				AppendConfiguredStringField(
					builder,
					metadata.ShortName,
					value,
					options
				);
			}
		}

		if ( options.IncludeExtendedCapabilities ) {
			AppendConfiguredExtendedCapabilities(
				builder,
				description,
				options
			);
		}

		if ( options.Layout == TerminalDescriptionSourceLayout.SingleLine ) {
			builder.Append( '\n' );
		}

		return builder.ToString();
	}

	private static IEnumerable<StandardCapabilityMetadata<TCapability>>
		OrderStandardCapabilities<TCapability>(
			IReadOnlyList<StandardCapabilityMetadata<TCapability>> metadata,
			TerminalDescriptionSourceCapabilityOrder order
		)
		where TCapability : struct, Enum
	{
		ArgumentNullException.ThrowIfNull( metadata );

		return order switch {
			TerminalDescriptionSourceCapabilityOrder.Database =>
				metadata,
			TerminalDescriptionSourceCapabilityOrder.TermInfoName =>
				metadata
					.OrderBy(
						item => item.ShortName,
						StringComparer.Ordinal
					)
					.ThenBy( item => item.BinaryIndex ),
			TerminalDescriptionSourceCapabilityOrder.LongName =>
				metadata
					.OrderBy(
						item => item.LongName,
						StringComparer.Ordinal
					)
					.ThenBy( item => item.BinaryIndex ),
			TerminalDescriptionSourceCapabilityOrder.TermcapCode =>
				metadata
					.OrderBy(
						item => item.TermcapCode,
						StringComparer.Ordinal
					)
					.ThenBy(
						item => item.ShortName,
						StringComparer.Ordinal
					)
					.ThenBy( item => item.BinaryIndex ),
			_ => throw new ArgumentOutOfRangeException(
				nameof( order ),
				order,
				"The capability order is not defined."
			),
		};
	}

	private static void AppendConfiguredHeader(
		StringBuilder builder,
		TerminalDescription description,
		TerminalDescriptionSourceLayout layout
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentNullException.ThrowIfNull( description );

		builder.Append( description.Name );
		foreach ( string alias in description.Aliases ) {
			builder.Append( '|' );
			builder.Append( alias );
		}
		if ( description.Description is string verboseDescription ) {
			builder.Append( '|' );
			builder.Append( verboseDescription );
		}
		builder.Append( ',' );

		if ( layout != TerminalDescriptionSourceLayout.SingleLine ) {
			builder.Append( '\n' );
		}
	}

	private static void AppendConfiguredBooleanField(
		StringBuilder builder,
		string name,
		TerminalDescriptionSourceRendererOptions options
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentException.ThrowIfNullOrWhiteSpace( name );
		ArgumentNullException.ThrowIfNull( options );

		AppendConfiguredFieldPrefix(
			builder,
			options.Layout
		);
		builder.Append( name );
		builder.Append( ',' );
		AppendConfiguredFieldSuffix(
			builder,
			options.Layout
		);
	}

	private static void AppendConfiguredNumericField(
		StringBuilder builder,
		string name,
		int value,
		TerminalDescriptionSourceRendererOptions options
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentException.ThrowIfNullOrWhiteSpace( name );
		ArgumentNullException.ThrowIfNull( options );

		if ( value < 0 ) {
			throw new InvalidOperationException(
				$"Numeric capability '{name}' has value {value}, which the frozen Source 1.1 grammar cannot represent losslessly."
			);
		}

		AppendConfiguredFieldPrefix(
			builder,
			options.Layout
		);
		builder.Append( name );
		builder.Append( '#' );
		builder.Append(
			value.ToString( CultureInfo.InvariantCulture )
		);
		builder.Append( ',' );
		AppendConfiguredFieldSuffix(
			builder,
			options.Layout
		);
	}

	private static void AppendConfiguredStringField(
		StringBuilder builder,
		string name,
		string value,
		TerminalDescriptionSourceRendererOptions options
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentException.ThrowIfNullOrWhiteSpace( name );
		ArgumentNullException.ThrowIfNull( value );
		ArgumentNullException.ThrowIfNull( options );

		AppendConfiguredFieldPrefix(
			builder,
			options.Layout
		);
		builder.Append( name );
		builder.Append( '=' );

		int lineLength =
			(options.Layout == TerminalDescriptionSourceLayout.SingleLine)
				? GetCurrentLineLength( builder )
				: CapabilityIndent.Length + name.Length + 1
		;

		foreach ( char valueCharacter in value ) {
			string encoded =
				EncodeStringCharacter(
					name,
					valueCharacter
				);

			if ( options.Layout == TerminalDescriptionSourceLayout.Canonical
				&& lineLength + encoded.Length + 1 > options.LineWidth
				&& lineLength > ContinuationIndent.Length ) {
				builder.Append( '\n' );
				builder.Append( ContinuationIndent );
				lineLength =
					ContinuationIndent.Length;
			}

			builder.Append( encoded );
			lineLength +=
				encoded.Length;
		}

		builder.Append( ',' );
		AppendConfiguredFieldSuffix(
			builder,
			options.Layout
		);
	}

	private static void AppendConfiguredExtendedCapabilities(
		StringBuilder builder,
		TerminalDescription description,
		TerminalDescriptionSourceRendererOptions options
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentNullException.ThrowIfNull( description );
		ArgumentNullException.ThrowIfNull( options );

		foreach (
			KeyValuePair<string, TermInfoCapabilityValue> pair
			in description.ExtendedCapabilities
				.OrderBy(
					item =>
						GetExtendedKindOrder( item.Value )
				)
				.ThenBy(
					item => item.Key,
					StringComparer.Ordinal
				)
		) {
			ValidateExtendedCapabilityName( pair.Key );

			switch ( pair.Value.Kind ) {
				case TermInfoCapabilityValueKind.Boolean:
					if ( pair.Value.BooleanValue ) {
						AppendConfiguredBooleanField(
							builder,
							pair.Key,
							options
						);
					}
					break;

				case TermInfoCapabilityValueKind.Number:
					AppendConfiguredNumericField(
						builder,
						pair.Key,
						pair.Value.NumberValue,
						options
					);
					break;

				case TermInfoCapabilityValueKind.String:
					AppendConfiguredStringField(
						builder,
						pair.Key,
						pair.Value.StringValue,
						options
					);
					break;

				default:
					throw new InvalidOperationException(
						$"Extended capability '{pair.Key}' has unsupported value kind '{pair.Value.Kind}'."
					);
			}
		}
	}

	private static void AppendConfiguredFieldPrefix(
		StringBuilder builder,
		TerminalDescriptionSourceLayout layout
	) {
		ArgumentNullException.ThrowIfNull( builder );

		if ( layout == TerminalDescriptionSourceLayout.SingleLine ) {
			builder.Append( ' ' );
			return;
		}

		builder.Append( CapabilityIndent );
	}

	private static void AppendConfiguredFieldSuffix(
		StringBuilder builder,
		TerminalDescriptionSourceLayout layout
	) {
		ArgumentNullException.ThrowIfNull( builder );

		if ( layout != TerminalDescriptionSourceLayout.SingleLine ) {
			builder.Append( '\n' );
		}
	}

	private static int GetCurrentLineLength(
		StringBuilder builder
	) {
		ArgumentNullException.ThrowIfNull( builder );

		for ( int index = builder.Length - 1; index >= 0; index-- ) {
			if ( builder[ index ] == '\n' ) {
				return builder.Length - index - 1;
			}
		}

		return builder.Length;
	}
}
