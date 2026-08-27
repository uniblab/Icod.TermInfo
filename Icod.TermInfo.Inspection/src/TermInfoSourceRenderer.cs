using System.Globalization;
using System.Text;
using Icod.TermInfo.Source;

namespace Icod.TermInfo.Inspection;

/// <summary>
/// Renders unresolved terminfo source entries and documents into a deterministic,
/// normalized source representation without flattening inheritance.
/// </summary>
/// <remarks>
/// <para>
/// Entry and field order are preserved. <c>use=</c> references, cancellation,
/// disabled fields, duplicate declarations, and source identity metadata remain
/// observable in the rendered source.
/// </para>
/// <para>
/// Comments, original whitespace, source spans, and the original spelling of
/// otherwise equivalent numeric and string values are intentionally not
/// preserved. Output uses LF line endings and deterministic wrapping.
/// </para>
/// </remarks>
public static class TermInfoSourceRenderer {
	private const int MaximumLineLength = 80;
	private const string CapabilityIndent = "    ";
	private const string ContinuationIndent = "        ";

	/// <summary>
	/// Renders one unresolved source entry into normalized terminfo source.
	/// </summary>
	/// <param name="entry">The unresolved source entry.</param>
	/// <returns>The normalized source representation.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="entry"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The entry contains a value which was not decoded successfully or otherwise
	/// cannot be represented from the structured Source 1.1 model.
	/// </exception>
	public static string Render(
		TermInfoSourceEntry entry
	) {
		ArgumentNullException.ThrowIfNull( entry );

		StringBuilder builder = new();
		AppendEntry(
			builder,
			entry
		);
		return builder.ToString();
	}

	/// <summary>
	/// Renders an unresolved source document into normalized terminfo source.
	/// </summary>
	/// <param name="document">The unresolved source document.</param>
	/// <returns>The normalized source representation.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="document"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// An entry contains a value which was not decoded successfully or otherwise
	/// cannot be represented from the structured Source 1.1 model.
	/// </exception>
	public static string Render(
		TermInfoSourceDocument document
	) {
		ArgumentNullException.ThrowIfNull( document );

		StringBuilder builder = new();
		AppendDocument(
			builder,
			document
		);
		return builder.ToString();
	}

	/// <summary>
	/// Writes one unresolved source entry as normalized terminfo source.
	/// </summary>
	/// <param name="writer">The destination writer.</param>
	/// <param name="entry">The unresolved source entry.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="writer"/> or <paramref name="entry"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The entry contains a value which was not decoded successfully or otherwise
	/// cannot be represented from the structured Source 1.1 model.
	/// </exception>
	public static void Write(
		TextWriter writer,
		TermInfoSourceEntry entry
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentNullException.ThrowIfNull( entry );

		writer.Write(
			Render( entry )
		);
	}

	/// <summary>
	/// Writes an unresolved source document as normalized terminfo source.
	/// </summary>
	/// <param name="writer">The destination writer.</param>
	/// <param name="document">The unresolved source document.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="writer"/> or <paramref name="document"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// An entry contains a value which was not decoded successfully or otherwise
	/// cannot be represented from the structured Source 1.1 model.
	/// </exception>
	public static void Write(
		TextWriter writer,
		TermInfoSourceDocument document
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentNullException.ThrowIfNull( document );

		writer.Write(
			Render( document )
		);
	}

	private static void AppendDocument(
		StringBuilder builder,
		TermInfoSourceDocument document
	) {
		for ( int index = 0; index < document.Entries.Count; index++ ) {
			if ( index != 0 ) {
				builder.Append( '\n' );
			}

			AppendEntry(
				builder,
				document.Entries[ index ]
			);
		}
	}

	private static void AppendEntry(
		StringBuilder builder,
		TermInfoSourceEntry entry
	) {
		AppendHeader(
			builder,
			entry
		);

		foreach ( TermInfoSourceField field in entry.Fields ) {
			AppendField(
				builder,
				field
			);
		}
	}

