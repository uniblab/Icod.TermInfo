/*
	Icod.TermInfo
	Provides managed terminfo runtime parsing, discovery, capabilities, and terminal profiles.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

namespace Icod.TermInfo;

internal static class XtermDirectTerminalProfile {
	// Baseline: ncurses development terminfo.src revision 1.1267
	// (2026-08-14), matching the xterm baseline used by T15-T16.
	internal static TerminalDescription Create() {
		return new TerminalDescriptionBuilder( "xterm-direct" )
			.SetDescription( "xterm with direct-color indexing" )
			.ApplyXtermCommon()
			.ApplyXtermDirectEightColor()
			.ApplyXtermKeys()
			.ApplyXtermModernMetadata()
			.Build();
	}

	internal static TerminalDescription Create16Color() {
		return new TerminalDescriptionBuilder( "xterm-direct16" )
			.SetDescription( "xterm with direct-colors and 16 indexed colors" )
			.ApplyXtermCommon()
			.ApplyXtermDirectSixteenColor()
			.ApplyXtermKeys()
			.ApplyXtermModernMetadata()
			.Build();
	}

	internal static TerminalDescription Create256Color() {
		return new TerminalDescriptionBuilder( "xterm-direct256" )
			.SetDescription( "xterm with direct-colors and 256 indexed colors" )
			.ApplyXtermCommon()
			.ApplyXtermDirect256Color()
			.ApplyXtermKeys()
			.ApplyXtermModernMetadata()
			.Build();
	}
}
