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
/// Represents one parsed but unresolved terminfo source entry.
/// </summary>
/// <remarks>
/// The entry is intentionally pre-resolution state. S05 classification and S06
/// cancellation semantics annotate or consume its fields without mutating it;
/// S07 resolves <c>use=</c> inheritance into a separate semantic result. S08
/// materializes that resolved result into <c>TerminalDescription</c> without
/// mutating this source representation.
/// </remarks>
public sealed class TermInfoSourceEntry {
	internal TermInfoSourceEntry(
		string canonicalName,
		IEnumerable<string> aliases,
		string? description,
		IEnumerable<TermInfoSourceField> fields,
		TermInfoSourceSpan span
	) {
		ArgumentNullException.ThrowIfNull( canonicalName );
		ArgumentNullException.ThrowIfNull( aliases );
		ArgumentNullException.ThrowIfNull( fields );
		ArgumentNullException.ThrowIfNull( span );

		CanonicalName = canonicalName;
		Aliases = aliases.ToArray();
		Description = description;
		Fields = fields.ToArray();
		Span = span;
	}

	/// <summary>
	/// Gets the canonical terminal name from the entry header.
	/// </summary>
	public string CanonicalName { get; }

	/// <summary>
	/// Gets alternate terminal names in source order.
	/// </summary>
	public IReadOnlyList<string> Aliases { get; }

	/// <summary>
	/// Gets the descriptive header component when one is present.
	/// </summary>
	public string? Description { get; }

	/// <summary>
	/// Gets unresolved entry fields in source order.
	/// </summary>
	public IReadOnlyList<TermInfoSourceField> Fields { get; }

	/// <summary>
	/// Gets the source span from the canonical name through the final semantic
	/// field or header component retained for this entry.
	/// </summary>
	public TermInfoSourceSpan Span { get; }
}
