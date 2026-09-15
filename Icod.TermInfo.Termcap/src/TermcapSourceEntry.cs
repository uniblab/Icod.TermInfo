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
/// Represents one unresolved termcap terminal description.
/// </summary>
/// <remarks>
/// TC01 deliberately preserves the complete header-name list without assigning
/// canonical-name, alias, or prose-description semantics. Those interpretations
/// belong to the later termcap semantic-model tranche.
/// </remarks>
public sealed class TermcapSourceEntry {
	internal TermcapSourceEntry(
		IEnumerable<string> names,
		IEnumerable<TermcapSourceField> fields,
		TermcapSourceSpan span
	) {
		ArgumentNullException.ThrowIfNull( names );
		ArgumentNullException.ThrowIfNull( fields );
		ArgumentNullException.ThrowIfNull( span );

		Names =
			new ReadOnlyCollection<string>(
				names.ToArray()
			);
		Fields =
			new ReadOnlyCollection<TermcapSourceField>(
				fields.ToArray()
			);
		Span = span;
	}

	/// <summary>
	/// Gets the ordered header components separated by <c>|</c> in the source.
	/// </summary>
	public IReadOnlyList<string> Names { get; }

	/// <summary>
	/// Gets the capability fields in source order.
	/// </summary>
	public IReadOnlyList<TermcapSourceField> Fields { get; }

	/// <summary>
	/// Gets the source span occupied by this terminal description.
	/// </summary>
	public TermcapSourceSpan Span { get; }
}
