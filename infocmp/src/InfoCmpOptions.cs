using System.Globalization;
using Icod.TermInfo.Inspection;

namespace Icod.TermInfo.InfoCmp;

internal sealed class InfoCmpOptions {
	internal InfoCmpOptions(
		string? databaseDirectory,
		string? terminalName,
		TerminalDescriptionSourceLayout layout,
		int lineWidth,
		bool lineWidthSpecified,
		TerminalDescriptionSourceCapabilityOrder capabilityOrder,
		bool includeExtendedCapabilities
	) {
		if ( lineWidth <= 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( lineWidth )
			);
		}

		DatabaseDirectory = databaseDirectory;
		TerminalName = terminalName;
		Layout = layout;
		LineWidth = lineWidth;
		LineWidthSpecified = lineWidthSpecified;
		CapabilityOrder = capabilityOrder;
		IncludeExtendedCapabilities = includeExtendedCapabilities;
	}

	internal string? DatabaseDirectory {
		get;
	}

	internal string? TerminalName {
		get;
	}

	internal TerminalDescriptionSourceLayout Layout {
		get;
	}

	internal int LineWidth {
		get;
	}

	internal bool LineWidthSpecified {
		get;
	}

	internal TerminalDescriptionSourceCapabilityOrder CapabilityOrder {
		get;
	}

	internal bool IncludeExtendedCapabilities {
		get;
	}
}

internal sealed class InfoCmpOptionsParseResult {
	internal InfoCmpOptionsParseResult(
		InfoCmpOptions? options,
		string? error
	) {
		if ( ( options is null ) == ( error is null ) ) {
			throw new ArgumentException(
				"Exactly one parse result payload must be supplied."
			);
		}

		Options = options;
		Error = error;
	}

	internal InfoCmpOptions? Options {
		get;
	}

	internal string? Error {
		get;
	}

	internal static InfoCmpOptionsParseResult Success(
		InfoCmpOptions options
	) {
		ArgumentNullException.ThrowIfNull( options );
		return new InfoCmpOptionsParseResult(
			options,
			null
		);
	}

	internal static InfoCmpOptionsParseResult Failure(
		string error
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( error );
		return new InfoCmpOptionsParseResult(
			null,
			error
		);
	}
}

internal static class InfoCmpOptionsParser {
	private const int DefaultLineWidth = 80;

