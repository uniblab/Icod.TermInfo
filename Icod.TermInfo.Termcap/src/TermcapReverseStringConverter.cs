using System.Globalization;
using System.Text;

namespace Icod.TermInfo.Termcap;

internal static class TermcapReverseStringConverter
{
	private const int MaximumTransformDepth = 16;

	internal static bool TryConvert(
		string source,
		bool parameterized,
		out string converted,
		out string? error
	) {
		ArgumentNullException.ThrowIfNull( source );

		if (
			!TrySplitDelaySuffix(
				source,
				out string body,
				out string? padding,
				out error
			)
		) {
			converted = string.Empty;
			return false;
		}

		string termcapBody;
		if ( parameterized ) {
			if (
				!TryDecodeParameters(
					body,
					out termcapBody,
					out error
				)
			) {
				converted = string.Empty;
				return false;
			}
		} else {
			termcapBody = body;
		}

		if (
			!TryEscape(
				termcapBody,
				out string escaped,
				out error
			)
		) {
			converted = string.Empty;
			return false;
		}

		converted =
			( padding is null )
				? escaped
				: padding + escaped
		;
		error = null;
		return true;
	}

	private static bool TrySplitDelaySuffix(
		string source,
		out string body,
		out string? padding,
		out string? error
	) {
		ArgumentNullException.ThrowIfNull( source );

		int delayStart =
			source.LastIndexOf(
				"$<",
				StringComparison.Ordinal
			);
		if ( delayStart < 0 ) {
			body = source;
			padding = null;
			error = null;
			return true;
		}

		if (
			delayStart != source.IndexOf( "$<", StringComparison.Ordinal )
			|| !source.EndsWith( "/>", StringComparison.Ordinal )
		) {
			body = string.Empty;
			padding = null;
			error = "Only one terminal mandatory terminfo delay suffix can be represented as traditional leading termcap padding.";
			return false;
		}

		string candidate =
			source.Substring(
				delayStart + 2,
				source.Length - delayStart - 4
			);
		if ( !IsValidPadding( candidate ) ) {
			body = string.Empty;
			padding = null;
			error = "The terminfo delay suffix is outside TC04's traditional termcap padding grammar.";
			return false;
		}

		body = source[..delayStart];
		padding = candidate;
		error = null;
		return true;
	}

	private static bool IsValidPadding(
		string padding
	) {
		ArgumentNullException.ThrowIfNull( padding );

		int position = 0;
		while (
			position < padding.Length
			&& IsAsciiDigit( padding[position] )
		) {
			position++;
		}
		if ( position == 0 ) {
			return false;
		}
		if ( position < padding.Length && padding[position] == '.' ) {
			if (
				position + 1 >= padding.Length
				|| !IsAsciiDigit( padding[position + 1] )
			) {
				return false;
			}
			position += 2;
		}
		if ( position < padding.Length && padding[position] == '*' ) {
			position++;
		}
		return position == padding.Length;
	}

	private static bool TryEscape(
		string source,
		out string escaped,
		out string? error
	) {
		ArgumentNullException.ThrowIfNull( source );

		StringBuilder builder =
			new( source.Length + 16 );
		foreach ( char value in source ) {
			switch ( value ) {
				case '\0':
					escaped = string.Empty;
					error = "Termcap strings cannot encode NUL without changing the source semantics.";
					return false;

				case '\x1b':
					builder.Append( "\\E" );
					break;

				case '\a':
					builder.Append( "\\a" );
					break;

				case '\n':
					builder.Append( "\\n" );
					break;

				case '\r':
					builder.Append( "\\r" );
					break;

				case '\t':
					builder.Append( "\\t" );
					break;

				case '\b':
					builder.Append( "\\b" );
					break;

				case '\f':
					builder.Append( "\\f" );
					break;

				case '\v':
					builder.Append( "\\v" );
					break;

				case ':':
					builder.Append( "\\072" );
					break;

				case '^':
					builder.Append( "\\^" );
					break;

				case '\\':
					builder.Append( "\\\\" );
					break;

				default:
					if ( value > byte.MaxValue ) {
						escaped = string.Empty;
						error = "Traditional termcap string rendering is restricted to one-byte character values.";
						return false;
					}
					if ( value < ' ' || value > '~' ) {
						builder.Append( '\\' );
						builder.Append(
							Convert.ToString( value, 8 )!.PadLeft( 3, '0' )
						);
					} else {
						builder.Append( value );
					}
					break;
			}
		}

		escaped = builder.ToString();
		error = null;
		return true;
	}

