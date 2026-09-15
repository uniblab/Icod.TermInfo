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

/// <summary>
/// Controls how terminfo padding directives are handled during output.
/// </summary>
public enum PaddingMode {
	/// <summary>
	/// Remove padding directives without delaying output.
	/// </summary>
	Ignore,

	/// <summary>
	/// Honor padding directives using the configured delay provider.
	/// </summary>
	Delay,

	/// <summary>
	/// Honor padding directives by emitting terminal pad characters when
	/// transport facts permit it. This mode requires the terminal-aware
	/// <see cref="TermInfoOutputOptions"/> overloads.
	/// </summary>
	PadCharacters,
}
