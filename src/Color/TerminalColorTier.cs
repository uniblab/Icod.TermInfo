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
/// Provides a convenient classification of a terminal's advertised color depth.
/// </summary>
public enum TerminalColorTier {
	/// <summary>
	/// No usable color selection is advertised.
	/// </summary>
	Monochrome,

	/// <summary>
	/// Four indexed colors are advertised.
	/// </summary>
	Color4,

	/// <summary>
	/// Eight indexed colors are advertised.
	/// </summary>
	Color8,

	/// <summary>
	/// Sixteen indexed colors are advertised.
	/// </summary>
	Color16,

	/// <summary>
	/// Two hundred fifty-six indexed colors are advertised.
	/// </summary>
	Color256,

	/// <summary>
	/// Direct RGB color with eight bits per red, green, and blue channel is
	/// advertised across the full 24-bit range.
	/// </summary>
	TrueColor,

	/// <summary>
	/// An indexed palette with another positive size is advertised.
	/// </summary>
	OtherIndexed,

	/// <summary>
	/// A direct RGB layout other than full 8/8/8 true color is advertised.
	/// </summary>
	OtherDirectRgb,
}