	private static bool TryDecodeParameters(
		string source,
		out string converted,
		out string? error
	) {
		ArgumentNullException.ThrowIfNull( source );

		StringBuilder builder =
			new( source.Length );
		HashSet<DecodeStateKey> failed = [];
		ParameterState state =
			new(
				"%p1",
				"%p2",
				0
			);
		if (
			TryDecodeAt(
				source,
				0,
				state,
				builder,
				failed
			)
		) {
			converted = builder.ToString();
			error = null;
			return true;
		}

		converted = string.Empty;
		error = "The terminfo parameter program is outside the exactly invertible TC04 classic termcap operator subset.";
		return false;
	}

	private static bool TryDecodeAt(
		string source,
		int position,
		ParameterState state,
		StringBuilder builder,
		ISet<DecodeStateKey> failed
	) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentNullException.ThrowIfNull( failed );

		if ( position == source.Length ) {
			return true;
		}

		DecodeStateKey key =
			new(
				position,
				state.First,
				state.Second,
				state.Current
			);
		if ( failed.Contains( key ) ) {
			return false;
		}

		int originalLength = builder.Length;
		char value = source[position];
		if ( value != '%' ) {
			builder.Append( value );
			if (
				TryDecodeAt(
					source,
					position + 1,
					state,
					builder,
					failed
				)
			) {
				return true;
			}
			builder.Length = originalLength;
			failed.Add( key );
			return false;
		}

		if ( StartsWith( source, position, "%%" ) ) {
			builder.Append( "%%" );
			if (
				TryDecodeAt(
					source,
					position + 2,
					state,
					builder,
					failed
				)
			) {
				return true;
			}
			builder.Length = originalLength;
		}

		// Native terminfo commonly retains %i as an explicit global operator. It
		// maps directly to the classic termcap operator; subsequent textual %pN
		// references are decoded normally. Re-conversion canonicalizes the program
		// to TC04's explicit increment expressions while preserving expansion semantics.
		if ( StartsWith( source, position, "%i" ) ) {
			builder.Append( "%i" );
			if (
				TryDecodeAt(
					source,
					position + 2,
					state,
					builder,
					failed
				)
			) {
				return true;
			}
			builder.Length = originalLength;
		}

		HashSet<ParameterState> transformed = [];
		if (
			TryDecodeOutput(
				source,
				position,
				state,
				builder,
				failed,
				transformed,
				0
			)
		) {
			return true;
		}

