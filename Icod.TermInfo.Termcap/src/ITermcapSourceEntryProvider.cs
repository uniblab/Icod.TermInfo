/*
	Icod.TermInfo.Termcap
	Provides managed termcap parsing, conversion, rendering, and interoperability.
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

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Supplies unresolved termcap source entries by terminal name.
/// </summary>
/// <remarks>
/// <para>
/// Providers are caller-owned acquisition components used only for explicit
/// <c>tc=</c> inheritance resolution. They may draw entries from parsed
/// documents, files, generated sources, or other caller-controlled stores.
/// </para>
/// <para>
/// Returning <see langword="false"/> means a clean lookup miss and requires a
/// null result. Returning <see langword="true"/> requires a non-null entry.
/// Provider failures must be reported by throwing rather than being converted
/// into clean misses.
/// </para>
/// </remarks>
public interface ITermcapSourceEntryProvider {
	/// <summary>
	/// Attempts to load an unresolved termcap source entry by terminal name.
	/// </summary>
	bool TryLoad(
		string name,
		[NotNullWhen( true )] out TermcapSourceEntry? entry
	);
}
