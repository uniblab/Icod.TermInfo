/*
	Hdb07.PermissionProbe
	Proves public hashed-database permission failures are propagated unchanged.
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

using Icod.TermInfo.BerkeleyDb;

namespace Hdb07.PermissionProbe;

internal static class Program {
	private static int Main( string[] args ) {
		if ( args.Length != 2 ) {
			Console.Error.WriteLine(
				"Usage: Hdb07.PermissionProbe DATABASE TERMINAL"
			);
			return 64;
		}

		try {
			_ = new BerkeleyDbTerminalDescriptionProvider(
				args[0]
			).TryLoad(
				args[1],
				out _
			);
			Console.Error.WriteLine(
				"The protected database was unexpectedly readable."
			);
			return 10;
		} catch ( UnauthorizedAccessException ) {
			return 0;
		} catch ( Exception exception ) {
			Console.Error.WriteLine( exception );
			return 11;
		}
	}
}
