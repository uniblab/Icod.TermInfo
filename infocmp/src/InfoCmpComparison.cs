using System.Globalization;
using System.Text;
using Icod.TermInfo.Inspection;

namespace Icod.TermInfo.InfoCmp;

internal sealed class InfoCmpTerminal {
	internal InfoCmpTerminal(
		string requestedName,
		TerminalDescription description
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( requestedName );
		ArgumentNullException.ThrowIfNull( description );

		RequestedName = requestedName;
		Description = description;
	}

	internal string RequestedName {
		get;
	}

	internal TerminalDescription Description {
		get;
	}
}

internal static class InfoCmpComparisonRenderer {
	internal static string Render(
		InfoCmpOptions options,
		IReadOnlyList<InfoCmpTerminal> terminals
	) {
		ArgumentNullException.ThrowIfNull( options );
		ArgumentNullException.ThrowIfNull( terminals );
		if ( terminals.Count < 2 ) {
			throw new ArgumentException(
				"At least two terminals are required for comparison.",
				nameof( terminals )
			);
		}
		if ( !options.ComparisonMode.HasValue ) {
			throw new ArgumentException(
				"A comparison mode is required.",
				nameof( options )
			);
		}
		foreach ( InfoCmpTerminal terminal in terminals ) {
			ArgumentNullException.ThrowIfNull( terminal );
		}

		return options.ComparisonMode.Value switch {
			InfoCmpComparisonMode.Differences =>
				RenderDifferences(
					terminals,
					options.IncludeExtendedCapabilities,
					options.ShortComparison
				),
			InfoCmpComparisonMode.Common =>
				RenderCommon(
					terminals,
					options.IncludeExtendedCapabilities,
					options.ShortComparison
				),
			InfoCmpComparisonMode.Absent =>
				RenderAbsent(
					terminals,
					options.ShortComparison
				),
			_ => throw new ArgumentOutOfRangeException(
				nameof( options )
			),
		};
	}

	private static string RenderDifferences(
		IReadOnlyList<InfoCmpTerminal> terminals,
		bool includeExtendedCapabilities,
		bool shortComparison
	) {
		ArgumentNullException.ThrowIfNull( terminals );

		StringBuilder builder = new();
		InfoCmpTerminal first = terminals[ 0 ];
		for ( int index = 1; index < terminals.Count; index++ ) {
			InfoCmpTerminal other = terminals[ index ];
			AppendPairHeading(
				builder,
				first.RequestedName,
				other.RequestedName,
				"Comparing",
				"with",
				"->",
				shortComparison
			);

			TermInfoComparisonResult comparison =
				TerminalDescriptionComparer.Compare(
					first.Description,
					other.Description
				);
			TermInfoDifference[] differences =
				comparison.Differences
					.Where(
						difference =>
							includeExtendedCapabilities
								|| difference.IsExtendedCapability != true
					)
					.ToArray();

			if ( differences.Length == 0 ) {
				if ( !shortComparison ) {
					AppendReportLine(
						builder,
						"no reported semantic differences.",
						shortComparison
					);
				}
			} else {
				foreach ( TermInfoDifference difference in differences ) {
					AppendDifference(
						builder,
						difference,
						shortComparison
					);
				}
			}

			AppendPairSeparator(
				builder,
				index,
				terminals.Count
			);
		}

		return builder.ToString();
	}

	private static string RenderCommon(
		IReadOnlyList<InfoCmpTerminal> terminals,
		bool includeExtendedCapabilities,
		bool shortComparison
	) {
		ArgumentNullException.ThrowIfNull( terminals );

		StringBuilder builder = new();
		InfoCmpTerminal first = terminals[ 0 ];
		for ( int index = 1; index < terminals.Count; index++ ) {
			InfoCmpTerminal other = terminals[ index ];
			AppendPairHeading(
				builder,
				first.RequestedName,
				other.RequestedName,
				"Common capabilities for",
				"and",
				"=",
				shortComparison
			);

			AppendCommonStandardCapabilities(
				builder,
				first.Description,
				other.Description,
				shortComparison
			);
			if ( includeExtendedCapabilities ) {
				AppendCommonExtendedCapabilities(
					builder,
					first.Description,
					other.Description,
					shortComparison
				);
			}

			AppendPairSeparator(
				builder,
				index,
				terminals.Count
			);
		}

		return builder.ToString();
	}

