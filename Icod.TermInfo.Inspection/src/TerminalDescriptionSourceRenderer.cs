using System.Globalization;
using System.Text;
using Icod.TermInfo;

namespace Icod.TermInfo.Inspection;

/// <summary>
/// Renders an effective <see cref="TerminalDescription"/> as deterministic
/// terminfo source.
/// </summary>
/// <remarks>
/// <para>
/// The renderer operates on effective terminal state. It does not reconstruct
/// source-only <c>use=</c> inheritance, cancellations, disabled fields,
/// comments, or source locations.
/// </para>
/// <para>
/// Standard capabilities are emitted in canonical compiled-table order.
/// Extended capabilities are emitted by value kind and then by ordinal,
/// case-sensitive name. Output uses LF line endings and deterministic wrapping.
/// </para>
/// </remarks>
public static partial class TerminalDescriptionSourceRenderer {
	private const int MaximumLineLength = 80;
	private const string CapabilityIndent = "    ";
	private const string ContinuationIndent = "        ";

	/// <summary>
	/// Renders one effective terminal description into canonical terminfo source.
	/// </summary>
	/// <param name="description">The effective terminal description.</param>
	/// <returns>The deterministic source representation.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="description"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The effective description contains identity or capability state which the
	/// frozen Source 1.1 grammar cannot represent losslessly.
	/// </exception>
	public static string Render(
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( description );

		return RenderCore( description );
	}

	/// <summary>
	/// Writes one effective terminal description as canonical terminfo source.
	/// </summary>
	/// <param name="writer">The destination writer.</param>
	/// <param name="description">The effective terminal description.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="writer"/> or <paramref name="description"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The effective description contains identity or capability state which the
	/// frozen Source 1.1 grammar cannot represent losslessly.
	/// </exception>
	public static void Write(
		TextWriter writer,
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentNullException.ThrowIfNull( description );

		writer.Write(
			RenderCore( description )
		);
	}

	private static string RenderCore(
		TerminalDescription description
	) {
		ValidateIdentity( description );

		StringBuilder builder = new();
		AppendHeader(
			builder,
			description
		);

		foreach (
			StandardCapabilityMetadata<BooleanCapability> metadata
			in StandardCapabilityCatalog.BooleanCapabilities
		) {
			if ( description.GetBoolean( metadata.Capability ) ) {
				AppendBooleanField(
					builder,
					metadata.ShortName
				);
			}
		}

		foreach (
			StandardCapabilityMetadata<NumericCapability> metadata
			in StandardCapabilityCatalog.NumericCapabilities
		) {
			int? value =
				description.GetNumber( metadata.Capability );
			if ( value.HasValue ) {
				AppendNumericField(
					builder,
					metadata.ShortName,
					value.Value
				);
			}
		}

		foreach (
			StandardCapabilityMetadata<StringCapability> metadata
			in StandardCapabilityCatalog.StringCapabilities
		) {
			string? value =
				description.GetString( metadata.Capability );
			if ( value is not null ) {
				AppendStringField(
					builder,
					metadata.ShortName,
					value
				);
			}
		}

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
						AppendBooleanField(
							builder,
							pair.Key
						);
					}
					break;

				case TermInfoCapabilityValueKind.Number:
					AppendNumericField(
						builder,
						pair.Key,
						pair.Value.NumberValue
					);
					break;

				case TermInfoCapabilityValueKind.String:
					AppendStringField(
						builder,
						pair.Key,
						pair.Value.StringValue
					);
					break;

