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

internal static class SampleAcquisition {
	internal static TerminalDescription ParseCompiledEntry(
		ReadOnlySpan<byte> entry
	) {
		return CompiledTermInfoParser.Parse(
			entry
		);
	}

	internal static TerminalDescription LoadExplicitRoot(
		string root,
		string name
	) {
		ArgumentNullException.ThrowIfNull( root );
		ArgumentNullException.ThrowIfNull( name );

		DirectoryTerminalDescriptionProvider provider =
			new(
				root
			);
		return new TerminalDatabase(
			new ITerminalDescriptionProvider[]
			{
				provider,
			}
		)
			.Load(
				name
			);
	}

	internal static SystemTerminalDescriptionProvider CreateRestrictedSystemProvider() {
		return new SystemTerminalDescriptionProvider(
			new SystemTerminalDescriptionProviderOptions(
				useEnvironment: false,
				useUserDatabase: false,
				useSystemDatabases: false
			)
		);
	}

	internal static SystemTerminalDescriptionProvider CreateSystemProvider() {
		return new SystemTerminalDescriptionProvider();
	}

	internal static TerminalDatabase CreateSystemWithBuiltInFallback(
		SystemTerminalDescriptionProvider? systemProvider = null
	) {
		SystemTerminalDescriptionProvider system =
			systemProvider
			?? CreateSystemProvider();

		return new TerminalDatabase(
			new ITerminalDescriptionProvider[]
			{
				system,
				TerminalDatabase.BuiltIn,
			}
		);
	}
}