	private static string RenderAbsent(
		IReadOnlyList<InfoCmpTerminal> terminals,
		bool shortComparison
	) {
		ArgumentNullException.ThrowIfNull( terminals );

		StringBuilder builder = new();
		if ( !shortComparison ) {
			builder.Append( "Standard capabilities absent from all compared terminals:" );
			builder.Append( Environment.NewLine );
		}

		foreach (
			StandardCapabilityMetadata<BooleanCapability> metadata
			in StandardCapabilityCatalog.BooleanCapabilities
		) {
			if ( terminals.All(
				terminal =>
					!terminal.Description.GetBoolean( metadata.Capability )
			) ) {
				AppendAbsentCapability(
					builder,
					metadata.ShortName,
					shortComparison
				);
			}
		}

		foreach (
			StandardCapabilityMetadata<NumericCapability> metadata
			in StandardCapabilityCatalog.NumericCapabilities
		) {
			if ( terminals.All(
				terminal =>
					!terminal.Description.GetNumber( metadata.Capability ).HasValue
			) ) {
				AppendAbsentCapability(
					builder,
					metadata.ShortName,
					shortComparison
				);
			}
		}

		foreach (
			StandardCapabilityMetadata<StringCapability> metadata
			in StandardCapabilityCatalog.StringCapabilities
		) {
			if ( terminals.All(
				terminal =>
					terminal.Description.GetString( metadata.Capability ) is null
			) ) {
				AppendAbsentCapability(
					builder,
					metadata.ShortName,
					shortComparison
				);
			}
		}

		return builder.ToString();
	}

	private static void AppendDifference(
		StringBuilder builder,
		TermInfoDifference difference,
		bool shortComparison
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentNullException.ThrowIfNull( difference );

		string line;
		switch ( difference.Kind ) {
			case TermInfoDifferenceKind.IdentityName:
				line =
					$"name: {FormatQuotedText( difference.LeftText )}, {FormatQuotedText( difference.RightText )}.";
				break;

			case TermInfoDifferenceKind.IdentityAliases:
				line =
					$"aliases: {FormatAliases( difference.LeftAliases )}, {FormatAliases( difference.RightAliases )}.";
				break;

			case TermInfoDifferenceKind.IdentityDescription:
				line =
					$"description: {FormatQuotedText( difference.LeftText )}, {FormatQuotedText( difference.RightText )}.";
				break;

			case TermInfoDifferenceKind.OnlyInLeft:
			case TermInfoDifferenceKind.OnlyInRight:
			case TermInfoDifferenceKind.DifferentValue:
			case TermInfoDifferenceKind.DifferentValueKind:
				line = FormatCapabilityDifference( difference );
				break;

			default:
				throw new InvalidOperationException(
					$"Unsupported effective difference kind '{difference.Kind}'."
				);
		}

		AppendReportLine(
			builder,
			line,
			shortComparison
		);
	}

	private static string FormatCapabilityDifference(
		TermInfoDifference difference
	) {
		ArgumentNullException.ThrowIfNull( difference );
		ArgumentException.ThrowIfNullOrWhiteSpace( difference.CapabilityName );

		TermInfoCapabilityValue? left = difference.LeftCapabilityValue;
		TermInfoCapabilityValue? right = difference.RightCapabilityValue;
		string leftText =
			left.HasValue
				? FormatCapabilityValue(
					left.Value,
					includeKind: difference.Kind == TermInfoDifferenceKind.DifferentValueKind
				)
				: FormatMissingCapabilityValue( right );
		string rightText =
			right.HasValue
				? FormatCapabilityValue(
					right.Value,
					includeKind: difference.Kind == TermInfoDifferenceKind.DifferentValueKind
				)
				: FormatMissingCapabilityValue( left );

		return $"{difference.CapabilityName}: {leftText}, {rightText}.";
	}