		builder.Length = originalLength;
		failed.Add( key );
		return false;
	}

	private static bool TryDecodeOutput(
		string source,
		int position,
		ParameterState state,
		StringBuilder builder,
		ISet<DecodeStateKey> failed,
		ISet<ParameterState> transformed,
		int depth
	) {
		if ( depth > MaximumTransformDepth || !transformed.Add( state ) ) {
			return false;
		}

		int originalLength = builder.Length;
		string expression = state.CurrentExpression;
		if (
			TryOutputCandidate(
				source,
				position,
				state,
				expression + "%d",
				"%d",
				builder,
				failed
			)
			|| TryOutputCandidate(
				source,
				position,
				state,
				Modulo( expression, 100 ) + "%02d",
				"%2",
				builder,
				failed
			)
			|| TryOutputCandidate(
				source,
				position,
				state,
				Modulo( expression, 1000 ) + "%03d",
				"%3",
				builder,
				failed
			)
			|| TryOutputCandidate(
				source,
				position,
				state,
				expression + "%c",
				"%.",
				builder,
				failed
			)
			|| TryPlusOutputCandidate(
				source,
				position,
				state,
				builder,
				failed
			)
		) {
			return true;
		}
		builder.Length = originalLength;

		if (
			TryTransform(
				source,
				position,
				state.Swap(),
				"%r",
				builder,
				failed,
				transformed,
				depth,
				false
			)
			|| TryTransform(
				source,
				position,
				state.IncrementBoth(),
				"%i",
				builder,
				failed,
				transformed,
				depth,
				false
			)
			|| TryTransform(
				source,
				position,
				state.XorBoth(),
				"%n",
				builder,
				failed,
				transformed,
				depth,
				false
			)
			|| TryTransform(
				source,
				position,
				state.TransformCurrent( ToBcd ),
				"%B",
				builder,
				failed,
				transformed,
				depth,
				true
			)
			|| TryTransform(
				source,
				position,
				state.TransformCurrent( ToDeltaData ),
				"%D",
				builder,
				failed,
				transformed,
				depth,
				true
			)
		) {
			return true;
		}
		builder.Length = originalLength;

		foreach (
			ConditionalAddCandidate candidate
			in FindConditionalAddCandidates(
				source,
				position,
				expression
			)
		) {
			ParameterState conditional =
				state.TransformCurrent(
					current =>
						ConditionalAdd(
							current,
							candidate.Threshold,
							candidate.Increment
						)
				);
			if (
				!CouldParticipateInTarget(
					source,
					position,
					conditional,
					true
				)
			) {
				continue;
			}

			builder.Append( "%>" );
			builder.Append( candidate.Threshold );
			builder.Append( candidate.Increment );
			if (
				TryDecodeOutput(
					source,
					position,
					conditional,
					builder,
					failed,
					transformed,
					depth + 1
				)
			) {
				return true;
			}
			builder.Length = originalLength;
		}

		return false;
	}

	private static bool TryTransform(
		string source,
		int position,
		ParameterState transformedState,
		string termcapOperator,
		StringBuilder builder,
		ISet<DecodeStateKey> failed,
		ISet<ParameterState> transformed,
		int depth,
		bool currentExpressionOnly
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( termcapOperator );

		if (
			!CouldParticipateInTarget(
				source,
				position,
				transformedState,
				currentExpressionOnly
			)
		) {
			return false;
		}

		int originalLength = builder.Length;
		builder.Append( termcapOperator );
		if (
			TryDecodeOutput(
				source,
				position,
				transformedState,
				builder,
				failed,
				transformed,
				depth + 1
			)
		) {
			return true;
		}
		builder.Length = originalLength;
		return false;
	}

	private static bool TryOutputCandidate(
		string source,
		int position,
		ParameterState state,
		string terminfoFragment,
		string termcapOperator,
		StringBuilder builder,
		ISet<DecodeStateKey> failed
	) {
		if ( !StartsWith( source, position, terminfoFragment ) ) {
			return false;
		}

		int originalLength = builder.Length;
		builder.Append( termcapOperator );
		if (
			TryDecodeAt(
				source,
				position + terminfoFragment.Length,
				state.AfterOutput(),
				builder,
				failed
			)
		) {
			return true;
		}
		builder.Length = originalLength;
		return false;
	}

	private static bool TryPlusOutputCandidate(
		string source,
		int position,
		ParameterState state,
		StringBuilder builder,
		ISet<DecodeStateKey> failed
	) {
		string prefix =
			state.CurrentExpression + "%{";
		if ( !StartsWith( source, position, prefix ) ) {
			return false;
		}

		int numberStart = position + prefix.Length;
		if (
			!TryReadDecimalConstant(
				source,
				numberStart,
				out int value,
				out int afterNumber
			)
			|| value > byte.MaxValue
			|| !StartsWith( source, afterNumber, "}%+%c" )
		) {
			return false;
		}

		int originalLength = builder.Length;
		builder.Append( "%+" );
		builder.Append( ( char )value );
		if (
			TryDecodeAt(
				source,
				afterNumber + 5,
				state.AfterOutput(),
				builder,
				failed
			)
		) {
			return true;
		}
		builder.Length = originalLength;
		return false;
	}

	private static IEnumerable<ConditionalAddCandidate> FindConditionalAddCandidates(
		string source,
		int position,
		string expression
	) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentException.ThrowIfNullOrWhiteSpace( expression );
		if ( position < 0 || position > source.Length ) {
			throw new ArgumentOutOfRangeException(
				nameof( position )
			);
		}

		string prefix = "%?" + expression + "%{";
		int searchPosition = position;
		while ( searchPosition < source.Length ) {
			int candidatePosition =
				source.IndexOf(
					prefix,
					searchPosition,
					StringComparison.Ordinal
				);
			if ( candidatePosition < 0 ) {
				yield break;
			}

			if (
				TryReadConditionalAddAt(
					source,
					candidatePosition,
					expression,
					out char threshold,
					out char increment
				)
			) {
				yield return new ConditionalAddCandidate(
					threshold,
					increment
				);
			}

			searchPosition = candidatePosition + 1;
		}
	}

	private static bool TryReadConditionalAddAt(
		string source,
		int position,
		string expression,
		out char threshold,
		out char increment
	) {
		string prefix = "%?" + expression + "%{";
		if ( !StartsWith( source, position, prefix ) ) {
			threshold = default;
			increment = default;
			return false;
		}

		int thresholdStart = position + prefix.Length;
		if (
			!TryReadDecimalConstant(
				source,
				thresholdStart,
				out int thresholdValue,
				out int afterThreshold
			)
			|| thresholdValue > byte.MaxValue
			|| !StartsWith( source, afterThreshold, "}%>%t" + expression + "%{" )
		) {
			threshold = default;
			increment = default;
			return false;
		}

		int incrementStart =
			afterThreshold
			+ 5
			+ expression.Length
			+ 2;
		if (
			!TryReadDecimalConstant(
				source,
				incrementStart,
				out int incrementValue,
				out int afterIncrement
			)
			|| incrementValue > byte.MaxValue
			|| !StartsWith( source, afterIncrement, "}%+%e" + expression + "%;" )
		) {
			threshold = default;
			increment = default;
			return false;
		}

		threshold = ( char )thresholdValue;
		increment = ( char )incrementValue;
		return true;
	}

	private static bool TryReadDecimalConstant(
		string source,
		int position,
		out int value,
		out int afterNumber
	) {
		value = 0;
		afterNumber = position;
		if (
			position >= source.Length
			|| !IsAsciiDigit( source[position] )
		) {
			return false;
		}

		while (
			afterNumber < source.Length
			&& IsAsciiDigit( source[afterNumber] )
		) {
			int digit = source[afterNumber] - '0';
			if ( value > ( int.MaxValue - digit ) / 10 ) {
				return false;
			}
			value = ( value * 10 ) + digit;
			afterNumber++;
		}
		return true;
	}

	private static bool CouldParticipateInTarget(
		string source,
		int position,
		ParameterState state,
		bool currentExpressionOnly
	) {
		ArgumentNullException.ThrowIfNull( source );
		if ( position < 0 || position > source.Length ) {
			throw new ArgumentOutOfRangeException(
				nameof( position )
			);
		}

		int remainingLength =
			source.Length - position;
		if ( currentExpressionOnly ) {
			string expression =
				state.CurrentExpression;
			return expression.Length <= remainingLength
				&& source.IndexOf(
					expression,
					position,
					StringComparison.Ordinal
				) >= 0
			;
		}

		return (
				state.First.Length <= remainingLength
				&& source.IndexOf(
					state.First,
					position,
					StringComparison.Ordinal
				) >= 0
			)
			|| (
				state.Second.Length <= remainingLength
				&& source.IndexOf(
					state.Second,
					position,
					StringComparison.Ordinal
				) >= 0
			)
		;
	}

	private static bool StartsWith(
		string source,
		int position,
		string value
	) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentNullException.ThrowIfNull( value );

		return position >= 0
			&& position <= source.Length
			&& source.AsSpan( position ).StartsWith(
				value,
				StringComparison.Ordinal
			)
		;
	}

	private static string Modulo(
		string expression,
		int modulus
	) {
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
		return expression + "%{1}%+";
	}

	private static string XorWith96(
		string expression
	) {
		return expression + "%{96}%^";
	}

	private static string ToBcd(
		string expression
	) {
		return
			expression
			+ "%{10}%/%{16}%*"
			+ expression
			+ "%{10}%m%+";
	}

	private static string ToDeltaData(
		string expression
	) {
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

	private readonly record struct ConditionalAddCandidate(
		char Threshold,
		char Increment
	);

	private readonly record struct DecodeStateKey(
		int Position,
		string First,
		string Second,
		int Current
	);

	private readonly record struct ParameterState(
		string First,
		string Second,
		int Current
	)
	{
		internal string CurrentExpression =>
			Current == 0
				? First
				: Second
		;

		internal ParameterState AfterOutput() {
			return this with { Current = 1 - Current };
		}

		internal ParameterState Swap() {
			return new ParameterState(
				Second,
				First,
				Current
			);
		}

		internal ParameterState IncrementBoth() {
			return new ParameterState(
				Increment( First ),
				Increment( Second ),
				Current
			);
		}

		internal ParameterState XorBoth() {
			return new ParameterState(
				XorWith96( First ),
				XorWith96( Second ),
				Current
			);
		}

		internal ParameterState TransformCurrent(
			Func<string, string> transform
		) {
			ArgumentNullException.ThrowIfNull( transform );

			return Current == 0
				? new ParameterState(
					transform( First ),
					Second,
					Current
				)
				: new ParameterState(
					First,
					transform( Second ),
					Current
				)
			;
		}
	}
}