				default:
					throw new InvalidOperationException(
						$"Extended capability '{pair.Key}' has unsupported value kind '{pair.Value.Kind}'."
					);
			}
		}

		return builder.ToString();
	}

	private static void AppendHeader(
		StringBuilder builder,
		TerminalDescription description
	) {
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
		builder.Append( '\n' );
	}

	private static void AppendBooleanField(
		StringBuilder builder,
		string name
	) {
		builder.Append( CapabilityIndent );
		builder.Append( name );
		builder.Append( ',' );
		builder.Append( '\n' );
	}

	private static void AppendNumericField(
		StringBuilder builder,
		string name,
		int value
	) {
		if ( value < 0 ) {
			throw new InvalidOperationException(
				$"Numeric capability '{name}' has value {value}, which the frozen Source 1.1 grammar cannot represent losslessly."
			);
		}

		builder.Append( CapabilityIndent );
		builder.Append( name );
		builder.Append( '#' );
		builder.Append(
			value.ToString( CultureInfo.InvariantCulture )
		);
		builder.Append( ',' );
		builder.Append( '\n' );
	}

	private static void AppendStringField(
		StringBuilder builder,
		string name,
		string value
	) {
		string prefix =
			CapabilityIndent
			+ name
			+ "=";
		builder.Append( prefix );

		int lineLength =
			prefix.Length;

		foreach ( char valueCharacter in value ) {
			string encoded =
				EncodeStringCharacter(
					name,
					valueCharacter
				);

			if ( lineLength + encoded.Length + 1 > MaximumLineLength
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
		builder.Append( '\n' );
	}

	private static string EncodeStringCharacter(
		string capabilityName,
		char value
	) {
		if ( value == '\0' ) {
			throw new InvalidOperationException(
				$"String capability '{capabilityName}' contains an embedded NUL, which terminfo source cannot represent losslessly."
			);
		}

		if ( value > '\xff' ) {
			throw new InvalidOperationException(
				$"String capability '{capabilityName}' contains U+{(int)value:X4}, which is outside the Latin-1 byte semantics of terminfo source."
			);
		}

		return value switch {
			'\x1b' => "\\E",
			'\a' => "\\a",
			'\n' => "\\n",
			'\r' => "\\r",
			'\t' => "\\t",
			'\b' => "\\b",
			'\f' => "\\f",
			' ' => "\\s",
			'\\' => "\\\\",
			',' => "\\,",
			'^' => "\\^",
			':' => "\\:",
			'|' => "\\|",
			>= '\x21' and <= '\x7e' => value.ToString(),
			_ => EncodeOctal( value ),
		};
	}

	private static string EncodeOctal(
		char value
	) {
		int number =
			value;

		char[] characters = [
			'\\',
			(char)( '0' + ( ( number >> 6 ) & 0x07 ) ),
			(char)( '0' + ( ( number >> 3 ) & 0x07 ) ),
			(char)( '0' + ( number & 0x07 ) ),
		];
		return new string( characters );
	}

	private static int GetExtendedKindOrder(
		TermInfoCapabilityValue value
	) {
		return value.Kind switch {
			TermInfoCapabilityValueKind.Boolean => 0,
			TermInfoCapabilityValueKind.Number => 1,
			TermInfoCapabilityValueKind.String => 2,
			_ => throw new InvalidOperationException(
				$"Unsupported extended capability value kind '{value.Kind}'."
			),
		};
	}

	private static void ValidateIdentity(
		TerminalDescription description
	) {
		ValidateHeaderName(
			description.Name,
			"canonical name"
		);

		foreach ( string alias in description.Aliases ) {
			ValidateHeaderName(
				alias,
				"alias"
			);
		}

		string? verboseDescription =
			description.Description;
		if ( verboseDescription is null ) {
			if ( description.Aliases.Count != 0 ) {
				throw new InvalidOperationException(
					"A TerminalDescription with aliases but no verbose description cannot be represented losslessly by the frozen Source 1.1 header grammar."
				);
			}

			return;
		}

		if ( !verboseDescription.Any( char.IsWhiteSpace ) ) {
			throw new InvalidOperationException(
				"A one-component verbose description without whitespace would be interpreted as both an alias and description by the frozen Source 1.1 header grammar."
			);
		}

		foreach ( char character in verboseDescription ) {
			if ( character == '|'
				|| character == ','
				|| character == '\0'
				|| character == '\r'
				|| character == '\n' ) {
				throw new InvalidOperationException(
					"The terminal verbose description contains a character which cannot be represented losslessly in a terminfo source header."
				);
			}
		}

		if ( HasOddTrailingBackslashRun( verboseDescription ) ) {
			throw new InvalidOperationException(
				"The terminal verbose description ends with an unpaired backslash which would escape the source field terminator."
			);
		}
	}

	private static void ValidateHeaderName(
		string name,
		string role
	) {
		foreach ( char character in name ) {
			if ( char.IsWhiteSpace( character )
				|| char.IsControl( character )
				|| character == '|'
				|| character == ',' ) {
				throw new InvalidOperationException(
					$"The terminal {role} '{name}' contains a character which cannot be represented losslessly in a terminfo source header."
				);
			}
		}

		if ( HasOddTrailingBackslashRun( name ) ) {
			throw new InvalidOperationException(
				$"The terminal {role} '{name}' ends with an unpaired backslash which would escape the source header separator."
			);
		}
	}

	private static void ValidateExtendedCapabilityName(
		string name
	) {
		if ( string.IsNullOrWhiteSpace( name )
			|| name[ 0 ] == '.' ) {
			throw new InvalidOperationException(
				$"Extended capability name '{name}' cannot be represented as an enabled terminfo source field."
			);
		}

		foreach ( char character in name ) {
			if ( char.IsWhiteSpace( character )
				|| char.IsControl( character )
				|| character == ','
				|| character == '#'
				|| character == '='
				|| character == '@'
				|| character == '\\' ) {
				throw new InvalidOperationException(
					$"Extended capability name '{name}' contains a source-significant character which cannot be represented losslessly."
				);
			}
		}
	}

	private static bool HasOddTrailingBackslashRun(
		string value
	) {
		int count = 0;
		for ( int index = value.Length - 1;
			index >= 0 && value[ index ] == '\\';
			index-- ) {
			count++;
		}

		return ( count & 1 ) != 0;
	}
}
