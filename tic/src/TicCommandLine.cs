namespace Icod.TermInfo.Tic;

internal sealed class TicCommandLineNormalizationResult {
	private TicCommandLineNormalizationResult(
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

	internal static TicCommandLineNormalizationResult Success(
		IEnumerable<string> arguments
	) {
		ArgumentNullException.ThrowIfNull( arguments );

		return new TicCommandLineNormalizationResult(
			arguments.ToArray(),
			null
		);
	}

	internal static TicCommandLineNormalizationResult Failure(
		string error
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( error );

		return new TicCommandLineNormalizationResult(
			null,
			error
		);
	}
}

internal static class TicCommandLineNormalizer {
	internal static TicCommandLineNormalizationResult Normalize(
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
					case 'c':
					case 'x':
					case 's':
						normalized.Add( $"-{option}" );
						break;

					case 'e':
					case 'o':
						normalized.Add( $"-{option}" );
						if ( offset + 1 < argument.Length ) {
							normalized.Add( argument[ (offset + 1).. ] );
						} else {
							preserveNextValue = true;
						}
						offset = argument.Length;
						break;

					default:
						return TicCommandLineNormalizationResult.Failure(
							$"unsupported option '-{option}' in clustered option '{argument}'"
						);
				}
			}
		}

		return TicCommandLineNormalizationResult.Success( normalized );
	}
}
