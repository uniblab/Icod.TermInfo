namespace Icod.TermInfo.Tic;

internal sealed class TicOptions {
	internal TicOptions(
		string sourceOperand,
		IReadOnlyList<string> selectedNames,
		bool allowUnknownExtensions
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( sourceOperand );
		ArgumentNullException.ThrowIfNull( selectedNames );

		SourceOperand = sourceOperand;
		SelectedNames = selectedNames;
		AllowUnknownExtensions = allowUnknownExtensions;
	}

	internal string SourceOperand {
		get;
	}

	internal IReadOnlyList<string> SelectedNames {
		get;
	}

	internal bool AllowUnknownExtensions {
		get;
	}
}

internal sealed class TicOptionsParseResult {
	private TicOptionsParseResult(
		TicOptions? options,
		string? error
	) {
		Options = options;
		Error = error;
	}

	internal TicOptions? Options {
		get;
	}

	internal string? Error {
		get;
	}

	internal static TicOptionsParseResult FromOptions(
		TicOptions options
	) {
		ArgumentNullException.ThrowIfNull( options );

		return new TicOptionsParseResult(
			options,
			null
		);
	}

	internal static TicOptionsParseResult FromError(
		string error
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( error );

		return new TicOptionsParseResult(
			null,
			error
		);
	}
}

internal static class TicOptionsParser {
	internal static TicOptionsParseResult Parse(
		IReadOnlyList<string> args
	) {
		ArgumentNullException.ThrowIfNull( args );

		bool checkOnly = false;
		bool allowUnknownExtensions = false;
		List<string> selectedNames = [];
		string? sourceOperand = null;

		for ( int index = 0; index < args.Count; index++ ) {
			string argument = args[ index ];
			switch ( argument ) {
				case "-c":
					checkOnly = true;
					break;

				case "-x":
					allowUnknownExtensions = true;
					break;

				case "-e":
					if ( index + 1 >= args.Count ) {
						return TicOptionsParseResult.FromError(
							"option '-e' requires a comma-separated name list"
						);
					}
					index++;
					string? selectionError =
						AddSelectedNames(
							args[ index ],
							selectedNames
						);
					if ( selectionError is not null ) {
						return TicOptionsParseResult.FromError(
							selectionError
						);
					}
					break;

				case "-D":
				case "-V":
				case "--version":
				case "--help":
					return TicOptionsParseResult.FromError(
						$"option '{argument}' must be used by itself in T04"
					);

				case "-":
					if ( sourceOperand is not null ) {
						return TicOptionsParseResult.FromError(
							"exactly one source operand is required"
						);
					}
					sourceOperand = argument;
					break;

				default:
					if ( argument.StartsWith( "-", StringComparison.Ordinal ) ) {
						return TicOptionsParseResult.FromError(
							$"unsupported T04 option '{argument}'"
						);
					}
					if ( sourceOperand is not null ) {
						return TicOptionsParseResult.FromError(
							"exactly one source operand is required"
						);
					}
					sourceOperand = argument;
					break;
			}
		}

		if ( !checkOnly ) {
			return TicOptionsParseResult.FromError(
				"T04 requires check-only mode '-c'; database publication is introduced by T05"
			);
		}
		if ( sourceOperand is null ) {
			return TicOptionsParseResult.FromError(
				"exactly one source operand is required"
			);
		}

		return TicOptionsParseResult.FromOptions(
			new TicOptions(
				sourceOperand,
				Array.AsReadOnly( selectedNames.ToArray() ),
				allowUnknownExtensions
			)
		);
	}

	private static string? AddSelectedNames(
		string value,
		ICollection<string> destination
	) {
		ArgumentNullException.ThrowIfNull( value );
		ArgumentNullException.ThrowIfNull( destination );

		string[] names = value.Split( ',' );
		foreach ( string rawName in names ) {
			string name = rawName.Trim();
			if ( name.Length == 0 ) {
				return "option '-e' contains an empty source entry name";
			}
			if ( !destination.Contains( name ) ) {
				destination.Add( name );
			}
		}

		return null;
	}
}
