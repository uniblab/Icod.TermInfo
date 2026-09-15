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

internal static class XtermIndexedTerminalProfile {
	// Baseline: ncurses development terminfo.src revision 1.1267
	// (2026-08-14), matching the xterm baseline used by T15/T15¾.
	internal static TerminalDescription Create16Color() {
		return new TerminalDescriptionBuilder( "xterm-16color" )
			.SetDescription( "xterm with 16 colors like aixterm" )
			.ApplyXtermCommon()
			.ApplyXtermSixteenColor()
			.ApplyXtermKeys()
			.ApplyXtermModernMetadata()
			.Build();
	}

	internal static TerminalDescription Create88Color() {
		return new TerminalDescriptionBuilder( "xterm-88color" )
			.SetDescription( "xterm with 88 colors" )
			.ApplyXtermCommon()
			.ApplyXtermExtendedIndexed( 88, 7744 )
			.ApplyXtermPaletteControls()
			.ApplyXtermKeys()
			.ApplyXtermModernMetadata()
			.Build();
	}

	internal static TerminalDescription Create256Color() {
		return new TerminalDescriptionBuilder( "xterm-256color" )
			.SetDescription( "xterm with 256 colors" )
			.ApplyXtermCommon()
			.ApplyXtermExtendedIndexed( 256, 65536 )
			.ApplyXtermPaletteControls()
			.ApplyXtermKeys()
			.ApplyXtermModernMetadata()
			.Build();
	}
}
