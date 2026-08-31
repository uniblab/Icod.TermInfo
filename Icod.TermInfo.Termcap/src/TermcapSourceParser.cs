using System.Text;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Parses conventional termcap source into an unresolved source model.
/// </summary>
/// <remarks>
/// TC01 recognizes termcap record structure, two-character capability fields,
/// continuation lines, cancellation, <c>tc=</c> references, numeric values, and
/// historical string escapes. It does not classify capabilities against the
/// terminfo catalog, resolve inheritance, or construct <c>TerminalDescription</c>
/// values.
/// </remarks>
public static class TermcapSourceParser
{
	/// <summary>
	/// Parses complete termcap source text.
	/// </summary>
	/// <param name="source">The complete source text.</param>
	/// <param name="sourceName">Optional caller-supplied source identity.</param>
	/// <param name="options">Optional parser resource limits.</param>
	/// <returns>The parsed unresolved document and diagnostics.</returns>
	public static TermcapSourceParseResult Parse(
		string source,
		string? sourceName = null,
		TermcapSourceParserOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( source );

		TermcapSourceParserOptions effectiveOptions =
			options ?? new TermcapSourceParserOptions();
		if ( source.Length > effectiveOptions.MaximumSourceLength ) {
			return CreateMaximumLengthResult(
				sourceName,
				effectiveOptions.MaximumSourceLength
			);
		}

		return ParseCore(
			source,
			sourceName
		);
	}

	/// <summary>
	/// Reads and parses termcap source text.
	/// </summary>
	/// <param name="reader">The source reader.</param>
	/// <param name="sourceName">Optional caller-supplied source identity.</param>
	/// <param name="options">Optional parser resource limits.</param>
	/// <returns>The parsed unresolved document and diagnostics.</returns>
	public static TermcapSourceParseResult Parse(
		TextReader reader,
		string? sourceName = null,
		TermcapSourceParserOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( reader );

		TermcapSourceParserOptions effectiveOptions =
			options ?? new TermcapSourceParserOptions();
		string? source =
			ReadBounded(
				reader,
				effectiveOptions.MaximumSourceLength
			);
		if ( source is null ) {
			return CreateMaximumLengthResult(
				sourceName,
				effectiveOptions.MaximumSourceLength
			);
		}

		return ParseCore(
			source,
			sourceName
		);
	}

