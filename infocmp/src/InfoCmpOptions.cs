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
	private readonly IReadOnlyList<string> _candidateRoots;

	internal InfoCmpOptions(
		string? databaseDirectory,
		string? comparisonDatabaseDirectory,
		IEnumerable<string> terminalNames,
		TerminalDescriptionSourceLayout layout,
		int lineWidth,
		bool lineWidthSpecified,
		TerminalDescriptionSourceCapabilityOrder capabilityOrder,
		bool includeExtendedCapabilities,
		bool relativeSynthesis,
		bool planning,
		bool json,
		bool allCandidates,
		IEnumerable<string> candidateRoots,
		int maximumSelectedParentCount,
		int maximumEvaluatedPlanCount,
		bool allowNonExhaustiveResult,
		InfoCmpComparisonMode? comparisonMode,
		bool shortComparison
	) {
		ArgumentNullException.ThrowIfNull( terminalNames );
		ArgumentNullException.ThrowIfNull( candidateRoots );
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
		if ( maximumSelectedParentCount < 0
			|| maximumSelectedParentCount
				> TerminalDescriptionSourcePlanningOptions.DefaultMaximumCandidateCount ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumSelectedParentCount )
			);
		}
		if ( maximumEvaluatedPlanCount < 1
			|| maximumEvaluatedPlanCount
				> TerminalDescriptionSourcePlanningOptions.MaximumSupportedEvaluatedPlanCount ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumEvaluatedPlanCount )
			);
		}
		if ( relativeSynthesis && planning ) {
			throw new ArgumentException(
				"Relative synthesis and planning are mutually exclusive."
			);
		}
		if ( allCandidates && !planning ) {
			throw new ArgumentException(
				"All-candidates mode requires relative-source planning.",
				nameof( allCandidates )
			);
		}

		string[] candidateRootArray = candidateRoots.ToArray();
		foreach ( string root in candidateRootArray ) {
			ArgumentException.ThrowIfNullOrWhiteSpace( root );
		}
		if ( candidateRootArray.Length != 0 && ( !planning || !allCandidates ) ) {
			throw new ArgumentException(
				"Candidate roots require all-candidates relative-source planning.",
				nameof( candidateRoots )
			);
		}

		string[] names = terminalNames.ToArray();
		foreach ( string name in names ) {
			ArgumentException.ThrowIfNullOrWhiteSpace( name );
		}
		if ( relativeSynthesis || planning ) {
			if ( relativeSynthesis && names.Length < 2 ) {
				throw new ArgumentException(
					"Relative synthesis requires a target and at least one parent terminal.",
					nameof( terminalNames )
				);
			}
			if ( planning
				&& ( allCandidates ? names.Length != 1 : names.Length < 2 ) ) {
				throw new ArgumentException(
					allCandidates
						? "All-candidates planning requires exactly one target terminal."
						: "Relative-source planning requires a target and at least one candidate terminal.",
					nameof( terminalNames )
				);
			}
			if ( comparisonMode.HasValue ) {
				throw new ArgumentException(
					"Relative synthesis or planning cannot retain a comparison mode.",
					nameof( comparisonMode )
				);
			}
			if ( shortComparison ) {
				throw new ArgumentException(
					"Relative synthesis or planning cannot use short comparison presentation.",
					nameof( shortComparison )
				);
			}
		} else {
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
		}

		DatabaseDirectory = databaseDirectory;
		ComparisonDatabaseDirectory = comparisonDatabaseDirectory;
		_terminalNames = Array.AsReadOnly( names );
		_candidateRoots = Array.AsReadOnly( candidateRootArray );
		Layout = layout;
		LineWidth = lineWidth;
		LineWidthSpecified = lineWidthSpecified;
		CapabilityOrder = capabilityOrder;
		IncludeExtendedCapabilities = includeExtendedCapabilities;
		RelativeSynthesis = relativeSynthesis;
		Planning = planning;
		Json = json;
		AllCandidates = allCandidates;
		MaximumSelectedParentCount = maximumSelectedParentCount;
		MaximumEvaluatedPlanCount = maximumEvaluatedPlanCount;
		AllowNonExhaustiveResult = allowNonExhaustiveResult;
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

	internal IReadOnlyList<string> CandidateRoots =>
		_candidateRoots;

	internal string? TerminalName =>
		_terminalNames.Count == 1
			? _terminalNames[ 0 ]
			: null;

	internal bool IsSynthesis =>
		RelativeSynthesis;

	internal bool IsPlanning =>
		Planning;

	internal bool IsComparison =>
		!RelativeSynthesis
			&& !Planning
			&& _terminalNames.Count >= 2;

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

	internal bool RelativeSynthesis {
		get;
	}

	internal bool Planning {
		get;
	}

	internal bool Json {
		get;
	}

	internal bool AllCandidates {
		get;
	}

	internal int MaximumSelectedParentCount {
		get;
	}

	internal int MaximumEvaluatedPlanCount {
		get;
	}

	internal bool AllowNonExhaustiveResult {
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
		var terminalNames = new List<string>();
		var candidateRoots = new List<string>();
		TerminalDescriptionSourceLayout layout =
			TerminalDescriptionSourceLayout.Canonical;
		int lineWidth =
			DefaultLineWidth;
		bool lineWidthSpecified = false;
		TerminalDescriptionSourceCapabilityOrder capabilityOrder =
			TerminalDescriptionSourceCapabilityOrder.Database;
		bool capabilityOrderSpecified = false;
		bool includeExtendedCapabilities = false;
		bool relativeSynthesis = false;
		bool planning = false;
		bool json = false;
		bool jsonSpecified = false;
		bool allCandidates = false;
		bool allCandidatesSpecified = false;
		int maximumSelectedParentCount =
			TerminalDescriptionSourcePlanningOptions.DefaultMaximumSelectedParentCount;
		bool maximumSelectedParentCountSpecified = false;
		int maximumEvaluatedPlanCount =
			TerminalDescriptionSourcePlanningOptions.DefaultMaximumEvaluatedPlanCount;
		bool maximumEvaluatedPlanCountSpecified = false;
		bool allowNonExhaustiveResult = false;
		bool exhaustivePolicySpecified = false;
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

					case "-u":
						relativeSynthesis = true;
						break;

					case "--plan-use":
						planning = true;
						break;

					case "--json":
						if ( jsonSpecified ) {
							return InfoCmpOptionsParseResult.Failure(
								"option '--json' may be specified only once"
							);
						}
						json = true;
						jsonSpecified = true;
						break;

					case "--all-candidates":
						if ( allCandidatesSpecified ) {
							return InfoCmpOptionsParseResult.Failure(
								"option '--all-candidates' may be specified only once"
							);
						}
						allCandidates = true;
						allCandidatesSpecified = true;
						break;

					case "--candidate-root":
					if ( !TryReadValue(
						args,
						ref index,
						argument,
						out string? candidateRoot,
						out string? candidateRootError
					) ) {
						return InfoCmpOptionsParseResult.Failure( candidateRootError! );
					}
					candidateRoots.Add( candidateRoot! );
					break;

				case "--max-parents":
						if ( maximumSelectedParentCountSpecified ) {
							return InfoCmpOptionsParseResult.Failure(
								"option '--max-parents' may be specified only once"
							);
						}
						if ( !TryReadValue(
								args,
								ref index,
								"--max-parents",
								out string? parentCountText,
								out string? parentCountError
							) ) {
							return InfoCmpOptionsParseResult.Failure(
								parentCountError!
							);
						}
						if ( !int.TryParse(
								parentCountText,
								NumberStyles.None,
								CultureInfo.InvariantCulture,
								out maximumSelectedParentCount
							)
							|| maximumSelectedParentCount < 0
							|| maximumSelectedParentCount
								> TerminalDescriptionSourcePlanningOptions.DefaultMaximumCandidateCount ) {
							return InfoCmpOptionsParseResult.Failure(
								$"option '--max-parents' requires a decimal count between 0 and "
									+ TerminalDescriptionSourcePlanningOptions.DefaultMaximumCandidateCount
							);
						}
						maximumSelectedParentCountSpecified = true;
						break;

					case "--max-plans":
						if ( maximumEvaluatedPlanCountSpecified ) {
							return InfoCmpOptionsParseResult.Failure(
								"option '--max-plans' may be specified only once"
							);
						}
						if ( !TryReadValue(
								args,
								ref index,
								"--max-plans",
								out string? planCountText,
								out string? planCountError
							) ) {
							return InfoCmpOptionsParseResult.Failure(
								planCountError!
							);
						}
						if ( !int.TryParse(
								planCountText,
								NumberStyles.None,
								CultureInfo.InvariantCulture,
								out maximumEvaluatedPlanCount
							)
							|| maximumEvaluatedPlanCount < 1
							|| maximumEvaluatedPlanCount
								> TerminalDescriptionSourcePlanningOptions.MaximumSupportedEvaluatedPlanCount ) {
							return InfoCmpOptionsParseResult.Failure(
								$"option '--max-plans' requires a decimal count between 1 and "
									+ TerminalDescriptionSourcePlanningOptions.MaximumSupportedEvaluatedPlanCount
							);
						}
						maximumEvaluatedPlanCountSpecified = true;
						break;

					case "--require-exhaustive":
						if ( exhaustivePolicySpecified && allowNonExhaustiveResult ) {
							return InfoCmpOptionsParseResult.Failure(
								"options '--require-exhaustive' and '--allow-bounded' are mutually exclusive"
							);
						}
						exhaustivePolicySpecified = true;
						allowNonExhaustiveResult = false;
						break;

					case "--allow-bounded":
						if ( exhaustivePolicySpecified && !allowNonExhaustiveResult ) {
							return InfoCmpOptionsParseResult.Failure(
								"options '--require-exhaustive' and '--allow-bounded' are mutually exclusive"
							);
						}
						exhaustivePolicySpecified = true;
						allowNonExhaustiveResult = true;
						break;

					case "-D":
						return InfoCmpOptionsParseResult.Failure(
							"option '-D' cannot be combined with other arguments"
						);

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

		if ( relativeSynthesis && planning ) {
			return InfoCmpOptionsParseResult.Failure(
				"options '-u' and '--plan-use' are mutually exclusive"
			);
		}
		if ( allCandidates && !planning ) {
			return InfoCmpOptionsParseResult.Failure(
				"option '--all-candidates' requires '--plan-use'"
			);
		}
		if ( candidateRoots.Count != 0 && ( !planning || !allCandidates ) ) {
			return InfoCmpOptionsParseResult.Failure(
				"option '--candidate-root' requires '--plan-use --all-candidates'"
			);
		}
		if ( !planning
			&& ( maximumSelectedParentCountSpecified
				|| maximumEvaluatedPlanCountSpecified
				|| exhaustivePolicySpecified ) ) {
			return InfoCmpOptionsParseResult.Failure(
				"planning-bound options require '--plan-use'"
			);
		}
		if ( json ) {
			if ( relativeSynthesis ) {
				return InfoCmpOptionsParseResult.Failure(
					"option '--json' cannot be combined with '-u'"
				);
			}
			if ( !planning
				&& ( layout != TerminalDescriptionSourceLayout.Canonical
					|| lineWidthSpecified
					|| capabilityOrderSpecified
					|| includeExtendedCapabilities ) ) {
				return InfoCmpOptionsParseResult.Failure(
					"options '-0', '-1', '-w', '-s', and '-x' cannot be combined with '--json' outside planning mode"
				);
			}
			if ( !planning
				&& comparisonMode.HasValue
				&& comparisonMode.Value != InfoCmpComparisonMode.Differences ) {
				return InfoCmpOptionsParseResult.Failure(
					"options '-c' and '-n' cannot be combined with '--json'"
				);
			}
			if ( !planning && shortComparison ) {
				return InfoCmpOptionsParseResult.Failure(
					"option '-q' cannot be combined with '--json'"
				);
			}
			if ( !planning && terminalNames.Count > 2 ) {
				return InfoCmpOptionsParseResult.Failure(
					"option '--json' comparison accepts exactly two terminal operands"
				);
			}
		}

		if ( planning ) {
		if ( candidateRoots.Count != 0 && ( !planning || !allCandidates ) ) {
			return InfoCmpOptionsParseResult.Failure(
				"option '--candidate-root' requires '--plan-use --all-candidates'"
			);
		}
		if ( allCandidates ) {
			if ( !planning ) {
				return InfoCmpOptionsParseResult.Failure(
					"option '--all-candidates' requires '--plan-use'"
				);
			}
			if ( comparisonDatabaseDirectory is not null && candidateRoots.Count != 0 ) {
				return InfoCmpOptionsParseResult.Failure(
					"options '-B' and '--candidate-root' are mutually exclusive for all-candidates planning"
				);
			}
			if ( comparisonDatabaseDirectory is null && candidateRoots.Count == 0 ) {
				return InfoCmpOptionsParseResult.Failure(
					"option '--all-candidates' requires one explicit '-B' directory or at least one '--candidate-root' directory"
				);
			}
			if ( terminalNames.Count != 1 ) {
				return InfoCmpOptionsParseResult.Failure(
					"option '--all-candidates' requires exactly one target terminal"
				);
			}
		} else if ( terminalNames.Count < 2 ) {
				return InfoCmpOptionsParseResult.Failure(
					"option '--plan-use' requires a target and at least one candidate terminal operand"
				);
			}
			if ( !allCandidates
				&& terminalNames.Count - 1
				> TerminalDescriptionSourcePlanningOptions.DefaultMaximumCandidateCount ) {
				return InfoCmpOptionsParseResult.Failure(
					$"option '--plan-use' accepts at most "
						+ $"{TerminalDescriptionSourcePlanningOptions.DefaultMaximumCandidateCount} candidate operands"
				);
			}
			if ( comparisonMode.HasValue ) {
				return InfoCmpOptionsParseResult.Failure(
					"options '-d', '-c', and '-n' cannot be combined with '--plan-use'"
				);
			}
			if ( shortComparison ) {
				return InfoCmpOptionsParseResult.Failure(
					"option '-q' cannot be combined with '--plan-use'"
				);
			}

			if ( !allCandidates ) {
				HashSet<string> candidateReferences = new( StringComparer.Ordinal );
				for ( int index = 1; index < terminalNames.Count; index++ ) {
					if ( !candidateReferences.Add( terminalNames[ index ] ) ) {
						return InfoCmpOptionsParseResult.Failure(
							$"planning candidate reference '{terminalNames[ index ]}' is duplicated"
						);
					}
				}
			}
		} else if ( relativeSynthesis ) {
			if ( terminalNames.Count < 2 ) {
				return InfoCmpOptionsParseResult.Failure(
					"option '-u' requires a target and at least one parent terminal operand"
				);
			}
			if ( terminalNames.Count - 1
				> TerminalDescriptionSourceSynthesisOptions.DefaultMaximumParentCount ) {
				return InfoCmpOptionsParseResult.Failure(
					$"option '-u' accepts at most "
						+ $"{TerminalDescriptionSourceSynthesisOptions.DefaultMaximumParentCount} parent operands"
				);
			}
			if ( comparisonMode.HasValue
				&& comparisonMode.Value != InfoCmpComparisonMode.Common ) {
				return InfoCmpOptionsParseResult.Failure(
					"options '-d' and '-n' cannot be combined with '-u'"
				);
			}
			if ( shortComparison ) {
				return InfoCmpOptionsParseResult.Failure(
					"option '-q' cannot be combined with '-u'"
				);
			}

			HashSet<string> parentReferences = new( StringComparer.Ordinal );
			for ( int index = 1; index < terminalNames.Count; index++ ) {
				if ( !parentReferences.Add( terminalNames[ index ] ) ) {
					return InfoCmpOptionsParseResult.Failure(
						$"relative synthesis parent reference '{terminalNames[ index ]}' is duplicated"
					);
				}
			}

			comparisonMode = null;
		} else if ( terminalNames.Count >= 2 ) {
			if ( layout != TerminalDescriptionSourceLayout.Canonical
				|| lineWidthSpecified
				|| capabilityOrderSpecified ) {
				return InfoCmpOptionsParseResult.Failure(
					"options '-0', '-1', '-w', and '-s' apply only to source listing or relative synthesis"
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
				relativeSynthesis,
				planning,
				json,
				allCandidates,
				candidateRoots,
				maximumSelectedParentCount,
				maximumEvaluatedPlanCount,
				allowNonExhaustiveResult,
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
			if ( currentMode.Value == requestedMode ) {
				error = null;
				return true;
			}

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
