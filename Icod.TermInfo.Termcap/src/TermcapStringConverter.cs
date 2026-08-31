using System.Globalization;
using System.Text;

namespace Icod.TermInfo.Termcap;

internal static class TermcapStringConverter
{
	private static readonly HashSet<string> ParameterizedCapabilityCodes =
		new(
			new string[] {
				"AB",
				"AF",
				"AL",
				"ch",
				"CM",
				"cm",
				"cs",
				"cv",
				"DC",
				"DK",
				"DL",
				"DO",
				"ec",
				"IC",
				"LE",
				"ML",
				"MT",
				"pO",
				"RI",
				"rp",
				"Sb",
				"SF",
				"Sf",
				"sp",
				"SR",
				"ts",
				"UP",
				"WG",
				"Xy",
				"Yw",
				"YZ",
				"Yz",
				"Zc",
				"Zf",
				"Zg",
				"Zh",
				"Zi",
				"Zl",
				"Zm",
				"Zn",
				"Zp",
				"ZY",
			},
			StringComparer.Ordinal
		);

	internal static bool IsParameterizedCapability(
		string termcapCode
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( termcapCode );

		return ParameterizedCapabilityCodes.Contains( termcapCode );
	}

	internal static bool ContainsParameterOperator(
		string source
	) {
		ArgumentNullException.ThrowIfNull( source );

		for ( int index = 0; index + 1 < source.Length; index++ ) {
			if ( source[index] != '%' ) {
				continue;
			}

			char code = source[index + 1];
			if (
				code == 'd'
				|| code == '2'
				|| code == '3'
				|| code == '.'
				|| code == '+'
				|| code == '>'
				|| code == 'r'
				|| code == 'i'
				|| code == 'n'
				|| code == 'B'
				|| code == 'D'
			) {
				return true;
			}
			if (
				code == '0'
				&& index + 2 < source.Length
				&& ( source[index + 2] == '2' || source[index + 2] == '3' )
			) {
				return true;
			}
		}

		return false;
	}

	internal static bool TryConvert(
		string source,
		bool parameterized,
		out string converted,
		out string? error
	) {
		ArgumentNullException.ThrowIfNull( source );

		SplitPadding(
			source,
			out string body,
			out string? padding
		);
		string convertedBody;
		if ( parameterized ) {
			if (
				!TryConvertParameters(
					body,
					out convertedBody,
					out error
				)
			) {
				converted = string.Empty;
				return false;
			}
		} else {
			convertedBody = body;
			error = null;
		}

		converted =
			( padding is null )
				? convertedBody
				: convertedBody + "$<" + padding + "/>"
		;
		return true;
	}

	private static void SplitPadding(
		string source,
		out string body,
		out string? padding
	) {
		ArgumentNullException.ThrowIfNull( source );

		int position = 0;
		while (
			position < source.Length
			&& IsAsciiDigit( source[position] )
		) {
			position++;
		}
		if ( position == 0 ) {
			body = source;
			padding = null;
			return;
		}

		if (
			position + 1 < source.Length
			&& source[position] == '.'
			&& IsAsciiDigit( source[position + 1] )
		) {
			position += 2;
		}

		bool proportional =
			position < source.Length
			&& source[position] == '*';
		int paddingEnd =
			proportional
				? checked( position + 1 )
				: position
		;
		padding = source[..paddingEnd];
		body = source[paddingEnd..];
	}