	private static void AppendCommonStandardCapabilities(
		StringBuilder builder,
		TerminalDescription left,
		TerminalDescription right,
		bool shortComparison
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentNullException.ThrowIfNull( left );
		ArgumentNullException.ThrowIfNull( right );

		foreach (
			StandardCapabilityMetadata<BooleanCapability> metadata
			in StandardCapabilityCatalog.BooleanCapabilities
		) {
			if ( left.GetBoolean( metadata.Capability )
				&& right.GetBoolean( metadata.Capability ) ) {
				AppendCommonCapability(
					builder,
					metadata.ShortName,
					new TermInfoCapabilityValue( true ),
					shortComparison
				);
			}
		}

		foreach (
			StandardCapabilityMetadata<NumericCapability> metadata
			in StandardCapabilityCatalog.NumericCapabilities
		) {
			int? leftValue = left.GetNumber( metadata.Capability );
			int? rightValue = right.GetNumber( metadata.Capability );
			if ( leftValue.HasValue
				&& rightValue.HasValue
				&& leftValue.Value == rightValue.Value ) {
				AppendCommonCapability(
					builder,
					metadata.ShortName,
					new TermInfoCapabilityValue( leftValue.Value ),
					shortComparison
				);
			}
		}

		foreach (
			StandardCapabilityMetadata<StringCapability> metadata
			in StandardCapabilityCatalog.StringCapabilities
		) {
			string? leftValue = left.GetString( metadata.Capability );
			string? rightValue = right.GetString( metadata.Capability );
			if ( leftValue is not null
				&& rightValue is not null
				&& string.Equals(
					leftValue,
					rightValue,
					StringComparison.Ordinal
				) ) {
				AppendCommonCapability(
					builder,
					metadata.ShortName,
					new TermInfoCapabilityValue( leftValue ),
					shortComparison
				);
			}
		}
	}

	private static void AppendCommonExtendedCapabilities(
		StringBuilder builder,
		TerminalDescription left,
		TerminalDescription right,
		bool shortComparison
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentNullException.ThrowIfNull( left );
		ArgumentNullException.ThrowIfNull( right );

		foreach (
			string name in left.ExtendedCapabilities.Keys
				.Intersect(
					right.ExtendedCapabilities.Keys,
					StringComparer.Ordinal
				)
				.OrderBy(
					value => value,
					StringComparer.Ordinal
				)
		) {
			TermInfoCapabilityValue leftValue =
				left.ExtendedCapabilities[ name ];
			TermInfoCapabilityValue rightValue =
				right.ExtendedCapabilities[ name ];
			if ( leftValue.Equals( rightValue ) ) {
				AppendCommonCapability(
					builder,
					name,
					leftValue,
					shortComparison
				);
			}
		}
	}

	private static void AppendCommonCapability(
		StringBuilder builder,
		string capabilityName,
		TermInfoCapabilityValue value,
		bool shortComparison
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentException.ThrowIfNullOrWhiteSpace( capabilityName );

		AppendReportLine(
			builder,
			$"{capabilityName} = {FormatCapabilityValue( value, includeKind: false )}.",
			shortComparison
		);
	}

	private static void AppendAbsentCapability(
		StringBuilder builder,
		string capabilityName,
		bool shortComparison
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentException.ThrowIfNullOrWhiteSpace( capabilityName );

		AppendReportLine(
			builder,
			$"!{capabilityName}.",
			shortComparison
		);
	}

