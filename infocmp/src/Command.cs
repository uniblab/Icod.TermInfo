using System.Reflection;
using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo.Inspection;

namespace Icod.TermInfo.InfoCmp;

/// <summary>
/// Implements the managed <c>infocmp</c> command.
/// </summary>
public static class Command {
	private const string CommandName = "infocmp";

	/// <summary>
	/// Runs the command with caller-owned standard streams.
	/// </summary>
	/// <param name="args">Command-line arguments excluding the executable name.</param>
	/// <param name="stdin">Standard input.</param>
	/// <param name="stdout">Standard output.</param>
	/// <param name="stderr">Standard error.</param>
	/// <param name="cancellationToken">Cancellation request.</param>
	/// <returns>The process exit status.</returns>
	public static async Task<int> RunAsync(
		string[] args,
		Stream stdin,
		Stream stdout,
		Stream stderr,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( args );
		ArgumentNullException.ThrowIfNull( stdin );
		ArgumentNullException.ThrowIfNull( stdout );
		ArgumentNullException.ThrowIfNull( stderr );

		if ( cancellationToken.IsCancellationRequested ) {
			return CommandExitCodes.Canceled;
		}

		try {
			InfoCmpCommandLineNormalizationResult normalized =
				InfoCmpCommandLineNormalizer.Normalize( args );
			if ( normalized.Error is string normalizationError ) {
				await WriteUsageErrorAsync(
					stderr,
					normalizationError,
					cancellationToken
				).ConfigureAwait( false );
				return CommandExitCodes.UsageError;
			}
			args =
				normalized.Arguments
				?? throw new InvalidOperationException(
					"The infocmp command-line normalizer returned neither arguments nor an error."
				);

			if ( IsSingleArgument( args, "--help" ) ) {
				await WriteAsync(
					stdout,
					GetHelpText(),
					cancellationToken
				).ConfigureAwait( false );
				return CommandExitCodes.Success;
			}

			if (
				IsSingleArgument( args, "--version" )
				|| IsSingleArgument( args, "-V" )
			) {
				await WriteAsync(
					stdout,
					$"{CommandName} (Icod.TermInfo) {GetSemanticVersion()}{Environment.NewLine}",
					cancellationToken
				).ConfigureAwait( false );
				return CommandExitCodes.Success;
			}

			if ( IsSingleArgument( args, "-D" ) ) {
				return await WriteDatabaseLocationsAsync(
					stdout,
					stderr,
					cancellationToken
				).ConfigureAwait( false );
			}

			InfoCmpOptionsParseResult parsedOptions =
				InfoCmpOptionsParser.Parse( args );
			if ( parsedOptions.Error is string usageError ) {
				await WriteUsageErrorAsync(
					stderr,
					usageError,
					cancellationToken
				).ConfigureAwait( false );
				return CommandExitCodes.UsageError;
			}

			InfoCmpOptions options =
				parsedOptions.Options
				?? throw new InvalidOperationException(
					"The infocmp option parser returned neither options nor an error."
				);

			if ( options.IsPlanning ) {
				return await InfoCmpInspector.PlanAsync(
					options,
					stdout,
					stderr,
					cancellationToken
				).ConfigureAwait( false );
			}

			if ( options.IsSynthesis ) {
				return await InfoCmpInspector.SynthesizeAsync(
					options,
					stdout,
					stderr,
					cancellationToken
				).ConfigureAwait( false );
			}

			return options.IsComparison
				? await InfoCmpInspector.CompareAsync(
					options,
					stdout,
					stderr,
					cancellationToken
				).ConfigureAwait( false )
				: await InfoCmpInspector.RenderAsync(
					options,
					stdout,
					stderr,
					cancellationToken
				).ConfigureAwait( false )
			;
		} catch ( OperationCanceledException ) {
			return CommandExitCodes.Canceled;
		}
	}

	private static async Task<int> WriteDatabaseLocationsAsync(
		Stream stdout,
		Stream stderr,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stdout );
		ArgumentNullException.ThrowIfNull( stderr );

		IReadOnlyList<TermInfoDatabaseLocation> locations;
		try {
			locations =
				TermInfoDatabaseInspector.GetSystemLocations();
		} catch ( Exception exception ) when ( IsOperationalException( exception ) ) {
			await InfoCmpDiagnosticWriter.WriteErrorAsync(
				stderr,
				"INFOCMP0005",
				"database discovery",
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
			return CommandExitCodes.Failure;
		}

		StringBuilder builder = new();
		foreach ( TermInfoDatabaseLocation location in locations ) {
			cancellationToken.ThrowIfCancellationRequested();
			builder.Append( location.Kind );
			builder.Append( '\t' );
			builder.Append(
				location.Path
				?? "<encoded>"
			);
			builder.Append( Environment.NewLine );
		}

		await WriteAsync(
			stdout,
			builder.ToString(),
			cancellationToken
		).ConfigureAwait( false );
		return CommandExitCodes.Success;
	}

	private static async Task WriteUsageErrorAsync(
		Stream stderr,
		string detail,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stderr );
		ArgumentException.ThrowIfNullOrWhiteSpace( detail );

