namespace Icod.TermInfo.Toe;

internal sealed class ToeCommandLineNormalizationResult {
	private ToeCommandLineNormalizationResult(
		string[]? arguments,
		string? error
	) {
		if ( ( arguments is null ) == ( error is null ) ) {
			throw new ArgumentException(
				"Exactly one command-line normalization result must be supplied."
			);
		}

		Arguments = arguments;
		Error = error;
	}

	internal string[]? Arguments {
		get;
	}

	internal string? Error {
		get;
	}

	internal static ToeCommandLineNormalizationResult Success(
		IEnumerable<string> arguments
	) {
		ArgumentNullException.ThrowIfNull( arguments );

		return new ToeCommandLineNormalizationResult(
			arguments.ToArray(),
			null
		);
	}

	internal static ToeCommandLineNormalizationResult Failure(
		string error
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( error );

		return new ToeCommandLineNormalizationResult(
			null,
			error
		);
	}
}

internal sealed class ToeSourceCommandLineResult {
	private ToeSourceCommandLineResult(
		bool isSourceMode,
		ToeSourceDependencyMode mode,
		string? sourcePath,
		string? error
	) {
		IsSourceMode = isSourceMode;
		Mode = mode;
		SourcePath = sourcePath;
		Error = error;
	}

	internal bool IsSourceMode {
		get;
	}

	internal ToeSourceDependencyMode Mode {
		get;
	}

	internal string? SourcePath {
		get;
	}

	internal string? Error {
		get;
	}

	internal static ToeSourceCommandLineResult NotSourceMode() {
		return new ToeSourceCommandLineResult(
			false,
			ToeSourceDependencyMode.Forward,
			null,
			null
		);
	}

	internal static ToeSourceCommandLineResult Success(
		ToeSourceDependencyMode mode,
		string sourcePath
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( sourcePath );

		return new ToeSourceCommandLineResult(
			true,
			mode,
			sourcePath,
			null
		);
	}

	internal static ToeSourceCommandLineResult Failure(
		ToeSourceDependencyMode mode,
		string error
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( error );

		return new ToeSourceCommandLineResult(
			true,
			mode,
			null,
			error
		);
	}
}

internal static class ToeCommandLine {
	internal static ToeSourceCommandLineResult ParseSourceMode(
		IReadOnlyList<string> args
	) {
		ArgumentNullException.ThrowIfNull( args );

		if ( args.Count == 0 ) {
			return ToeSourceCommandLineResult.NotSourceMode();
		}

		string first =
			args[ 0 ]
				?? throw new ArgumentException(
					"Command arguments cannot contain null.",
					nameof( args )
				);
		ToeSourceDependencyMode mode;
		string? attachedPath = null;

		if ( string.Equals( first, "-u", StringComparison.Ordinal ) ) {
			mode = ToeSourceDependencyMode.Forward;
		} else if ( string.Equals( first, "-U", StringComparison.Ordinal ) ) {
			mode = ToeSourceDependencyMode.Reverse;
		} else if (
			first.Length > 2
			&& first.StartsWith( "-u", StringComparison.Ordinal )
		) {
			mode = ToeSourceDependencyMode.Forward;
			attachedPath = first[ 2.. ];
		} else if (
			first.Length > 2
			&& first.StartsWith( "-U", StringComparison.Ordinal )
		) {
			mode = ToeSourceDependencyMode.Reverse;
			attachedPath = first[ 2.. ];
		} else {
			return ToeSourceCommandLineResult.NotSourceMode();
		}

		if ( attachedPath is not null ) {
			if ( args.Count != 1 || string.IsNullOrWhiteSpace( attachedPath ) ) {
				return ToeSourceCommandLineResult.Failure(
					mode,
					$"source dependency option '{first[ ..2 ]}' requires exactly one source file operand"
				);
			}

			return ToeSourceCommandLineResult.Success(
				mode,
				attachedPath
			);
		}

		if (
			args.Count == 3
			&& string.Equals(
				args[ 1 ],
				"--",
				StringComparison.Ordinal
			)
			&& !string.IsNullOrWhiteSpace( args[ 2 ] )
		) {
			return ToeSourceCommandLineResult.Success(
				mode,
				args[ 2 ]
			);
		}

		if (
			args.Count == 2
			&& !string.IsNullOrWhiteSpace( args[ 1 ] )
		) {
			return ToeSourceCommandLineResult.Success(
				mode,
				args[ 1 ]
			);
		}

		return ToeSourceCommandLineResult.Failure(
			mode,
			$"source dependency option '{first}' requires exactly one source file operand"
		);
	}

	internal static ToeCommandLineNormalizationResult NormalizeListing(
		IReadOnlyList<string> args
	) {
		ArgumentNullException.ThrowIfNull( args );

		var normalized = new List<string>();
		bool optionsEnded = false;

		foreach ( string? rawArgument in args ) {
			string argument =
				rawArgument
					?? throw new ArgumentException(
						"Command arguments cannot contain null.",
						nameof( args )
					);

			if ( optionsEnded ) {
				normalized.Add( argument );
				continue;
			}

			if ( string.Equals( argument, "--", StringComparison.Ordinal ) ) {
				normalized.Add( argument );
				optionsEnded = true;
				continue;
			}

			if (
				argument.Length <= 1
				|| argument[ 0 ] != '-'
				|| argument.StartsWith( "--", StringComparison.Ordinal )
				|| string.Equals( argument, "-D", StringComparison.Ordinal )
				|| string.Equals( argument, "-V", StringComparison.Ordinal )
				|| string.Equals( argument, "-u", StringComparison.Ordinal )
				|| string.Equals( argument, "-U", StringComparison.Ordinal )
			) {
				normalized.Add( argument );
				continue;
			}

			for ( int offset = 1; offset < argument.Length; offset++ ) {
				char option = argument[ offset ];
				switch ( option ) {
					case 'a':
					case 'h':
					case 's':
						normalized.Add( $"-{option}" );
						break;

					default:
						return ToeCommandLineNormalizationResult.Failure(
							$"unsupported option '-{option}' in clustered option '{argument}'"
						);
				}
			}
		}

		return ToeCommandLineNormalizationResult.Success( normalized );
	}
}
