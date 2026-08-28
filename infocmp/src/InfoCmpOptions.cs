using System.Globalization;
using Icod.TermInfo.Inspection;

namespace Icod.TermInfo.InfoCmp;

internal enum InfoCmpComparisonMode {
	Differences = 0,
	Common = 1,
	Absent = 2,
}

internal sealed class InfoCmpOptions {
	private readonly IReadOnlyList<string> _terminalNames;

	internal InfoCmpOptions(
		string? databaseDirectory,
		string? comparisonDatabaseDirectory,
		IEnumerable<string> terminalNames,
		TerminalDescriptionSourceLayout layout,
		int lineWidth,
		bool lineWidthSpecified,
		TerminalDescriptionSourceCapabilityOrder capabilityOrder,
		bool includeExtendedCapabilities,
		InfoCmpComparisonMode? comparisonMode,
		bool shortComparison
	) {
		ArgumentNullException.ThrowIfNull( terminalNames );
		if ( lineWidth <= 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( lineWidth )
			);
		}
		if ( !Enum.IsDefined( typeof( TerminalDescriptionSourceLayout ), layout ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( layout )
			);
		}
		if ( !Enum.IsDefined(
			typeof( TerminalDescriptionSourceCapabilityOrder ),
			capabilityOrder
		) ) {
			throw new ArgumentOutOfRangeException(
				nameof( capabilityOrder )
			);
		}
		if ( comparisonMode.HasValue
			&& !Enum.IsDefined(
				typeof( InfoCmpComparisonMode ),
				comparisonMode.Value
			) ) {
			throw new ArgumentOutOfRangeException(
				nameof( comparisonMode )
			);
		}

		string[] names = terminalNames.ToArray();
		foreach ( string name in names ) {
			ArgumentException.ThrowIfNullOrWhiteSpace( name );
		}
		if ( names.Length >= 2 && !comparisonMode.HasValue ) {
			throw new ArgumentException(
				"Comparison options require an explicit comparison mode.",
				nameof( comparisonMode )
			);
		}
		if ( names.Length < 2 && comparisonMode.HasValue ) {
			throw new ArgumentException(
				"A comparison mode requires at least two terminal names.",
				nameof( comparisonMode )
			);
		}

		DatabaseDirectory = databaseDirectory;
		ComparisonDatabaseDirectory = comparisonDatabaseDirectory;
		_terminalNames = Array.AsReadOnly( names );
		Layout = layout;
		LineWidth = lineWidth;
		LineWidthSpecified = lineWidthSpecified;
		CapabilityOrder = capabilityOrder;
		IncludeExtendedCapabilities = includeExtendedCapabilities;
		ComparisonMode = comparisonMode;
		ShortComparison = shortComparison;
	}

	internal string? DatabaseDirectory {
		get;
	}

	internal string? ComparisonDatabaseDirectory {
		get;
	}

	internal IReadOnlyList<string> TerminalNames =>
		_terminalNames;

	internal string? TerminalName =>
		_terminalNames.Count == 1
			? _terminalNames[ 0 ]
			: null;

	internal bool IsComparison =>
		_terminalNames.Count >= 2;

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

	internal InfoCmpComparisonMode? ComparisonMode {
		get;
	}

	internal bool ShortComparison {
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
		string? comparisonDatabaseDirectory = null;
		List<string> terminalNames = [];
		TerminalDescriptionSourceLayout layout =
			TerminalDescriptionSourceLayout.Canonical;
		int lineWidth =
			DefaultLineWidth;
		bool lineWidthSpecified = false;
		TerminalDescriptionSourceCapabilityOrder capabilityOrder =
			TerminalDescriptionSourceCapabilityOrder.Database;
		bool capabilityOrderSpecified = false;
		bool includeExtendedCapabilities = false;
		InfoCmpComparisonMode? comparisonMode = null;
		bool shortComparison = false;
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

					case "-B":
						if ( comparisonDatabaseDirectory is not null ) {
							return InfoCmpOptionsParseResult.Failure(
								"option '-B' may be specified only once"
							);
						}
						if ( !TryReadValue(
								args,
								ref index,
								"-B",
								out string? comparisonDirectory,
								out string? comparisonDirectoryError
							) ) {
							return InfoCmpOptionsParseResult.Failure(
								comparisonDirectoryError!
							);
						}
						comparisonDatabaseDirectory = comparisonDirectory;
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

					case "-d":
						if ( !TrySetComparisonMode(
								ref comparisonMode,
								InfoCmpComparisonMode.Differences,
								out string? differenceModeError
							) ) {
							return InfoCmpOptionsParseResult.Failure(
								differenceModeError!
							);
						}
						break;

					case "-c":
						if ( !TrySetComparisonMode(
								ref comparisonMode,
								InfoCmpComparisonMode.Common,
								out string? commonModeError
							) ) {
							return InfoCmpOptionsParseResult.Failure(
								commonModeError!
							);
						}
						break;

					case "-n":
						if ( !TrySetComparisonMode(
								ref comparisonMode,
								InfoCmpComparisonMode.Absent,
								out string? absentModeError
							) ) {
							return InfoCmpOptionsParseResult.Failure(
								absentModeError!
							);
						}
						break;

					case "-q":
						shortComparison = true;
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

			if ( string.IsNullOrWhiteSpace( argument ) ) {
				return InfoCmpOptionsParseResult.Failure(
					"terminal operand cannot be empty or whitespace"
				);
			}
			terminalNames.Add( argument );
		}

		if ( lineWidthSpecified
			&& layout != TerminalDescriptionSourceLayout.Canonical ) {
			return InfoCmpOptionsParseResult.Failure(
				"option '-w' cannot be combined with '-0' or '-1'"
			);
		}

		if ( terminalNames.Count >= 2 ) {
			if ( layout != TerminalDescriptionSourceLayout.Canonical
				|| lineWidthSpecified
				|| capabilityOrderSpecified ) {
				return InfoCmpOptionsParseResult.Failure(
					"options '-0', '-1', '-w', and '-s' apply only to one-terminal source listing"
				);
			}

			comparisonMode ??= InfoCmpComparisonMode.Differences;
		} else {
			if ( comparisonDatabaseDirectory is not null ) {
				return InfoCmpOptionsParseResult.Failure(
					"option '-B' requires two or more terminal operands"
				);
			}
			if ( comparisonMode.HasValue ) {
				return InfoCmpOptionsParseResult.Failure(
					"options '-d', '-c', and '-n' require two or more terminal operands"
				);
			}
			if ( shortComparison ) {
				return InfoCmpOptionsParseResult.Failure(
					"option '-q' requires two or more terminal operands"
				);
			}
		}

		return InfoCmpOptionsParseResult.Success(
			new InfoCmpOptions(
				databaseDirectory,
				comparisonDatabaseDirectory,
				terminalNames,
				layout,
				lineWidth,
				lineWidthSpecified,
				capabilityOrder,
				includeExtendedCapabilities,
				comparisonMode,
				shortComparison
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

	private static bool TrySetComparisonMode(
		ref InfoCmpComparisonMode? currentMode,
		InfoCmpComparisonMode requestedMode,
		out string? error
	) {
		if ( !Enum.IsDefined( typeof( InfoCmpComparisonMode ), requestedMode ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( requestedMode )
			);
		}

		if ( currentMode.HasValue ) {
			error = "options '-d', '-c', and '-n' are mutually exclusive";
			return false;
		}

		currentMode = requestedMode;
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