	internal static InfoCmpOptionsParseResult Parse(
		IReadOnlyList<string> args
	) {
		ArgumentNullException.ThrowIfNull( args );

		string? databaseDirectory = null;
		string? terminalName = null;
		TerminalDescriptionSourceLayout layout =
			TerminalDescriptionSourceLayout.Canonical;
		int lineWidth =
			DefaultLineWidth;
		bool lineWidthSpecified = false;
		TerminalDescriptionSourceCapabilityOrder capabilityOrder =
			TerminalDescriptionSourceCapabilityOrder.Database;
		bool capabilityOrderSpecified = false;
		bool includeExtendedCapabilities = false;
		bool optionsEnded = false;

		for ( int index = 0; index < args.Count; index++ ) {
			string argument =
				args[ index ]
				?? throw new ArgumentException(
					"Command arguments cannot contain null.",
					nameof( args )
				);

			if ( !optionsEnded
				&& string.Equals(
					argument,
					"--",
					StringComparison.Ordinal
				) ) {
				optionsEnded = true;
				continue;
			}

			if ( !optionsEnded && argument.StartsWith( "-", StringComparison.Ordinal ) ) {
				switch ( argument ) {
					case "-A":
						if ( databaseDirectory is not null ) {
							return InfoCmpOptionsParseResult.Failure(
								"option '-A' may be specified only once"
							);
						}
						if ( !TryReadValue(
								args,
								ref index,
								"-A",
								out string? directory,
								out string? directoryError
							) ) {
							return InfoCmpOptionsParseResult.Failure(
								directoryError!
							);
						}
						databaseDirectory = directory;
						break;

					case "-0":
						if ( layout == TerminalDescriptionSourceLayout.OneCapabilityPerLine ) {
							return InfoCmpOptionsParseResult.Failure(
								"options '-0' and '-1' cannot be combined"
							);
						}
						layout = TerminalDescriptionSourceLayout.SingleLine;
						break;

					case "-1":
						if ( layout == TerminalDescriptionSourceLayout.SingleLine ) {
							return InfoCmpOptionsParseResult.Failure(
								"options '-0' and '-1' cannot be combined"
							);
						}
						layout = TerminalDescriptionSourceLayout.OneCapabilityPerLine;
						break;

					case "-w":
						if ( lineWidthSpecified ) {
							return InfoCmpOptionsParseResult.Failure(
								"option '-w' may be specified only once"
							);
						}
						if ( !TryReadValue(
								args,
								ref index,
								"-w",
								out string? widthText,
								out string? widthError
							) ) {
							return InfoCmpOptionsParseResult.Failure(
								widthError!
							);
						}
						if ( !int.TryParse(
								widthText,
								NumberStyles.None,
								CultureInfo.InvariantCulture,
								out lineWidth
							)
							|| lineWidth <= 0 ) {
							return InfoCmpOptionsParseResult.Failure(
								"option '-w' requires a positive decimal width"
							);
						}
						lineWidthSpecified = true;
						break;

					case "-s":
						if ( capabilityOrderSpecified ) {
							return InfoCmpOptionsParseResult.Failure(
								"option '-s' may be specified only once"
							);
						}
						if ( !TryReadValue(
								args,
								ref index,
								"-s",
								out string? sortKey,
								out string? sortError
							) ) {
							return InfoCmpOptionsParseResult.Failure(
								sortError!
							);
						}
						if ( !TryParseCapabilityOrder(
								sortKey!,
								out capabilityOrder
							) ) {
							return InfoCmpOptionsParseResult.Failure(
								"option '-s' requires one of: d, i, l, c"
							);
						}
						capabilityOrderSpecified = true;
						break;

					case "-x":
						includeExtendedCapabilities = true;
						break;

					default:
						return InfoCmpOptionsParseResult.Failure(
							$"unsupported option '{argument}'"
						);
				}
				continue;
			}

			if ( terminalName is not null ) {
				return InfoCmpOptionsParseResult.Failure(
					"two or more terminal operands are not available until T07"
				);
			}
			if ( string.IsNullOrWhiteSpace( argument ) ) {
				return InfoCmpOptionsParseResult.Failure(
					"terminal operand cannot be empty or whitespace"
				);
			}
			terminalName = argument;
		}

		if ( lineWidthSpecified
			&& layout != TerminalDescriptionSourceLayout.Canonical ) {
			return InfoCmpOptionsParseResult.Failure(
				"option '-w' cannot be combined with '-0' or '-1'"
			);
		}

		return InfoCmpOptionsParseResult.Success(
			new InfoCmpOptions(
				databaseDirectory,
				terminalName,
				layout,
				lineWidth,
				lineWidthSpecified,
				capabilityOrder,
				includeExtendedCapabilities
			)
		);
	}

	private static bool TryReadValue(
		IReadOnlyList<string> args,
		ref int index,
		string option,
		out string? value,
		out string? error
	) {
		ArgumentNullException.ThrowIfNull( args );
		ArgumentException.ThrowIfNullOrWhiteSpace( option );

		if ( index + 1 >= args.Count ) {
			value = null;
			error = $"option '{option}' requires a value";
			return false;
		}

		index++;
		value =
			args[ index ]
			?? throw new ArgumentException(
				"Command arguments cannot contain null.",
				nameof( args )
			);
		if ( string.IsNullOrWhiteSpace( value ) ) {
			error = $"option '{option}' requires a non-empty value";
			return false;
		}

		error = null;
		return true;
	}

	private static bool TryParseCapabilityOrder(
		string key,
		out TerminalDescriptionSourceCapabilityOrder order
	) {
		ArgumentNullException.ThrowIfNull( key );

		order = key switch {
			"d" => TerminalDescriptionSourceCapabilityOrder.Database,
			"i" => TerminalDescriptionSourceCapabilityOrder.TermInfoName,
			"l" => TerminalDescriptionSourceCapabilityOrder.LongName,
			"c" => TerminalDescriptionSourceCapabilityOrder.TermcapCode,
			_ => (TerminalDescriptionSourceCapabilityOrder)(-1),
		};

		return order != (TerminalDescriptionSourceCapabilityOrder)(-1);
	}
}
