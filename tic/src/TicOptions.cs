namespace Icod.TermInfo.Tic;

internal sealed class TicOptions {
	internal TicOptions(
		string sourceOperand,
		IReadOnlyList<string> selectedNames,
		bool allowUnknownExtensions,
		bool checkOnly,
		string? outputDirectory,
		bool summary,
		bool force
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( sourceOperand );
		ArgumentNullException.ThrowIfNull( selectedNames );

		SourceOperand = sourceOperand;
		SelectedNames = selectedNames;
		AllowUnknownExtensions = allowUnknownExtensions;
		CheckOnly = checkOnly;
		OutputDirectory = outputDirectory;
		Summary = summary;
		Force = force;
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

	internal bool CheckOnly {
		get;
	}

	internal string? OutputDirectory {
		get;
	}

	internal bool Summary {
		get;
	}

	internal bool Force {
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
		bool summary = false;
		bool force = false;
		List<string> selectedNames = [];
		string? outputDirectory = null;
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

				case "-s":
					summary = true;
					break;

				case "--force":
					force = true;
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

				case "-o":
					if ( index + 1 >= args.Count ) {
						return TicOptionsParseResult.FromError(
							"option '-o' requires an output directory"
						);
					}
					if ( outputDirectory is not null ) {
						return TicOptionsParseResult.FromError(
							"option '-o' may be specified only once"
						);
					}
					index++;
					if ( string.IsNullOrWhiteSpace( args[ index ] ) ) {
						return TicOptionsParseResult.FromError(
							"option '-o' requires a non-empty output directory"
						);
					}
					outputDirectory = args[ index ];
					break;

				case "-D":
				case "-V":
				case "--version":
				case "--help":
					return TicOptionsParseResult.FromError(
						$"option '{argument}' must be used by itself"
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
							$"unsupported option '{argument}'"
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

		if ( sourceOperand is null ) {
			return TicOptionsParseResult.FromError(
				"exactly one source operand is required"
			);
		}

		if (
			checkOnly
			&& (
				outputDirectory is not null
				|| summary
				|| force
			)
		) {
			return TicOptionsParseResult.FromError(
				"options '-o', '-s', and '--force' are not valid with check-only mode '-c'"
			);
		}

		return TicOptionsParseResult.FromOptions(
			new TicOptions(
				sourceOperand,
				Array.AsReadOnly( selectedNames.ToArray() ),
				allowUnknownExtensions,
				checkOnly,
				outputDirectory,
				summary,
				force
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