		await WriteAsync(
			stderr,
			$"{CommandName}: {detail}.{Environment.NewLine}Try '{CommandName} --help' for more information.{Environment.NewLine}",
			cancellationToken
		).ConfigureAwait( false );
	}

	private static bool IsOperationalException(
		Exception exception
	) {
		ArgumentNullException.ThrowIfNull( exception );

		return exception is ArgumentException
			or IOException
			or UnauthorizedAccessException
			or NotSupportedException
			or InvalidOperationException;
	}

	private static bool IsSingleArgument(
		IReadOnlyList<string> args,
		string value
	) {
		ArgumentNullException.ThrowIfNull( args );
		ArgumentNullException.ThrowIfNull( value );

		return args.Count == 1
			&& string.Equals(
				args[ 0 ],
				value,
				StringComparison.Ordinal
			);
	}

	private static string GetHelpText() {
		return $"Usage: {CommandName} [options] [terminal ...]{Environment.NewLine}"
			+ $"       {CommandName} -u [options] target parent [parent ...]{Environment.NewLine}"
			+ $"       {CommandName} --plan-use [options] target candidate [candidate ...]{Environment.NewLine}"
			+ $"       {CommandName} -D{Environment.NewLine}"
			+ $"       {CommandName} -V | --version{Environment.NewLine}"
			+ Environment.NewLine
			+ "Inspect one effective terminal, compare terminals semantically, synthesize "
			+ $"relative source, or select relative-source parents.{Environment.NewLine}"
			+ $"With no terminal operand, TERM supplies the requested one-terminal name.{Environment.NewLine}"
			+ $"With two or more operands and no -u or --plan-use, the first is compared with each subsequent terminal.{Environment.NewLine}"
			+ Environment.NewLine
			+ $"Database selection:{Environment.NewLine}"
			+ $"  -A directory    use this explicit database for the first terminal{Environment.NewLine}"
			+ $"  -B directory    use this explicit database for subsequent terminals{Environment.NewLine}"
			+ Environment.NewLine
			+ $"Source presentation:{Environment.NewLine}"
			+ $"  -0              emit one logical source line without wrapping{Environment.NewLine}"
			+ $"  -1              emit one capability per line without continuation wrapping{Environment.NewLine}"
			+ $"  -w width        request canonical string-capability wrapping width{Environment.NewLine}"
			+ $"  -s key          order standard capabilities by d, i, l, or c{Environment.NewLine}"
			+ Environment.NewLine
			+ $"Relative synthesis:{Environment.NewLine}"
			+ $"  -u              rewrite target relative to each ordered parent using use={Environment.NewLine}"
			+ $"  -c -u           accepted as an ncurses-compatible synonym for -u{Environment.NewLine}"
			+ $"  -x              allow required local extended declarations/cancellations{Environment.NewLine}"
			+ Environment.NewLine
			+ $"Relative-source planning:{Environment.NewLine}"
			+ $"      --plan-use           select ordered use= parents from explicit candidates{Environment.NewLine}"
			+ $"      --max-parents count  limit selected ordered parents; default 2{Environment.NewLine}"
			+ $"      --max-plans count    limit evaluated plans; default 4097{Environment.NewLine}"
			+ $"      --require-exhaustive reject a plan space larger than the budget; default{Environment.NewLine}"
			+ $"      --allow-bounded      return the best deterministic evaluated prefix{Environment.NewLine}"
			+ Environment.NewLine
			+ $"Comparison modes:{Environment.NewLine}"
			+ $"  -d              list semantic differences; default for two or more operands{Environment.NewLine}"
			+ $"  -c              list capabilities with common effective values{Environment.NewLine}"
			+ $"  -n              list standard capabilities absent from all compared entries{Environment.NewLine}"
			+ $"  -q              use shorter comparison presentation{Environment.NewLine}"
			+ Environment.NewLine
			+ $"Other options:{Environment.NewLine}"
			+ $"  -x              include effective extended capabilities where defined{Environment.NewLine}"
			+ $"  -D              print Runtime database discovery locations and exit{Environment.NewLine}"
			+ $"  -V, --version   print the Icod.TermInfo tool-suite version and exit{Environment.NewLine}"
			+ $"      --help      display this help and exit{Environment.NewLine}"
			+ Environment.NewLine
			+ "Sort keys: d=compiled-table order, i=terminfo short name, "
			+ $"l=long variable name, c=termcap code.{Environment.NewLine}"
			+ "Without -x, relative synthesis fails if reproducing the target requires "
			+ $"local extended directives.{Environment.NewLine}"
			+ $"The -n universe is the closed standard capability catalog even when -x is supplied.{Environment.NewLine}"
			+ "Unambiguous short options may be clustered; -A, -B, -w, and -s accept "
			+ $"attached values; use -- before a terminal name beginning with '-'.{Environment.NewLine}"
			+ "Relative use= references preserve parent operand spelling and order; "
			+ $"-B applies to every parent or planning candidate.{Environment.NewLine}"
			+ "Planning writes only the selected source to stdout. Comparison selectors, "
			+ $"-u, -q, and -D cannot be combined with --plan-use.{Environment.NewLine}";
	}

	private static string GetSemanticVersion() {
		string informationalVersion =
			typeof( Command )
				.Assembly
				.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
				?.InformationalVersion
				?? "0.0.0";
		int metadataSeparator =
			informationalVersion.IndexOf(
				'+',
				StringComparison.Ordinal
			);

		return (metadataSeparator < 0)
			? informationalVersion
			: informationalVersion[ ..metadataSeparator ]
		;
	}

	private static async Task WriteAsync(
		Stream stream,
		string text,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stream );
		ArgumentNullException.ThrowIfNull( text );

		using StreamWriter writer =
			new(
				stream,
				new UTF8Encoding( false ),
				bufferSize: 1024,
				leaveOpen: true
			);
		await writer.WriteAsync(
			text.AsMemory(),
			cancellationToken
		).ConfigureAwait( false );
		await writer.FlushAsync(
			cancellationToken
		).ConfigureAwait( false );
	}
}
