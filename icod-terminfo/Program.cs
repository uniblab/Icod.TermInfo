namespace Icod.TermInfo.Router;

/// <summary>
/// Provides the process entry point for the <c>icod-terminfo</c> command router.
/// </summary>
public static class Program {
	/// <summary>
	/// Runs the router using the process standard streams and translates a console
	/// interrupt into command cancellation.
	/// </summary>
	/// <param name="args">Command-line arguments.</param>
	/// <returns>The router or selected-command exit status.</returns>
	public static async Task<int> Main( string[] args ) {
		ArgumentNullException.ThrowIfNull( args );

		using var cancellation =
			new CancellationTokenSource();
		ConsoleCancelEventHandler handler =
			( _, eventArgs ) => {
				eventArgs.Cancel = true;
				cancellation.Cancel();
			};
		Console.CancelKeyPress += handler;

		try {
			Stream stdin =
				Console.OpenStandardInput();
			Stream stdout =
				Console.OpenStandardOutput();
			Stream stderr =
				Console.OpenStandardError();

			return await Command.RunAsync(
				args,
				stdin,
				stdout,
				stderr,
				cancellation.Token
			).ConfigureAwait( false );
		} finally {
			Console.CancelKeyPress -= handler;
		}
	}
}
