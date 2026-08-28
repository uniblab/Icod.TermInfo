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

			return await InfoCmpInspector.RenderAsync(
				options,
				stdout,
				stderr,
				cancellationToken
			).ConfigureAwait( false );
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
		return $"Usage: {CommandName} [options] [terminal]{Environment.NewLine}"
			+ $"       {CommandName} -D{Environment.NewLine}"
			+ $"       {CommandName} -V | --version{Environment.NewLine}"
			+ Environment.NewLine
			+ $"Inspect one effective terminal description as deterministic terminfo source.{Environment.NewLine}"
			+ $"With no terminal operand, TERM supplies the requested terminal name.{Environment.NewLine}"
			+ Environment.NewLine
			+ $"  -A directory    read the terminal from this explicit database root{Environment.NewLine}"
			+ $"  -0              emit one logical source line without wrapping{Environment.NewLine}"
			+ $"  -1              emit one capability per line without continuation wrapping{Environment.NewLine}"
			+ $"  -w width        request canonical string-capability wrapping width{Environment.NewLine}"
			+ $"  -s key          order standard capabilities by d, i, l, or c{Environment.NewLine}"
			+ $"  -x              include effective extended capabilities{Environment.NewLine}"
			+ $"  -D              print Runtime database discovery locations and exit{Environment.NewLine}"
			+ $"  -V, --version   print the Icod.TermInfo tool-suite version and exit{Environment.NewLine}"
			+ $"      --help      display this help and exit{Environment.NewLine}"
			+ Environment.NewLine
			+ $"Sort keys: d=compiled-table order, i=terminfo short name, l=long variable name, c=termcap code.{Environment.NewLine}"
			+ $"Default listing includes standard capabilities only; use -x for extended capabilities.{Environment.NewLine}"
			+ $"Comparison of two or more terminal operands is introduced by T07.{Environment.NewLine}"
			+ $"Rendered output is effective state; original comments, whitespace, use= history, cancellations, and provenance are not reconstructed.{Environment.NewLine}";
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