	private static bool TryConvertParameters(
		string source,
		out string converted,
		out string? error
	) {
		ArgumentNullException.ThrowIfNull( source );

		StringBuilder builder =
			new( source.Length + 16 );
		string[] expressions = [ "%p1", "%p2" ];
		int current = 0;
		int outputCount = 0;

		for ( int position = 0; position < source.Length; position++ ) {
			char value = source[position];
			if ( value != '%' ) {
				builder.Append( value );
				continue;
			}
			if ( position + 1 >= source.Length ) {
				converted = string.Empty;
				error = "A termcap parameter string ends with an incomplete '%' operator.";
				return false;
			}

			char code = source[++position];
			switch ( code ) {
				case '%':
					builder.Append( "%%" );
					break;

				case 'd':
					if (
						!AppendOutput(
							builder,
							expressions,
							ref current,
							ref outputCount,
							"%d",
							out error
						)
					) {
						converted = string.Empty;
						return false;
					}
					break;

				case '2':
					expressions[current] =
						Modulo(
							expressions[current],
							100
						);
					if (
						!AppendOutput(
							builder,
							expressions,
							ref current,
							ref outputCount,
							"%02d",
							out error
						)
					) {
						converted = string.Empty;
						return false;
					}
					break;

				case '3':
					expressions[current] =
						Modulo(
							expressions[current],
							1000
						);
					if (
						!AppendOutput(
							builder,
							expressions,
							ref current,
							ref outputCount,
							"%03d",
							out error
						)
					) {
						converted = string.Empty;
						return false;
					}
					break;

				case '0':
					if (
						position + 1 >= source.Length
						|| ( source[position + 1] != '2' && source[position + 1] != '3' )
					) {
						converted = string.Empty;
						error = "Termcap '%0' is only supported as the '%02' or '%03' compatibility spelling.";
						return false;
					}
					char width = source[++position];
					int modulus =
						( width == '2' )
							? 100
							: 1000
					;
					expressions[current] =
						Modulo(
							expressions[current],
							modulus
						);
					string format =
						( width == '2' )
							? "%02d"
							: "%03d"
					;
					if (
						!AppendOutput(
							builder,
							expressions,
							ref current,
							ref outputCount,
							format,
							out error
						)
					) {
						converted = string.Empty;
						return false;
					}
					break;

				case '.':
					if (
						!AppendOutput(
							builder,
							expressions,
							ref current,
							ref outputCount,
							"%c",
							out error
						)
					) {
						converted = string.Empty;
						return false;
					}
					break;

				case '+':
					if ( position + 1 >= source.Length ) {
						converted = string.Empty;
						error = "Termcap '%+' requires one following character.";
						return false;
					}
					char addend = source[++position];
					if ( addend > byte.MaxValue ) {
						converted = string.Empty;
						error = "Termcap '%+' uses a character outside one-byte termcap semantics.";
						return false;
					}
					expressions[current] =
						expressions[current]
							+ IntegerConstant( addend )
							+ "%+";
					if (
						!AppendOutput(
							builder,
							expressions,
							ref current,
							ref outputCount,
							"%c",
							out error
						)
					) {
						converted = string.Empty;
						return false;
					}
					break;

				case '>':
					if ( position + 2 >= source.Length ) {
						converted = string.Empty;
						error = "Termcap '%>' requires two following characters.";
						return false;
					}
					char threshold = source[++position];
					char increment = source[++position];
					if ( threshold > byte.MaxValue || increment > byte.MaxValue ) {
						converted = string.Empty;
						error = "Termcap '%>' uses a character outside one-byte termcap semantics.";
						return false;
					}
					expressions[current] =
						ConditionalAdd(
							expressions[current],
							threshold,
							increment
						);
					break;

				case 'r':
					( expressions[0], expressions[1] ) =
						( expressions[1], expressions[0] );
					break;

				case 'i':
					expressions[0] = Increment( expressions[0] );
					expressions[1] = Increment( expressions[1] );
					break;

				case 'n':
					expressions[0] = XorWith96( expressions[0] );
					expressions[1] = XorWith96( expressions[1] );
					break;

				case 'B':
					expressions[current] = ToBcd( expressions[current] );
					break;

				case 'D':
					expressions[current] = ToDeltaData( expressions[current] );
					break;

				default:
					converted = string.Empty;
					error = $"Termcap parameter operator '%{code}' is not supported by TC04.";
					return false;
			}
		}

		converted = builder.ToString();
		error = null;
		return true;
	}

	private static bool AppendOutput(
		StringBuilder builder,
		IReadOnlyList<string> expressions,
		ref int current,
		ref int outputCount,
		string format,
		out string? error
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentNullException.ThrowIfNull( expressions );
		ArgumentException.ThrowIfNullOrWhiteSpace( format );

		if ( outputCount >= 2 ) {
			error = "Traditional termcap parameter strings cannot consume more than two output parameters.";
			return false;
		}

		builder.Append( expressions[current] );
		builder.Append( format );
		outputCount++;
		current = 1 - current;
		error = null;
		return true;
	}

	private static string Modulo(
		string expression,
		int modulus
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( expression );
		if ( modulus < 1 ) {
			throw new ArgumentOutOfRangeException(
				nameof( modulus )
			);
		}

		return
			expression
			+ "%{"
			+ modulus.ToString( CultureInfo.InvariantCulture )
			+ "}%m";
	}

	private static string ConditionalAdd(
		string expression,
		char threshold,
		char increment
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( expression );

		return
			"%?"
			+ expression
			+ IntegerConstant( threshold )
			+ "%>%t"
			+ expression
			+ IntegerConstant( increment )
			+ "%+%e"
			+ expression
			+ "%;";
	}

	private static string Increment(
		string expression
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( expression );
		return expression + "%{1}%+";
	}

	private static string XorWith96(
		string expression
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( expression );
		return expression + "%{96}%^";
	}

	private static string ToBcd(
		string expression
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( expression );

		return
			expression
			+ "%{10}%/%{16}%*"
			+ expression
			+ "%{10}%m%+";
	}

	private static string ToDeltaData(
		string expression
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( expression );

		return
			expression
			+ expression
			+ "%{16}%m%{2}%*%-";
	}

	private static string IntegerConstant(
		char value
	) {
		return
			"%{"
			+ ( ( int )value ).ToString( CultureInfo.InvariantCulture )
			+ "}";
	}

	private static bool IsAsciiDigit(
		char value
	) {
		return value >= '0' && value <= '9';
	}
}
