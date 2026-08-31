using System.Reflection;
using System.Text;
using CapToInfoCommand = Icod.TermInfo.CapToInfo.Command;
using InfoToCapCommand = Icod.TermInfo.InfoToCap.Command;
using InfoCmpCommand = Icod.TermInfo.InfoCmp.Command;
using TicCommand = Icod.TermInfo.Tic.Command;
using ToeCommand = Icod.TermInfo.Toe.Command;

namespace Icod.TermInfo.Router;

/// <summary>
/// Routes <c>icod-terminfo COMMAND [args...]</c> to the managed terminfo
/// commands without duplicating their command-line semantics.
/// </summary>
public static class Command {
	private const string CommandName = "icod-terminfo";
	private const int UsageError = 2;
	private const int Canceled = 130;

	/// <summary>
	/// Runs the router with caller-owned standard streams.
	/// </summary>
	/// <param name="args">Router arguments.</param>
	/// <param name="stdin">Standard input.</param>
	/// <param name="stdout">Standard output.</param>
	/// <param name="stderr">Standard error.</param>
	/// <param name="cancellationToken">Cancellation request.</param>
	/// <returns>The router or selected-command exit status.</returns>
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
			return Canceled;
		}

		try {
			if ( 0 == args.Length ) {
				await WriteAsync(
					stderr,
					$"{CommandName}: missing command; use --help to list supported commands{Environment.NewLine}",
					cancellationToken
				).ConfigureAwait( false );
				return UsageError;
			}

			string commandName =
				args[ 0 ];
			if (
				"--help" == commandName
				|| "-h" == commandName
			) {
				await WriteAsync(
					stdout,
					GetHelpText(),
					cancellationToken
				).ConfigureAwait( false );
				return 0;
			}
			if (
				"--version" == commandName
				|| "-V" == commandName
			) {
				await WriteAsync(
					stdout,
					$"{CommandName} (Icod.TermInfo) {GetSemanticVersion()}{Environment.NewLine}",
					cancellationToken
				).ConfigureAwait( false );
				return 0;
			}

			if ( !IsKnownCommand( commandName ) ) {
				await WriteAsync(
					stderr,
					$"{CommandName}: unknown command '{commandName}'; use --help to list supported commands{Environment.NewLine}",
					cancellationToken
				).ConfigureAwait( false );
				return UsageError;
			}

			string[] commandArguments =
				CopyCommandArguments( args );
			switch ( commandName ) {
				case "tic":
					return await TicCommand.RunAsync(
						commandArguments,
						stdin,
						stdout,
						stderr,
						cancellationToken
					).ConfigureAwait( false );
				case "infocmp":
					return await InfoCmpCommand.RunAsync(
						commandArguments,
						stdin,
						stdout,
						stderr,
						cancellationToken
					).ConfigureAwait( false );
				case "toe":
					return await ToeCommand.RunAsync(
						commandArguments,
						stdin,
						stdout,
						stderr,
						cancellationToken
					).ConfigureAwait( false );
				case "captoinfo":
					return await CapToInfoCommand.RunAsync(
						commandArguments,
						stdin,
						stdout,
						stderr,
						cancellationToken
					).ConfigureAwait( false );
				case "infotocap":
					return await InfoToCapCommand.RunAsync(
						commandArguments,
						stdin,
						stdout,
						stderr,
						cancellationToken
					).ConfigureAwait( false );
				default:
					throw new InvalidOperationException(
						"Known command dispatch was incomplete."
					);
			}
		} catch ( OperationCanceledException ) {
			return Canceled;
		}
	}

	private static bool IsKnownCommand(
		string commandName
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( commandName );

		return commandName is
			"tic"
			or "infocmp"
			or "toe"
			or "captoinfo"
			or "infotocap";
	}

	private static string[] CopyCommandArguments(
		IReadOnlyList<string> arguments
	) {
		ArgumentNullException.ThrowIfNull( arguments );

		string[] commandArguments =
			new string[ arguments.Count - 1 ];
		for ( int index = 1; index < arguments.Count; ++index ) {
			commandArguments[ index - 1 ] =
				arguments[ index ];
		}
		return commandArguments;
	}

	private static string GetHelpText() {
		return $"Usage: {CommandName} COMMAND [OPTION]... [ARG]...{Environment.NewLine}"
			+ Environment.NewLine
			+ $"Commands:{Environment.NewLine}"
			+ $" tic       validate and publish terminfo source{Environment.NewLine}"
			+ $" infocmp   render and semantically compare terminal descriptions{Environment.NewLine}"
			+ $" toe       enumerate databases and analyze use= dependencies{Environment.NewLine}"
			+ $" captoinfo convert termcap descriptions to effective terminfo source{Environment.NewLine}"
			+ $" infotocap convert effective terminfo source to termcap descriptions{Environment.NewLine}"
			+ Environment.NewLine
			+ $"Router options:{Environment.NewLine}"
			+ $" -h, --help       display this help and exit{Environment.NewLine}"
			+ $" -V, --version    output the coordinated suite version and exit{Environment.NewLine}"
			+ Environment.NewLine
			+ $"Run '{CommandName} COMMAND --help' for command-specific help.{Environment.NewLine}";
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
		if ( 0 <= metadataSeparator ) {
			return informationalVersion[ ..metadataSeparator ];
		}
		return informationalVersion;
	}

	private static async Task WriteAsync(
		Stream stream,
		string text,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stream );
		ArgumentNullException.ThrowIfNull( text );

		byte[] data =
			Encoding.UTF8.GetBytes( text );
		await stream.WriteAsync(
			data,
			cancellationToken
		).ConfigureAwait( false );
	}
}
