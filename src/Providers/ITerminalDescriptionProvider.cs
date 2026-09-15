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

using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo;

/// <summary>
/// Supplies terminal descriptions by canonical name or alias.
/// </summary>
/// <remarks>
/// Providers are caller-owned acquisition components. Implementations may use
/// memory, explicit directory trees, system discovery, or composed databases,
/// but must preserve the clean-miss versus failure boundary of
/// <see cref="TryLoad"/>.
/// </remarks>
public interface ITerminalDescriptionProvider {
	/// <summary>
	/// Attempts to load a terminal description by canonical name or alias.
	/// </summary>
	/// <remarks>
	/// Returning <see langword="false"/> means a clean provider miss and
	/// requires a null result. Returning <see langword="true"/> requires a
	/// non-null immutable description. A provider must not convert permission,
	/// I/O, malformed-data, unsupported-format, or internal parsing failures
	/// into a clean miss.
	/// </remarks>
	bool TryLoad(
		string name,
		[NotNullWhen( true )] out TerminalDescription? terminal
	);
}
