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

using System.Collections.ObjectModel;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Represents a parsed collection of unresolved termcap terminal descriptions.
/// </summary>
public sealed class TermcapSourceDocument {
	internal TermcapSourceDocument(
		IEnumerable<TermcapSourceEntry> entries
	) {
		ArgumentNullException.ThrowIfNull( entries );

		Entries =
			new ReadOnlyCollection<TermcapSourceEntry>(
				entries.ToArray()
			);
	}

	/// <summary>
	/// Gets parsed terminal descriptions in source order.
	/// </summary>
	public IReadOnlyList<TermcapSourceEntry> Entries { get; }
}
