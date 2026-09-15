/*
	Icod.TermInfo.Sample
	Demonstrates Icod.TermInfo APIs and integration patterns.
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

namespace Icod.TermInfo.Sample;

internal static class SampleTerminalResolver {
	private static readonly TerminalDatabase DefaultDatabase =
		SampleAcquisition.CreateSystemWithBuiltInFallback();

	internal static TerminalDescription Resolve( string[] arguments ) {
		ArgumentNullException.ThrowIfNull( arguments );

		for ( int i = 0; i < arguments.Length; i++ ) {
			if (
				!string.Equals(
					arguments[i],
					"--profile",
					StringComparison.Ordinal
				)
			) {
				continue;
			}

			if ( i + 1 >= arguments.Length ) {
				throw new ArgumentException(
					"--profile requires a built-in terminal name.",
					nameof( arguments )
				);
			}

			return TerminalDatabase.BuiltIn.Load( arguments[i + 1] );
		}

		return TerminalEnvironment.Resolve(
			DefaultDatabase,
			TerminalProfiles.Dumb
		);
	}

	internal static bool TryResolveSize(
		TerminalDescription terminal,
		out TerminalSize size,
		out string source
	) {
		ArgumentNullException.ThrowIfNull( terminal );

		if ( TerminalEnvironment.TryGetLiveSize( out size ) ) {
			source = "live";
			return true;
		}

		if ( TerminalEnvironment.TryGetEnvironmentSize( out size ) ) {
			source = "environment";
			return true;
		}

		if ( TerminalEnvironment.TryGetProfileSize( terminal, out size ) ) {
			source = "profile";
			return true;
		}

		source = string.Empty;
		return false;
	}
}
