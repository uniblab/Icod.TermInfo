/*
	infocmp
	Selects an acquisition provider for an explicit database root.
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
using Icod.TermInfo.Inspection;

namespace Icod.TermInfo.InfoCmp;

internal static class InfoCmpTerminalProviderFactory {
	internal static ITerminalDescriptionProvider Create(
		string? explicitRoot,
		out string displayLabel
	) {
		if ( explicitRoot is null ) {
			displayLabel = "system terminfo search";
			return new SystemTerminalDescriptionProvider();
		}

		if ( File.Exists( explicitRoot ) ) {
			BerkeleyDbTerminalDescriptionProvider provider =
				new( explicitRoot );
			displayLabel = provider.DatabasePath;
			return provider;
		}

		DirectoryTerminalDescriptionProvider directoryProvider =
			new( explicitRoot );
		displayLabel = directoryProvider.Root;
		return directoryProvider;
	}
}
