/*
	Icod.TermInfo.Source
	Parses, resolves, renders, and plans terminfo source descriptions.
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

namespace Icod.TermInfo.Source;

/// <summary>
/// Classifies a capability name found in terminfo source.
/// </summary>
public enum TermInfoSourceCapabilityClassification {
	/// <summary>
	/// The name maps to one capability in the standard terminfo catalog.
	/// </summary>
	Standard = 0,

	/// <summary>
	/// The name is a recognized non-standard capability.
	/// </summary>
	KnownExtended = 1,

	/// <summary>
	/// The name is syntactically valid but is not currently recognized.
	/// </summary>
	UnknownExtended = 2,

	/// <summary>
	/// The name is invalid or reserved in terminfo source.
	/// </summary>
	Invalid = 3,
}
