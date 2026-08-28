using System.Reflection;
using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo.Inspection;

namespace Icod.TermInfo.Tic;

/// <summary>
/// Implements the managed <c>tic</c> command.
/// </summary>
public static class Command {
	private const string CommandName = "tic";

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
				await WriteDatabaseLocationsAsync(
					stdout,
					cancellationToken
				).ConfigureAwait( false );
				return CommandExitCodes.Success;
			}

			TicOptionsParseResult parsedOptions =
				TicOptionsParser.Parse( args );
			if ( parsedOptions.Error is string usageError ) {
				await WriteUsageErrorAsync(
					stderr,
					usageError,
					cancellationToken
				).ConfigureAwait( false );
				return CommandExitCodes.UsageError;
			}

			TicOptions options =
				parsedOptions.Options
				?? throw new InvalidOperationException(
					"The tic option parser returned neither options nor an error."
				);

			return await TicSourceValidator.ValidateAsync(
				options,
				stdin,
				stderr,
				cancellationToken
			).ConfigureAwait( false );
		} catch ( OperationCanceledException ) {
			return CommandExitCodes.Canceled;
		}
	}

	private static async Task WriteDatabaseLocationsAsync(
		Stream stdout,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stdout );

		StringBuilder builder = new();
		foreach (
			TermInfoDatabaseLocation location
			in TermInfoDatabaseInspector.GetSystemLocations()
		) {
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
		return $"Usage: {CommandName} -c [options] file{Environment.NewLine}"
			+ $"       {CommandName} -D{Environment.NewLine}"
			+ $"       {CommandName} -V | --version{Environment.NewLine}"
			+ Environment.NewLine
			+ $"Validate terminfo source without modifying a terminfo database.{Environment.NewLine}"
			+ Environment.NewLine
			+ $"  -c              check source only; required for source validation in T04{Environment.NewLine}"
			+ $"  -e name,...     validate only selected canonical names or aliases{Environment.NewLine}"
			+ $"  -x              permit unknown extended capability names{Environment.NewLine}"
			+ $"  -D              print Runtime database discovery locations and exit{Environment.NewLine}"
			+ $"  -V, --version   print the Icod.TermInfo tool-suite version and exit{Environment.NewLine}"
			+ $"      --help      display this help and exit{Environment.NewLine}"
			+ Environment.NewLine
			+ $"Use '-' as file to read UTF-8 source from standard input.{Environment.NewLine}"
			+ $"Database publication options are introduced by T05.{Environment.NewLine}";
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