	private static void AppendHeader(
		StringBuilder builder,
		TermInfoSourceEntry entry
	) {
		builder.Append( entry.CanonicalName );

		string? description =
			entry.Description;
		int aliasesToWrite =
			entry.Aliases.Count;

		if ( description is null ) {
			if ( aliasesToWrite != 0 ) {
				throw new InvalidOperationException(
					$"Source entry '{entry.CanonicalName}' has aliases but no description, which cannot be reconstructed by the Source 1.1 header grammar."
				);
			}
		}
		else if ( description.Length != 0
			&& !description.Any( char.IsWhiteSpace ) ) {
			if ( aliasesToWrite == 0
				|| !string.Equals(
					entry.Aliases[ aliasesToWrite - 1 ],
					description,
					StringComparison.Ordinal
				) ) {
				throw new InvalidOperationException(
					$"Source entry '{entry.CanonicalName}' has a one-component description which is not represented by the Source 1.1 dual alias/description header form."
				);
			}

			aliasesToWrite--;
		}

		for ( int index = 0; index < aliasesToWrite; index++ ) {
			builder.Append( '|' );
			builder.Append( entry.Aliases[ index ] );
		}

		if ( description is not null ) {
			builder.Append( '|' );
			builder.Append( description );
		}

		builder.Append( ',' );
		builder.Append( '\n' );
	}

	private static void AppendField(
		StringBuilder builder,
		TermInfoSourceField field
	) {
		switch ( field.Kind ) {
			case TermInfoSourceFieldKind.BooleanCapability:
				AppendBooleanField(
					builder,
					GetCapabilityName( field )
				);
				break;

			case TermInfoSourceFieldKind.NumericCapability:
				AppendNumericField(
					builder,
					GetCapabilityName( field ),
					GetNumericValue( field )
				);
				break;

			case TermInfoSourceFieldKind.StringCapability:
				AppendStringField(
					builder,
					GetCapabilityName( field ),
					GetStringValue( field )
				);
				break;

			case TermInfoSourceFieldKind.CancelledCapability:
				AppendSimpleField(
					builder,
					GetCapabilityName( field )
						+ "@"
				);
				break;

			case TermInfoSourceFieldKind.UseReference:
				AppendUseField(
					builder,
					field
				);
				break;

			case TermInfoSourceFieldKind.DisabledCapability:
				AppendSimpleField(
					builder,
					"."
						+ GetCapabilityName( field )
				);
				break;

			default:
				throw new InvalidOperationException(
					$"Unsupported source field kind '{field.Kind}'."
				);
		}
	}

	private static void AppendBooleanField(
		StringBuilder builder,
		string name
	) {
		AppendSimpleField(
			builder,
			name
		);
	}

	private static void AppendNumericField(
		StringBuilder builder,
		string name,
		int value
	) {
		AppendSimpleField(
			builder,
			name
				+ "#"
				+ value.ToString( CultureInfo.InvariantCulture )
		);
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

	private static void AppendUseField(
		StringBuilder builder,
		TermInfoSourceField field
	) {
		string referenceName =
			field.ReferenceName
			?? throw new InvalidOperationException(
				$"A use= field in source entry at offset {field.Span.Offset} has no reference name."
			);

		AppendSimpleField(
			builder,
			"use="
				+ referenceName
		);
	}

	private static void AppendSimpleField(
		StringBuilder builder,
		string fieldText
	) {
		builder.Append( CapabilityIndent );
		builder.Append( fieldText );
		builder.Append( ',' );
		builder.Append( '\n' );
	}

	private static string GetCapabilityName(
		TermInfoSourceField field
	) {
		string? name =
			field.CapabilityName;
		if ( string.IsNullOrWhiteSpace( name ) ) {
			throw new InvalidOperationException(
				$"Source field '{field.Kind}' at offset {field.Span.Offset} has no capability name."
			);
		}

		return name;
	}

	private static int GetNumericValue(
		TermInfoSourceField field
	) {
		return field.NumericValue
			?? throw new InvalidOperationException(
				$"Numeric source capability '{GetCapabilityName( field )}' has no decoded value and cannot be normalized."
			);
	}

	private static string GetStringValue(
		TermInfoSourceField field
	) {
		return field.StringValue
			?? throw new InvalidOperationException(
				$"String source capability '{GetCapabilityName( field )}' has no decoded value and cannot be normalized."
			);
	}

	private static string EncodeStringCharacter(
		string capabilityName,
		char value
	) {
		if ( value == '\0' ) {
			throw new InvalidOperationException(
				$"String source capability '{capabilityName}' contains an embedded NUL and cannot be normalized losslessly."
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
			<= '\xff' => EncodeOctal( value ),
			_ => value.ToString(),
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
}
