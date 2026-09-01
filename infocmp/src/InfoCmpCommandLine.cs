namespace Icod.TermInfo.InfoCmp;

internal sealed class InfoCmpCommandLineNormalizationResult {
	private InfoCmpCommandLineNormalizationResult(
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

	internal static InfoCmpCommandLineNormalizationResult Success(
		IEnumerable<string> arguments
	) {
		ArgumentNullException.ThrowIfNull( arguments );

		return new InfoCmpCommandLineNormalizationResult(
			arguments.ToArray(),
			null
		);
	}

	internal static InfoCmpCommandLineNormalizationResult Failure(
		string error
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( error );

		return new InfoCmpCommandLineNormalizationResult(
			null,
			error
		);
	}
}

internal static class InfoCmpCommandLineNormalizer {
	internal static InfoCmpCommandLineNormalizationResult Normalize(
		IReadOnlyList<string> args
	) {
		ArgumentNullException.ThrowIfNull( args );

		var normalized = new List<string>();
		bool optionsEnded = false;
		bool preserveNextValue = false;

		foreach ( string? rawArgument in args ) {
			string argument =
				rawArgument
				?? throw new ArgumentException(
					"Command arguments cannot contain null.",
					nameof( args )
				);

			if ( preserveNextValue ) {
				normalized.Add( argument );
				preserveNextValue = false;
				continue;
			}

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
			) {
				normalized.Add( argument );
				continue;
			}

			for ( int offset = 1; offset < argument.Length; offset++ ) {
				char option = argument[ offset ];
				switch ( option ) {
					case '0':
					case '1':
					case 'd':
					case 'c':
					case 'n':
					case 'q':
					case 'u':
					case 'x':
						normalized.Add( $"-{option}" );
						break;

					case 'A':
					case 'B':
					case 'w':
					case 's':
						normalized.Add( $"-{option}" );
						if ( offset + 1 < argument.Length ) {
							normalized.Add( argument[ (offset + 1).. ] );
						} else {
							preserveNextValue = true;
						}
						offset = argument.Length;
						break;

					default:
						return InfoCmpCommandLineNormalizationResult.Failure(
							$"unsupported option '-{option}' in clustered option '{argument}'"
						);
				}
			}
		}

		return InfoCmpCommandLineNormalizationResult.Success( normalized );
	}
}
