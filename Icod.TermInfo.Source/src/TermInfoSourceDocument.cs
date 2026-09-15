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
/// Represents one parsed terminfo source document before inheritance resolution.
/// </summary>
public sealed class TermInfoSourceDocument {
	internal TermInfoSourceDocument(
		IEnumerable<TermInfoSourceEntry> entries,
		IEnumerable<TermInfoSourceToken> tokens
	) {
		ArgumentNullException.ThrowIfNull( entries );
		ArgumentNullException.ThrowIfNull( tokens );

		Entries = entries.ToArray();
		Tokens = tokens.ToArray();
	}

	/// <summary>
	/// Gets parsed entries in document order.
	/// </summary>
	public IReadOnlyList<TermInfoSourceEntry> Entries { get; }

	/// <summary>
	/// Gets the complete lexical token stream retained from S02.
	/// </summary>
	/// <remarks>
	/// Retaining the token stream preserves comments, invalid lexical units,
	/// exact field text, and source spans for later diagnostics and inspection
	/// without making them part of resolved terminal semantics.
	/// </remarks>
	public IReadOnlyList<TermInfoSourceToken> Tokens { get; }
}