	private static void AppendPairHeading(
		StringBuilder builder,
		string leftName,
		string rightName,
		string longPrefix,
		string longSeparator,
		string shortSeparator,
		bool shortComparison
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentException.ThrowIfNullOrWhiteSpace( leftName );
		ArgumentException.ThrowIfNullOrWhiteSpace( rightName );
		ArgumentException.ThrowIfNullOrWhiteSpace( longPrefix );
		ArgumentException.ThrowIfNullOrWhiteSpace( longSeparator );
		ArgumentException.ThrowIfNullOrWhiteSpace( shortSeparator );

		if ( shortComparison ) {
			builder.Append( FormatQuotedText( leftName ) );
			builder.Append( ' ' );
			builder.Append( shortSeparator );
			builder.Append( ' ' );
			builder.Append( FormatQuotedText( rightName ) );
			builder.Append( Environment.NewLine );
			return;
		}

		builder.Append( longPrefix );
		builder.Append( ' ' );
		builder.Append( FormatQuotedText( leftName ) );
		builder.Append( ' ' );
		builder.Append( longSeparator );
		builder.Append( ' ' );
		builder.Append( FormatQuotedText( rightName ) );
		builder.Append( ':' );
		builder.Append( Environment.NewLine );
	}

	private static void AppendPairSeparator(
		StringBuilder builder,
		int currentIndex,
		int terminalCount
	) {
		ArgumentNullException.ThrowIfNull( builder );
		if ( currentIndex < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( currentIndex ) );
		}
		if ( terminalCount < 2 ) {
			throw new ArgumentOutOfRangeException( nameof( terminalCount ) );
		}

		if ( currentIndex + 1 < terminalCount ) {
			builder.Append( Environment.NewLine );
		}
	}

	private static void AppendReportLine(
		StringBuilder builder,
		string line,
		bool shortComparison
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentNullException.ThrowIfNull( line );

		if ( !shortComparison ) {
			builder.Append( "    " );
		}
		builder.Append( line );
		builder.Append( Environment.NewLine );
	}

	private static string FormatMissingCapabilityValue(
		TermInfoCapabilityValue? otherValue
	) {
		return otherValue.HasValue
			&& otherValue.Value.Kind == TermInfoCapabilityValueKind.Boolean
				? "F"
				: "NULL";
	}

	private static string FormatCapabilityValue(
		TermInfoCapabilityValue value,
		bool includeKind
	) {
		string formatted = value.Kind switch {
			TermInfoCapabilityValueKind.Boolean =>
				value.BooleanValue
					? "T"
					: "F",
			TermInfoCapabilityValueKind.Number =>
				value.NumberValue.ToString( CultureInfo.InvariantCulture ),
			TermInfoCapabilityValueKind.String =>
				FormatQuotedText( value.StringValue ),
			_ => throw new ArgumentOutOfRangeException(
				nameof( value )
			),
		};

		if ( !includeKind ) {
			return formatted;
		}

		string kind = value.Kind switch {
			TermInfoCapabilityValueKind.Boolean => "boolean",
			TermInfoCapabilityValueKind.Number => "number",
			TermInfoCapabilityValueKind.String => "string",
			_ => throw new ArgumentOutOfRangeException(
				nameof( value )
			),
		};
		return $"{kind}:{formatted}";
	}

	private static string FormatAliases(
		IReadOnlyList<string>? aliases
	) {
		if ( aliases is null ) {
			return "NULL";
		}

		return FormatQuotedText(
			string.Join(
				"|",
				aliases
			)
		);
	}

	private static string FormatQuotedText(
		string? value
	) {
		if ( value is null ) {
			return "NULL";
		}

		StringBuilder builder = new();
		builder.Append( '\'' );
		foreach ( char character in value ) {
			switch ( character ) {
				case '\\':
					builder.Append( "\\\\" );
					break;
				case '\'':
					builder.Append( "\\'" );
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
				default:
					if ( char.IsControl( character ) ) {
						builder.Append( "\\u" );
						builder.Append(
							((int)character).ToString(
								"X4",
								CultureInfo.InvariantCulture
							)
						);
					} else {
						builder.Append( character );
					}
					break;
			}
		}
		builder.Append( '\'' );
		return builder.ToString();
	}
}
