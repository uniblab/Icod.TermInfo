namespace Icod.TermInfo.InfoCmp;

/// <summary>
/// Provides the executable entry point for the <c>infocmp</c> command.
/// </summary>
public static class Program {
	/// <summary>
	/// Runs <c>infocmp</c> using the process standard streams and translates
	/// a console interrupt into command cancellation.
	/// </summary>
	/// <param name="args">Command-line arguments.</param>
	/// <returns>The command exit status.</returns>
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
			var stdin =
				Console.OpenStandardInput();
			var stdout =
				Console.OpenStandardOutput();
			var stderr =
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
