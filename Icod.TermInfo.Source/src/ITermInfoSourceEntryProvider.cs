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

using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo.Source;

/// <summary>
/// Supplies unresolved terminfo source entries by canonical name or alias.
/// </summary>
/// <remarks>
/// <para>
/// Providers are caller-owned acquisition components for source inheritance.
/// They may draw entries from one or more parsed documents, files, generated
/// sources, or other stores.
/// </para>
/// <para>
/// Returning <see langword="false"/> means a clean lookup miss and requires a
/// null result. Returning <see langword="true"/> requires a non-null entry.
/// Provider failures must be reported by throwing rather than being converted
/// into clean misses.
/// </para>
/// </remarks>
public interface ITermInfoSourceEntryProvider {
	/// <summary>
	/// Attempts to load an unresolved source entry by canonical name or alias.
	/// </summary>
	bool TryLoad(
		string name,
		[NotNullWhen( true )] out TermInfoSourceEntry? entry
	);
}