	private static string? ReadBounded(
		TextReader reader,
		int maximumSourceLength
	) {
		ArgumentNullException.ThrowIfNull( reader );
		if ( maximumSourceLength < 1 ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumSourceLength )
			);
		}

		StringBuilder builder =
			new(
				Math.Min(
					maximumSourceLength,
					16 * 1024
				)
			);
		char[] buffer =
			new char[4096];

		while ( true ) {
			int remaining =
				maximumSourceLength - builder.Length;
			int requested =
				Math.Min(
					buffer.Length,
					remaining + 1
				);
			int count =
				reader.Read(
					buffer,
					0,
					requested
				);
			if ( count == 0 ) {
				return builder.ToString();
			}
			if ( count > remaining ) {
				return null;
			}

			builder.Append(
				buffer,
				0,
				count
			);
		}
	}

	private static TermcapSourceParseResult ParseCore(
		string source,
		string? sourceName
	) {
		ArgumentNullException.ThrowIfNull( source );

		List<TermcapSourceEntry> entries = [];
		List<TermcapSourceDiagnostic> diagnostics = [];
		LogicalRecordBuilder? record = null;

		int offset = 0;
		int line = 1;
		while ( offset < source.Length ) {
			int lineStart = offset;
			int lineEnd = offset;
			while (
				lineEnd < source.Length
				&& source[lineEnd] != '\r'
				&& source[lineEnd] != '\n'
			) {
				lineEnd++;
			}

			int newlineLength =
				GetNewlineLength(
					source,
					lineEnd
				);
			ReadOnlySpan<char> physicalLine =
				source.AsSpan(
					lineStart,
					lineEnd - lineStart
				);
			if ( record is null ) {
				if ( IsBlankLine( physicalLine ) ) {
					offset =
						checked( lineEnd + newlineLength );
					line++;
					continue;
				}
				if (
					physicalLine.Length != 0
					&& physicalLine[0] == '#'
				) {
					offset =
						checked( lineEnd + newlineLength );
					line++;
					continue;
				}

				record =
					new LogicalRecordBuilder(
						sourceName
					);
			}

			bool continues =
				newlineLength != 0
				&& physicalLine.Length != 0
				&& physicalLine[^1] == '\\';
			int logicalLength =
				physicalLine.Length - ( continues ? 1 : 0 );
			record.Append(
				physicalLine[..logicalLength],
				lineStart,
				line
			);

			if ( !continues ) {
				ParseRecord(
					record.Build(),
					entries,
					diagnostics
				);
				record = null;
			}

			offset =
				checked( lineEnd + newlineLength );
			if ( newlineLength != 0 ) {
				line++;
			}
		}

		if ( record is not null ) {
			ParseRecord(
				record.Build(),
				entries,
				diagnostics
			);
		}

		TermcapSourceDiagnostic[] orderedDiagnostics =
			diagnostics
				.Select(
					(diagnostic, ordinal) =>
						new
						{
							Diagnostic = diagnostic,
							Ordinal = ordinal,
						}
				)
				.OrderBy(
					item => item.Diagnostic.Span?.Offset
						?? int.MaxValue
				)
				.ThenBy(
					item => item.Diagnostic.Span?.Length
						?? int.MaxValue
				)
				.ThenBy(
					item => item.Ordinal
				)
				.Select(
					item => item.Diagnostic
				)
				.ToArray();

		return new TermcapSourceParseResult(
			new TermcapSourceDocument(
				entries
			),
			orderedDiagnostics
		);
	}

	private static void ParseRecord(
		LogicalRecord record,
		ICollection<TermcapSourceEntry> entries,
		ICollection<TermcapSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( record );
		ArgumentNullException.ThrowIfNull( entries );
		ArgumentNullException.ThrowIfNull( diagnostics );

		if ( string.IsNullOrWhiteSpace( record.Text ) ) {
			return;
		}

		int headerEnd =
			record.Text.IndexOf( ':' );
		if ( headerEnd < 0 ) {
			diagnostics.Add(
				new TermcapSourceDiagnostic(
					TermcapSourceDiagnosticCodes.MissingHeaderTerminator,
					TermcapSourceDiagnosticSeverity.Error,
					"A termcap terminal description must terminate its name list with ':'.",
					record.CreateSpan(
						0,
						record.Text.Length
					)
				)
			);
			return;
		}

		List<string> names =
			ParseNames(
				record,
				headerEnd,
				diagnostics
			);
		if ( names.Count == 0 ) {
			return;
		}

		List<TermcapSourceField> fields = [];
		int fieldStart =
			headerEnd + 1;
		while ( fieldStart <= record.Text.Length ) {
			int fieldEnd =
				record.Text.IndexOf(
					':',
					fieldStart
				);
			bool hasTerminator =
				fieldEnd >= 0;
			if ( !hasTerminator ) {
				fieldEnd = record.Text.Length;
			}

			string fieldText =
				record.Text[
					fieldStart..fieldEnd
				];
			if ( !string.IsNullOrWhiteSpace( fieldText ) ) {
				TermcapSourceField? field =
					ParseField(
						record,
						fieldStart,
						fieldText,
						diagnostics
					);
				if ( field is not null ) {
					fields.Add( field );
				}
			}

			if ( !hasTerminator ) {
				break;
			}
			fieldStart =
				fieldEnd + 1;
		}

		if (
			record.Text.Length != 0
			&& record.Text[^1] != ':'
		) {
			diagnostics.Add(
				new TermcapSourceDiagnostic(
					TermcapSourceDiagnosticCodes.MissingTrailingColon,
					TermcapSourceDiagnosticSeverity.Warning,
					"A conventional termcap terminal description ends with ':'.",
					record.CreateSpan(
						record.Text.Length,
						0
					)
				)
			);
		}

		for ( int index = 0; index < fields.Count; index++ ) {
			if (
				fields[index].Kind == TermcapSourceFieldKind.Reference
				&& index != fields.Count - 1
			) {
				diagnostics.Add(
					new TermcapSourceDiagnostic(
						TermcapSourceDiagnosticCodes.ReferenceMustBeLast,
						TermcapSourceDiagnosticSeverity.Error,
						"The termcap tc= inheritance reference must be the final capability field.",
						fields[index].Span
					)
				);
			}
		}

		entries.Add(
			new TermcapSourceEntry(
				names,
				fields,
				record.CreateSpan(
					0,
					record.Text.Length
				)
			)
		);
	}

	private static List<string> ParseNames(
		LogicalRecord record,
		int headerEnd,
		ICollection<TermcapSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( record );
		ArgumentNullException.ThrowIfNull( diagnostics );
		if ( headerEnd < 0 || headerEnd > record.Text.Length ) {
			throw new ArgumentOutOfRangeException(
				nameof( headerEnd )
			);
		}

		List<string> names = [];
		int componentStart = 0;
		while ( componentStart <= headerEnd ) {
			int separator =
				record.Text.IndexOf(
					'|',
					componentStart,
					headerEnd - componentStart
				);
			int componentEnd =
				( separator < 0 )
					? headerEnd
					: separator
			;
			string name =
				record.Text[
					componentStart..componentEnd
				]
				.Trim();

			if ( name.Length == 0 ) {
				diagnostics.Add(
					new TermcapSourceDiagnostic(
						TermcapSourceDiagnosticCodes.EmptyTerminalName,
						TermcapSourceDiagnosticSeverity.Error,
						"A termcap terminal-name component cannot be empty.",
						record.CreateSpan(
							componentStart,
							componentEnd - componentStart
						)
					)
				);
			}
			else {
				names.Add( name );
			}

			if ( separator < 0 ) {
				break;
			}
			componentStart =
				separator + 1;
		}

		return names;
	}

	private static TermcapSourceField? ParseField(
		LogicalRecord record,
		int fieldStart,
		string fieldText,
		ICollection<TermcapSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( record );
		ArgumentNullException.ThrowIfNull( fieldText );
		ArgumentNullException.ThrowIfNull( diagnostics );

		TermcapSourceSpan span =
			record.CreateSpan(
				fieldStart,
				fieldText.Length
			);
		bool disabled =
			fieldText.Length != 0
				&& fieldText[0] == '.';
		int capabilityOffset =
			disabled
				? 1
				: 0
		;
		if (
			fieldText.Length < capabilityOffset + 2
			|| !IsValidCapabilityCharacter( fieldText[capabilityOffset] )
			|| !IsValidCapabilityCharacter( fieldText[capabilityOffset + 1] )
		) {
			diagnostics.Add(
				new TermcapSourceDiagnostic(
					TermcapSourceDiagnosticCodes.InvalidCapabilityName,
					TermcapSourceDiagnosticSeverity.Error,
					"A termcap capability name must contain exactly two non-separator characters.",
					span
				)
			);
			return null;
		}

		string capabilityName =
			fieldText.Substring(
				capabilityOffset,
				2
			);
		string suffix =
			fieldText[( capabilityOffset + 2 )..];
		if ( disabled ) {
			return new TermcapSourceField(
				TermcapSourceFieldKind.DisabledCapability,
				capabilityName,
				null,
				null,
				null,
				fieldText,
				span
			);
		}
		if ( suffix.Length == 0 ) {
			return new TermcapSourceField(
				TermcapSourceFieldKind.BooleanCapability,
				capabilityName,
				null,
				null,
				null,
				fieldText,
				span
			);
		}
		if ( suffix == "@" ) {
			return new TermcapSourceField(
				TermcapSourceFieldKind.CancelledCapability,
				capabilityName,
				null,
				null,
				null,
				fieldText,
				span
			);
		}
		if ( suffix[0] == '#' ) {
			return ParseNumericField(
				capabilityName,
				fieldText,
				suffix[1..],
				span,
				diagnostics
			);
		}
		if ( suffix[0] == '=' ) {
			if ( capabilityName == "tc" ) {
				string referenceName =
					suffix[1..].Trim();
				if ( referenceName.Length == 0 ) {
					diagnostics.Add(
						new TermcapSourceDiagnostic(
							TermcapSourceDiagnosticCodes.MissingReferenceName,
							TermcapSourceDiagnosticSeverity.Error,
							"A termcap tc= inheritance reference must name another terminal description.",
							span
						)
					);
				}

				return new TermcapSourceField(
					TermcapSourceFieldKind.Reference,
					capabilityName,
					null,
					null,
					referenceName.Length == 0
						? null
						: referenceName,
					fieldText,
					span
				);
			}

			string? value =
				DecodeString(
					record,
					fieldStart + 3,
					suffix[1..],
					diagnostics
				);
			return new TermcapSourceField(
				TermcapSourceFieldKind.StringCapability,
				capabilityName,
				null,
				value,
				null,
				fieldText,
				span
			);
		}

		diagnostics.Add(
			new TermcapSourceDiagnostic(
				TermcapSourceDiagnosticCodes.MalformedCapability,
				TermcapSourceDiagnosticSeverity.Error,
				"A termcap capability must be Boolean, numeric (#), string (=), or canceled (@).",
				span
			)
		);
		return null;
	}

	private static TermcapSourceField ParseNumericField(
		string capabilityName,
		string fieldText,
		string spelling,
		TermcapSourceSpan span,
		ICollection<TermcapSourceDiagnostic> diagnostics
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( capabilityName );
		ArgumentNullException.ThrowIfNull( fieldText );
		ArgumentNullException.ThrowIfNull( spelling );
		ArgumentNullException.ThrowIfNull( span );
		ArgumentNullException.ThrowIfNull( diagnostics );

		int? value = null;
		if ( spelling.Length == 0 ) {
			diagnostics.Add(
				new TermcapSourceDiagnostic(
					TermcapSourceDiagnosticCodes.MissingNumericValue,
					TermcapSourceDiagnosticSeverity.Error,
					"A numeric termcap capability must contain a value after '#'.",
					span
				)
			);
		}
		else {
			int numberBase;
			int digitStart;
			if (
				spelling.Length >= 2
				&& spelling[0] == '0'
				&& ( spelling[1] == 'x' || spelling[1] == 'X' )
			) {
				numberBase = 16;
				digitStart = 2;
			}
			else if ( spelling.Length > 1 && spelling[0] == '0' ) {
				numberBase = 8;
				digitStart = 1;
			}
			else {
				numberBase = 10;
				digitStart = 0;
			}

			if ( digitStart == spelling.Length ) {
				diagnostics.Add(
					new TermcapSourceDiagnostic(
						TermcapSourceDiagnosticCodes.InvalidNumericValue,
						TermcapSourceDiagnosticSeverity.Error,
						"A termcap numeric capability must contain digits after its numeric-base prefix.",
						span
					)
				);
			}
			else {
				int parsed = 0;
				bool invalid = false;
				bool outOfRange = false;
				for ( int index = digitStart; index < spelling.Length; index++ ) {
					int digit =
						GetDigitValue( spelling[index] );
					if ( digit < 0 || digit >= numberBase ) {
						invalid = true;
						break;
					}
					if ( parsed > ( int.MaxValue - digit ) / numberBase ) {
						outOfRange = true;
						break;
					}
					parsed =
						( parsed * numberBase ) + digit;
				}

				if ( invalid ) {
					diagnostics.Add(
						new TermcapSourceDiagnostic(
							TermcapSourceDiagnosticCodes.InvalidNumericValue,
							TermcapSourceDiagnosticSeverity.Error,
							"A termcap numeric capability must use valid decimal, octal, or hexadecimal digits.",
							span
						)
					);
				}
				else if ( outOfRange ) {
					diagnostics.Add(
						new TermcapSourceDiagnostic(
							TermcapSourceDiagnosticCodes.NumericValueOutOfRange,
							TermcapSourceDiagnosticSeverity.Error,
							$"The numeric capability value exceeds the supported signed 32-bit maximum of {int.MaxValue}.",
							span
						)
					);
				}
				else {
					value = parsed;
				}
			}
		}

		return new TermcapSourceField(
			TermcapSourceFieldKind.NumericCapability,
			capabilityName,
			value,
			null,
			null,
			fieldText,
			span
		);
	}

	private static string? DecodeString(
		LogicalRecord record,
		int valueStart,
		string spelling,
		ICollection<TermcapSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( record );
		ArgumentNullException.ThrowIfNull( spelling );
		ArgumentNullException.ThrowIfNull( diagnostics );

		StringBuilder value =
			new( spelling.Length );
		bool hasErrors = false;

		for ( int index = 0; index < spelling.Length; index++ ) {
			char current =
				spelling[index];
			if ( current == '\0' ) {
				diagnostics.Add(
					CreateStringDiagnostic(
						record,
						TermcapSourceDiagnosticCodes.EmbeddedNullCharacter,
						TermcapSourceDiagnosticSeverity.Error,
						"A termcap string cannot contain a literal NUL character.",
						valueStart + index,
						1
					)
				);
				hasErrors = true;
				continue;
			}

			if ( current == '^' ) {
				if ( index + 1 >= spelling.Length ) {
					diagnostics.Add(
						CreateStringDiagnostic(
							record,
							TermcapSourceDiagnosticCodes.IncompleteControlEscape,
							TermcapSourceDiagnosticSeverity.Error,
							"A '^' termcap control escape must be followed by a character.",
							valueStart + index,
							1
						)
					);
					hasErrors = true;
					break;
				}

				char target =
					spelling[++index];
				int controlValue =
					target == '?'
						? 0x7f
						: target & 0x1f;
				if ( controlValue == 0 ) {
					diagnostics.Add(
						CreateStringDiagnostic(
							record,
							TermcapSourceDiagnosticCodes.EmbeddedNullCharacter,
							TermcapSourceDiagnosticSeverity.Error,
							"A termcap string control escape cannot encode NUL.",
							valueStart + index - 1,
							2
						)
					);
					hasErrors = true;
					continue;
				}
				value.Append( (char)controlValue );
				continue;
			}

			if ( current != '\\' ) {
				value.Append( current );
				continue;
			}

			if ( index + 1 >= spelling.Length ) {
				diagnostics.Add(
					CreateStringDiagnostic(
						record,
						TermcapSourceDiagnosticCodes.IncompleteBackslashEscape,
						TermcapSourceDiagnosticSeverity.Error,
						"A termcap backslash escape must be followed by a character.",
						valueStart + index,
						1
					)
				);
				hasErrors = true;
				break;
			}

			char escape =
				spelling[++index];
			if ( escape >= '0' && escape <= '7' ) {
				int number =
					escape - '0';
				int digits = 1;
				while (
					digits < 3
					&& index + 1 < spelling.Length
					&& spelling[index + 1] >= '0'
					&& spelling[index + 1] <= '7'
				) {
					number =
						( number * 8 )
						+ ( spelling[++index] - '0' );
					digits++;
				}

				if ( number == 0 ) {
					diagnostics.Add(
						CreateStringDiagnostic(
							record,
							TermcapSourceDiagnosticCodes.EmbeddedNullCharacter,
							TermcapSourceDiagnosticSeverity.Error,
							"A termcap string octal escape cannot encode NUL.",
							valueStart + index - digits,
							digits + 1
						)
					);
					hasErrors = true;
					continue;
				}
				if ( number > byte.MaxValue ) {
					diagnostics.Add(
						CreateStringDiagnostic(
							record,
							TermcapSourceDiagnosticCodes.OctalEscapeOutOfRange,
							TermcapSourceDiagnosticSeverity.Error,
							"A termcap octal string escape must fit in one byte.",
							valueStart + index - digits,
							digits + 1
						)
					);
					hasErrors = true;
					continue;
				}
				value.Append( (char)number );
				continue;
			}

			char translated;
			bool known = true;
			switch ( escape ) {
				case 'E':
				case 'e':
					translated = '\x1b';
					break;
				case 'a':
					translated = '\a';
					break;
				case 'n':
				case 'l':
					translated = '\n';
					break;
				case 'r':
					translated = '\r';
					break;
				case 't':
					translated = '\t';
					break;
				case 'b':
					translated = '\b';
					break;
				case 'f':
					translated = '\f';
					break;
				case 'v':
					translated = '\v';
					break;
				case 's':
					translated = ' ';
					break;
				case '^':
					translated = '^';
					break;
				case '\\':
					translated = '\\';
					break;
				default:
					translated = escape;
					known = false;
					break;
			}

			if ( !known ) {
				diagnostics.Add(
					CreateStringDiagnostic(
						record,
						TermcapSourceDiagnosticCodes.UnknownStringEscape,
						TermcapSourceDiagnosticSeverity.Warning,
						$"'\\{escape}' is not a defined termcap string escape; the escaped character is retained.",
						valueStart + index - 1,
						2
					)
				);
			}
			value.Append( translated );
		}

		return hasErrors
			? null
			: value.ToString()
		;
	}

	private static TermcapSourceDiagnostic CreateStringDiagnostic(
		LogicalRecord record,
		string code,
		TermcapSourceDiagnosticSeverity severity,
		string message,
		int logicalOffset,
		int length
	) {
		ArgumentNullException.ThrowIfNull( record );
		ArgumentException.ThrowIfNullOrWhiteSpace( code );
		ArgumentNullException.ThrowIfNull( message );

		return new TermcapSourceDiagnostic(
			code,
			severity,
			message,
			record.CreateSpan(
				logicalOffset,
				length
			)
		);
	}

	private static bool IsValidCapabilityCharacter(
		char value
	) {
		return value >= '!'
			&& value <= '~'
			&& value != ':'
		;
	}

	private static int GetDigitValue(
		char value
	) {
		if ( value >= '0' && value <= '9' ) {
			return value - '0';
		}
		if ( value >= 'a' && value <= 'f' ) {
			return value - 'a' + 10;
		}
		if ( value >= 'A' && value <= 'F' ) {
			return value - 'A' + 10;
		}
		return -1;
	}

	private static bool IsBlankLine(
		ReadOnlySpan<char> value
	) {
		for ( int index = 0; index < value.Length; index++ ) {
			if ( !char.IsWhiteSpace( value[index] ) ) {
				return false;
			}
		}
		return true;
	}

	private static int GetNewlineLength(
		string source,
		int lineEnd
	) {
		ArgumentNullException.ThrowIfNull( source );
		if ( lineEnd < 0 || lineEnd > source.Length ) {
			throw new ArgumentOutOfRangeException(
				nameof( lineEnd )
			);
		}
		if ( lineEnd == source.Length ) {
			return 0;
		}
		if (
			source[lineEnd] == '\r'
			&& lineEnd + 1 < source.Length
			&& source[lineEnd + 1] == '\n'
		) {
			return 2;
		}
		return 1;
	}

	private static TermcapSourceParseResult CreateMaximumLengthResult(
		string? sourceName,
		int maximumSourceLength
	) {
		return new TermcapSourceParseResult(
			new TermcapSourceDocument(
				Array.Empty<TermcapSourceEntry>()
			),
			new[]
			{
				new TermcapSourceDiagnostic(
					TermcapSourceDiagnosticCodes.MaximumSourceLengthExceeded,
					TermcapSourceDiagnosticSeverity.Error,
					$"The termcap source exceeds the configured maximum length of {maximumSourceLength} UTF-16 code units.",
					new TermcapSourceSpan(
						sourceName,
						0,
						1,
						1,
						0
					)
				),
			}
		);
	}

	private sealed class LogicalRecordBuilder
	{
		private readonly string? sourceName;
		private readonly StringBuilder text = new();
		private readonly List<LogicalSegment> segments = [];

		public LogicalRecordBuilder(
			string? sourceName
		) {
			this.sourceName = sourceName;
		}

		public void Append(
			ReadOnlySpan<char> value,
			int originalOffset,
			int line
		) {
			if ( originalOffset < 0 ) {
				throw new ArgumentOutOfRangeException(
					nameof( originalOffset )
				);
			}
			if ( line < 1 ) {
				throw new ArgumentOutOfRangeException(
					nameof( line )
				);
			}

			int logicalStart =
				text.Length;
			text.Append( value );
			segments.Add(
				new LogicalSegment(
					logicalStart,
					value.Length,
					originalOffset,
					line,
					1
				)
			);
		}

		public LogicalRecord Build() {
			return new LogicalRecord(
				sourceName,
				text.ToString(),
				segments.ToArray()
			);
		}
	}

	private sealed class LogicalRecord
	{
		private readonly string? sourceName;
		private readonly LogicalSegment[] segments;

		public LogicalRecord(
			string? sourceName,
			string text,
			LogicalSegment[] segments
		) {
			ArgumentNullException.ThrowIfNull( text );
			ArgumentNullException.ThrowIfNull( segments );

			this.sourceName = sourceName;
			Text = text;
			this.segments = segments;
		}

		public string Text { get; }

		public TermcapSourceSpan CreateSpan(
			int logicalOffset,
			int length
		) {
			if (
				logicalOffset < 0
				|| logicalOffset > Text.Length
			) {
				throw new ArgumentOutOfRangeException(
					nameof( logicalOffset )
				);
			}
			if (
				length < 0
				|| length > Text.Length - logicalOffset
			) {
				throw new ArgumentOutOfRangeException(
					nameof( length )
				);
			}

			LogicalPosition start =
				FindPosition(
					logicalOffset
				);
			LogicalPosition end =
				FindPosition(
					checked( logicalOffset + length )
				);
			return new TermcapSourceSpan(
				sourceName,
				start.Offset,
				start.Line,
				start.Column,
				checked( end.Offset - start.Offset )
			);
		}

		private LogicalPosition FindPosition(
			int logicalOffset
		) {
			if (
				logicalOffset < 0
				|| logicalOffset > Text.Length
			) {
				throw new ArgumentOutOfRangeException(
					nameof( logicalOffset )
				);
			}
			if ( segments.Length == 0 ) {
				return new LogicalPosition(
					0,
					1,
					1
				);
			}

			for ( int index = 0; index < segments.Length; index++ ) {
				LogicalSegment segment =
					segments[index];
				int segmentEnd =
					checked( segment.LogicalStart + segment.Length );
				if (
					logicalOffset >= segment.LogicalStart
					&& logicalOffset < segmentEnd
				) {
					int relative =
						logicalOffset - segment.LogicalStart;
					return new LogicalPosition(
						checked( segment.OriginalOffset + relative ),
						segment.Line,
						checked( segment.Column + relative )
					);
				}
			}

			LogicalSegment last =
				segments[^1];
			return new LogicalPosition(
				checked( last.OriginalOffset + last.Length ),
				last.Line,
				checked( last.Column + last.Length )
			);
		}
	}

	private readonly record struct LogicalSegment(
		int LogicalStart,
		int Length,
		int OriginalOffset,
		int Line,
		int Column
	);

	private readonly record struct LogicalPosition(
		int Offset,
		int Line,
		int Column
	);
}
