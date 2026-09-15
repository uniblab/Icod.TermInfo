/*
	icod-terminfo
	Routes the managed Icod.TermInfo command-line tool suite.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

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
